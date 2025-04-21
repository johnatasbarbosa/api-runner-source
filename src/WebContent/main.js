import 'tippy.js/dist/tippy.css';
import './styles.css';
import { initTooltips } from './Utils/utils.js';

// Enums
import { WebMessageAction } from './Enums/web-message-action.enum.js';

// Managers
import { configureButtonListeners, configureWebviewMessageListener } from './Managers/event-listeners-manager.js';
import { updateButtonsState } from './Managers/state-manager.js';

// Utils
import { postMessage } from './Utils/utils.js';

// Ações iniciais enviadas ao WebView
const INITIAL_ACTIONS = [
    { action: WebMessageAction.LoadData },
    { action: WebMessageAction.ReleaseIP }
];

/**
 * Inicializa a aplicação configurando o estado inicial e os event listeners.
 */
function initializeApp() {
    // Atualiza o estado inicial dos botões
    updateButtonsState();

    // Envia mensagens iniciais para o WebView
    INITIAL_ACTIONS.forEach(postMessage);

    // Configura os listeners para eventos de UI e mensagens do WebView
    configureButtonListeners();
    configureWebviewMessageListener();

    initTooltips();
}

// Aguarda o carregamento do DOM para inicializar a aplicação
document.addEventListener("DOMContentLoaded", initializeApp);