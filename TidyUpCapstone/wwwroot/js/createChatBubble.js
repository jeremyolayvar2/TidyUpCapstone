function getCurrentUserId() {
    return document.body.dataset.userid || null;
}

function getCurrentUserName() {
    return document.body.dataset.username || "Anonymous";
}

function createChatBubble(sellerId, sellerName, itemId, itemTitle, buyerId = getCurrentUserId(), buyerName = getCurrentUserName()) {
    if (buyerId === sellerId) {
        console.warn("User is trying to chat with themselves. Aborting.");
        return;
    }

    // Check if chat bubble already exists to avoid duplicates
    const existingBubble = document.querySelector(`[data-buyer-id="${buyerId}"][data-seller-id="${sellerId}"][data-item-id="${itemId}"]`);
    if (existingBubble) {
        console.log("Chat bubble already exists, opening existing chat");
        // Click the existing bubble to open the chat
        existingBubble.click();
        return;
    }

    // Create the chat bubble visually in the message list
    createChatBubbleInList(sellerId, sellerName, itemId, itemTitle, buyerId, buyerName);

    // Open the chat window immediately
    openChatWindow(sellerName, itemTitle);

    // Set conversation data on chat window
    const chatWindow = document.querySelector('.chat-window');
    if (chatWindow) {
        chatWindow.dataset.buyerId = buyerId;
        chatWindow.dataset.sellerId = sellerId;
        chatWindow.dataset.itemId = itemId;
    }

    // Join the conversation group and notify backend
    connection.invoke("StartChat", buyerId, sellerId, parseInt(itemId))
        .then(() => {
            connection.invoke("JoinConversation", buyerId, sellerId, parseInt(itemId));
            // Load existing messages for this conversation
            fetchConversation(buyerId, sellerId, itemId);
        })
        .catch(err => console.error("Error starting chat:", err.toString()));
}

function createChatBubbleInList(sellerId, sellerName, itemId, itemTitle, buyerId, buyerName) {
    const chatBubble = document.createElement('div');
    chatBubble.className = 'message-item';
    chatBubble.setAttribute('role', 'listitem');
    chatBubble.setAttribute('tabindex', '0');

    // ✅ Add data attributes to help with fetching later
    chatBubble.dataset.buyerId = buyerId;
    chatBubble.dataset.sellerId = sellerId;
    chatBubble.dataset.itemId = itemId;

    const avatarWrapper = document.createElement('div');
    avatarWrapper.className = 'avatar-wrapper';
    avatarWrapper.innerHTML = `<img src="/assets/person-image.svg" alt="${sellerName}" class="online" />`;

    const messageText = document.createElement('div');
    messageText.className = 'message-text';

    // Show the other person's name (not current user)
    const currentUserId = getCurrentUserId();
    const displayName = currentUserId === buyerId ? sellerName : buyerName;

    messageText.innerHTML = `
        <span class="message-sender">${displayName}</span>
        <span class="message-status">${itemTitle}</span>
    `;

    chatBubble.appendChild(avatarWrapper);
    chatBubble.appendChild(messageText);

    // Add click handler to open chat when this bubble is clicked
    chatBubble.addEventListener('click', function () {
        openChatWindow(displayName, itemTitle);

        const chatWindow = document.querySelector('.chat-window');
        if (chatWindow) {
            chatWindow.dataset.buyerId = buyerId;
            chatWindow.dataset.sellerId = sellerId;
            chatWindow.dataset.itemId = itemId;
        }

        // Update chat header
        const headerTitle = document.querySelector('.header-chat h2');
        if (headerTitle) {
            headerTitle.textContent = displayName;
        }

        fetchConversation(buyerId, sellerId, itemId);
    });

    const messagesList = document.querySelector('.messages-list');
    if (messagesList) {
        messagesList.appendChild(chatBubble);
    }
}

// Make sure this function is available globally
window.createChatBubble = createChatBubble;
window.createChatBubbleInList = createChatBubbleInList;