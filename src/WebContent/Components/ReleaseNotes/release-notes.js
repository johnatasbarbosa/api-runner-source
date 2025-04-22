import { initTooltips } from '../../Utils/utils.js';
import './release-notes.css';
import releaseNotesHtml from './release-notes.html?raw';


// Importa todos os arquivos .js da pasta versions
const versionFiles = import.meta.glob('./versions/*.js', { eager: true });

/**
 * Compara versões para ordenação (ex.: "2.0.1" > "2.0.0").
 * @param {string} a - Primeira versão.
 * @param {string} b - Segunda versão.
 * @returns {number} Resultado da comparação.
 */
function compareVersions(a, b) {
  const aParts = a.split('.').map(Number);
  const bParts = b.split('.').map(Number);

  for (let i = 0; i < aParts.length; i++) {
    if (aParts[i] > bParts[i]) return -1; // a é maior, deve vir primeiro
    if (aParts[i] < bParts[i]) return 1;  // b é maior, deve vir primeiro
  }
  return 0;
}

/**
 * Renderiza o conteúdo das versões em um container.
 * @param {HTMLElement} contentContainer - Elemento onde renderizar as versões.
 */
function renderVersionsContent(contentContainer) {
  // Obtém todas as versões
  const versions = Object.values(versionFiles).map(file => file.default);

  // Ordena as versões (maior para menor)
  versions.sort((a, b) => compareVersions(a.version, b.version));

  // Limpa o container antes de renderizar
  contentContainer.innerHTML = '';

  // Renderiza cada versão
  versions.forEach((version, index) => {
    const versionElement = document.createElement('div');
    versionElement.classList.add('version-section');

    // Cabeçalho da versão
    const header = document.createElement('div');
    header.classList.add('version-header');

    const versionNumber = document.createElement('span');
    versionNumber.classList.add('version-number');
    if (index === 0) {
      // Versão mais recente
      versionNumber.classList.add('latest');
      versionNumber.innerHTML = `
        <i class="fas fa-fire-flame-curved"></i>
        V${version.version}
      `;
    } else {
      // Outras versões
      versionNumber.innerHTML = `
        <i class="fas fa-bookmark"></i>
        V${version.version}
      `;
    }

    const versionDate = document.createElement('span');
    versionDate.classList.add('version-date');
    versionDate.textContent = version.date;

    header.appendChild(versionNumber);
    header.appendChild(versionDate);
    versionElement.appendChild(header);

    // Novo
    if (version.changes.new && version.changes.new.length > 0) {
      const newSection = document.createElement('div');
      newSection.classList.add('change-category');

      const categoryTitle = document.createElement('h3');
      categoryTitle.classList.add('category-title', 'category-new');

      const badge = document.createElement('span');
      badge.classList.add('category-badge');
      badge.textContent = 'Novo';

      categoryTitle.appendChild(badge);
      newSection.appendChild(categoryTitle);

      const newList = document.createElement('ul');
      version.changes.new.forEach(item => {
        const li = document.createElement('li');
        li.textContent = item;
        newList.appendChild(li);
      });
      newSection.appendChild(newList);
      versionElement.appendChild(newSection);
    }

    // Melhoria
    if (version.changes.improvement && version.changes.improvement.length > 0) {
      const improvementSection = document.createElement('div');
      improvementSection.classList.add('change-category');

      const categoryTitle = document.createElement('h3');
      categoryTitle.classList.add('category-title', 'category-improvement');

      const badge = document.createElement('span');
      badge.classList.add('category-badge');
      badge.textContent = 'Melhoria';

      categoryTitle.appendChild(badge);
      improvementSection.appendChild(categoryTitle);

      const improvementList = document.createElement('ul');
      version.changes.improvement.forEach(item => {
        const li = document.createElement('li');
        li.textContent = item;
        improvementList.appendChild(li);
      });
      improvementSection.appendChild(improvementList);
      versionElement.appendChild(improvementSection);
    }

    // Correção
    if (version.changes.correction && version.changes.correction.length > 0) {
      const correctionSection = document.createElement('div');
      correctionSection.classList.add('change-category');

      const categoryTitle = document.createElement('h3');
      categoryTitle.classList.add('category-title', 'category-correction');

      const badge = document.createElement('span');
      badge.classList.add('category-badge');
      badge.textContent = 'Correção';

      categoryTitle.appendChild(badge);
      correctionSection.appendChild(categoryTitle);

      const correctionList = document.createElement('ul');
      version.changes.correction.forEach(item => {
        const li = document.createElement('li');
        li.textContent = item;
        correctionList.appendChild(li);
      });
      correctionSection.appendChild(correctionList);
      versionElement.appendChild(correctionSection);
    }

    contentContainer.appendChild(versionElement);
  });
}

/**
 * Cria o modal de notas de versão.
 * @returns {Object} Objeto com o elemento do modal e métodos para mostrar e fechar.
 */
export function createReleaseNotesModal() {
  // Cria o elemento do componente para ter acesso aos elementos do HTML
  const container = document.createElement('div');
  container.innerHTML = releaseNotesHtml;

  // Obtém o modal do template HTML
  const modalElement = container.querySelector('#releaseNotesModal');
  if (!modalElement) {
    console.error('Elemento do modal não encontrado no HTML');
    return null;
  }

  // Adiciona o modal ao DOM
  document.body.appendChild(modalElement);

  // Renderiza as notas de versão dentro do corpo do modal
  const contentContainer = modalElement.querySelector('#releaseNotesContent');
  if (contentContainer) {
    renderVersionsContent(contentContainer);
  }

  initTooltips({ container: modalElement });

  // Configura os eventos de fechar
  const closeButton = modalElement.querySelector('.modal-close');
  const footerCloseButton = modalElement.querySelector('#btnCloseReleaseNotesFooter');

  const closeModal = () => {
    modalElement.style.display = 'none';
    try {
      if (document.body.contains(modalElement)) {
        document.body.removeChild(modalElement);
      }
    } catch (error) {
      console.warn("Erro ao fechar modal de release notes:", error);
    }
  };

  if (closeButton) {
    closeButton.addEventListener('click', closeModal);
  }
  if (footerCloseButton) {
    footerCloseButton.addEventListener('click', closeModal);
  }

  // Retorna o objeto do modal com métodos
  return {
    element: modalElement,
    show: () => (modalElement.style.display = 'flex'),
    close: closeModal,
  };
}

/**
 * Renderiza as notas de versão (para uso fora do modal, se necessário).
 * @returns {HTMLElement} Container com o conteúdo renderizado das notas de versão.
 */
export function renderReleaseNotes() {
  // Cria o elemento do componente
  const container = document.createElement('div');
  container.innerHTML = releaseNotesHtml;

  // Obtém o contêiner onde as notas serão renderizadas
  const contentContainer = container.querySelector('#releaseNotesContent');
  if (contentContainer) {
    renderVersionsContent(contentContainer);
  }

  return container;
}