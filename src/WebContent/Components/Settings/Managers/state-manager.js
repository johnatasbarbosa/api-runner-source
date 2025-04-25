import { revertGeneralFields } from '../Tabs/General/general-tab.js';
import { revertParamsFields } from '../Tabs/Params/params-tab.js';

// Constantes para IDs
const ELEMENT_IDS = {
  BTN_SAVE_SETTINGS: 'btnSaveSettings'
};

// Estado compartilhado
export const unsavedChangesFields = new Set();
export let previousAppValue = '';
export let previousEnvValue = '';
export let configModal = null;
export let activeConfirmationModal = null;

/**
 * Define o valor anterior do seletor de aplicações.
 * @param {string} value - Valor anterior.
 */
export function setPreviousAppValue(value) {
  previousAppValue = value;
}

/**
 * Define o valor anterior do seletor de ambientes.
 * @param {string} value - Valor anterior.
 */
export function setPreviousEnvValue(value) {
  previousEnvValue = value;
}

/**
 * Define a referência ao modal de configurações.
 * @param {Object|null} modal - Objeto do modal.
 */
export function setConfigModal(modal) {
  configModal = modal;
}

/**
 * Define a referência ao modal de confirmação.
 * @param {HTMLElement|null} modal - Elemento do modal.
 */
export function setActiveConfirmationModal(modal) {
  activeConfirmationModal = modal;
}

/**
 * Atualiza o estado do botão de salvar com base nas mudanças não salvas.
 */
export function updateSaveButtonState() {
  const saveBtn = document.getElementById(ELEMENT_IDS.BTN_SAVE_SETTINGS);
  if (saveBtn) {
    const hasUnsavedChanges = unsavedChangesFields.size > 0;
    saveBtn.disabled = !hasUnsavedChanges;
    saveBtn.style.opacity = hasUnsavedChanges ? '1' : '0.5';
  }
}

/**
 * Reverte campos específicos para seus valores originais.
 * @param {Set<string>} fieldsToRevert - Conjunto de IDs de campos para reverter.
 */
export function revertChangedFieldsByScope(fieldsToRevert) {
  const generalFields = new Set(['email', 'compactInterface']);
  const paramsFields = new Set([...fieldsToRevert].filter(field =>
    field === 'appName' || field === 'appPath' || field.startsWith('param:')
  ));

  // Reverter campos da aba Geral
  if ([...fieldsToRevert].some(field => generalFields.has(field))) {
    revertGeneralFields(fieldsToRevert);
  }

  // Reverter campos da aba Parâmetros
  if (paramsFields.size > 0) {
    revertParamsFields(paramsFields);
  }

  updateSaveButtonState();
}

/**
 * Reverte todos os campos alterados para seus valores originais.
 */
export function revertChangedFields() {
  revertGeneralFields(unsavedChangesFields);
  revertParamsFields(unsavedChangesFields);
  unsavedChangesFields.clear();
  updateSaveButtonState();
}