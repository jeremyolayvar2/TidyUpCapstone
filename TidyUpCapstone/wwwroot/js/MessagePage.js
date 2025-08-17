// MessagePage.js - Enhanced with SignalR real-time functionality and Fixed Report Modal

let currentUser = null;
let otherUser = null;
let currentChatId = null;
let connection = null;
let typingTimer = null;
let isTyping = false;
let currentReportMessageId = null;
let reportModal = null;

document.addEventListener('DOMContentLoaded', function () {
    // Initialize message page functionality
    initializeUsers();
    initializeSignalR();
    initializeMessageInputs();
    initializeDeliveryToggle();
    initializeReportModal();

    // Handle viewport changes (especially useful for mobile keyboards)
    handleViewportChanges();
});

function initializeUsers() {
    // Get user data from the page (set by the server)
    const userDataElement = document.getElementById('userData');
    if (userDataElement) {
        const userData = JSON.parse(userDataElement.textContent);
        currentUser = userData.currentUser;
        otherUser = userData.otherUser;

        // Load existing messages
        if (currentUser && otherUser) {
            loadMessages();
        }
    }
}

async function initializeSignalR() {
    try {
        // Create SignalR connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chathub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Set up event handlers
        setupSignalREventHandlers();

        // Start the connection
        await connection.start();
        console.log("SignalR Connected");

        // Test the connection
        await connection.invoke("TestConnection");

        showNotification("Connected to real-time chat!", "success");

    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        showNotification("Failed to connect to real-time chat. Falling back to polling.", "warning");

        // Fallback to polling if SignalR fails
        startMessagePolling();
    }
}

function setupSignalREventHandlers() {
    // Receive new messages
    connection.on("ReceiveMessage", function (messageData) {
        console.log("Received message via SignalR:", messageData);

        // Only add message if it's for the current chat and not from current user
        if (messageData.chatId === currentChatId && messageData.senderId !== currentUser?.id) {
            addMessageToChat(
                messageData.message,
                messageData.senderId,
                messageData.messageId,
                messageData.sentAt,
                false // Not from current user
            );

            // Play notification sound (optional)
            playNotificationSound();

            // Mark messages as read if chat is visible
            if (document.hasFocus()) {
                markMessagesAsRead();
            }
        }
    });

    // Handle typing indicators
    connection.on("UserTyping", function (data) {
        if (data.chatId === currentChatId && data.userId !== currentUser?.id) {
            showTypingIndicator(data.userName, data.isTyping);
        }
    });

    // Handle messages marked as read
    connection.on("MessagesRead", function (data) {
        if (data.chatId === currentChatId) {
            updateMessageReadStatus(data.messageIds);
        }
    });

    // Handle user status changes
    connection.on("UserStatusChanged", function (data) {
        updateUserOnlineStatus(data.userId, data.isOnline);
    });

    // Handle errors
    connection.on("Error", function (errorMessage) {
        console.error("SignalR Error:", errorMessage);
        showNotification("Error: " + errorMessage, "error");
    });

    // Test response
    connection.on("TestResponse", function (message) {
        console.log("SignalR Test Response:", message);
    });

    // Connection state changes
    connection.onreconnecting(() => {
        showNotification("Reconnecting to chat...", "info");
    });

    connection.onreconnected(() => {
        showNotification("Reconnected to chat!", "success");
        // Rejoin the current chat
        if (currentChatId) {
            joinChat(currentChatId);
        }
    });

    connection.onclose(() => {
        showNotification("Disconnected from chat. Trying to reconnect...", "warning");
    });
}

async function joinChat(chatId) {
    if (connection && chatId) {
        try {
            await connection.invoke("JoinChat", chatId.toString());
            currentChatId = chatId;
            console.log(`Joined chat ${chatId}`);
        } catch (err) {
            console.error("Error joining chat:", err);
        }
    }
}

async function leaveChat(chatId) {
    if (connection && chatId) {
        try {
            await connection.invoke("LeaveChat", chatId.toString());
            console.log(`Left chat ${chatId}`);
        } catch (err) {
            console.error("Error leaving chat:", err);
        }
    }
}

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

    // Typing indicators
    input.addEventListener('input', function () {
        handleInputChange(input);
        handleTypingIndicator();
    });

    // Stop typing when user stops typing
    input.addEventListener('keyup', function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(() => {
            if (isTyping) {
                sendTypingIndicator(false);
            }
        }, 1000);
    });
}

