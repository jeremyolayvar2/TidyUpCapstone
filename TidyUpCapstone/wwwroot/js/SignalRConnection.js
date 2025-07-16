const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

// Start the SignalR connection
connection.start()
    .then(() => {
        console.log("SignalR Connected");
        // Load existing chats after connection is established
        loadExistingChats();
    })
    .catch(err => console.error("SignalR Connection Error: ", err.toString()));

// ✅ Receive message handler
connection.on("ReceiveMessage", function (text, timestamp, senderId) {
    const log = document.querySelector('.message-log');
    if (!log) return;

    // ✅ Don't add your own messages from SignalR since we already added them optimistically
    const currentUserId = getCurrentUserId();
    if (senderId === currentUserId) {
        console.log("Ignoring own message from SignalR (already added optimistically)");
        return;
    }

    const date = new Date(timestamp);
    const formattedTime = date.toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
    });

    // Only add timestamp if it's different from the last one
    const lastTimestamp = log.querySelector('.message.timestamp:last-of-type p');
    if (!lastTimestamp || lastTimestamp.textContent !== formattedTime) {
        const timestampDiv = document.createElement("div");
        timestampDiv.className = "message timestamp";
        timestampDiv.innerHTML = `<p>${formattedTime}</p>`;
        log.appendChild(timestampDiv);
    }

    const msgElement = document.createElement("div");
    msgElement.className = "message received";
    msgElement.innerHTML = `<p>${text}</p>`;
    log.appendChild(msgElement);

    // Scroll to bottom
    log.scrollTop = log.scrollHeight;
});

// ✅ Handle NewChatStarted — when another user initiates a chat with you
connection.on("NewChatStarted", function (buyerId, postId) {
    console.log("New chat started:", buyerId, postId);

    // ✅ UPDATED: Changed from /Home/GetChatPreview to /Chat/GetChatPreview
    fetch(`/Chat/GetChatPreview?buyerId=${buyerId}&postId=${postId}`)
        .then(res => {
            if (!res.ok) throw new Error('Failed to get chat preview');
            return res.json();
        })
        .then(data => {
            const currentUserId = getCurrentUserId();

            // Check if chat bubble already exists
            const existingBubble = document.querySelector(
                `[data-buyer-id="${buyerId}"][data-seller-id="${currentUserId}"][data-item-id="${postId}"]`
            );

            if (!existingBubble) {
                // Create chat bubble for the seller's side
                // The current user is the seller, the other person is the buyer
                createChatBubbleInList(
                    currentUserId, // sellerId (current user)
                    data.buyerName, // display the buyer's name
                    postId,
                    data.itemTitle,
                    buyerId, // buyerId
                    data.buyerName
                );
                console.log("Created new chat bubble for seller");
            } else {
                console.log("Chat bubble already exists");
            }
        })
        .catch(err => console.error("Error loading chat preview:", err));
});

// ✅ Send button handler
document.addEventListener('DOMContentLoaded', function () {
    const sendButton = document.getElementById("sendMessage");
    const messageInput = document.getElementById("chatMessageInput");

    if (sendButton) {
        sendButton.addEventListener("click", sendMessage);
    }

    if (messageInput) {
        messageInput.addEventListener("keypress", function (e) {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });
    }
});

