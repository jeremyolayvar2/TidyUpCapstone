document.addEventListener('DOMContentLoaded', () => {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');
    const newMessageBtn = document.getElementById('newMessageBtn');
    const closeAppBtn = document.getElementById('closeAppBtn');
    const closeChatBtn = document.getElementById('closeChatBtn');
    const messageList = document.querySelector('.messages-list');

    // Hide both panels by default
    if (appContainer) appContainer.classList.remove('active');
    if (chatWindow) chatWindow.classList.remove('active');

    // Show the messages panel when "Messages" button is clicked
    if (newMessageBtn) {
        newMessageBtn.addEventListener('click', () => {
            if (appContainer) appContainer.classList.add('active');
            if (chatWindow) chatWindow.classList.remove('active');

            // ✅ Always reload chats when messages panel is opened to ensure fresh data
            console.log("Messages button clicked, reloading chats...");
            setTimeout(() => {
                if (typeof loadExistingChats === 'function') {
                    loadExistingChats();
                }
            }, 100); // Small delay to ensure UI is ready
        });
    }

    // Close the whole chat system
    if (closeAppBtn) {
        closeAppBtn.addEventListener('click', () => {
            if (appContainer) appContainer.classList.remove('active');
            if (chatWindow) chatWindow.classList.remove('active');
        });
    }

    // ✅ Event delegation for dynamically added .message-item elements
    if (messageList) {
        messageList.addEventListener('click', (event) => {
            const target = event.target.closest('.message-item');
            // ✅ Since we removed hardcoded bubbles, only check for data attributes
            if (target && (target.dataset.buyerId && target.dataset.sellerId && target.dataset.itemId)) {
                const buyerId = target.dataset.buyerId;
                const sellerId = target.dataset.sellerId;
                const itemId = target.dataset.itemId;

                console.log("Chat bubble clicked from event delegation:", { buyerId, sellerId, itemId });

                openChatWindow();

                // Set conversation data on chat window
                if (chatWindow) {
                    chatWindow.dataset.buyerId = buyerId;
                    chatWindow.dataset.sellerId = sellerId;
                    chatWindow.dataset.itemId = itemId;
                }

                // Update header with other person's name
                const senderNameElement = target.querySelector('.message-sender');
                const otherPersonName = senderNameElement ? senderNameElement.textContent : "Contact";

                const headerTitle = chatWindow ? chatWindow.querySelector('.header-chat h2') : null;
                if (headerTitle) {
                    headerTitle.textContent = otherPersonName;
                }

                console.log("Loading conversation:", { buyerId, sellerId, itemId });
                fetchConversation(buyerId, sellerId, itemId);

                // Join the conversation group
                if (typeof connection !== 'undefined') {
                    connection.invoke("JoinConversation", buyerId, sellerId, parseInt(itemId))
                        .catch(err => console.error("Error joining conversation:", err));
                }
            }
        });
    }

    // Return from chat window to messages list
    if (closeChatBtn) {
        closeChatBtn.addEventListener('click', () => {
            if (chatWindow) chatWindow.classList.remove('active');
            if (appContainer) appContainer.classList.add('active');
        });
    }

    // ESC key to close everything
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            if (appContainer) appContainer.classList.remove('active');
            if (chatWindow) chatWindow.classList.remove('active');
        }
    });
});

// ✅ Define this function globally
function openChatWindow(contactName = "Contact", itemTitle = "Item") {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');

    if (appContainer && chatWindow) {
        appContainer.classList.remove('active');
        chatWindow.classList.add('active');

        // Update chat header
        const headerTitle = chatWindow.querySelector('.header-chat h2');
        const headerSubtitle = chatWindow.querySelector('.header-chat p');

        if (headerTitle) {
            headerTitle.textContent = contactName;
        }
        if (headerSubtitle) {
            headerSubtitle.textContent = "Active now"; // or itemTitle if you prefer
        }
    }
}

// ✅ Fetch messages for a conversation
function fetchConversation(buyerId, sellerId, itemId) {
    fetch(`/Home/GetMessages?buyerId=${buyerId}&sellerId=${sellerId}&postId=${itemId}`)
        .then(res => {
            if (!res.ok) throw new Error('Failed to fetch messages');
            return res.json();
        })
        .then(messages => {
            const log = document.querySelector('.message-log');
            if (!log) return;

            log.innerHTML = ""; // Clear existing messages

            let lastTimestamp = null;
            const currentUserId = getCurrentUserId();

            messages.forEach(msg => {
                // Add timestamp if it's different from the last one
                if (msg.timestamp !== lastTimestamp) {
                    const timestampDiv = document.createElement("div");
                    timestampDiv.className = "message timestamp";
                    timestampDiv.innerHTML = `<p>${msg.timestamp}</p>`;
                    log.appendChild(timestampDiv);
                    lastTimestamp = msg.timestamp;
                }

                const msgElement = document.createElement("div");
                msgElement.className = msg.senderId === currentUserId ? "message sent" : "message received";
                msgElement.innerHTML = `<p>${msg.text}</p>`;
                log.appendChild(msgElement);
            });

            // Scroll to bottom
            log.scrollTop = log.scrollHeight;
        })
        .catch(err => console.error("Error loading conversation:", err));
}

// Make functions available globally
window.openChatWindow = openChatWindow;
window.fetchConversation = fetchConversation;