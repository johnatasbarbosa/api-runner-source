import { WebMessageAction } from '../../Enums/web-message-action.enum.js';
import {
  appsData,
  originalFieldValues,
  setInitialCompactInterfaceState,
  setPendingToastAppIndex,
  setSystemLoading,
  updateButtonsState,
  updateOriginalFieldValue
} from '../../Managers/state-manager.js';
import { initTooltips, postMessage, showToast } from '../../Utils/utils.js';
import unsavedChangesModalHtml from './Extras/unsaved-changes-modal.html?raw';
import {
  configModal,
  revertChangedFields,
  revertChangedFieldsByScope,
  setActiveConfirmationModal,
  setConfigModal,
  unsavedChangesFields,
  updateSaveButtonState
} from './Managers/state-manager.js';
import settingsHtml from './settings-component.html?raw';
import './settings.css';
import { getGeneralSettingsData, renderGeneralTab, setupGeneralTab } from './Tabs/General/general-tab.js';
import { getParamsSettingsData, renderParamsTab, setupParamsTab } from './Tabs/Params/params-tab.js';


// Constantes para IDs
const ELEMENT_IDS = {
  CONFIG_MODAL: 'configModal',
  GENERAL_TAB: 'generalTab',
  PARAMS_TAB: 'paramsTab',
  GENERAL_TAB_CONTENT: 'generalTabContent',
  PARAMS_TAB_CONTENT: 'paramsTabContent',
  BTN_SAVE_SETTINGS: 'btnSaveSettings',
  BTN_CLOSE_SETTINGS: 'btnCloseSettings',
  BTN_CANCEL: 'btnCancel'
};

const TAB_IDS = {
  GERAL: 'Geral',
  PARAMETROS: 'Parâmetros'
};

/**
 * Cria e exibe o modal de configurações.
 */
export function createConfigModal() {
  if (configModal) {
    configModal.show();
    return configModal;
  }

  const modalElement = document.createElement('div');
  modalElement.innerHTML = settingsHtml;
  document.body.appendChild(modalElement);

  const modalInstance = {
    element: modalElement,
    show: () => modalElement.style.display = 'block',
    close: () => {
      modalElement.style.display = 'none';
      document.body.removeChild(modalElement);
      setConfigModal(null);
    }
  };

  // Preencher os conteúdos dinâmicos das abas
  const generalTabContent = modalElement.querySelector(`#${ELEMENT_IDS.GENERAL_TAB_CONTENT}`);
  generalTabContent.innerHTML = renderGeneralTab();

  const paramsTabContent = modalElement.querySelector(`#${ELEMENT_IDS.PARAMS_TAB_CONTENT}`);
  paramsTabContent.innerHTML = renderParamsTab();

  initTooltips({ container: modalElement });

  setConfigModal(modalInstance);
  modalInstance.show();

  setupModalEvents(modalElement);

  return modalInstance;
}

