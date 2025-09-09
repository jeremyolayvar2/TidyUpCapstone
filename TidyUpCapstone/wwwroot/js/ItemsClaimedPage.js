// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//For toggling the sidebar -----------------------------------------------------------------------------------------

const toggleButton = document.getElementById('icon-toggle');
const sidebar = document.getElementById('sidebar');
const toggleIcon = toggleButton?.querySelector('img');

function toggleSidebar() {
    sidebar?.classList.toggle('close');
    toggleIcon?.classList.toggle('rotate');
}

//Phone View -----------------------------------------------------------------------------------------

const dockItems = document.querySelectorAll(".dock-item");
const distance = 100;
const maxScale = 1.8;

const dockPanel = document.getElementById("dockPanel");

if (dockPanel) {
    dockPanel.addEventListener("mousemove", (e) => {
        const rect = dockPanel.getBoundingClientRect();
        const mouseX = e.clientX;

        dockItems.forEach((item) => {
            const itemRect = item.getBoundingClientRect();
            const itemCenter = itemRect.left + itemRect.width / 2;
            const dist = Math.abs(mouseX - itemCenter);
            const scale = Math.max(1, maxScale - dist / distance);

            item.style.transform = `scale(${scale})`;
        });
    });

    dockPanel.addEventListener("mouseleave", () => {
        dockItems.forEach((item) => {
            item.style.transform = "scale(1)";
        });
    });
}

dockItems.forEach((item) => {
    const label = item.querySelector(".dock-label");
    if (label) {
        item.addEventListener("mouseenter", () => {
            label.style.opacity = "1";
            label.style.transform = "translateX(-50%) translateY(-5px)";
        });
        item.addEventListener("mouseleave", () => {
            label.style.opacity = "0";
            label.style.transform = "translateX(-50%)";
        });
    }
});

// Enhanced Alert System -----------------------------------------------------------------------------------------

class AlertSystem {
    constructor() {
        this.toastContainer = document.getElementById('toast-container');
        this.modalContainer = document.getElementById('alert-modal-container');
        this.toastQueue = [];
        this.isProcessingQueue = false;
    }

    // Toast Notifications
    showToast(title, message, type = 'info', duration = 5000) {
        this.toastQueue.push({ title, message, type, duration });
        this.processToastQueue();
    }

    async processToastQueue() {
        if (this.isProcessingQueue || this.toastQueue.length === 0) return;

        this.isProcessingQueue = true;

        while (this.toastQueue.length > 0) {
            const toast = this.toastQueue.shift();
            await this.createToast(toast);
            await this.delay(300); // Small delay between toasts
        }

        this.isProcessingQueue = false;
    }

    createToast({ title, message, type, duration }) {
        return new Promise((resolve) => {
            const toast = document.createElement('div');
            toast.className = `toast toast-${type}`;

            const icon = this.getToastIcon(type);

            toast.innerHTML = `
                <div class="toast-icon">
                    ${icon}
                </div>
                <div class="toast-content">
                    <div class="toast-title">${title}</div>
                    <div class="toast-message">${message}</div>
                </div>
                <button class="toast-close" onclick="this.parentElement.remove()">
                    <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
                    </svg>
                </button>
            `;

            this.toastContainer.appendChild(toast);

            // Auto remove after duration
            setTimeout(() => {
                this.removeToast(toast);
                resolve();
            }, duration);

            // Click to dismiss
            toast.addEventListener('click', (e) => {
                if (e.target.closest('.toast-close')) {
                    this.removeToast(toast);
                    resolve();
                }
            });
        });
    }

    removeToast(toast) {
        toast.classList.add('hiding');
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }

    getToastIcon(type) {
        const icons = {
            success: `<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
                      </svg>`,
            error: `<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
                    </svg>`,
            warning: `<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
                      </svg>`,
            info: `<svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                     <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path>
                   </svg>`
        };
        return icons[type] || icons.info;
    }

