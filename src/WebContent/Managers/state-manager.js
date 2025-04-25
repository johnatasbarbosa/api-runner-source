import { updateBranchCell } from '../Managers/dom-manager.js';
import { setButtonLoading } from '../Utils/utils.js';

const CURSOR_NOT_ALLOWED = "not-allowed";
const CURSOR_POINTER = "pointer";
const STATUS_RELOADING = "Recarregando dados";
const STATUS_LOADING = "Carregando dados";
const STATUS_UPDATING = "Atualizando dados";
const STATUS_RELOAD = "Recarregar dados";

export let isReloading = false; // Para o recarregamento de dados
export let activeModal = null; // Para verificar se o modal está ativo
export let loadingSystems = new Set(); // Para controlar quais sistemas estão carregando
export let configModal = null; // Para controlar o modal de configurações
export let releaseNotesModal = null; // Para controlar o modal de notas de versão
export let appsData = []; // Para armazenar os dados das aplicações
export let initialLoadComplete = false; // Flag para indicar que o carregamento inicial COMPLETO terminou
export let buttonDisableTimeout = null; // Timer para reabilitar botões
export let unsavedChangesFields = new Set(); // Rastreia campos específicos alterados
export let originalFieldValues = {}; // Armazena valores originais dos campos no modal
export let pendingToastAppIndex = null; // Índice da app aguardando toast após salvar parâmetros
export let initialCompactInterfaceState = true; // Armazena o estado inicial do toggle
export let vsOpenApps = new Set(); // Armazena os aplicativos abertos no VS Studio
export let currentIpStatus = "Verificando IP"; // Armazena o status atual do IP

// Funções setter para modificar as variáveis
export function setIsReloading(value) {
  isReloading = value;
}

export function setActiveModal(modal) {
  activeModal = modal;
}

export function setConfigModal(modal) {
  configModal = modal;
}

export function setAppsData(newData) {
  appsData.length = 0;
  appsData.push(...newData);
}

export function clearAppsData() {
  appsData.length = 0;
}

export function clearLoadingSystems() {
  loadingSystems.clear();
}

export function setInitialLoadComplete(value) {
  initialLoadComplete = value;
}

export function setButtonDisableTimeout(timeout) {
  buttonDisableTimeout = timeout;
}

export function setPendingToastAppIndex(index) {
  pendingToastAppIndex = index;
}

export function setCurrentIpStatus(newStatus) {
  currentIpStatus = newStatus;
}

export function updateOriginalFieldValue(key, value) {
  originalFieldValues[key] = value;
}

export function setInitialCompactInterfaceState(value) {
  initialCompactInterfaceState = value;
}

export function setReleaseNotesModal(modal) {
  releaseNotesModal = modal;
  activeModal = modal;
}

/**
 * Gerencia o estado de loading de um sistema.
 * @param {number} index O índice do sistema.
 * @param {boolean} isLoading True para ativar loading, false para desativar.
 */
export function setSystemLoading(index, isLoading, isGitUpdating) {
  if (isLoading) {
    loadingSystems.add(index);
    updateBranchCell(index, isLoading, null, null);
  } else {
    loadingSystems.delete(index);
    const systemCell = document.querySelector(`#dgvApps tbody tr:nth-child(${index + 1}) .system-name .system-loading`);
    if (systemCell) {
      systemCell.style.display = 'none';
    }
    if (index >= 0 && index < appsData.length) {
      const app = appsData[index];
      const branchText = app.currentBranch ? `${app.currentBranch} (${app.commits ?? '?'})` : '--';
      updateBranchCell(index, false, app.currentBranch || '--', app.commits);
    }
  }
}

/**
 * Atualiza o estado dos botões com base no estado atual do sistema.
 */
export function updateButtonsState() {
  const btnConfig = document.getElementById('btnSettings');
  const btnReload = document.getElementById('btnReload');

  if (buttonDisableTimeout) {
    clearTimeout(buttonDisableTimeout);
    buttonDisableTimeout = null;
  }

  const isSystemBusy = isReloading || loadingSystems.size > 0;
  const isInitialPhase = !initialLoadComplete;
  const allFilesMissing = appsData.length > 0 && appsData.every(app => !app.fileExists);

  updateButtonState(btnConfig, shouldDisableConfig(isInitialPhase, isSystemBusy, allFilesMissing), allFilesMissing);
  updateReloadButtonState(btnReload, isInitialPhase, isSystemBusy, allFilesMissing);
}

/**
 * Atualiza o estado do botão de configuração.
 */
function updateButtonState(button, shouldDisable, allFilesMissing) {
  if (!button) return;

  button.disabled = shouldDisable;
  button.title = shouldDisable
    ? isReloading
      ? STATUS_RELOADING
      : STATUS_LOADING
    : allFilesMissing
      ? "Configure os caminhos das aplicações"
      : "";
  button.style.cursor = shouldDisable ? CURSOR_NOT_ALLOWED : CURSOR_POINTER;
}

/**
 * Atualiza o estado do botão de recarregar.
 */
function updateReloadButtonState(button, isInitialPhase, isSystemBusy, allFilesMissing) {
  if (!button) return;

  const shouldDisable = shouldDisableConfig(isInitialPhase, isSystemBusy, allFilesMissing);
  const isVisuallyLoading = button.querySelector('.branch-loading:not([style*="display: none"])');

  if (shouldDisable && !isVisuallyLoading) {
    button.disabled = true;
    button.style.cursor = CURSOR_NOT_ALLOWED;
    button.innerHTML = `
      <i class="fas fa-sync-alt icon-spin"></i>
      ${isInitialPhase ? STATUS_LOADING : isReloading ? STATUS_RELOADING : STATUS_UPDATING}
    `;
    setReloadTimeout(button);
  } else if (!shouldDisable && !isVisuallyLoading) {
    setButtonLoading(button, false);
    button.disabled = false;
    button.style.cursor = CURSOR_POINTER;
    button.innerHTML = `
      <i class="fas fa-sync-alt"></i>
      ${STATUS_RELOAD}
    `;
  }
}

/**
 * Define um timeout para reabilitar o botão de recarregar.
 */
function setReloadTimeout(button) {
  buttonDisableTimeout = setTimeout(() => {
    if (button && button.disabled && button.style.cursor === CURSOR_NOT_ALLOWED) {
      if (!button.querySelector('.branch-loading:not([style*="display: none"])')) {
        setButtonLoading(button, false);
        button.disabled = false;
        button.title = "";
        button.style.cursor = CURSOR_POINTER;
        setIsReloading(false);
        setInitialLoadComplete(true);
        button.innerHTML = `
          <i class="fas fa-sync-alt"></i>
          ${STATUS_RELOAD}
        `;
      }
    }
    buttonDisableTimeout = null;
  }, 15000);
}

/**
 * Verifica se o botão deve ser desabilitado.
 */
function shouldDisableConfig(isInitialPhase, isSystemBusy, allFilesMissing) {
  return (isInitialPhase || isSystemBusy) && !allFilesMissing;
}