/**
 * Configura os eventos do modal.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
function setupModalEvents(modalElement) {
  const generalTab = modalElement.querySelector(`#${ELEMENT_IDS.GENERAL_TAB}`);
  const paramsTab = modalElement.querySelector(`#${ELEMENT_IDS.PARAMS_TAB}`);
  const btnSalvar = modalElement.querySelector(`#${ELEMENT_IDS.BTN_SAVE_SETTINGS}`);
  const btnFechar = modalElement.querySelector(`#${ELEMENT_IDS.BTN_CLOSE_SETTINGS}`);
  const btnCancel = modalElement.querySelector(`#${ELEMENT_IDS.BTN_CANCEL}`);
  const modalContent = modalElement.querySelector('.modal-content');

  // Configurar abas
  setupGeneralTab(modalElement);
  setupParamsTab(modalElement, showUnsavedChangesModal);

  // Eventos de clique nas abas
  generalTab.addEventListener('click', () => handleTabClick(TAB_IDS.GERAL, modalElement));
  paramsTab.addEventListener('click', () => handleTabClick(TAB_IDS.PARAMETROS, modalElement));

  // Evento de salvar
  btnSalvar.addEventListener('click', () => saveSettings(modalElement));

  // Eventos de fechar
  btnFechar.addEventListener('click', () => checkUnsavedChangesBeforeClose(modalElement));
  btnCancel.addEventListener('click', () => checkUnsavedChangesBeforeClose(modalElement));

  // Impedir fechamento ao clicar fora (backdrop estático)
  modalElement.addEventListener('click', (event) => {
    if (!modalContent.contains(event.target)) {
      event.stopPropagation();
    }
  });

  // Impedir fechamento com a tecla Esc
  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && modalElement.style.display === 'block') {
      event.preventDefault();
    }
  });
}

/**
 * Manipula o clique em uma aba.
 * @param {string} tabId - ID da aba.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
function handleTabClick(tabId, modalElement) {
  const currentTab = modalElement.querySelector('.tab-button.active').dataset.tab;
  if (currentTab === tabId) return;

  const paramChanges = new Set([...unsavedChangesFields].filter(field =>
    field === 'appName' || field === 'appPath' || field.startsWith('param:')
  ));

  if (tabId === TAB_IDS.GERAL && paramChanges.size > 0) {
    showUnsavedChangesModal(
      () => {
        revertChangedFieldsByScope(paramChanges);
        switchTab(tabId, modalElement);
      },
      () => { },
      paramChanges
    );
  } else {
    switchTab(tabId, modalElement);
  }
}

/**
 * Alterna entre abas.
 * @param {string} tabId - ID da aba.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
function switchTab(tabId, modalElement) {
  const tabs = modalElement.querySelectorAll('.tab-button');
  const panels = modalElement.querySelectorAll('.tab-panel');

  tabs.forEach(tab => {
    tab.classList.toggle('active', tab.dataset.tab === tabId);
  });

  panels.forEach(panel => {
    panel.style.display = panel.id === (tabId === TAB_IDS.GERAL ? ELEMENT_IDS.GENERAL_TAB_CONTENT : ELEMENT_IDS.PARAMS_TAB_CONTENT) ? 'block' : 'none';
  });
}

/**
 * Exibe o modal de confirmação para alterações não salvas.
 * @param {Function} onConfirm - Função a executar ao confirmar.
 * @param {Function} onCancel - Função a executar ao cancelar.
 */
function showUnsavedChangesModal(onConfirm, onCancel, fieldsToList = unsavedChangesFields) {
  if (document.getElementById('unsavedChangesModal')) return;

  const changedFieldsList = [...fieldsToList].map(fieldId => {
    if (fieldId === 'email') return 'Email institucional';
    if (fieldId === 'compactInterface') return 'Modo compacto de exibição';
    if (fieldId === 'appName') return 'Nome da Aplicação';
    if (fieldId === 'appPath') return 'Caminho da Aplicação';
    if (fieldId.startsWith('param:')) return `Parâmetro: ${fieldId.substring(6)}`;
    return fieldId;
  });

  const listHtml = changedFieldsList.length > 0
    ? `<p class="modal-subtitle">Campos modificados:</p><ul class="changed-fields-list">${changedFieldsList.map(name => `<li>${name}</li>`).join('')}</ul>`
    : '';

  const tempDiv = document.createElement('div');
  tempDiv.innerHTML = unsavedChangesModalHtml;

  const modalBody = tempDiv.querySelector('.modal-body');
  if (modalBody) {
    const description = modalBody.querySelector('.modal-description');
    if (description) {
      description.insertAdjacentHTML('afterend', listHtml);
    }
  }

  const modalElement = tempDiv.firstElementChild;
  document.body.appendChild(modalElement);

  const modalInstance = {
    element: modalElement,
    show: () => modalElement.style.display = 'block',
    close: () => {
      modalElement.style.display = 'none';
      document.body.removeChild(modalElement);
      setActiveConfirmationModal(null);
    }
  };

  setActiveConfirmationModal(modalInstance);
  modalInstance.show();

  const btnConfirm = modalElement.querySelector('#btnConfirmUnsaved');
  const btnCancel = modalElement.querySelector('#btnCancelUnsaved');
  const btnClose = modalElement.querySelector('#btnCloseUnsavedModal');

  btnConfirm.addEventListener('click', () => {
    modalInstance.close();
    onConfirm();
  });

  btnCancel.addEventListener('click', () => {
    modalInstance.close();
    onCancel();
  });

  btnClose.addEventListener('click', () => {
    modalInstance.close();
    onCancel();
  });

  modalElement.addEventListener('click', (event) => {
    const modalContent = modalElement.querySelector('.modal-content');
    if (!modalContent.contains(event.target)) {
      event.stopPropagation();
    }
  });

  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && modalElement.style.display === 'block') {
      event.preventDefault();
    }
  });

  initTooltips({ container: modalElement });
}