function handleTypingIndicator() {
    if (!isTyping && currentChatId && currentUser) {
        isTyping = true;
        sendTypingIndicator(true);
    }
}

async function sendTypingIndicator(typing) {
    if (connection && currentChatId && currentUser) {
        try {
            await connection.invoke("UserTyping", currentChatId, currentUser.id, currentUser.name, typing);
            isTyping = typing;
        } catch (err) {
            console.error("Error sending typing indicator:", err);
        }
    }
}

async function sendMessage(input) {
    const message = input.value.trim();
    if (!message || !currentUser || !otherUser) return;

    try {
        // If using SignalR and we have a connection
        if (connection && connection.state === signalR.HubConnectionState.Connected && currentChatId) {
            // Send via SignalR Hub
            await connection.invoke("SendMessage", currentChatId, currentUser.id, message);

            // Add message to UI immediately for current user
            addMessageToChat(message, currentUser.id, Date.now(), new Date().toISOString(), true);

            // Clear input
            input.value = '';
            input.focus();

            // Stop typing indicator
            if (isTyping) {
                sendTypingIndicator(false);
            }
        } else {
            // Fallback to HTTP API
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({
                    recipientId: otherUser.id,
                    message: message
                })
            });

            const result = await response.json();

            if (result.success) {
                // Add message to chat immediately
                addMessageToChat(result.message, result.senderId, result.messageId, result.sentAt, true);

                // Set chat ID if we got one
                if (result.chatId) {
                    currentChatId = result.chatId;
                    joinChat(currentChatId);
                }

                // Clear input
                input.value = '';
                input.focus();
            } else {
                showNotification('Error sending message: ' + result.message, 'error');
            }
        }
    } catch (error) {
        console.error('Error sending message:', error);
        showNotification('Error sending message', 'error');
    }
}

async function loadMessages() {
    if (!currentUser || !otherUser) return;

    try {
        const response = await fetch(`/Chat/GetMessages?otherUserId=${otherUser.id}`);
        const result = await response.json();

        if (result.success) {
            const chatMessages = document.getElementById('chatMessages');
            chatMessages.innerHTML = ''; // Clear existing messages

            // Set current chat ID
            if (result.chatId) {
                currentChatId = result.chatId;
                // Join the chat room via SignalR
                if (connection && connection.state === signalR.HubConnectionState.Connected) {
                    await joinChat(currentChatId);
                }
            }

            result.messages.forEach(msg => {
                addMessageToChat(msg.message, msg.senderId, msg.messageId, msg.sentAt, msg.isCurrentUser);
            });

            // Mark messages as read
            if (currentChatId) {
                markMessagesAsRead();
            }
        }
    } catch (error) {
        console.error('Error loading messages:', error);
    }
}

