// MessagePage.js - Enhanced with mobile message input handling

document.addEventListener('DOMContentLoaded', function () {
    // Initialize message page functionality
    initializeMessageInputs();
    initializeDeliveryToggle();

    // Handle viewport changes (especially useful for mobile keyboards)
    handleViewportChanges();
});

function initializeMessageInputs() {
    // Desktop message input
    const desktopInput = document.querySelector('.chat-section .message-input');
    const desktopSendBtn = document.querySelector('.chat-section .send-button');

    // Mobile message input
    const mobileInput = document.getElementById('mobileMessageInput');
    const mobileSendBtn = document.getElementById('mobileSendButton');

    // Handle desktop input
    if (desktopInput && desktopSendBtn) {
        setupMessageInput(desktopInput, desktopSendBtn);
    }

    // Handle mobile input
    if (mobileInput && mobileSendBtn) {
        setupMessageInput(mobileInput, mobileSendBtn);
    }

    // Sync inputs on larger screens if both are visible
    syncInputs(desktopInput, mobileInput);
}

function setupMessageInput(input, sendButton) {
    // Send message on button click
    sendButton.addEventListener('click', function () {
        sendMessage(input);
    });

    // Send message on Enter key
    input.addEventListener('keypress', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage(input);
        }
    });

    // Auto-resize textarea behavior (if needed)
    input.addEventListener('input', function () {
        // Add any input validation or character counting here
        handleInputChange(input);
    });
}

function sendMessage(input) {
    const message = input.value.trim();
    if (message) {
        // Add message to chat
        addMessageToChat(message, 'sent');

        // Clear input
        input.value = '';

        // Focus back on input for better UX
        input.focus();

        // Scroll to bottom of chat
        scrollToBottom();

        // Here you would typically send the message to your backend
        console.log('Sending message:', message);
    }
}

function addMessageToChat(message, type) {
    const chatMessages = document.getElementById('chatMessages');
    const messageDiv = document.createElement('div');

    if (type === 'sent') {
        messageDiv.className = 'flex justify-end mb-3';
        messageDiv.innerHTML = `
            <div class="bg-[#344e41] text-white rounded-2xl px-4 py-2 max-w-xs shadow-sm">
                ${escapeHtml(message)}
            </div>
        `;
    } else {
        messageDiv.className = 'flex justify-start mb-3';
        messageDiv.innerHTML = `
            <div class="bg-white text-black rounded-2xl px-4 py-2 max-w-xs shadow-sm">
                ${escapeHtml(message)}
            </div>
        `;
    }

    chatMessages.appendChild(messageDiv);
    scrollToBottom();
}

function scrollToBottom() {
    const chatMessages = document.getElementById('chatMessages');
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, function (m) { return map[m]; });
}

function syncInputs(desktopInput, mobileInput) {
    if (!desktopInput || !mobileInput) return;

    // Only sync on desktop/tablet where both might be visible
    const syncInputs = () => {
        if (window.innerWidth > 768) {
            desktopInput.addEventListener('input', () => {
                mobileInput.value = desktopInput.value;
            });

            mobileInput.addEventListener('input', () => {
                desktopInput.value = mobileInput.value;
            });
        }
    };

    syncInputs();
    window.addEventListener('resize', syncInputs);
}

function handleInputChange(input) {
    // Add any real-time input handling here
    // For example: typing indicators, character limits, etc.

    // Simple character limit example
    const maxLength = 500;
    if (input.value.length > maxLength) {
        input.value = input.value.substring(0, maxLength);
    }
}

function initializeDeliveryToggle() {
    const toggleBtn = document.getElementById('toggleDeliveryBtn');
    const deliveryOptions = document.getElementById('deliveryOptions');

    if (toggleBtn && deliveryOptions) {
        toggleBtn.addEventListener('click', function () {
            deliveryOptions.classList.toggle('show');
            toggleBtn.classList.toggle('active');
        });

        // Handle delivery option selection
        const deliveryButtons = deliveryOptions.querySelectorAll('button');
        deliveryButtons.forEach(button => {
            button.addEventListener('click', function () {
                // Remove active class from all buttons
                deliveryButtons.forEach(btn => btn.classList.remove('active'));

                // Add active class to clicked button
                this.classList.add('active');

                console.log('Delivery method selected:', this.textContent);
            });
        });
    }
}

function handleViewportChanges() {
    // Handle mobile keyboard appearance/disappearance
    let initialViewportHeight = window.innerHeight;

    window.addEventListener('resize', function () {
        if (window.innerWidth <= 768) {
            const currentHeight = window.innerHeight;
            const heightDifference = initialViewportHeight - currentHeight;

            // If viewport height decreased significantly (keyboard appeared)
            if (heightDifference > 150) {
                adjustForKeyboard(true);
            } else {
                adjustForKeyboard(false);
            }
        }
    });

    // Handle orientation changes
    window.addEventListener('orientationchange', function () {
        setTimeout(() => {
            initialViewportHeight = window.innerHeight;
            const mobileInputContainer = document.querySelector('.message-input-container-fixed');
            if (mobileInputContainer) {
                // Force reflow to ensure proper positioning
                mobileInputContainer.style.display = 'none';
                mobileInputContainer.offsetHeight; // Trigger reflow
                mobileInputContainer.style.display = 'flex';
            }
        }, 500);
    });
}

function adjustForKeyboard(keyboardVisible) {
    const mobileInputContainer = document.querySelector('.message-input-container-fixed');
    const chatMessages = document.getElementById('chatMessages');

    if (keyboardVisible) {
        // Keyboard is visible - adjust positioning
        if (mobileInputContainer) {
            mobileInputContainer.style.bottom = '10px';
        }
        if (chatMessages) {
            chatMessages.style.paddingBottom = '120px';
        }
    } else {
        // Keyboard is hidden - restore original positioning
        if (mobileInputContainer) {
            mobileInputContainer.style.bottom = '80px';
        }
        if (chatMessages) {
            chatMessages.style.paddingBottom = '80px';
        }
    }

    // Scroll to bottom to show latest messages
    setTimeout(scrollToBottom, 100);
}

// Export functions for external use if needed
window.MessagePage = {
    sendMessage,
    addMessageToChat,
    scrollToBottom
};