    // Modal Alerts
    showAlert(title, message, type = 'info', options = {}) {
        return new Promise((resolve) => {
            const modal = this.createModal(title, message, type, options, resolve);
            this.modalContainer.appendChild(modal);
            this.modalContainer.classList.remove('hidden');

            // Focus trap and accessibility
            setTimeout(() => {
                const firstButton = modal.querySelector('button');
                if (firstButton) firstButton.focus();
            }, 100);
        });
    }

    createModal(title, message, type, options, resolve) {
        const {
            confirmText = 'OK',
            cancelText = 'Cancel',
            showCancel = false,
            confirmClass = 'primary'
        } = options;

        const modal = document.createElement('div');
        modal.className = 'alert-modal';

        const icon = this.getModalIcon(type);

        modal.innerHTML = `
            <div class="alert-modal-backdrop" onclick="event.stopPropagation()"></div>
            <div class="alert-modal-content">
                <div class="alert-modal-header">
                    <div class="alert-modal-icon ${type}">
                        ${icon}
                    </div>
                    <h3 class="alert-modal-title">${title}</h3>
                    <p class="alert-modal-message">${message}</p>
                </div>
                <div class="alert-modal-actions">
                    ${showCancel ? `<button class="alert-modal-btn secondary" data-action="cancel">${cancelText}</button>` : ''}
                    <button class="alert-modal-btn ${confirmClass}" data-action="confirm">${confirmText}</button>
                </div>
            </div>
        `;

        // Event listeners
        modal.addEventListener('click', (e) => {
            const action = e.target.dataset.action;
            if (action) {
                this.closeModal();
                resolve(action === 'confirm');
            } else if (e.target.classList.contains('alert-modal-backdrop')) {
                this.closeModal();
                resolve(false);
            }
        });

        // Keyboard support
        modal.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.closeModal();
                resolve(false);
            }
        });

        return modal;
    }

    getModalIcon(type) {
        const icons = {
            success: `<svg fill="currentColor" viewBox="0 0 20 20" class="w-6 h-6">
                        <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
                      </svg>`,
            error: `<svg fill="currentColor" viewBox="0 0 20 20" class="w-6 h-6">
                      <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
                    </svg>`,
            warning: `<svg fill="currentColor" viewBox="0 0 20 20" class="w-6 h-6">
                        <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
                      </svg>`,
            info: `<svg fill="currentColor" viewBox="0 0 20 20" class="w-6 h-6">
                     <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path>
                   </svg>`
        };
        return icons[type] || icons.info;
    }

    closeModal() {
        this.modalContainer.classList.add('hidden');
        setTimeout(() => {
            this.modalContainer.innerHTML = '';
        }, 300);
    }

    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    // Convenience methods
    success(title, message, duration) {
        this.showToast(title, message, 'success', duration);
    }

    error(title, message, duration) {
        this.showToast(title, message, 'error', duration);
    }

    warning(title, message, duration) {
        this.showToast(title, message, 'warning', duration);
    }

    info(title, message, duration) {
        this.showToast(title, message, 'info', duration);
    }

    async confirm(title, message, options = {}) {
        return await this.showAlert(title, message, 'warning', {
            showCancel: true,
            confirmText: 'Confirm',
            confirmClass: 'danger',
            ...options
        });
    }
}

// Initialize Alert System
const alertSystem = new AlertSystem();

// Items Claimed Page Specific Functionality -----------------------------------------------------------------------------------------

document.addEventListener('DOMContentLoaded', function () {
    // Initialize page functionality
    initializeFilters();
    initializeItemActions();
    initializeSearch();
    updateItemCounts();

    // Update sidebar margin based on state
    updateSidebarMargin();
});

// Filter and Search Functionality
function initializeFilters() {
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const sortFilter = document.getElementById('sortFilter');

    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('input', debounce(filterItems, 300));
    }

    // Filter functionality
    [statusFilter, categoryFilter, sortFilter].forEach(filter => {
        if (filter) {
            filter.addEventListener('change', filterItems);
        }
    });
}

function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                filterItems();
            }
        });
    }
}