function sendMessage() {
    const input = document.getElementById("chatMessageInput");
    const message = input.value.trim();

    if (message === "") return;

    // Get conversation details from the chat window
    const chatWindow = document.querySelector('.chat-window');
    const buyerId = chatWindow.dataset.buyerId;
    const sellerId = chatWindow.dataset.sellerId;
    const itemId = chatWindow.dataset.itemId;
    const senderId = getCurrentUserId();

    if (!buyerId || !sellerId || !itemId || !senderId) {
        console.error("Missing conversation data:", { buyerId, sellerId, itemId, senderId });
        return;
    }

    // Add message to UI immediately (optimistic update)
    const now = new Date();
    const timestamp = now.toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
    });

    const log = document.querySelector('.message-log');

    // Add timestamp if different from last one
    const lastTimestamp = log.querySelector('.message.timestamp:last-of-type p');
    if (!lastTimestamp || lastTimestamp.textContent !== timestamp) {
        const timestampDiv = document.createElement("div");
        timestampDiv.className = "message timestamp";
        timestampDiv.innerHTML = `<p>${timestamp}</p>`;
        log.appendChild(timestampDiv);
    }

    const msgElement = document.createElement("div");
    msgElement.className = "message sent";
    msgElement.innerHTML = `<p>${message}</p>`;
    log.appendChild(msgElement);

    // Scroll to bottom
    log.scrollTop = log.scrollHeight;

    // Send via SignalR
    connection.invoke("Send", message, senderId, buyerId, sellerId, parseInt(itemId))
        .catch(err => {
            console.error("Error sending message:", err.toString());
            // Optionally remove the optimistically added message on error
            msgElement.remove();
        });

    input.value = "";
}

function loadExistingChats() {
    // ✅ UPDATED: Changed from /Home/GetUserChats to /Chat/GetUserChats
    fetch('/Chat/GetUserChats')
        .then(res => {
            if (!res.ok) throw new Error('Failed to load chats');
            return res.json();
        })
        .then(data => {
            const messagesList = document.querySelector('.messages-list');
            if (!messagesList) return;

            // ✅ Clear ALL existing chat bubbles since we removed the hardcoded one
            messagesList.innerHTML = '';

            data.forEach(chat => {
                const currentUserId = getCurrentUserId();
                const isCurrentUserBuyer = chat.buyerId === currentUserId;

                // For display purposes, show the OTHER person's name
                const displayName = isCurrentUserBuyer ? chat.sellerName : chat.buyerName;

                // ✅ Create properly structured chat bubble
                const chatBubble = document.createElement('div');
                chatBubble.className = 'message-item';
                chatBubble.setAttribute('role', 'listitem');
                chatBubble.setAttribute('tabindex', '0');

                // ✅ Set the correct data attributes
                chatBubble.dataset.buyerId = chat.buyerId;
                chatBubble.dataset.sellerId = chat.sellerId;
                chatBubble.dataset.itemId = chat.postId;

                const avatarWrapper = document.createElement('div');
                avatarWrapper.className = 'avatar-wrapper';
                avatarWrapper.innerHTML = `<img src="/assets/person-image.svg" alt="${displayName}" class="online" />`;

                const messageText = document.createElement('div');
                messageText.className = 'message-text';
                messageText.innerHTML = `
                    <span class="message-sender">${displayName}</span>
                    <span class="message-status">${chat.itemTitle}</span>
                `;

                chatBubble.appendChild(avatarWrapper);
                chatBubble.appendChild(messageText);

                // ✅ Add click handler for this specific chat
                chatBubble.addEventListener('click', function () {
                    console.log("Chat bubble clicked:", {
                        buyerId: chat.buyerId,
                        sellerId: chat.sellerId,
                        postId: chat.postId
                    });

                    openChatWindow(displayName, chat.itemTitle);

                    const chatWindow = document.querySelector('.chat-window');
                    if (chatWindow) {
                        chatWindow.dataset.buyerId = chat.buyerId;
                        chatWindow.dataset.sellerId = chat.sellerId;
                        chatWindow.dataset.itemId = chat.postId;
                    }

                    // Update chat header
                    const headerTitle = document.querySelector('.header-chat h2');
                    if (headerTitle) {
                        headerTitle.textContent = displayName;
                    }

                    // Load the conversation
                    fetchConversation(chat.buyerId, chat.sellerId, chat.postId);

                    // Join the conversation group
                    if (typeof connection !== 'undefined') {
                        connection.invoke("JoinConversation", chat.buyerId, chat.sellerId, parseInt(chat.postId))
                            .catch(err => console.error("Error joining conversation:", err));
                    }
                });

                messagesList.appendChild(chatBubble);
            });

            console.log(`Loaded ${data.length} existing chats`);
        })
        .catch(err => console.error("Failed to load chat bubbles:", err));
}