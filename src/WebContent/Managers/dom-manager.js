import { EnvironmentEnum } from '../Enums/environment.enum.js';
import { IpStatus } from '../Enums/ip-status.enum.js';
import { WebMessageAction } from '../Enums/web-message-action.enum.js';
import { gitPull, toggleEnv } from '../Handlers/business-handler.js';
import { currentIpStatus, loadingSystems, setSystemLoading } from '../Managers/state-manager.js';
import { postMessage, setButtonLoading } from '../Utils/utils.js';

const ENV_NAMES = Object.keys(EnvironmentEnum);

// TABLE POPULATION
/**
 * Preenche a tabela de aplicações com os dados recebidos.
 * @param {Array} apps Array de objetos representando as aplicações.
 */
export function fillTable(apps) {
  const tbody = document.querySelector("#dgvApps tbody");
  if (!tbody || !Array.isArray(apps)) return;

  const scrollY = tbody.scrollTop;
  const scrollX = tbody.scrollLeft;

  tbody.innerHTML = '';

  apps.forEach((app, index) => {
    if (!app || typeof app !== 'object') return;

    const row = tbody.insertRow();
    createCell(row, 'system-name', app, index);
    createBranchCell(row, app, index);
    createGitCell(row, app, index);
    createEnvironmentCells(row, app, index);
  });

  tbody.scrollTop = scrollY;
  tbody.scrollLeft = scrollX;
}

// CELL CREATION
/**
 * Cria uma célula para o nome do sistema.
 * @param {HTMLTableRowElement} row Linha da tabela.
 * @param {string} className Classe CSS da célula.
 * @param {Object} app Dados da aplicação.
 * @param {number} index Índice da aplicação.
 */
export function createCell(row, className, app, index) {
  const cell = row.insertCell();
  cell.className = className;

  const systemName = app.name || "Nome Indefinido";
  if (!app.fileExists) {
    cell.innerHTML = `
      <span class="system-name-text">${systemName}</span>
      <span class="icon-container">
        <i class="fas fa-exclamation-circle system-alert-color"></i>
        <div class="tooltip">Sistema não encontrado</div>
      </span>
    `;
  } else {
    cell.textContent = systemName;
  }

  cell.addEventListener('click', () => {
    if (!loadingSystems.has(index)) {
      setSystemLoading(index, true, app.isGitUpdating);
      postMessage({
        action: app.fileExists ? WebMessageAction.OpenVS : WebMessageAction.SelectDirectory,
        index
      });
    }
  });
}

/**
 * Cria a célula para o nome da branch.
 * @param {HTMLTableRowElement} row Linha da tabela.
 * @param {Object} app Dados da aplicação.
 * @param {number} index Índice da aplicação.
 */
export function createBranchCell(row, app, index) {
  const cellBranch = row.insertCell();
  cellBranch.className = "branch-name";

  if (app.isGitUpdating) {
    cellBranch.innerHTML = '<span class="branch-loading"><i class="fas fa-spinner icon-spin"></i></span>';
  } else {
    const branchText = getBranchText(app);
    cellBranch.innerHTML = `<span class="branch-text">${branchText}</span>`;
  }
}

/**
 * Cria a célula para o botão Git Pull.
 * @param {HTMLTableRowElement} row Linha da tabela.
 * @param {Object} app Dados da aplicação.
 * @param {number} index Índice da aplicação.
 */
export function createGitCell(row, app, index) {
  const cellGit = row.insertCell();
  cellGit.className = "git-cell";

  if (!app.fileExists) {
    cellGit.innerHTML = '--';
    return;
  }

  const btnGit = document.createElement('button');
  btnGit.className = 'btn btn-git-pull';
  btnGit.innerHTML = `<span class="icon-original"><i class="fas fa-download"></i></span>`;

  const isBranchInvalid = !app.currentBranch || app.currentBranch === "--";
  const isDisabled = isBranchInvalid || app.isGitUpdating;
  btnGit.disabled = isDisabled;
  btnGit.style.cursor = isDisabled ? 'not-allowed' : 'pointer';

  const tooltipText = isBranchInvalid ? 'Branch indisponível' : (app.isGitUpdating ? 'Atualizando branch' : 'Atualizar branch');
  const tooltip = document.createElement('div');
  tooltip.className = 'tooltip';
  tooltip.textContent = tooltipText;
  btnGit.appendChild(tooltip);

  btnGit.addEventListener('mouseenter', () => tooltip.style.visibility = 'visible');
  btnGit.addEventListener('mouseleave', () => tooltip.style.visibility = 'hidden');
  btnGit.onclick = () => gitPull(btnGit, index);

  cellGit.appendChild(btnGit);
}

