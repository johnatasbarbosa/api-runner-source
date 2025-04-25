import { WebMessageAction } from '../../../../Enums/web-message-action.enum.js';
import { appsData, originalFieldValues, updateOriginalFieldValue } from '../../../../Managers/state-manager.js';
import { postMessage } from '../../../../Utils/utils.js';
import {
  previousAppValue,
  previousEnvValue,
  setPreviousAppValue,
  setPreviousEnvValue,
  unsavedChangesFields,
  updateSaveButtonState
} from '../../Managers/state-manager.js';
import paramsTabHtml from './params-tab.html?raw';


// Constantes para IDs
const ELEMENT_IDS = {
  APP_SELECTOR: 'appSelector',
  ENV_SELECTOR: 'envSelector',
  APP_NAME: 'appName',
  APP_PATH: 'appPath',
  PARAMETROS_CONTAINER: 'parametrosContainer',
  BTN_CRIAR_AMBIENTE: 'btnCriarAmbiente'
};

/**
 * Renderiza o HTML da aba Parâmetros.
 * @returns {string} HTML da aba.
 */
export function renderParamsTab() {
  return paramsTabHtml;
}

/**
 * Configura os elementos e eventos da aba Parâmetros.
 * @param {HTMLElement} modalElement - Elemento do modal.
 * @param {Function} showUnsavedChangesModal - Função para exibir modal de alterações não salvas.
 */
export function setupParamsTab(modalElement, showUnsavedChangesModal) {
  const appSelector = modalElement.querySelector(`#${ELEMENT_IDS.APP_SELECTOR}`);
  const envSelector = modalElement.querySelector(`#${ELEMENT_IDS.ENV_SELECTOR}`);
  const appNameInput = modalElement.querySelector(`#${ELEMENT_IDS.APP_NAME}`);
  const appPathInput = modalElement.querySelector(`#${ELEMENT_IDS.APP_PATH}`);

  // Inicializar seletor de aplicações
  populateAppSelector(appSelector);

  // Atualizar parâmetros iniciais
  updateParameters(modalElement);

  // Configurar eventos
  if (appSelector) {
    appSelector.addEventListener('focus', () => storePreviousAppValue(appSelector));
    appSelector.addEventListener('change', () => handleAppChange(modalElement, showUnsavedChangesModal));
  }

  if (envSelector) {
    envSelector.addEventListener('focus', () => storePreviousEnvValue(envSelector));
    envSelector.addEventListener('change', () => handleEnvChange(modalElement, showUnsavedChangesModal));
  }

  if (appNameInput) {
    appNameInput.addEventListener('input', () => handleFieldInput('appName', appNameInput.value));
  }

  if (appPathInput) {
    appPathInput.addEventListener('input', () => handleFieldInput('appPath', appPathInput.value));
  }
}

/**
 * Popula o seletor de aplicações.
 * @param {HTMLElement} appSelector - Elemento do seletor.
 * @returns {string} O nome da aplicação inicial selecionada.
 */
function populateAppSelector(appSelector) {
  if (!appSelector) return '';

  const previousValue = previousAppValue;
  appSelector.innerHTML = '';

  appsData.forEach(app => {
    const option = document.createElement('option');
    option.value = app.name;
    option.textContent = app.name + (app.fileExists ? '' : ' (Indisponível)');
    appSelector.appendChild(option);
  });

  let initialAppName = '';
  if (appsData.some(app => app.name === previousValue)) {
    appSelector.value = previousValue;
    initialAppName = previousValue;
  } else if (appsData.length > 0) {
    appSelector.value = appsData[0].name;
    initialAppName = appsData[0].name;
  }

  return initialAppName;
}