function filterItems() {
    const searchTerm = document.getElementById('searchInput')?.value.toLowerCase() || '';
    const statusFilter = document.getElementById('statusFilter')?.value || '';
    const categoryFilter = document.getElementById('categoryFilter')?.value || '';
    const sortFilter = document.getElementById('sortFilter')?.value || 'newest';

    const itemCards = document.querySelectorAll('.item-card');
    const emptyState = document.getElementById('emptyState');
    let visibleItems = 0;

    // Filter items
    itemCards.forEach(card => {
        const title = card.querySelector('.item-title')?.textContent.toLowerCase() || '';
        const description = card.querySelector('.item-description')?.textContent.toLowerCase() || '';
        const status = card.dataset.status || '';
        const category = card.dataset.category || '';

        const matchesSearch = title.includes(searchTerm) || description.includes(searchTerm);
        const matchesStatus = !statusFilter || status === statusFilter;
        const matchesCategory = !categoryFilter || category === categoryFilter;

        if (matchesSearch && matchesStatus && matchesCategory) {
            card.style.display = 'flex';
            card.classList.remove('fade-out');
            card.classList.add('fade-in');
            visibleItems++;
        } else {
            card.classList.add('fade-out');
            card.classList.remove('fade-in');
            setTimeout(() => {
                if (card.classList.contains('fade-out')) {
                    card.style.display = 'none';
                }
            }, 300);
        }
    });

    // Show/hide empty state
    if (emptyState) {
        emptyState.style.display = visibleItems === 0 ? 'block' : 'none';
    }

    // Sort items
    if (visibleItems > 0) {
        sortItems(sortFilter);
    }

    // Show filter result toast
    if (searchTerm || statusFilter || categoryFilter) {
        alertSystem.info('Filter Applied', `Found ${visibleItems} item(s) matching your criteria`, 3000);
    }
}

function sortItems(sortBy) {
    const itemsGrid = document.querySelector('.items-grid');
    const itemCards = Array.from(document.querySelectorAll('.item-card')).filter(card =>
        card.style.display !== 'none'
    );

    itemCards.sort((a, b) => {
        switch (sortBy) {
            case 'newest':
                return new Date(b.dataset.claimedDate || 0) - new Date(a.dataset.claimedDate || 0);
            case 'oldest':
                return new Date(a.dataset.claimedDate || 0) - new Date(b.dataset.claimedDate || 0);
            case 'price-high':
                const priceA = parseFloat(a.querySelector('.item-price')?.textContent.replace(/[₱,]/g, '') || 0);
                const priceB = parseFloat(b.querySelector('.item-price')?.textContent.replace(/[₱,]/g, '') || 0);
                return priceB - priceA;
            case 'price-low':
                const priceA2 = parseFloat(a.querySelector('.item-price')?.textContent.replace(/[₱,]/g, '') || 0);
                const priceB2 = parseFloat(b.querySelector('.item-price')?.textContent.replace(/[₱,]/g, '') || 0);
                return priceA2 - priceB2;
            default:
                return 0;
        }
    });

    // Re-append sorted items
    itemCards.forEach(card => {
        if (itemsGrid && card.parentNode === itemsGrid) {
            itemsGrid.appendChild(card);
        }
    });
}

