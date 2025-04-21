import { updateParameters } from '../Components/Settings/Tabs/Params/params-tab.js';
import { WebMessageAction } from '../Enums/web-message-action.enum.js';
import { showErrorModal } from '../Handlers/error-handler.js';
import {
  fillTable,
  updateBranchCell,
  updateBranchCellOnGitError,
  updateBranchSpinners,
  updateCompactInterfaceToggle,
  updateEmailField,
  updateEnvButton,
  updateGitButtonOnError,
  updateIpButton,
  updateVersionBadge,
  updateVersionButton
} from '../Managers/dom-manager.js';
import { setInitialCompactInterfaceState, setIsReloading, updateOriginalFieldValue } from '../Managers/state-manager.js';
import { postMessage, setButtonClick, setButtonLoading, showToast } from '../Utils/utils.js';

import {
  appsData,
  clearAppsData,
  clearLoadingSystems,
  currentIpStatus,
  isReloading,
  loadingSystems,
  pendingToastAppIndex,
  setAppsData,
  setCurrentIpStatus,
  setInitialLoadComplete,
  setPendingToastAppIndex,
  setSystemLoading,
  updateButtonsState
} from '../Managers/state-manager.js';

const ELEMENT_IDS = {
  BTN_RELOAD: 'btnReload',
  BTN_UPDATE_IP: 'btnUpdateIP',
  BTN_UPDATE_VERSION: 'btnUpdateVersion',
  TXT_EMAIL: 'txtEmail',
  VERSION_BADGE: '.version-badge',
  COMPACT_INTERFACE_TOGGLE: '#compactInterfaceToggle'
};

/**
 * Manipula o evento de dados carregados
 * @param {Object} data - Dados recebidos
 */
export function handleDataLoaded(data) {
  const btnReload = document.getElementById(ELEMENT_IDS.BTN_RELOAD);
  if (btnReload) setButtonLoading(btnReload, false);

  const previouslyLoadingSystems = new Set(loadingSystems);
  clearAppsData();
  setAppsData(data.apps || []);
  clearLoadingSystems();
  setIsReloading(false);
  fillTable(appsData);

  if (pendingToastAppIndex !== null) {
    const appJustFinishedLoading = previouslyLoadingSystems.has(pendingToastAppIndex) &&
      !loadingSystems.has(pendingToastAppIndex);
    if (appJustFinishedLoading && pendingToastAppIndex < appsData.length) {
      showToast('Parâmetros salvos com sucesso!');
      setPendingToastAppIndex(null);
    }
  }

  if (data.isInitialLoad === true) {
    setInitialLoadComplete(true);
  }

  if (data.isInitialLoad === true && data.compactInterface !== undefined) {
    setInitialCompactInterfaceState(data.compactInterface);
    updateOriginalFieldValue('compactInterface', data.compactInterface);
    updateCompactInterfaceToggle(data.compactInterface);
  }

  updateButtonsState();
  updateEmailField(data.email);
  updateVersionBadge(data.version);

  const modalElement = document.getElementById('configModal');
  if (updateParameters && modalElement) updateParameters(modalElement);
}

/**
 * Manipula o início do recarregamento
 */
export function handleReloadStarted() {
  setIsReloading(true);
  updateButtonsState();
  updateBranchSpinners();
}

/**
 * Manipula o toggle de ambiente
 * @param {Object} data - Dados do evento
 */
export function handleEnvToggled(data) {
  const { index, env, isRunning } = data;
  const app = appsData[index];

  updateEnvButton(index, env, isRunning);

  if (app?.environment && app.environment !== env) {
    updateEnvButton(index, app.environment, false);
  }

  if (app) app.environment = isRunning ? env : null;
}

/**
 * Manipula o status do IP
 * @param {Object} data - Dados do status do IP
 */
export function handleIpStatus(data) {
  const { status, enabled } = data;
  updateIpButton(status, enabled);

  if (currentIpStatus !== status) {
    setCurrentIpStatus(status);
    if (appsData.length > 0) fillTable(appsData);
  }

  setButtonClick(ELEMENT_IDS.BTN_UPDATE_IP, () => {
    postMessage({ action: WebMessageAction.ReleaseIP });
  });
}

/**
 * Manipula o status da versão
 * @param {Object} data - Dados do status da versão
 */
export function handleVersionStatus(data) {
  const updateAvailable = data.updateAvailable ?? false;
  updateVersionButton(updateAvailable);
}

/**
 * Manipula a mudança de estado de atualização do Git
 * @param {Object} data - Dados do evento
 */
export function handleGitUpdateStateChanged(data) {
  if (data.index >= 0 && data.index < appsData.length) {
    const app = appsData[data.index];
    app.isGitUpdating = data.isUpdating;

    if (!data.isUpdating) {
      app.currentBranch = data.branch;
      app.commits = data.commits;
      const row = document.querySelector(`#dgvApps tbody tr:nth-child(${data.index + 1})`);
      const btnGitPull = row?.querySelector('.btn-git-pull');
      if (btnGitPull) setButtonLoading(btnGitPull, false);
    }

    updateBranchCell(data.index, data.isUpdating, data.branch, data.commits);
  }
}

/**
 * Manipula erros de pull do Git
 * @param {Object} data - Dados do erro
 */
export function handleGitPullError(data) {
  updateGitButtonOnError(data.index);
  showErrorModal(data.message);
}

/**
 * Manipula o carregamento do sistema
 * @param {Object} data - Dados do carregamento
 */
export function handleSystemLoading(data) {
  setSystemLoading(data.index, data.isLoading, data.isGitUpdating);
  updateButtonsState();
}

/**
 * Manipula erros de conexão do Git
 * @param {Object} data - Dados do erro
 */
export function handleGitConnectionError(data) {
  if (window.currentReloadTimeout) {
    clearTimeout(window.currentReloadTimeout);
    window.currentReloadTimeout = null;
  }

  showErrorModal(data.message);

  if (isReloading) {
    setIsReloading(false);
    const btnReload = document.getElementById(ELEMENT_IDS.BTN_RELOAD);
    if (btnReload) setButtonLoading(btnReload, false);
    updateButtonsState();
  }

  if (data.appIndex >= 0 && data.appIndex < appsData.length) {
    const app = appsData[data.appIndex];
    if (app) {
      app.isGitUpdating = false;
      updateBranchCellOnGitError(data.appIndex);
    }
  }
}

/**
 * Alterna o ambiente
 * @param {HTMLElement} buttonElement - Elemento do botão
 * @param {number} index - Índice da aplicação
 * @param {string} env - Ambiente
 */
export function toggleEnv(buttonElement, index, env) {
  const icon = buttonElement.querySelector('.icon-original i');
  const isPause = icon.classList.contains('fa-pause');

  if (isPause) {
    postMessage({ action: WebMessageAction.ToggleEnv, index, env });
  } else {
    setButtonLoading(buttonElement, true);
    postMessage({ action: WebMessageAction.ToggleEnv, index, env });
  }
}

/**
 * Executa git pull
 * @param {HTMLElement} buttonElement - Elemento do botão
 * @param {number} index - Índice da aplicação
 */
export function gitPull(buttonElement, index) {
  setButtonLoading(buttonElement, true);
  postMessage({ action: WebMessageAction.PullGit, index });
}