/**
 * Atualiza os parâmetros exibidos no modal.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
export function updateParameters(modalElement) {
  const appSelector = modalElement.querySelector(`#${ELEMENT_IDS.APP_SELECTOR}`);
  const envSelector = modalElement.querySelector(`#${ELEMENT_IDS.ENV_SELECTOR}`);
  const parametrosContainer = modalElement.querySelector(`#${ELEMENT_IDS.PARAMETROS_CONTAINER}`);
  const appNameInput = modalElement.querySelector(`#${ELEMENT_IDS.APP_NAME}`);
  const appPathInput = modalElement.querySelector(`#${ELEMENT_IDS.APP_PATH}`);
  const ambienteTitle = modalElement.querySelector('#ambienteTitle');

  if (!appSelector || !envSelector || !parametrosContainer || !appNameInput || !appPathInput || !ambienteTitle) return;

  const selectedApp = appSelector.value;
  const selectedEnv = envSelector.value;
  const selectedEnvLower = selectedEnv.toLowerCase();

  ambienteTitle.textContent = `Parâmetros do Ambiente: ${selectedEnv}`;
  const app = appsData.find(a => a.name === selectedApp);

  if (app) {
    appNameInput.value = app.name || '';
    appPathInput.value = app.path || '';
    updateOriginalFieldValue('appName', app.name || '');
    updateOriginalFieldValue('appPath', app.path || '');
  } else {
    appNameInput.value = '';
    appPathInput.value = '';
    parametrosContainer.innerHTML = '<p class="no-params">Selecione uma aplicação válida.</p>';
    return;
  }

  const params = app ? app[selectedEnvLower] : null;

  if (params === null || (typeof params === 'object' && Object.keys(params).length === 0)) {
    updateOriginalFieldValue('params', {});
    parametrosContainer.innerHTML = `
      <p class="no-params">Parâmetros indisponíveis para este ambiente.</p>
      <button class="btn btn-primary smt1" id="${ELEMENT_IDS.BTN_CRIAR_AMBIENTE}">
        <i class="fas fa-plus"></i> Criar ambiente ${selectedEnv}
      </button>
    `;

    const btnCriarAmbiente = modalElement.querySelector(`#${ELEMENT_IDS.BTN_CRIAR_AMBIENTE}`);
    if (btnCriarAmbiente) {
      btnCriarAmbiente.replaceWith(btnCriarAmbiente.cloneNode(true));
      const newBtn = modalElement.querySelector(`#${ELEMENT_IDS.BTN_CRIAR_AMBIENTE}`);
      newBtn.addEventListener('click', () => {
        newBtn.innerHTML = `<i class="fas fa-spinner icon-spin"></i> Criando ambiente ${selectedEnv}...`;
        newBtn.disabled = true;
        postMessage({
          action: WebMessageAction.CreateEnvironment,
          app: { name: selectedApp, environment: selectedEnv }
        });
      });
    }
  } else {
    updateOriginalFieldValue('params', { ...params });
    let html = '';
    for (const [key, value] of Object.entries(params)) {
      html += `
        <div class="input-group">
          <label class="input-label" title="${key}">${key}</label>
          <div class="input-wrapper">
            <input type="text" class="input-field" data-key="${key}" value="${value || ''}" placeholder="${key}">
            <button class="btn-clear" title="Limpar valor"><i class="fas fa-times"></i></button>
          </div>
        </div>
      `;
    }
    parametrosContainer.innerHTML = html;

    parametrosContainer.querySelectorAll('input.input-field').forEach(input => {
      input.addEventListener('input', () => handleFieldInput(`param:${input.dataset.key}`, input.value));
    });

    parametrosContainer.querySelectorAll('.btn-clear').forEach(btn => {
      const newBtn = btn.cloneNode(true);
      btn.replaceWith(newBtn);
      newBtn.addEventListener('click', () => {
        const input = newBtn.previousElementSibling;
        if (input) {
          input.value = '';
          input.dispatchEvent(new Event('input', { bubbles: true }));
        }
      });
    });
  }

  updateSaveButtonState();
}

/**
 * Manipula a entrada em campos da aba Parâmetros.
 * @param {string} fieldKey - Chave do campo.
 * @param {any} value - Valor do campo.
 */
function handleFieldInput(fieldKey, value) {
  let originalValue;
  if (fieldKey.startsWith('param:')) {
    const paramKey = fieldKey.substring(6);
    originalValue = originalFieldValues.params?.[paramKey] ?? '';
  } else {
    originalValue = originalFieldValues[fieldKey] ?? '';
  }

  const hasChanged = String(value) !== String(originalValue);

  if (hasChanged) {
    if (!unsavedChangesFields.has(fieldKey)) unsavedChangesFields.add(fieldKey);
  } else {
    if (unsavedChangesFields.has(fieldKey)) unsavedChangesFields.delete(fieldKey);
  }

  updateSaveButtonState();
}


/**
 * Prepara os dados da aba Parâmetros para salvamento.
 * @param {HTMLElement} modalElement - Elemento do modal.
 * @returns {Object} Objeto com os dados a serem salvos.
 */
export function getParamsSettingsData(modalElement) {
  const appSelector = modalElement.querySelector(`#${ELEMENT_IDS.APP_SELECTOR}`);
  const envSelector = modalElement.querySelector(`#${ELEMENT_IDS.ENV_SELECTOR}`);
  const appNameInput = modalElement.querySelector(`#${ELEMENT_IDS.APP_NAME}`);
  const appPathInput = modalElement.querySelector(`#${ELEMENT_IDS.APP_PATH}`);
  const parametrosContainer = modalElement.querySelector(`#${ELEMENT_IDS.PARAMETROS_CONTAINER}`);

  if (!appSelector || !envSelector || !appNameInput || !appPathInput || !parametrosContainer) {
    return null;
  }

  const params = {};
  parametrosContainer.querySelectorAll('input[data-key]').forEach(input => {
    params[input.dataset.key] = input.value;
  });

  return {
    selectedApp: appSelector.value,
    selectedEnv: envSelector.value.toLowerCase(),
    newName: appNameInput.value,
    newPath: appPathInput.value,
    params
  };
}