function addMessageToChat(message, senderId, messageId, sentAt, isCurrentUser) {
    const chatMessages = document.getElementById('chatMessages');
    const messageDiv = document.createElement('div');
    const messageTime = new Date(sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    if (isCurrentUser) {
        messageDiv.className = 'flex justify-end mb-3';
        messageDiv.innerHTML = `
            <div class="bg-[#344e41] text-white rounded-2xl px-4 py-2 max-w-xs shadow-sm relative message-bubble" data-message-id="${messageId}">
                <div class="message-content">${escapeHtml(message)}</div>
                <div class="text-xs opacity-75 mt-1">${messageTime}</div>
            </div>
        `;
    } else {
        messageDiv.className = 'flex justify-start mb-3';
        messageDiv.innerHTML = `
            <div class="bg-[#F5F5F5] text-black rounded-2xl px-4 py-2 max-w-xs shadow-sm relative message-bubble group hover:shadow-md transition-all" data-message-id="${messageId}">
                <div class="message-content">${escapeHtml(message)}</div>
                <div class="text-xs opacity-75 mt-1 flex items-center justify-between">
                    <span>${messageTime}</span>
                    <button 
                        class="report-btn ml-2 text-red-500 hover:text-red-700 transition-all duration-200 transform hover:scale-110 opacity-0 group-hover:opacity-100" 
                        onclick="showReportModal(${messageId})" 
                        title="Report this message"
                        style="background: none; border: none; cursor: pointer; padding: 2px 4px; font-size: 14px;">
                        🚩
                    </button>
                </div>
            </div>
        `;
    }

    chatMessages.appendChild(messageDiv);
    scrollToBottom();
}

function showTypingIndicator(userName, isTyping) {
    const chatMessages = document.getElementById('chatMessages');
    const existingIndicator = document.getElementById('typing-indicator');

    if (isTyping) {
        if (!existingIndicator) {
            const typingDiv = document.createElement('div');
            typingDiv.id = 'typing-indicator';
            typingDiv.className = 'flex justify-start mb-3';
            typingDiv.innerHTML = `
                <div class="bg-gray-200 text-gray-600 rounded-2xl px-4 py-2 max-w-xs shadow-sm">
                    <div class="flex items-center space-x-1">
                        <span class="text-sm">${userName} is typing</span>
                        <div class="flex space-x-1">
                            <div class="w-1 h-1 bg-gray-400 rounded-full animate-bounce"></div>
                            <div class="w-1 h-1 bg-gray-400 rounded-full animate-bounce" style="animation-delay: 0.1s"></div>
                            <div class="w-1 h-1 bg-gray-400 rounded-full animate-bounce" style="animation-delay: 0.2s"></div>
                        </div>
                    </div>
                </div>
            `;
            chatMessages.appendChild(typingDiv);
            scrollToBottom();
        }
    } else {
        if (existingIndicator) {
            existingIndicator.remove();
        }
    }
}

async function markMessagesAsRead() {
    if (!currentChatId) return;

    try {
        const response = await fetch('/Chat/MarkMessagesAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ chatId: currentChatId })
        });

        const result = await response.json();
        if (!result.success) {
            console.error('Error marking messages as read:', result.message);
        }
    } catch (error) {
        console.error('Error marking messages as read:', error);
    }
}

function updateMessageReadStatus(messageIds) {
    // Add read indicators to messages
    messageIds.forEach(messageId => {
        const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
        if (messageElement) {
            // Add read indicator (like checkmarks)
            const readIndicator = document.createElement('span');
            readIndicator.className = 'text-xs text-green-500 ml-1';
            readIndicator.innerHTML = '✓✓';
            readIndicator.title = 'Read';

            const timeElement = messageElement.querySelector('.text-xs.opacity-75');
            if (timeElement && !timeElement.querySelector('.text-green-500')) {
                timeElement.appendChild(readIndicator);
            }
        }
    });
}

function updateUserOnlineStatus(userId, isOnline) {
    // Update UI to show user online/offline status
    const statusElements = document.querySelectorAll(`[data-user-id="${userId}"]`);
    statusElements.forEach(element => {
        const statusIndicator = element.querySelector('.status-indicator') || document.createElement('div');
        statusIndicator.className = `status-indicator w-3 h-3 rounded-full ${isOnline ? 'bg-green-500' : 'bg-gray-400'}`;
        if (!element.querySelector('.status-indicator')) {
            element.appendChild(statusIndicator);
        }
    });
}

function playNotificationSound() {
    // Play a subtle notification sound
    try {
        const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmMcAzuL0fPTgC4GIWq+8N+UQgwPVqzp66lXFQlHot7rwWYUAkWMrO9kgI8O');
        audio.volume = 0.1;
        audio.play().catch(() => { }); // Ignore errors if audio fails
    } catch (e) {
        // Ignore audio errors
    }
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

        const deliveryButtons = deliveryOptions.querySelectorAll('button');
        deliveryButtons.forEach(button => {
            button.addEventListener('click', function () {
                deliveryButtons.forEach(btn => btn.classList.remove('active'));
                this.classList.add('active');
                console.log('Delivery method selected:', this.textContent);
            });
        });
    }
}

// ========================================
// FIXED REPORT MODAL FUNCTIONS - COMPLETE
// ========================================