/**
 * Cria as células para os ambientes.
 * @param {HTMLTableRowElement} row Linha da tabela.
 * @param {Object} app Dados da aplicação.
 * @param {number} index Índice da aplicação.
 */
export function createEnvironmentCells(row, app, index) {
  ENV_NAMES.forEach(envName => {
    const cellEnv = row.insertCell();
    if (!app.fileExists) {
      cellEnv.innerHTML = '--';
      return;
    }

    const envConfig = app[envName.toLowerCase()];
    if (envConfig !== null) {
      const isConfigured = app[`is${envName}Configured`];
      const isRunningThisEnv = app.environment === envName;
      const btnEnv = createEnvButton(index, envName, isConfigured, isRunningThisEnv);
      cellEnv.appendChild(btnEnv);
    } else {
      cellEnv.innerHTML = '--';
    }
  });
}

/**
 * Cria o botão de ambiente.
 * @param {number} index Índice da aplicação.
 * @param {string} envName Nome do ambiente.
 * @param {boolean} isConfigured Se o ambiente está configurado.
 * @param {boolean} isRunningThisEnv Se o ambiente está em execução.
 * @returns {HTMLButtonElement} Botão do ambiente.
 */
export function createEnvButton(index, envName, isConfigured, isRunningThisEnv) {
  const btnEnv = document.createElement('button');
  btnEnv.className = "btn btn-icon btn-toggle-env";
  btnEnv.id = `btn-toggle-${index}-${envName}`;

  const { iconClass, tooltipText, isDisabled } = getEnvButtonProps(envName, isConfigured, isRunningThisEnv);

  btnEnv.disabled = isDisabled;
  btnEnv.style.cursor = isDisabled ? 'not-allowed' : 'pointer';
  if (isDisabled) btnEnv.classList.add('ip-blocked-env');

  btnEnv.innerHTML = `
    <span class="icon-original"><i class="fas ${iconClass}"></i></span>
    <span class="branch-loading dp-none"><i class="fas fa-spinner icon-spin"></i></span>
  `;

  if (!isDisabled) {
    btnEnv.onclick = () => toggleEnv(btnEnv, index, envName);
  }

  const tooltip = document.createElement('div');
  tooltip.textContent = tooltipText;
  tooltip.className = ['Homolog', 'Prod'].includes(envName) ? 'tooltip-right' : 'tooltip';
  btnEnv.appendChild(tooltip);

  return btnEnv;
}

// HELPER FUNCTIONS
/**
 * Obtém o texto da branch com contagem de commits.
 * @param {Object} app Dados da aplicação.
 * @returns {string} Texto da branch.
 */
export function getBranchText(app) {
  return app.currentBranch
    ? (app.commits > 0 ? `${app.currentBranch} (${app.commits})` : app.currentBranch)
    : '--';
}

/**
 * Obtém as propriedades do botão de ambiente.
 * @param {string} envName Nome do ambiente.
 * @param {boolean} isConfigured Se o ambiente está configurado.
 * @param {boolean} isRunningThisEnv Se o ambiente está em execução.
 * @returns {Object} Propriedades do botão.
 */