function saveSettings(modalElement) {
  if (unsavedChangesFields.size === 0) {
    closeModal(modalElement);
    return;
  }

  // Processamento de dados da aba General
  if ([...unsavedChangesFields].some(field => ['email', 'compactInterface'].includes(field))) {
    const generalData = getGeneralSettingsData(modalElement);
    const currentEmail = document.getElementById('txtEmail');

    if (currentEmail && originalFieldValues['email'] !== generalData.email && unsavedChangesFields.has('email')) {
      currentEmail.value = generalData.email;
      postMessage({ action: WebMessageAction.UpdateEmail, email: generalData.email });
      unsavedChangesFields.delete('email');
      showToast('Email salvo com sucesso!');
    }

    if (originalFieldValues['compactInterface'] !== generalData.compactInterface && unsavedChangesFields.has('compactInterface')) {
      postMessage({ action: WebMessageAction.UpdateCompactInterface, enabled: generalData.compactInterface });
      setInitialCompactInterfaceState(generalData.compactInterface);
      updateOriginalFieldValue('compactInterface', generalData.compactInterface);
      unsavedChangesFields.delete('compactInterface');
      showToast('Configuração de interface salva com sucesso!');
    }
  }

  // Processamento de dados da aba Parâmetros
  const paramsFields = [...unsavedChangesFields].filter(field =>
    field === 'appName' || field === 'appPath' || field.startsWith('param:')
  );

  if (paramsFields.length > 0) {
    const paramsData = getParamsSettingsData(modalElement);

    if (paramsData) {
      const appIndex = appsData.findIndex(a => a.name === paramsData.selectedApp);
      if (appIndex !== -1) {
        setSystemLoading(appIndex, true, false);
        setPendingToastAppIndex(appIndex);
        updateButtonsState();
      }

      const config = {
        action: WebMessageAction.UpdateConfig,
        app: {
          oldName: paramsData.selectedApp,
          name: paramsData.newName,
          path: paramsData.newPath,
          environment: paramsData.selectedEnv,
          params: paramsData.params
        }
      };

      postMessage(config);

      if (unsavedChangesFields.has('appName')) unsavedChangesFields.delete('appName');
      if (unsavedChangesFields.has('appPath')) unsavedChangesFields.delete('appPath');
      unsavedChangesFields.forEach(field => {
        if (field.startsWith('param:')) unsavedChangesFields.delete(field);
      });
    }
  }

  updateSaveButtonState();
  closeModal(modalElement);
}

/**
 * Verifica alterações não salvas antes de fechar o modal.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
function checkUnsavedChangesBeforeClose(modalElement) {
  if (unsavedChangesFields.size > 0) {
    showUnsavedChangesModal(
      () => {
        revertChangedFields();
        closeModal(modalElement);
      },
      () => { }
    );
  } else {
    closeModal(modalElement);
  }
}

/**
 * Fecha o modal de configurações.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
function closeModal(modalElement) {
  if (configModal) {
    configModal.element.style.display = 'none';
    try {
      if (document.body.contains(configModal.element)) {
        document.body.removeChild(configModal.element);
      }
      setConfigModal(null);
    } catch (error) {
      console.warn("Erro ao fechar modal de configurações:", error);
    }
  }
}