class EscrowManager {
    constructor() {
        this.currentTransactionId = null;
        this.transactionStatus = null;
        this.currentUser = null;
    }

    initialize(currentUser, transactionId, initialStatus) {
        this.currentUser = currentUser;
        this.currentTransactionId = transactionId;
        this.transactionStatus = initialStatus;

        console.log('EscrowManager initialized:', {
            transactionId,
            status: initialStatus?.Status,
            userRole: initialStatus?.UserRole
        });

        if (this.transactionStatus?.Status === 'Escrowed') {
            this.showEscrowMessage();
            this.showTransactionButtons();
        }
    }

    showEscrowMessage() {
        this.addSystemMessage('Tokens are now held in escrow by the system.');
    }

    showTransactionButtons() {
        const chatHeader = document.querySelector('.chat-header');
        let buttonsContainer = document.getElementById('escrowButtons');

        if (!buttonsContainer && chatHeader) {
            buttonsContainer = document.createElement('div');
            buttonsContainer.id = 'escrowButtons';
            buttonsContainer.className = 'escrow-buttons bg-white/10 backdrop-blur-sm rounded-lg p-4 mx-6 mb-4';
            buttonsContainer.innerHTML = `
                <div class="flex gap-3 justify-center">
                    <button onclick="escrowManager.confirmTransaction()" 
                            class="bg-green-500 hover:bg-green-600 text-white px-6 py-2 rounded-md transition-all">
                        Confirm Transaction
                    </button>
                    <button onclick="escrowManager.cancelTransaction()" 
                            class="bg-red-500 hover:bg-red-600 text-white px-6 py-2 rounded-md transition-all">
                        Cancel Transaction
                    </button>
                </div>
            `;
            chatHeader.parentNode.insertBefore(buttonsContainer, chatHeader.nextSibling);
        }
    }

    async confirmTransaction() {
        const confirmed = confirm('Are you sure you want to confirm this transaction? Tokens will be released once both parties confirm.');
        if (!confirmed) return;

        try {
            const response = await fetch('/api/escrow/confirm', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({ transactionId: this.currentTransactionId })
            });

            const result = await response.json();
            if (result.success) {
                if (result.bothConfirmed) {
                    this.addSystemMessage('Transaction completed. Tokens released to Seller.');
                    this.hideButtons();
                } else {
                    this.addSystemMessage('Your confirmation recorded. Waiting for other party to confirm.');
                }
            } else {
                alert('Error: ' + result.message);
            }
        } catch (error) {
            console.error('Error confirming transaction:', error);
            alert('Error confirming transaction');
        }
    }

    async cancelTransaction() {
        const confirmed = confirm('Are you sure you want to cancel this transaction? Escrowed tokens will be refunded to the Buyer.');
        if (!confirmed) return;

        try {
            const response = await fetch('/api/escrow/cancel', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({ transactionId: this.currentTransactionId })
            });

            const result = await response.json();
            if (result.success) {
                this.addSystemMessage('Transaction cancelled. Tokens refunded to Buyer.');
                this.hideButtons();
            } else {
                alert('Error: ' + result.message);
            }
        } catch (error) {
            console.error('Error cancelling transaction:', error);
            alert('Error cancelling transaction');
        }
    }

    hideButtons() {
        const buttonsContainer = document.getElementById('escrowButtons');
        if (buttonsContainer) {
            buttonsContainer.remove();
        }
    }

    addSystemMessage(message) {
        const chatMessages = document.getElementById('chatMessages');
        if (!chatMessages) return;

        const messageDiv = document.createElement('div');
        messageDiv.className = 'flex justify-center mb-3';
        messageDiv.innerHTML = `
            <div class="bg-yellow-100 text-yellow-800 rounded-lg px-4 py-2 text-sm max-w-md text-center shadow-sm">
                <i class="fas fa-info-circle mr-2"></i>
                ${message}
            </div>
        `;
        chatMessages.appendChild(messageDiv);

        if (window.MessagePage && window.MessagePage.scrollToBottom) {
            window.MessagePage.scrollToBottom();
        }
    }
}

const escrowManager = new EscrowManager();
window.escrowManager = escrowManager;