function getEnvButtonProps(envName, isConfigured, isRunningThisEnv) {
  let iconClass, tooltipText, isDisabled;

  if (!isConfigured) {
    iconClass = "fa-play status-iniciar";
    tooltipText = "Ambiente não configurado";
    isDisabled = true;
  } else if (currentIpStatus !== IpStatus.IpReleased && ['Stage', 'Homolog', 'Prod'].includes(envName)) {
    iconClass = isRunningThisEnv ? "fa-pause status-parar" : "fa-play status-iniciar";
    tooltipText = getIpStatusText();
    isDisabled = true;
  } else {
    iconClass = isRunningThisEnv ? "fa-pause status-parar" : "fa-play status-iniciar";
    tooltipText = isRunningThisEnv ? `Parar ${envName}` : `Iniciar ${envName}`;
    isDisabled = false;
  }

  return { iconClass, tooltipText, isDisabled };
}

/**
 * Obtém o texto de status do IP.
 * @returns {string} Texto de status do IP.
 */
function getIpStatusText(status) {
  switch (status) {
    case IpStatus.IpReleased: return "IP liberado";
    case IpStatus.IpBlocked: return "IP bloqueado";
    case IpStatus.CheckingIp: return "Verificando IP";
    case IpStatus.ReleasingIp: return "Liberando IP";
    default: return "Desconhecido";
  }
}

// UPDATE FUNCTIONS
/**
 * Atualiza a célula da branch.
 * @param {number} index Índice da aplicação.
 * @param {boolean} isUpdating Se a branch está sendo atualizada.
 * @param {string} branch Nome da branch.
 * @param {number} commits Número de commits.
 */
export function updateBranchCell(index, isUpdating, branch, commits) {
  const row = document.querySelector(`#dgvApps tbody tr:nth-child(${index + 1})`);
  if (!row) return;

  const cellBranch = row.querySelector('.branch-name');
  if (!cellBranch) return;

  if (isUpdating) {
    cellBranch.innerHTML = '<span class="branch-loading"><i class="fas fa-spinner icon-spin"></i></span>';
  } else {
    const branchText = branch ? (commits > 0 ? `${branch} (${commits})` : branch) : '--';
    cellBranch.innerHTML = `<span class="branch-text">${branchText}</span>`;
  }

  const btnGitPull = row?.querySelector('.btn-git-pull');
  if (btnGitPull) {
    btnGitPull.disabled = isUpdating;
    btnGitPull.style.cursor = isUpdating ? 'not-allowed' : 'pointer';

    const tooltip = btnGitPull.querySelector('.tooltip');
    if (tooltip) {
      tooltip.textContent = isUpdating ? 'Atualizando branch' : 'Atualizar branch';
    }
  }
}

/**
 * Atualiza os spinners das branches.
 */
export function updateBranchSpinners() {
  document.querySelectorAll('#dgvApps tbody tr').forEach(row => {
    const cellBranch = row.querySelector('.branch-name');
    if (cellBranch && !cellBranch.textContent.includes('--')) {
      cellBranch.innerHTML = '<span class="branch-loading"><i class="fas fa-spinner icon-spin"></i></span>';
    }
  });
}

/**
 * Atualiza a célula da branch em caso de erro de conexão Git.
 * @param {number} appIndex Índice da aplicação.
 */
export function updateBranchCellOnGitError(appIndex) {
  const row = document.querySelector(`#dgvApps tbody tr:nth-child(${appIndex + 1})`);
  if (row) {
    const cellBranch = row.querySelector('.branch-name');
    if (cellBranch) cellBranch.innerHTML = '<span class="branch-text error">--</span>';
  }
}

/**
 * Atualiza o botão de git pull em caso de erro.
 * @param {number} index Índice da aplicação.
 */
export function updateGitButtonOnError(index) {
  const btnGitPull = document.querySelector(`#dgvApps tbody tr:nth-child(${index + 1}) .btn-git-pull`);
  if (btnGitPull) {
    setButtonLoading(btnGitPull, false);
  }
}

/**
 * Atualiza o botão de ambiente.
 * @param {number} index Índice da aplicação.
 * @param {string} env Nome do ambiente.
 * @param {boolean} isRunning Se o ambiente está em execução.
 */