// Item Actions Functionality
function initializeItemActions() {
    // Contact Seller buttons
    document.querySelectorAll('.contact-seller').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            alertSystem.info('Opening Chat', `Connecting you with the seller for "${itemTitle}"`, 3000);

            // Simulate loading
            await simulateLoading(this, 'Opening chat...');

            // Add actual contact seller functionality here
        });
    });

    // View Details buttons
    document.querySelectorAll('.view-details').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            alertSystem.info('Loading Details', `Opening detailed view for "${itemTitle}"`, 2000);

            // Add navigation to item details page
        });
    });

    // Cancel Claim buttons
    document.querySelectorAll('.cancel-claim').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            const confirmed = await alertSystem.confirm(
                'Cancel Claim',
                `Are you sure you want to cancel your claim for "${itemTitle}"? This action cannot be undone.`,
                {
                    confirmText: 'Yes, Cancel Claim',
                    cancelText: 'Keep Claim'
                }
            );

            if (confirmed) {
                const itemCard = this.closest('.item-card');

                await simulateStatusChange(itemCard, this, 'cancelled', 'Cancelling claim...');

                alertSystem.success('Claim Cancelled', `Your claim for "${itemTitle}" has been cancelled successfully.`);
                updateItemCounts();
                initializeItemActionsForCard(itemCard);
            }
        });
    });

    // Mark Complete buttons
    document.querySelectorAll('.mark-complete').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            const confirmed = await alertSystem.confirm(
                'Mark as Complete',
                `Mark "${itemTitle}" as completed? This indicates you have successfully received the item.`,
                {
                    confirmText: 'Mark Complete',
                    cancelText: 'Not Yet',
                    confirmClass: 'success'
                }
            );

            if (confirmed) {
                const itemCard = this.closest('.item-card');

                await simulateStatusChange(itemCard, this, 'completed', 'Completing transaction...');

                alertSystem.success('Transaction Complete!', `"${itemTitle}" has been marked as completed. Don't forget to rate the seller!`);
                updateItemCounts();
                initializeItemActionsForCard(itemCard);
            }
        });
    });

    // Rate Seller buttons
    document.querySelectorAll('.rate-seller').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            alertSystem.info('Rate Seller', `Opening rating interface for "${itemTitle}" seller`, 2000);
            // Add rating modal functionality here
        });
    });

    // Download Receipt buttons
    document.querySelectorAll('.download-receipt').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            await simulateLoading(this, 'Generating receipt...');

            alertSystem.success('Receipt Ready', `Receipt for "${itemTitle}" has been downloaded to your device.`);
            // Add receipt download functionality here
        });
    });

    // Reclaim Item buttons
    document.querySelectorAll('.reclaim-item').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = this.closest('.item-card').querySelector('.item-title')?.textContent;

            const confirmed = await alertSystem.confirm(
                'Claim Again',
                `Would you like to claim "${itemTitle}" again? You will be added to the pending list.`,
                {
                    confirmText: 'Yes, Claim Again',
                    cancelText: 'Cancel',
                    confirmClass: 'primary'
                }
            );

            if (confirmed) {
                const itemCard = this.closest('.item-card');

                await simulateStatusChange(itemCard, this, 'pending', 'Processing claim...');

                alertSystem.success('Claim Submitted!', `Your claim for "${itemTitle}" has been submitted successfully.`);
                updateItemCounts();
                initializeItemActionsForCard(itemCard);
            }
        });
    });
}

// Helper Functions
async function simulateLoading(button, loadingText) {
    const originalText = button.innerHTML;
    button.disabled = true;
    button.innerHTML = `<i class="fas fa-spinner fa-spin"></i> ${loadingText}`;

    await new Promise(resolve => setTimeout(resolve, 1500));

    button.disabled = false;
    button.innerHTML = originalText;
}

