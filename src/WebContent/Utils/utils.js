// Estado global para toasts
export const toastState = {
  queue: [], // Fila de toasts
  isVisible: false // Estado do toast
};

export const initializedTooltips = new WeakMap();


/**
 * Envia uma mensagem para o backend C# via WebView2.
 * @param {object} payload O objeto a ser enviado como JSON.
 */
export function postMessage(payload) {
  if (window.chrome?.webview) {
    try {
      window.chrome.webview.postMessage(payload);
    } catch (error) {
      console.error("Erro ao enviar mensagem via postMessage:", error);
    }
  } else {
    console.warn("WebView não está pronto ou disponível para enviar mensagem.");
  }
}

/**
 * Ativa/Desativa o estado de loading de um botão.
 * @param {HTMLElement} buttonElement O elemento do botão.
 * @param {boolean} isLoading True para ativar loading, false para desativar.
 */
export function setButtonLoading(buttonElement, isLoading) {
  if (!buttonElement) return;

  buttonElement.disabled = isLoading;

  buttonElement.style.cursor = isLoading ? 'not-allowed' : 'pointer';
}

/**
 * Exibe um toast com uma mensagem.
 * @param {string} message A mensagem a ser exibida.
 * @param {string} type O tipo de toast (ex.: 'success').
 * @param {number} duration Duração em milissegundos.
 */
export function showToast(message, type = 'success', duration = 3000) {
  toastState.queue.push({ message, type, duration });
  if (!toastState.isVisible) {
    displayNextToast();
  }
}

/**
 * Exibe o próximo toast da fila.
 */
function displayNextToast() {
  if (toastState.queue.length === 0 || toastState.isVisible) return;

  toastState.isVisible = true;
  const { message, type, duration } = toastState.queue.shift();

  const toast = createToastElement(message, type);
  document.body.appendChild(toast);

  // Força o reflow para ativar a animação
  toast.offsetHeight;

  requestAnimationFrame(() => toast.classList.add('show'));

  setTimeout(() => {
    toast.classList.remove('show');
    toast.addEventListener('transitionend', () => {
      toast.remove();
      toastState.isVisible = false;
      setTimeout(displayNextToast, 50);
    }, { once: true });
  }, duration);
}

/**
 * Cria o elemento DOM para o toast.
 * @param {string} message A mensagem do toast.
 * @param {string} type O tipo do toast.
 * @returns {HTMLElement} O elemento do toast.
 */
function createToastElement(message, type) {
  const toast = document.createElement('div');
  toast.className = `toast-notification ${type}`;
  toast.textContent = message;
  return toast;
}

/**
 * Verifica se há conexão com a internet.
 * @returns {Promise<boolean>} True se houver conexão, false caso contrário.
 */
export async function checkInternetConnection() {
  if (!navigator.onLine) return false;

  try {
    const testUrl = `https://www.google.com/generate_204?nocache=${Date.now()}`;
    await fetch(testUrl, {
      method: 'HEAD',
      mode: 'no-cors',
      cache: 'no-cache'
    });
    return true;
  } catch (error) {
    console.error("Falha na verificação de conexão:", error);
    return false;
  }
}

/**
 * Configura um evento de clique em um botão, removendo listeners anteriores.
 * @param {string|HTMLElement} buttonIdOrElement - ID do botão ou o próprio elemento.
 * @param {Function} clickHandler - Função a ser executada no clique.
 * @param {boolean} [disableOnClick=false] - Se verdadeiro, desabilita o botão após o clique.
 * @returns {HTMLElement} O novo elemento do botão.
 */
export function setButtonClick(buttonIdOrElement, clickHandler, disableOnClick = false) {
  const button = typeof buttonIdOrElement === 'string'
    ? document.getElementById(buttonIdOrElement)
    : buttonIdOrElement;

  if (!button) return null;

  const newButton = button.cloneNode(true);
  button.replaceWith(newButton);

  newButton.addEventListener("click", (e) => {
    if (disableOnClick) newButton.disabled = true;
    clickHandler(e);
  });

  return newButton;
}

/**
 * Inicializa tooltips em elementos no DOM, evitando duplicações
 * @param {Object} options - Opções de configuração
 * @param {string|HTMLElement|NodeList} options.selector - Seletor CSS ou elemento(s)
 * @param {string} options.placement - Posição do tooltip ('top', 'right', 'bottom', 'left')
 * @param {HTMLElement} options.container - Container onde buscar elementos (opcional)
 * @param {Object} options.tippyOptions - Outras opções do Tippy
 * @returns {Promise} Promise resolvida após inicializar os tooltips
 */
export function initTooltips({
  selector = '[data-tippy-content]',
  placement = 'top',
  container = document,
  tippyOptions = {}
} = {}) {
  return import('tippy.js').then(({ default: tippy }) => {
    let elements = [];

    if (typeof selector === 'string') {
      elements = Array.from(container.querySelectorAll(selector));
    } else if (selector instanceof Element) {
      elements = [selector];
    } else if (selector instanceof NodeList || Array.isArray(selector)) {
      elements = Array.from(selector);
    }

    const newElements = elements.filter(el => !initializedTooltips.has(el));

    if (newElements.length === 0) return [];

    const options = {
      placement,
      ...tippyOptions
    };

    const instances = tippy(newElements, options);

    newElements.forEach(el => initializedTooltips.set(el, true));

    return instances;
  });
}