/**
 * Reverte os campos alterados para seus valores originais.
 * @param {Set<string>} fieldsToRevert - Conjunto de IDs de campos para reverter.
 */
export function revertParamsFields(fieldsToRevert = unsavedChangesFields) {
  const appNameInput = document.getElementById(ELEMENT_IDS.APP_NAME);
  const appPathInput = document.getElementById(ELEMENT_IDS.APP_PATH);
  const parametrosContainer = document.getElementById(ELEMENT_IDS.PARAMETROS_CONTAINER);

  fieldsToRevert.forEach(fieldId => {
    try {
      if (fieldId === 'appName' && appNameInput) {
        appNameInput.value = originalFieldValues['appName'] ?? '';
        unsavedChangesFields.delete('appName');
      } else if (fieldId === 'appPath' && appPathInput) {
        appPathInput.value = originalFieldValues['appPath'] ?? '';
        unsavedChangesFields.delete('appPath');
      } else if (fieldId.startsWith('param:') && parametrosContainer) {
        const key = fieldId.substring(6);
        const input = parametrosContainer.querySelector(`input[data-key="${key}"]`);
        if (input) {
          input.value = originalFieldValues.params?.[key] ?? '';
          unsavedChangesFields.delete(fieldId);
        }
      }
    } catch (error) {
      console.error(`Erro ao reverter campo ${fieldId}:`, error);
    }
  });

  updateSaveButtonState();
}

/**
 * Manipula a mudança na seleção de aplicação.
 * @param {HTMLElement} modalElement - Elemento do modal.
 * @param {Function} showUnsavedChangesModal - Função para exibir modal de alterações não salvas.
 */
function handleAppChange(modalElement, showUnsavedChangesModal) {
  const appSelector = modalElement.querySelector(`#${ELEMENT_IDS.APP_SELECTOR}`);
  if (!appSelector) return;

  // Captura o novo valor desejado
  const newValue = appSelector.value;

  if (newValue === previousAppValue) return;

  appSelector.value = previousAppValue;

  const paramChanges = new Set([...unsavedChangesFields].filter(field =>
    field === 'appName' || field === 'appPath' || field.startsWith('param:')
  ));

  if (paramChanges.size > 0) {
    showUnsavedChangesModal(
      () => {
        revertParamsFields(paramChanges);
        appSelector.value = newValue;
        setPreviousAppValue(newValue);
        updateParameters(modalElement);
      },
      () => { },
      paramChanges
    );
  } else {
    appSelector.value = newValue;
    setPreviousAppValue(newValue);
    updateParameters(modalElement);
  }
}

/**
 * Manipula a mudança na seleção de ambiente.
 * @param {HTMLElement} modalElement - Elemento do modal.
 * @param {Function} showUnsavedChangesModal - Função para exibir modal de alterações não salvas.
 */
function handleEnvChange(modalElement, showUnsavedChangesModal) {
  const envSelector = modalElement.querySelector(`#${ELEMENT_IDS.ENV_SELECTOR}`);
  if (!envSelector) return;

  const newValue = envSelector.value;

  if (newValue === previousEnvValue) return;

  envSelector.value = previousEnvValue;

  const paramChanges = new Set([...unsavedChangesFields].filter(field =>
    field === 'appName' || field === 'appPath' || field.startsWith('param:')
  ));

  if (paramChanges.size > 0) {
    showUnsavedChangesModal(
      () => {
        revertParamsFields(paramChanges);
        envSelector.value = newValue;
        setPreviousEnvValue(newValue);
        updateParameters(modalElement);
      },
      () => { },
      paramChanges
    );
  } else {
    envSelector.value = newValue;
    setPreviousEnvValue(newValue);
    updateParameters(modalElement);
  }
}

/**
 * Armazena o valor anterior da aplicação.
 * @param {HTMLElement} appSelector - Elemento do seletor.
 */
function storePreviousAppValue(appSelector) {
  if (appSelector) setPreviousAppValue(appSelector.value);
}

/**
 * Armazena o valor anterior do ambiente.
 * @param {HTMLElement} envSelector - Elemento do seletor.
 */
function storePreviousEnvValue(envSelector) {
  if (envSelector) setPreviousEnvValue(envSelector.value);
}