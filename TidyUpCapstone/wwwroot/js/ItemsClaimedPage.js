// ItemsClaimedPage.js - Clean version for server-side data

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
            await this.delay(300);
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

            setTimeout(() => {
                this.removeToast(toast);
                resolve();
            }, duration);

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
}

// Initialize Alert System
const alertSystem = new AlertSystem();

// Items Claimed Page Specific Functionality -----------------------------------------------------------------------------------------

document.addEventListener('DOMContentLoaded', function () {
    // Initialize page functionality
    initializeFilters();
    initializeItemActions();
    initializeSearch();

    // Update sidebar margin based on state
    updateSidebarMargin();
});

// Filter and Search Functionality
function initializeFilters() {
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const sortFilter = document.getElementById('sortFilter');

    // Search functionality with debounce
    if (searchInput) {
        searchInput.addEventListener('input', debounce(filterItems, 500));
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
    const searchTerm = document.getElementById('searchInput')?.value || '';
    const statusFilter = document.getElementById('statusFilter')?.value || '';
    const categoryFilter = document.getElementById('categoryFilter')?.value || '';
    const sortFilter = document.getElementById('sortFilter')?.value || 'newest';

    // Build URL with query parameters for server-side filtering
    const params = new URLSearchParams();
    if (searchTerm.trim()) params.set('search', searchTerm.trim());
    if (statusFilter) params.set('status', statusFilter);
    if (categoryFilter) params.set('categoryId', categoryFilter);
    if (sortFilter && sortFilter !== 'newest') params.set('sortBy', sortFilter);

    // Navigate to filtered URL
    const url = window.location.pathname + (params.toString() ? '?' + params.toString() : '');

    // Show loading notification
    alertSystem.info('Applying Filters', 'Loading filtered results...', 2000);

    window.location.href = url;
}

// Item Actions Functionality
function initializeItemActions() {
    // Contact Seller buttons
    document.querySelectorAll('.contact-seller').forEach(btn => {
        btn.addEventListener('click', function (e) {
            const itemTitle = this.closest('.item-card')?.querySelector('.item-title')?.textContent;
            if (itemTitle) {
                alertSystem.info('Opening Chat', `Connecting you with the seller for "${itemTitle}"`, 2000);
            }
        });
    });

    // View Details buttons
    document.querySelectorAll('.view-details').forEach(btn => {
        btn.addEventListener('click', function (e) {
            const itemTitle = this.closest('.item-card')?.querySelector('.item-title')?.textContent;
            if (itemTitle) {
                alertSystem.info('Loading Details', `Opening detailed view for "${itemTitle}"`, 2000);
            }
        });
    });
}

// Update sidebar margin based on current state
function updateSidebarMargin() {
    const container = document.querySelector('.items-claimed-container');
    const sidebar = document.getElementById('sidebar');

    if (container && sidebar) {
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
        setTimeout(updateSidebarMargin, 100);
    });
}

// Keyboard Navigation Support
document.addEventListener('keydown', function (e) {
    // Enter key on focused buttons
    if (e.key === 'Enter' && e.target.classList.contains('btn')) {
        e.target.click();
    }
});

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

// Lazy loading for images
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

// Initialize features
document.addEventListener('DOMContentLoaded', function () {
    setupLazyLoading();

    // Show welcome message after a delay
    setTimeout(() => {
        alertSystem.info('Welcome Back!', 'Here are your claimed items. Use filters to find specific items quickly.', 4000);
    }, 1000);
});

// URL parameter handling for maintaining filter state
function getUrlParameter(name) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(name);
}

// Set initial filter values from URL parameters
function setInitialFilterValues() {
    const searchInput = document.getElementById('searchInput');
    const statusFilter = document.getElementById('statusFilter');
    const categoryFilter = document.getElementById('categoryFilter');
    const sortFilter = document.getElementById('sortFilter');

    if (searchInput) {
        const searchParam = getUrlParameter('search');
        if (searchParam) searchInput.value = searchParam;
    }

    if (statusFilter) {
        const statusParam = getUrlParameter('status');
        if (statusParam) statusFilter.value = statusParam;
    }

    if (categoryFilter) {
        const categoryParam = getUrlParameter('categoryId');
        if (categoryParam) categoryFilter.value = categoryParam;
    }

    if (sortFilter) {
        const sortParam = getUrlParameter('sortBy');
        if (sortParam) sortFilter.value = sortParam;
    }
}

// Initialize filter values on page load
document.addEventListener('DOMContentLoaded', function () {
    setInitialFilterValues();
});