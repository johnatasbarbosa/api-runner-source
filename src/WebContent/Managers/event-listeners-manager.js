import { createReleaseNotesModal } from '../Components/ReleaseNotes/release-notes.js';
import { createConfigModal } from '../Components/Settings/settings-component.js';
import { WebMessageAction } from '../Enums/web-message-action.enum.js';
import { WebMessageType } from '../Enums/web-message-type.enum.js';
import {
  handleDataLoaded,
  handleEnvToggled,
  handleGitConnectionError,
  handleGitPullError,
  handleGitUpdateStateChanged,
  handleIpStatus,
  handleReloadStarted,
  handleSystemLoading,
  handleVersionStatus,
} from '../Handlers/business-handler.js';
import { showErrorModal } from '../Handlers/error-handler.js';
import {
  activeModal,
  initialCompactInterfaceState,
  isReloading,
  setConfigModal,
  setInitialCompactInterfaceState,
  setInitialLoadComplete,
  setIsReloading,
  setReleaseNotesModal,
  setSystemLoading,
  updateButtonsState,
  updateOriginalFieldValue,
  vsOpenApps
} from '../Managers/state-manager.js';
import { checkInternetConnection, postMessage, setButtonLoading } from '../Utils/utils.js';

// Constantes para IDs dos botões
const BUTTON_IDS = {
  RELOAD: 'btnReload',
  UPDATE_IP: 'btnAtualizarIP',
  UPDATE_VERSION: 'btnUpdateVersion',
  SETTINGS: 'btnSettings',
  RELEASE_NOTES: 'btnReleaseNotes',
};

/**
 * Configura os event listeners para os botões principais.
 */
export const configureButtonListeners = () => {
  configureReloadButton();
  configureUpdateIPButton();
  configureUpdateVersionButton();
  configureConfigButton();
  configureReleaseNotesButton();
};

/**
 * Configura o botão de recarregar dados.
 */
const configureReloadButton = () => {
  const btnReload = document.getElementById(BUTTON_IDS.RELOAD);
  if (!btnReload) return;

  btnReload.addEventListener('click', () => {
    if (btnReload.disabled || isReloading) return;

    setButtonLoading(btnReload, true);
    setIsReloading(true);
    updateButtonsState();
    postMessage({ action: WebMessageAction.Reload });
  });
};

/**
 * Configura o botão de atualização de IP.
 */
const configureUpdateIPButton = () => {
  const btnUpdateIP = document.getElementById(BUTTON_IDS.UPDATE_IP);
  if (!btnUpdateIP) return;

  btnUpdateIP.addEventListener('click', async () => {
    if (btnUpdateIP.disabled) return;

    const isOnline = await checkInternetConnection();
    if (!isOnline) {
      showErrorModal('Sem conexão com a internet. Verifique sua rede e tente novamente.');
      return;
    }

    setButtonLoading(btnUpdateIP, true);
    postMessage({ action: WebMessageAction.ReleaseIP });
  });
};

/**
 * Configura o botão de atualização de versão.
 */
const configureUpdateVersionButton = () => {
  const btnUpdateVersion = document.getElementById(BUTTON_IDS.UPDATE_VERSION);
  if (!btnUpdateVersion) return;

  btnUpdateVersion.addEventListener('click', async () => {
    if (btnUpdateVersion.disabled) return;

    const isOnline = await checkInternetConnection();
    if (!isOnline) {
      showErrorModal('Sem conexão com a internet. Verifique sua rede e tente novamente.');
      return;
    }

    setButtonLoading(btnUpdateVersion, true);
    postMessage({ action: WebMessageAction.UpdateVersion });
  });
};

/**
 * Configura o botão de configurações.
 */
const configureConfigButton = () => {
  const btnConfig = document.getElementById(BUTTON_IDS.SETTINGS);
  if (!btnConfig) return;

  btnConfig.addEventListener('click', () => {
    if (btnConfig.disabled) return;

    activeModal?.close();
    const modal = createConfigModal();
    setConfigModal(modal);
  });
};

/**
 * Configura o botão de notas de versão.
 */
export const configureReleaseNotesButton = () => {
  const btnReleaseNotes = document.getElementById(BUTTON_IDS.RELEASE_NOTES);
  if (!btnReleaseNotes) return;

  btnReleaseNotes.addEventListener('click', () => {
    if (btnReleaseNotes.disabled) return;

    activeModal?.close();
    const modal = createReleaseNotesModal();
    if (modal) {
      modal.show(); // Adicionando esta linha para mostrar o modal
      setReleaseNotesModal(modal);
    }
  });
};

/**
 * Configura o listener principal para mensagens do WebView.
 */
export const configureWebviewMessageListener = () => {
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', (event) => {
      const message = event.data;
      if (message.type == null) return;

      // Handlers para mensagens relacionadas a dados e estado
      switch (message.type) {
        case WebMessageType.DataLoaded:
          handleDataLoaded(message);
          break;

        case WebMessageType.ReloadStarted:
          handleReloadStarted();
          break;

        case WebMessageType.EnvToggled:
          handleEnvToggled(message);
          break;

        // Handlers para status
        case WebMessageType.IpStatus:
          handleIpStatus(message);
          break;

        case WebMessageType.VersionStatus:
          handleVersionStatus(message);
          break;

        // Handlers para Git
        case WebMessageType.GitUpdateStateChanged:
          handleGitUpdateStateChanged(message);
          break;

        case WebMessageType.GitPullError:
          handleGitPullError(message);
          break;

        case WebMessageType.GitConnectionError:
          handleGitConnectionError(message);
          break;

        // Handlers para sistema
        case WebMessageType.SystemLoading:
          handleSystemLoading(message);
          break;

        case WebMessageType.VsProcessExited:
          if (message.index !== undefined) {
            vsOpenApps.delete(message.index);
          }
          break;

        case WebMessageType.InitialCompactInterface:
          setInitialCompactInterfaceState(message.compactInterface);
          updateOriginalFieldValue('compactInterface', initialCompactInterfaceState);
          const compactInterfaceToggle = document.querySelector('#compactInterfaceToggle');
          if (compactInterfaceToggle) {
            compactInterfaceToggle.checked = initialCompactInterfaceState;
          }
          break;

        // Handlers para ações concluídas
        case WebMessageType.DirectoryActionCompleted:
        case WebMessageType.VsOpenActionCompleted:
          if (message.index !== undefined) {
            setSystemLoading(message.index, false, false);
            if (message.type === WebMessageType.DirectoryActionCompleted && !message.canceled) {
              const btnReload = document.getElementById(BUTTON_IDS.RELOAD);
              setButtonLoading(btnReload, false);
              setInitialLoadComplete(true);
              setIsReloading(false);
            }
          }
          break;

        default:
          break;
      }

      updateButtonsState();
    });
  } else {
    console.warn('WebView não está disponível para configurar o listener de mensagens.');
  }
};