// Initialize report modal once when page loads
function initializeReportModal() {
    console.log('Initializing report modal...');

    // Remove existing modal if it exists
    const existingModal = document.getElementById('reportModal');
    if (existingModal) {
        existingModal.remove();
        console.log('Removed existing modal');
    }

    // Create modal container - ATTACH TO BODY, NOT INSIDE PAGE CONTENT
    const modalContainer = document.createElement('div');
    modalContainer.id = 'reportModal';
    modalContainer.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        z-index: 2147483647;
        background-color: rgba(0, 0, 0, 0.5);
        backdrop-filter: blur(5px);
        display: none;
        align-items: center;
        justify-content: center;
        padding: 1rem;
        box-sizing: border-box;
        pointer-events: auto;
    `;

    // Create modal content with proper styling
    modalContainer.innerHTML = `
        <div style="
            background: white;
            border-radius: 8px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            width: 100%;
            max-width: 450px;
            max-height: 80vh;
            overflow-y: auto;
            position: relative;
            z-index: 2147483647;
            pointer-events: auto;
        ">
            <div style="padding: 24px;">
                <!-- Header -->
                <div style="
                    display: flex; 
                    align-items: center; 
                    justify-content: space-between; 
                    margin-bottom: 24px;
                    border-bottom: 1px solid #e5e7eb;
                    padding-bottom: 16px;
                ">
                    <h3 style="
                        font-size: 18px; 
                        font-weight: 600; 
                        color: #111827; 
                        display: flex; 
                        align-items: center; 
                        margin: 0;
                        font-family: inherit;
                    ">
                        <span style="color: #ef4444; margin-right: 8px; font-size: 20px;">🚩</span>
                        Report Message
                    </h3>
                    <button type="button" id="closeReportModal" style="
                        color: #9ca3af;
                        background: none;
                        border: none;
                        font-size: 24px;
                        font-weight: bold;
                        padding: 4px;
                        cursor: pointer;
                        line-height: 1;
                        border-radius: 4px;
                        width: 32px;
                        height: 32px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        pointer-events: auto;
                    ">&times;</button>
                </div>
                
                <!-- Form -->
                <form id="reportForm" style="pointer-events: auto;">
                    <div style="margin-bottom: 20px;">
                        <label style="
                            display: block; 
                            font-size: 14px; 
                            font-weight: 500; 
                            color: #374151; 
                            margin-bottom: 12px;
                            font-family: inherit;
                        ">
                            Reason for reporting:
                        </label>
                        
                        <div style="display: flex; flex-direction: column; gap: 8px;">
                            <label class="radio-option" style="
                                display: flex; 
                                align-items: center; 
                                padding: 12px; 
                                border: 2px solid #e5e7eb; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                transition: all 0.2s;
                                font-family: inherit;
                                pointer-events: auto;
                            ">
                                <input type="radio" name="reason" value="Spam" required style="
                                    margin-right: 12px; 
                                    accent-color: #ef4444;
                                    width: 16px;
                                    height: 16px;
                                    pointer-events: auto;
                                ">
                                <span style="font-size: 14px; font-weight: 500; color: #374151;">Spam</span>
                            </label>
                            
                            <label class="radio-option" style="
                                display: flex; 
                                align-items: center; 
                                padding: 12px; 
                                border: 2px solid #e5e7eb; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                transition: all 0.2s;
                                font-family: inherit;
                                pointer-events: auto;
                            ">
                                <input type="radio" name="reason" value="Inappropriate" required style="
                                    margin-right: 12px; 
                                    accent-color: #ef4444;
                                    width: 16px;
                                    height: 16px;
                                    pointer-events: auto;
                                ">
                                <span style="font-size: 14px; font-weight: 500; color: #374151;">Inappropriate Content</span>
                            </label>
                            
                            <label class="radio-option" style="
                                display: flex; 
                                align-items: center; 
                                padding: 12px; 
                                border: 2px solid #e5e7eb; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                transition: all 0.2s;
                                font-family: inherit;
                                pointer-events: auto;
                            ">
                                <input type="radio" name="reason" value="Scam" required style="
                                    margin-right: 12px; 
                                    accent-color: #ef4444;
                                    width: 16px;
                                    height: 16px;
                                    pointer-events: auto;
                                ">
                                <span style="font-size: 14px; font-weight: 500; color: #374151;">Scam/Fraud</span>
                            </label>
                            
                            <label class="radio-option" style="
                                display: flex; 
                                align-items: center; 
                                padding: 12px; 
                                border: 2px solid #e5e7eb; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                transition: all 0.2s;
                                font-family: inherit;
                                pointer-events: auto;
                            ">
                                <input type="radio" name="reason" value="Harassment" required style="
                                    margin-right: 12px; 
                                    accent-color: #ef4444;
                                    width: 16px;
                                    height: 16px;
                                    pointer-events: auto;
                                ">
                                <span style="font-size: 14px; font-weight: 500; color: #374151;">Harassment</span>
                            </label>
                            
                            <label class="radio-option" style="
                                display: flex; 
                                align-items: center; 
                                padding: 12px; 
                                border: 2px solid #e5e7eb; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                transition: all 0.2s;
                                font-family: inherit;
                                pointer-events: auto;
                            ">
                                <input type="radio" name="reason" value="FakeListing" required style="
                                    margin-right: 12px; 
                                    accent-color: #ef4444;
                                    width: 16px;
                                    height: 16px;
                                    pointer-events: auto;
                                ">
                                <span style="font-size: 14px; font-weight: 500; color: #374151;">Fake Listing</span>
                            </label>
                            
                            <label class="radio-option" style="
                                display: flex; 
                                align-items: center; 
                                padding: 12px; 
                                border: 2px solid #e5e7eb; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                transition: all 0.2s;
                                font-family: inherit;
                                pointer-events: auto;
                            ">
                                <input type="radio" name="reason" value="Other" required style="
                                    margin-right: 12px; 
                                    accent-color: #ef4444;
                                    width: 16px;
                                    height: 16px;
                                    pointer-events: auto;
                                ">
                                <span style="font-size: 14px; font-weight: 500; color: #374151;">Other</span>
                            </label>
                        </div>
                    </div>
                    
                    <div style="margin-bottom: 24px;">
                        <label for="reportDescription" style="
                            display: block; 
                            font-size: 14px; 
                            font-weight: 500; 
                            color: #374151; 
                            margin-bottom: 8px;
                            font-family: inherit;
                        ">
                            Additional details (optional):
                        </label>
                        
                        <textarea 
                            id="reportDescription" 
                            name="description"
                            rows="3" 
                            placeholder="Please provide more details about why you're reporting this message..."
                            maxlength="500"
                            style="
                                width: 100%; 
                                padding: 12px; 
                                border: 2px solid #d1d5db; 
                                border-radius: 6px; 
                                resize: vertical; 
                                font-family: inherit; 
                                font-size: 14px;
                                min-height: 80px;
                                box-sizing: border-box;
                                outline: none;
                                pointer-events: auto;
                            "
                        ></textarea>
                        
                        <div id="charCount" style="
                            font-size: 12px; 
                            color: #6b7280; 
                            margin-top: 4px;
                            text-align: right;
                        ">
                            0/500 characters
                        </div>
                    </div>
                    
                    <div style="
                        display: flex; 
                        justify-content: flex-end; 
                        gap: 12px; 
                        border-top: 1px solid #e5e7eb;
                        padding-top: 16px;
                    ">
                        <button 
                            type="button" 
                            id="cancelReport"
                            style="
                                padding: 10px 20px; 
                                color: #374151; 
                                border: 2px solid #d1d5db; 
                                border-radius: 6px; 
                                background: white; 
                                cursor: pointer; 
                                font-size: 14px;
                                font-weight: 500;
                                font-family: inherit;
                                transition: all 0.2s;
                                pointer-events: auto;
                            "
                        >
                            Cancel
                        </button>
                        
                        <button 
                            type="submit" 
                            id="reportSubmitBtn"
                            style="
                                padding: 10px 20px; 
                                background: #ef4444; 
                                color: white; 
                                border: 2px solid #ef4444; 
                                border-radius: 6px; 
                                cursor: pointer; 
                                display: flex; 
                                align-items: center; 
                                font-size: 14px;
                                font-weight: 500;
                                font-family: inherit;
                                transition: all 0.2s;
                                pointer-events: auto;
                            "
                        >
                            <span style="margin-right: 8px;">🚩</span>
                            Report Message
                        </button>
                    </div>
                </form>
            </div>
        </div>
    `;

    // CRITICAL FIX: Add modal DIRECTLY to document.body, not to the page content
    // This ensures it's not affected by the overflow: hidden rules
    document.body.appendChild(modalContainer);

    // Get modal reference
    reportModal = modalContainer;

    // Setup event listeners immediately
    setupReportModalEventListeners();

    console.log('Report modal created successfully');
    console.log('Modal exists:', !!document.getElementById('reportModal'));
    console.log('Radio buttons found:', modalContainer.querySelectorAll('input[type="radio"]').length);
    console.log('Form exists:', !!modalContainer.querySelector('#reportForm'));
}

// Setup all event listeners for the modal
function setupReportModalEventListeners() {
    if (!reportModal) {
        console.error('Report modal not found for event listener setup');
        return;
    }

    const closeBtn = reportModal.querySelector('#closeReportModal');
    const cancelBtn = reportModal.querySelector('#cancelReport');
    const reportForm = reportModal.querySelector('#reportForm');
    const reportDescription = reportModal.querySelector('#reportDescription');
    const charCount = reportModal.querySelector('#charCount');

    // Close button handlers
    if (closeBtn) {
        closeBtn.addEventListener('click', hideReportModal);
        closeBtn.addEventListener('mouseover', function () {
            this.style.color = '#6b7280';
            this.style.backgroundColor = '#f3f4f6';
        });
        closeBtn.addEventListener('mouseout', function () {
            this.style.color = '#9ca3af';
            this.style.backgroundColor = 'transparent';
        });
    }

    if (cancelBtn) {
        cancelBtn.addEventListener('click', hideReportModal);
        cancelBtn.addEventListener('mouseover', function () {
            this.style.backgroundColor = '#f9fafb';
            this.style.borderColor = '#9ca3af';
        });
        cancelBtn.addEventListener('mouseout', function () {
            this.style.backgroundColor = 'white';
            this.style.borderColor = '#d1d5db';
        });
    }

    // Submit button hover effects
    const submitBtn = reportModal.querySelector('#reportSubmitBtn');
    if (submitBtn) {
        submitBtn.addEventListener('mouseover', function () {
            this.style.backgroundColor = '#dc2626';
            this.style.borderColor = '#dc2626';
        });
        submitBtn.addEventListener('mouseout', function () {
            this.style.backgroundColor = '#ef4444';
            this.style.borderColor = '#ef4444';
        });
    }

    // Form submission
    if (reportForm) {
        reportForm.addEventListener('submit', handleReportSubmission);
    }

    // Character counter
    if (reportDescription && charCount) {
        reportDescription.addEventListener('input', function () {
            const count = this.value.length;
            charCount.textContent = `${count}/500 characters`;
            charCount.style.color = count > 450 ? '#ef4444' : '#6b7280';
        });

        // Focus effects for textarea
        reportDescription.addEventListener('focus', function () {
            this.style.borderColor = '#ef4444';
            this.style.boxShadow = '0 0 0 3px rgba(239, 68, 68, 0.1)';
        });

        reportDescription.addEventListener('blur', function () {
            this.style.borderColor = '#d1d5db';
            this.style.boxShadow = 'none';
        });
    }

    // Close modal when clicking outside
    reportModal.addEventListener('click', function (e) {
        if (e.target === reportModal) {
            hideReportModal();
        }
    });

    // Close modal with Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && reportModal && reportModal.style.display === 'flex') {
            hideReportModal();
        }
    });

    // Radio button styling enhancements
    const radioOptions = reportModal.querySelectorAll('.radio-option');
    radioOptions.forEach(label => {
        const radio = label.querySelector('input[type="radio"]');
        if (radio) {
            label.addEventListener('mouseover', function () {
                if (!radio.checked) {
                    this.style.backgroundColor = '#f9fafb';
                    this.style.borderColor = '#d1d5db';
                }
            });

            label.addEventListener('mouseout', function () {
                if (!radio.checked) {
                    this.style.backgroundColor = 'white';
                    this.style.borderColor = '#e5e7eb';
                }
            });

            radio.addEventListener('change', function () {
                // Remove selected styling from all labels
                radioOptions.forEach(l => {
                    l.style.backgroundColor = 'white';
                    l.style.borderColor = '#e5e7eb';
                });

                // Add selected styling to current label
                if (this.checked) {
                    label.style.backgroundColor = '#fef2f2';
                    label.style.borderColor = '#ef4444';
                }
            });
        }
    });

    console.log('Event listeners set up successfully');
}

// Show report modal function
function showReportModal(messageId) {
    console.log('Showing report modal for message:', messageId);

    // Ensure modal exists - if not, create it
    if (!reportModal || !document.getElementById('reportModal')) {
        console.log('Modal not found, initializing...');
        initializeReportModal();

        // Wait a bit to ensure DOM is ready
        setTimeout(() => {
            showReportModal(messageId);
        }, 50);
        return;
    }

    currentReportMessageId = messageId;

    // Reset form
    const reportForm = reportModal.querySelector('#reportForm');
    const reportDescription = reportModal.querySelector('#reportDescription');
    const charCount = reportModal.querySelector('#charCount');

    if (reportForm) {
        reportForm.reset();
    }

    if (reportDescription) {
        reportDescription.value = '';
    }

    if (charCount) {
        charCount.textContent = '0/500 characters';
        charCount.style.color = '#6b7280';
    }

    // Remove any previous selected styling
    const radioOptions = reportModal.querySelectorAll('.radio-option');
    radioOptions.forEach(label => {
        label.style.backgroundColor = 'white';
        label.style.borderColor = '#e5e7eb';
    });

    // CRITICAL FIX: Override overflow restrictions temporarily
    const originalBodyStyle = document.body.style.overflow;
    const originalDocumentStyle = document.documentElement.style.overflow;

    // Force body and html to allow overflow
    document.body.style.overflow = 'auto';
    document.documentElement.style.overflow = 'auto';

    // Add modal-open class to body for additional CSS targeting
    document.body.classList.add('modal-open', 'report-modal-open');

    // Show modal with proper display
    reportModal.style.display = 'flex';
    reportModal.style.visibility = 'visible';
    reportModal.style.opacity = '1';

    // Focus management
    setTimeout(() => {
        const firstRadio = reportModal.querySelector('input[type="radio"]');
        if (firstRadio) {
            firstRadio.focus();
        }
    }, 100);

    console.log('Modal should now be visible');
    console.log('Modal display style:', reportModal.style.display);
    console.log('Modal visibility:', reportModal.style.visibility);
    console.log('Modal in DOM:', document.contains(reportModal));

    // Store original styles for restoration
    reportModal._originalBodyOverflow = originalBodyStyle;
    reportModal._originalDocumentOverflow = originalDocumentStyle;
}

// Hide report modal function
function hideReportModal() {
    console.log('Hiding report modal');

    if (reportModal) {
        // Hide modal
        reportModal.style.display = 'none';
        reportModal.style.visibility = 'hidden';
        reportModal.style.opacity = '0';

        // Restore original overflow styles
        if (reportModal._originalBodyOverflow !== undefined) {
            document.body.style.overflow = reportModal._originalBodyOverflow;
        } else {
            document.body.style.overflow = '';
        }

        if (reportModal._originalDocumentOverflow !== undefined) {
            document.documentElement.style.overflow = reportModal._originalDocumentOverflow;
        } else {
            document.documentElement.style.overflow = '';
        }

        // Remove modal classes
        document.body.classList.remove('modal-open', 'report-modal-open');
    }

    currentReportMessageId = null;
    console.log('Modal hidden');
}

// Handle report form submission
async function handleReportSubmission(event) {
    event.preventDefault();

    console.log('Handling report submission for message:', currentReportMessageId);

    if (!currentReportMessageId) {
        alert('Error: No message selected for reporting');
        return;
    }

    // Get form data
    const formData = new FormData(event.target);
    const reason = formData.get('reason');
    const description = reportModal.querySelector('#reportDescription')?.value || '';

    console.log('Report data:', { messageId: currentReportMessageId, reason, description });

    if (!reason) {
        alert('Please select a reason for reporting');
        return;
    }

    // Update submit button
    const submitBtn = reportModal.querySelector('#reportSubmitBtn');
    const originalHTML = submitBtn.innerHTML;
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span style="margin-right: 8px;">⏳</span> Submitting...';

    try {
        const response = await fetch('/Chat/ReportMessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                messageId: parseInt(currentReportMessageId),
                reason: reason,
                description: description
            })
        });

        console.log('Report response status:', response.status);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log('Report result:', result);

        if (result.success) {
            alert('Message reported successfully. Thank you for helping keep our community safe.');
            hideReportModal();
            markMessageAsReported(currentReportMessageId);
        } else {
            alert('Error reporting message: ' + (result.message || 'Unknown error'));
        }
    } catch (error) {
        console.error('Error reporting message:', error);
        alert('Error reporting message. Please check your connection and try again.');
    } finally {
        // Reset submit button
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalHTML;
        }
    }
}

// Mark message as reported
function markMessageAsReported(messageId) {
    const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
    if (messageElement) {
        const reportBtn = messageElement.querySelector('.report-btn');
        if (reportBtn) {
            reportBtn.innerHTML = '✅';
            reportBtn.title = 'Message reported';
            reportBtn.onclick = null;
            reportBtn.style.color = '#10b981';
            reportBtn.style.cursor = 'default';
        }
    }
}

// Test function to manually trigger modal
function testReportModal() {
    console.log('Testing report modal...');
    showReportModal(12345); // Test with dummy message ID
}

// Make functions available globally
window.showReportModal = showReportModal;
window.hideReportModal = hideReportModal;
window.testReportModal = testReportModal;

// ========================================
// END OF REPORT MODAL FUNCTIONS
// ========================================

function startMessagePolling() {
    // Fallback polling if SignalR fails
    setInterval(async () => {
        if (currentUser && otherUser && (!connection || connection.state !== signalR.HubConnectionState.Connected)) {
            await loadMessages();
        }
    }, 3000);
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');

    const icons = {
        success: '✅',
        error: '❌',
        warning: '⚠️',
        info: 'ℹ️'
    };

    const colors = {
        success: 'bg-green-500',
        error: 'bg-red-500',
        warning: 'bg-yellow-500',
        info: 'bg-blue-500'
    };

    notification.className = `fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg transition-all duration-300 text-white max-w-sm ${colors[type] || colors.info}`;
    notification.innerHTML = `
        <div class="flex items-start">
            <span class="mr-2 text-lg">${icons[type] || icons.info}</span>
            <span class="text-sm">${message}</span>
        </div>
    `;

    document.body.appendChild(notification);

    // Animate in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
        notification.style.opacity = '1';
    }, 10);

    // Remove after delay
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        notification.style.opacity = '0';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, type === 'success' ? 4000 : 3000);
}

async function switchTestUser(userId) {
    try {
        // Leave current chat if connected
        if (currentChatId) {
            await leaveChat(currentChatId);
        }

        const response = await fetch('/Chat/SwitchTestUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ userId: userId })
        });

        const result = await response.json();

        if (result.success) {
            showNotification(result.message, 'success');
            window.location.href = `/Home/MessagePage?otherUserId=${otherUser?.id || ''}`;
        } else {
            showNotification('Error switching user: ' + result.message, 'error');
        }
    } catch (error) {
        console.error('Error switching user:', error);
        showNotification('Error switching user', 'error');
    }
}

function handleViewportChanges() {
    let initialViewportHeight = window.innerHeight;

    window.addEventListener('resize', function () {
        if (window.innerWidth <= 768) {
            const currentHeight = window.innerHeight;
            const heightDifference = initialViewportHeight - currentHeight;

            if (heightDifference > 150) {
                adjustForKeyboard(true);
            } else {
                adjustForKeyboard(false);
            }
        }
    });

    window.addEventListener('orientationchange', function () {
        setTimeout(() => {
            initialViewportHeight = window.innerHeight;
            const mobileInputContainer = document.querySelector('.message-input-container-fixed');
            if (mobileInputContainer) {
                mobileInputContainer.style.display = 'none';
                mobileInputContainer.offsetHeight;
                mobileInputContainer.style.display = 'flex';
            }
        }, 500);
    });
}

function adjustForKeyboard(keyboardVisible) {
    const mobileInputContainer = document.querySelector('.message-input-container-fixed');
    const chatMessages = document.getElementById('chatMessages');

    if (keyboardVisible) {
        if (mobileInputContainer) {
            mobileInputContainer.style.bottom = '10px';
        }
        if (chatMessages) {
            chatMessages.style.paddingBottom = '120px';
        }
    } else {
        if (mobileInputContainer) {
            mobileInputContainer.style.bottom = '80px';
        }
        if (chatMessages) {
            chatMessages.style.paddingBottom = '80px';
        }
    }

    setTimeout(scrollToBottom, 100);
}

function debugModal() {
    console.log('Modal element:', reportModal);
    console.log('Modal display style:', reportModal?.style.display);
    console.log('Modal computed style:', reportModal ? window.getComputedStyle(reportModal).display : 'null');
    console.log('Modal classes:', reportModal?.className);

    if (reportModal) {
        const form = reportModal.querySelector('#reportForm');
        console.log('Form element found:', !!form);
        console.log('Radio buttons found:', reportModal.querySelectorAll('input[type="radio"]').length);
    }
}

// Handle page visibility for read receipts
document.addEventListener('visibilitychange', function () {
    if (!document.hidden && currentChatId) {
        markMessagesAsRead();
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', function () {
    if (connection && currentChatId) {
        leaveChat(currentChatId);
    }
});

// Export functions for external use
window.MessagePage = {
    sendMessage,
    addMessageToChat,
    scrollToBottom,
    showReportModal,
    hideReportModal,
    switchTestUser,
    joinChat,
    leaveChat
};