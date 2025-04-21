import { initialCompactInterfaceState, originalFieldValues, updateOriginalFieldValue } from '../../../../Managers/state-manager.js';
import {
  unsavedChangesFields,
  updateSaveButtonState
} from '../../Managers/state-manager.js';
import generalTabHtml from './general-tab.html?raw';


// Constantes para IDs
const ELEMENT_IDS = {
  MODAL_EMAIL: 'modalEmail',
  COMPACT_INTERFACE_TOGGLE: 'compactInterfaceToggle'
};

/**
 * Renderiza o HTML da aba Geral.
 * @returns {string} HTML da aba.
 */
export function renderGeneralTab() {
  return generalTabHtml;
}

/**
 * Configura os elementos e eventos da aba Geral.
 * @param {HTMLElement} modalElement - Elemento do modal.
 */
export function setupGeneralTab(modalElement) {
  const currentEmailInput = document.getElementById('txtEmail');
  const modalEmailInput = modalElement.querySelector(`#${ELEMENT_IDS.MODAL_EMAIL}`);
  const compactInterfaceToggle = modalElement.querySelector(`#${ELEMENT_IDS.COMPACT_INTERFACE_TOGGLE}`);

  // Inicializar valores
  updateOriginalFieldValue('email', currentEmailInput?.value || '');
  updateOriginalFieldValue('compactInterface', initialCompactInterfaceState);

  if (compactInterfaceToggle) {
    compactInterfaceToggle.checked = initialCompactInterfaceState;
  }

  if (currentEmailInput && modalEmailInput) {
    modalEmailInput.value = currentEmailInput.value;
  }

  // Configurar eventos
  if (modalEmailInput) {
    modalEmailInput.addEventListener('input', () => handleFieldInput('email', modalEmailInput.value));
  }

  if (compactInterfaceToggle) {
    compactInterfaceToggle.addEventListener('change', () => {
      handleFieldInput('compactInterface', compactInterfaceToggle.checked);
    });
  }
}

/**
 * Manipula a entrada em campos da aba Geral.
 * @param {string} fieldKey - Chave do campo.
 * @param {any} value - Valor do campo.
 */
function handleFieldInput(fieldKey, value) {
  const originalValue = originalFieldValues[fieldKey] ?? (fieldKey === 'compactInterface' ? false : '');
  const hasChanged = String(value) !== String(originalValue);

  if (hasChanged) {
    if (!unsavedChangesFields.has(fieldKey)) unsavedChangesFields.add(fieldKey);
  } else {
    if (unsavedChangesFields.has(fieldKey)) unsavedChangesFields.delete(fieldKey);
  }

  updateSaveButtonState();
}

/**
 * Prepara os dados da aba Geral para salvamento.
 * @param {HTMLElement} modalElement - Elemento do modal.
 * @returns {Object} Objeto com os dados a serem salvos.
 */
export function getGeneralSettingsData(modalElement) {
  const modalEmailInput = modalElement.querySelector(`#${ELEMENT_IDS.MODAL_EMAIL}`);
  const compactInterfaceToggle = modalElement.querySelector(`#${ELEMENT_IDS.COMPACT_INTERFACE_TOGGLE}`);

  return {
    email: modalEmailInput ? modalEmailInput.value : null,
    compactInterface: compactInterfaceToggle ? compactInterfaceToggle.checked : null
  };
}

/**
 * Reverte os campos alterados para seus valores originais.
 * @param {Set<string>} fieldsToRevert - Conjunto de IDs de campos para reverter (opcional).
 */
export function revertGeneralFields(fieldsToRevert = unsavedChangesFields) {
  const modalEmailInput = document.getElementById(ELEMENT_IDS.MODAL_EMAIL);
  const compactInterfaceToggle = document.getElementById(ELEMENT_IDS.COMPACT_INTERFACE_TOGGLE);

  fieldsToRevert.forEach(fieldId => {
    try {
      if (fieldId === 'email' && modalEmailInput) {
        modalEmailInput.value = originalFieldValues['email'] ?? '';
        unsavedChangesFields.delete('email');
      } else if (fieldId === 'compactInterface' && compactInterfaceToggle) {
        compactInterfaceToggle.checked = originalFieldValues['compactInterface'] ?? initialCompactInterfaceState;
        unsavedChangesFields.delete('compactInterface');
      }
    } catch (error) {
      console.error(`Erro ao reverter campo ${fieldId}:`, error);
    }
  });

  updateSaveButtonState();
}