async function simulateStatusChange(itemCard, button, newStatus, loadingText) {
    const originalText = button.innerHTML;
    itemCard.style.opacity = '0.5';
    button.disabled = true;
    button.innerHTML = `<i class="fas fa-spinner fa-spin"></i> ${loadingText}`;

    await new Promise(resolve => setTimeout(resolve, 1500));

    // Update status
    itemCard.dataset.status = newStatus;
    const statusBadge = itemCard.querySelector('.status-badge');
    const actionsContainer = itemCard.querySelector('.item-actions');

    // Update status badge
    switch (newStatus) {
        case 'cancelled':
            statusBadge.className = 'status-badge status-cancelled';
            statusBadge.innerHTML = '<i class="fas fa-ban"></i> Cancelled';
            actionsContainer.innerHTML = `
                <button class="btn btn-outline btn-sm view-details">
                    <i class="fas fa-eye"></i>
                    View Details
                </button>
                <button class="btn btn-primary btn-sm reclaim-item">
                    <i class="fas fa-redo"></i>
                    Claim Again
                </button>
            `;
            break;
        case 'completed':
            statusBadge.className = 'status-badge status-completed';
            statusBadge.innerHTML = '<i class="fas fa-check"></i> Completed';
            actionsContainer.innerHTML = `
                <button class="btn btn-outline btn-sm rate-seller">
                    <i class="fas fa-star"></i>
                    Rate Seller
                </button>
                <button class="btn btn-outline btn-sm view-details">
                    <i class="fas fa-eye"></i>
                    View Details
                </button>
                <button class="btn btn-secondary btn-sm download-receipt">
                    <i class="fas fa-download"></i>
                    Receipt
                </button>
            `;
            break;
        case 'pending':
            statusBadge.className = 'status-badge status-pending';
            statusBadge.innerHTML = '<i class="fas fa-clock"></i> Pending';
            actionsContainer.innerHTML = `
                <button class="btn btn-primary btn-sm contact-seller">
                    <i class="fas fa-comments"></i>
                    Contact Seller
                </button>
                <button class="btn btn-outline btn-sm view-details">
                    <i class="fas fa-eye"></i>
                    View Details
                </button>
                <button class="btn btn-danger btn-sm cancel-claim">
                    <i class="fas fa-times"></i>
                    Cancel
                </button>
            `;
            break;
    }

    itemCard.style.opacity = '1';
}

function initializeItemActionsForCard(card) {
    // Re-initialize event listeners for a specific card after content change
    const contactBtn = card.querySelector('.contact-seller');
    const viewBtn = card.querySelector('.view-details');
    const cancelBtn = card.querySelector('.cancel-claim');
    const completeBtn = card.querySelector('.mark-complete');
    const rateBtn = card.querySelector('.rate-seller');
    const receiptBtn = card.querySelector('.download-receipt');
    const reclaimBtn = card.querySelector('.reclaim-item');

    if (contactBtn) {
        contactBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;
            alertSystem.info('Opening Chat', `Connecting you with the seller for "${itemTitle}"`, 3000);
            await simulateLoading(this, 'Opening chat...');
        });
    }

    if (viewBtn) {
        viewBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;
            alertSystem.info('Loading Details', `Opening detailed view for "${itemTitle}"`, 2000);
        });
    }

    if (cancelBtn) {
        cancelBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;

            const confirmed = await alertSystem.confirm(
                'Cancel Claim',
                `Are you sure you want to cancel your claim for "${itemTitle}"?`,
                {
                    confirmText: 'Yes, Cancel Claim',
                    cancelText: 'Keep Claim'
                }
            );

            if (confirmed) {
                await simulateStatusChange(card, this, 'cancelled', 'Cancelling claim...');
                alertSystem.success('Claim Cancelled', `Your claim for "${itemTitle}" has been cancelled successfully.`);
                updateItemCounts();
                initializeItemActionsForCard(card);
            }
        });
    }

    if (completeBtn) {
        completeBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;

            const confirmed = await alertSystem.confirm(
                'Mark as Complete',
                `Mark "${itemTitle}" as completed?`,
                {
                    confirmText: 'Mark Complete',
                    cancelText: 'Not Yet',
                    confirmClass: 'success'
                }
            );

            if (confirmed) {
                await simulateStatusChange(card, this, 'completed', 'Completing transaction...');
                alertSystem.success('Transaction Complete!', `"${itemTitle}" has been marked as completed!`);
                updateItemCounts();
                initializeItemActionsForCard(card);
            }
        });
    }

    if (rateBtn) {
        rateBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;
            alertSystem.info('Rate Seller', `Opening rating interface for "${itemTitle}" seller`, 2000);
        });
    }

    if (receiptBtn) {
        receiptBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;
            await simulateLoading(this, 'Generating receipt...');
            alertSystem.success('Receipt Ready', `Receipt for "${itemTitle}" has been downloaded.`);
        });
    }

    if (reclaimBtn) {
        reclaimBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const itemTitle = card.querySelector('.item-title')?.textContent;

            const confirmed = await alertSystem.confirm(
                'Claim Again',
                `Would you like to claim "${itemTitle}" again?`,
                {
                    confirmText: 'Yes, Claim Again',
                    cancelText: 'Cancel',
                    confirmClass: 'primary'
                }
            );

            if (confirmed) {
                await simulateStatusChange(card, this, 'pending', 'Processing claim...');
                alertSystem.success('Claim Submitted!', `Your claim for "${itemTitle}" has been submitted successfully.`);
                updateItemCounts();
                initializeItemActionsForCard(card);
            }
        });
    }
}