export function updateEnvButton(index, env, isRunning) {
  const btnId = `btn-toggle-${index}-${env}`;
  const btn = document.getElementById(btnId);
  if (btn) {
    setButtonLoading(btn, false);
    const iconClass = isRunning ? 'fa-pause status-parar' : 'fa-play status-iniciar';
    btn.querySelector('.icon-original i').className = `fas ${iconClass}`;
    btn.title = isRunning ? `Parar ${env}` : `Iniciar ${env}`;
  }
}

/**
 * Atualiza o botão de IP.
 * @param {IpStatus} status Status do IP.
 * @param {boolean} enabled Se o botão está habilitado.
 */
export function updateIpButton(status, enabled) {
  const btnUpdateIP = document.getElementById('btnUpdateIP');
  if (!btnUpdateIP) return;

  btnUpdateIP.disabled = !enabled;
  btnUpdateIP.classList.remove('status-success', 'status-error', 'status-loading');

  const statusText = getIpStatusText(status);
  btnUpdateIP.setAttribute('data-text', statusText);

  switch (status) {
    case IpStatus.IpReleased:
      btnUpdateIP.classList.add('status-success');
      btnUpdateIP.innerHTML = `<i class="fas fa-shield-alt"></i> ${statusText}`;
      break;
    case IpStatus.IpBlocked:
      btnUpdateIP.innerHTML = `<i class="fas fa-shield-alt"></i> ${statusText}`;
      break;
    case IpStatus.CheckingIp:
    case IpStatus.ReleasingIp:
      btnUpdateIP.classList.add('status-loading');
      btnUpdateIP.innerHTML = `<i class="fas fa-spinner icon-spin"></i> ${statusText}`;
      break;
  }
}

/**
 * Atualiza o botão de versão.
 * @param {boolean} updateAvailable Se há atualização disponível.
 */
export function updateVersionButton(updateAvailable) {
  const updateVersionButton = document.getElementById('btnUpdateVersion');
  if (!updateVersionButton) return;

  const newText = updateAvailable ? 'Atualizar Sistema' : 'Sistema Atualizado';
  const iconElement = updateVersionButton.querySelector('i');
  const currentText = updateVersionButton.textContent.trim().replace(iconElement?.outerHTML || '', '').trim();

  const textChanged = currentText !== newText;
  if (textChanged) {
    updateVersionButton.classList.add('btn-flip');
    setTimeout(() => {
      updateVersionButton.innerHTML = updateAvailable ? newText : `${iconElement?.outerHTML || ''} ${newText}`;
    }, 400);
    setTimeout(() => updateVersionButton.classList.remove('btn-flip'), 800);
  }

  const visualStateChanged = updateAvailable !== updateVersionButton.classList.contains('status-fade');
  if (textChanged || visualStateChanged) {
    setTimeout(() => {
      if (updateAvailable) {
        updateVersionButton.classList.remove('status-success');
        updateVersionButton.classList.add('status-fade', 'fade-border');
      } else {
        updateVersionButton.classList.remove('status-fade', 'fade-border');
        updateVersionButton.classList.add('status-success');
      }
    }, textChanged ? 800 : 0);
  }

  updateVersionButton.disabled = !updateAvailable;
  updateVersionButton.style.pointerEvents = updateAvailable ? 'auto' : 'none';
}

/**
 * Atualiza o campo de e-mail.
 * @param {string} email Endereço de e-mail.
 */
export function updateEmailField(email) {
  const txtEmail = document.getElementById('txtEmail');
  if (txtEmail) txtEmail.value = email || '';
}

/**
 * Atualiza o badge da versão.
 * @param {string} version Versão do sistema.
 */
export function updateVersionBadge(version) {
  const versionBadge = document.querySelector('.version-badge');
  if (versionBadge) versionBadge.textContent = version || 'v.?.?';
}

/**
 * Atualiza o toggle de interface compacta.
 * @param {boolean} compactInterface Estado da interface compacta.
 */
export function updateCompactInterfaceToggle(compactInterface) {
  const compactInterfaceToggle = document.querySelector('#compactInterfaceToggle');
  if (compactInterfaceToggle) compactInterfaceToggle.checked = compactInterface;
}