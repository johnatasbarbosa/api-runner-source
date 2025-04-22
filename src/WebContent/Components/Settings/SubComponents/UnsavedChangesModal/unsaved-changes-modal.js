import { initTooltips } from '../../../../Utils/utils.js';
import { setActiveConfirmationModal } from '../../Managers/state-manager.js';
import unsavedChangesModalHtml from './unsaved-changes-modal.html?raw';

/**
 * Exibe o modal de confirmação para alterações não salvas.
 * @param {Function} onConfirm - Função a executar ao confirmar.
 * @param {Function} onCancel - Função a executar ao cancelar.
 * @param {Set} fieldsToList - Campos com alterações não salvas.
 */
export function showUnsavedChangesModal(onConfirm, onCancel, fieldsToList = new Set()) {
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