// Update item counts in header stats
function updateItemCounts() {
    const itemCards = document.querySelectorAll('.item-card');
    const totalStat = document.querySelector('.stat-card:nth-child(1) .stat-number');
    const pendingStat = document.querySelector('.stat-card:nth-child(2) .stat-number');
    const completedStat = document.querySelector('.stat-card:nth-child(3) .stat-number');

    let total = 0;
    let pending = 0;
    let completed = 0;

    itemCards.forEach(card => {
        const status = card.dataset.status;
        total++;
        if (status === 'pending') pending++;
        if (status === 'completed') completed++;
    });

    if (totalStat) {
        animateNumber(totalStat, parseInt(totalStat.textContent), total);
    }
    if (pendingStat) {
        animateNumber(pendingStat, parseInt(pendingStat.textContent), pending);
    }
    if (completedStat) {
        animateNumber(completedStat, parseInt(completedStat.textContent), completed);
    }
}

// Animate number changes
function animateNumber(element, from, to) {
    const duration = 500;
    const steps = 20;
    const stepValue = (to - from) / steps;
    const stepDuration = duration / steps;

    let current = from;
    let step = 0;

    const timer = setInterval(() => {
        step++;
        current += stepValue;

        if (step >= steps) {
            current = to;
            clearInterval(timer);
        }

        element.textContent = Math.round(current);
    }, stepDuration);
}

// Update sidebar margin based on current state
function updateSidebarMargin() {
    const container = document.querySelector('.items-claimed-container');
    const sidebar = document.getElementById('sidebar');

    if (container && sidebar) {
        // Check if sidebar is closed
        if (sidebar.classList.contains('close')) {
            container.style.marginLeft = '82px';
        } else {
            container.style.marginLeft = '250px';
        }
    }
}

// Listen for sidebar toggle
if (toggleButton) {
    toggleButton.addEventListener('click', function () {
        setTimeout(updateSidebarMargin, 100); // Small delay to account for transition
    });
}

// Keyboard Navigation Support
document.addEventListener('keydown', function (e) {
    // Escape key to close any open modals
    if (e.key === 'Escape') {
        const modal = document.querySelector('#alert-modal-container:not(.hidden)');
        if (modal) {
            alertSystem.closeModal();
        }
    }

    // Enter key on focused buttons
    if (e.key === 'Enter' && e.target.classList.contains('btn')) {
        e.target.click();
    }
});

// Auto-refresh functionality (optional)
function startAutoRefresh() {
    setInterval(() => {
        // Simulate checking for updates
        const randomChance = Math.random();
        if (randomChance < 0.1) { // 10% chance
            alertSystem.info('Update Available', 'New activity detected on your claimed items.', 3000);
        }
    }, 30000); // Check every 30 seconds
}

// Performance optimizations
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Lazy loading for images (if needed)
function setupLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src || img.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[loading="lazy"]').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

// Initialize optional features
document.addEventListener('DOMContentLoaded', function () {
    setupLazyLoading();

    // Only start auto-refresh in production
    if (window.location.hostname !== 'localhost') {
        startAutoRefresh();
    }

    // Show welcome message
    setTimeout(() => {
        alertSystem.info('Welcome Back!', 'Here are your claimed items. Use filters to find specific items quickly.', 4000);
    }, 1000);
});

// Export for testing (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { AlertSystem, alertSystem };
}