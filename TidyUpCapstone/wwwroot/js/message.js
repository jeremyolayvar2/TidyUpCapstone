// messages.js - Chat and messaging functionality
// Handles all message/chat window interactions

document.addEventListener('DOMContentLoaded', function () {
    initializeMessages();
});

function initializeMessages() {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    // Only initialize if message elements exist
    if (!appContainer && !chatWindow) {
        return;
    }

    setupMessageContainers();
    setupMessageButtons();
    setupMessageItems();
    setupMessageClosing();
    setupMessageKeyboardNavigation();
}

function setupMessageContainers() {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    // Hide both containers on load
    if (appContainer) {
        appContainer.classList.remove('active');
        appContainer.setAttribute('aria-hidden', 'true');
    }

    if (chatWindow) {
        chatWindow.classList.remove('active');
        chatWindow.setAttribute('aria-hidden', 'true');
    }
}

function setupMessageButtons() {
    const newMessageBtn = document.getElementById('new-message-btn');

    if (newMessageBtn) {
        newMessageBtn.addEventListener('click', function (e) {
            e.preventDefault();
            openAppContainer();
        });

        // Add keyboard support
        newMessageBtn.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                openAppContainer();
            }
        });
    }
}

function setupMessageItems() {
    const messageItems = document.querySelectorAll('.message-item');

    messageItems.forEach(function (item, index) {
        // Add accessibility attributes
        item.setAttribute('tabindex', '0');
        item.setAttribute('role', 'button');
        item.setAttribute('aria-label', `Open conversation ${index + 1}`);

        // Click handler
        item.addEventListener('click', function () {
            openChatWindow();
        });

        // Keyboard handler
        item.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                openChatWindow();
            }
        });

        // Hover effects
        item.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.02)';
        });

        item.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    });
}

function setupMessageClosing() {
    const closeBtn = document.getElementById('closeBtn');
    const chatCloseBtn = document.getElementById('chatCloseBtn');

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            closeAllMessageWindows();
        });
    }

    if (chatCloseBtn) {
        chatCloseBtn.addEventListener('click', function () {
            closeAllMessageWindows();
        });
    }

    // Close on escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const appContainer = document.querySelector('.app-container');
            const chatWindow = document.querySelector('.chat-window');

            if ((appContainer && appContainer.classList.contains('active')) ||
                (chatWindow && chatWindow.classList.contains('active'))) {
                closeAllMessageWindows();
            }
        }
    });

    // Close on background click
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    [appContainer, chatWindow].forEach(container => {
        if (container) {
            container.addEventListener('click', function (e) {
                // Close if clicking the background (not the content)
                if (e.target === container) {
                    closeAllMessageWindows();
                }
            });
        }
    });
}

function setupMessageKeyboardNavigation() {
    // Tab navigation within message windows
    const messageContainers = document.querySelectorAll('.app-container, .chat-window');

    messageContainers.forEach(container => {
        container.addEventListener('keydown', function (e) {
            if (e.key === 'Tab') {
                handleTabNavigation(e, container);
            }
        });
    });
}

function handleTabNavigation(e, container) {
    const focusableElements = container.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );

    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    if (e.shiftKey) {
        // Shift + Tab (backward)
        if (document.activeElement === firstElement) {
            e.preventDefault();
            lastElement.focus();
        }
    } else {
        // Tab (forward)
        if (document.activeElement === lastElement) {
            e.preventDefault();
            firstElement.focus();
        }
    }
}

// Core message window functions
function openAppContainer() {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    if (appContainer) {
        appContainer.classList.add('active');
        appContainer.setAttribute('aria-hidden', 'false');

        // Focus the first interactive element
        const firstButton = appContainer.querySelector('button, input, [tabindex="0"]');
        if (firstButton) {
            setTimeout(() => firstButton.focus(), 100);
        }
    }

    if (chatWindow) {
        chatWindow.classList.remove('active');
        chatWindow.setAttribute('aria-hidden', 'true');
    }

    // Add body class to prevent scrolling
    document.body.classList.add('message-open');

    console.log('App container opened');
}

function openChatWindow() {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    if (appContainer) {
        appContainer.classList.remove('active');
        appContainer.setAttribute('aria-hidden', 'true');
    }

    if (chatWindow) {
        chatWindow.classList.add('active');
        chatWindow.setAttribute('aria-hidden', 'false');

        // Focus the message input or first interactive element
        const messageInput = chatWindow.querySelector('input[type="text"], textarea');
        const firstButton = chatWindow.querySelector('button, [tabindex="0"]');

        setTimeout(() => {
            if (messageInput) {
                messageInput.focus();
            } else if (firstButton) {
                firstButton.focus();
            }
        }, 100);
    }

    // Add body class to prevent scrolling
    document.body.classList.add('message-open');

    console.log('Chat window opened');
}

function closeAllMessageWindows() {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    if (appContainer) {
        appContainer.classList.remove('active');
        appContainer.setAttribute('aria-hidden', 'true');
    }

    if (chatWindow) {
        chatWindow.classList.remove('active');
        chatWindow.setAttribute('aria-hidden', 'true');
    }

    // Remove body class to restore scrolling
    document.body.classList.remove('message-open');

    // Return focus to the trigger button
    const newMessageBtn = document.getElementById('new-message-btn');
    if (newMessageBtn) {
        newMessageBtn.focus();
    }

    console.log('All message windows closed');
}

// Message sending functionality
function sendMessage(messageText, recipientId) {
    if (!messageText || !messageText.trim()) {
        console.error('Message text is required');
        return;
    }

    // Show loading state
    const sendButton = document.querySelector('.chat-window .send-button');
    if (sendButton) {
        sendButton.disabled = true;
        sendButton.textContent = 'Sending...';
    }

    // AJAX call to send message
    fetch('/Messages/Send', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify({
            message: messageText.trim(),
            recipientId: recipientId
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Message sent successfully');
                addMessageToChat(messageText, 'sent');
                clearMessageInput();
            } else {
                console.error('Failed to send message:', data.message);
                showMessageError('Failed to send message');
            }
        })
        .catch(error => {
            console.error('Error sending message:', error);
            showMessageError('Network error occurred');
        })
        .finally(() => {
            // Reset send button
            if (sendButton) {
                sendButton.disabled = false;
                sendButton.textContent = 'Send';
            }
        });
}

function addMessageToChat(messageText, type = 'sent') {
    const chatMessages = document.querySelector('.chat-messages');
    if (!chatMessages) return;

    const messageElement = document.createElement('div');
    messageElement.className = `message ${type}`;
    messageElement.innerHTML = `
        <div class="message-content">${escapeHtml(messageText)}</div>
        <div class="message-time">${new Date().toLocaleTimeString()}</div>
    `;

    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function clearMessageInput() {
    const messageInput = document.querySelector('.chat-window input[type="text"], .chat-window textarea');
    if (messageInput) {
        messageInput.value = '';
        messageInput.focus();
    }
}

function showMessageError(errorText) {
    const errorElement = document.querySelector('.message-error');
    if (errorElement) {
        errorElement.textContent = errorText;
        errorElement.style.display = 'block';

        setTimeout(() => {
            errorElement.style.display = 'none';
        }, 5000);
    }
}

// Utility functions
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Export message functions
window.TidyUpMessages = {
    openAppContainer,
    openChatWindow,
    closeAllMessageWindows,
    sendMessage
};