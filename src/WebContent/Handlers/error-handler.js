import { activeModal, setActiveModal } from '../Managers/state-manager.js';
import { DEFAULT_ERROR_DESCRIPTION, DEFAULT_ERROR_TITLE, ERROR_PATTERNS } from '../Patterns/error-git-patterns.js';

/**
 * Analisa mensagens de erro do Git e retorna um objeto com título, descrição e erro original.
 * @param {string} errorMessage A mensagem de erro do Git.
 * @returns {Object} Objeto contendo título, descrição e erro original.
 */
export function parseGitError(errorMessage) {
  const normalizedError = errorMessage.replace(/\r\n/g, '\n').replace(/\r/g, '\n');

  for (const { pattern, title, description } of ERROR_PATTERNS) {
    const match = normalizedError.match(pattern);
    if (match) {
      const errorDescription = typeof description === 'function' ? description(match) : description;
      return { title, description: errorDescription, originalError: errorMessage };
    }
  }

  return { title: DEFAULT_ERROR_TITLE, description: DEFAULT_ERROR_DESCRIPTION, originalError: errorMessage };
}

/**
 * Gera o HTML do modal de erro.
 * @param {Object} parsedError Objeto com título, descrição e erro original.
 * @returns {string} HTML do modal.
 */
function createModalHtml(parsedError) {
  return `
        <div class="modal-overlay">
            <div class="modal-content">
                <div class="modal-header error-git-modal-header">
                    <i class="fas fa-times-circle modal-icon"></i>
                    <h3 class="modal-title">${parsedError.title}</h3>
                </div>
                <p class="modal-description">${parsedError.description}</p>
                <p class="modal-description error-git-modal-original-error">${parsedError.originalError}</p>
                <button class="modal-button">OK</button>
            </div>
        </div>
  `;
}

/**
 * Exibe um modal de erro com base na mensagem fornecida.
 * @param {string} error A mensagem de erro a ser exibida.
 */
export function showErrorModal(error) {
  if (activeModal) {
    activeModal.remove();
    setActiveModal(null);
  }

  const parsedError = parseGitError(error);
  const modalHtml = createModalHtml(parsedError);

  const modalElement = document.createElement('div');
  modalElement.innerHTML = modalHtml;
  document.body.appendChild(modalElement);
  setActiveModal(modalElement);

  const okButton = modalElement.querySelector('.modal-button');
  okButton.addEventListener('click', () => {
    modalElement.remove();
    setActiveModal(null);
  });
}