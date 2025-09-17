// FIXED site.js with improved navigation and UI consistency

// FIXED: Enhanced sidebar toggle functionality with proper error handling
function toggleSidebar() {
    console.log('toggleSidebar called from site.js');

    const sidebar = document.getElementById('sidebar');
    const toggleIcon = document.querySelector('#icon-toggle .toggle, #icon-toggle img');

    if (!sidebar) {
        console.error('Sidebar element not found');
        return false;
    }

    if (!toggleIcon) {
        console.error('Toggle icon not found');
        return false;
    }

    console.log('Toggling sidebar...');

    // Toggle sidebar class
    sidebar.classList.toggle('close');

    // Toggle icon rotation
    toggleIcon.classList.toggle('rotate');

    // Add smooth transition
    sidebar.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';

    // Update main content margins
    const mainContent = document.querySelector('.main-content');
    if (mainContent) {
        if (sidebar.classList.contains('close')) {
            mainContent.style.marginLeft = '82px';
            mainContent.style.width = 'calc(100vw - 82px)';
        } else {
            mainContent.style.marginLeft = '250px';
            mainContent.style.width = 'calc(100vw - 250px)';
        }
    }

    // Update body class for responsive layouts
    document.body.classList.toggle('sidebar-collapsed');

    console.log('Sidebar toggled successfully');
    return true;
}

// Enhanced Mobile Dock with Premium Interactions
const dockItems = document.querySelectorAll(".dock-item");
const distance = 120;
const maxScale = 2.0;
const dockPanel = document.getElementById("dockPanel");

if (dockPanel && dockItems.length > 0) {
    // Enhanced hover effect with smoother animations
    dockPanel.addEventListener("mousemove", (e) => {
        const rect = dockPanel.getBoundingClientRect();
        const mouseX = e.clientX;

        dockItems.forEach((item) => {
            const itemRect = item.getBoundingClientRect();
            const itemCenter = itemRect.left + itemRect.width / 2;
            const dist = Math.abs(mouseX - itemCenter);
            const scale = Math.max(1, maxScale - (dist / distance));

            item.style.transform = `scale(${scale})`;
            item.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        });
    });

    // Reset scales when mouse leaves dock
    dockPanel.addEventListener("mouseleave", () => {
        dockItems.forEach((item) => {
            item.style.transform = "scale(1)";
            item.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        });
    });

    // Enhanced label animations
    dockItems.forEach((item) => {
        const label = item.querySelector(".dock-label");

        if (label) {
            item.addEventListener("mouseenter", () => {
                label.style.opacity = "1";
                label.style.transform = "translateX(-50%) translateY(-8px) scale(1.05)";
                label.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';

                // Add subtle glow effect
                item.style.boxShadow = "0 8px 25px rgba(107, 144, 128, 0.3)";
            });

            item.addEventListener("mouseleave", () => {
                label.style.opacity = "0";
                label.style.transform = "translateX(-50%) translateY(0) scale(1)";
                label.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';

                // Remove glow effect
                item.style.boxShadow = "none";
            });
        }
    });
}

// Enhanced Navigation System with Active States
class NavigationManager {
    constructor() {
        this.currentPage = this.getCurrentPage();
        this.init();
    }

    init() {
        this.setupNavigationListeners();
        this.setActiveStates();
        this.setupTokenModal();
    }

    getCurrentPage() {
        const path = window.location.pathname.toLowerCase();

        if (path.includes('/home/main') || path === '/') {
            return 'home';
        } else if (path.includes('/home/settings')) {
            return 'settings';
        } else if (path.includes('/browse')) {
            return 'browse';
        } else if (path.includes('/claimed')) {
            return 'claimed';
        } else if (path.includes('/community')) {
            return 'community';
        } else if (path.includes('/shop')) {
            return 'shop';
        } else if (path.includes('/messages')) {
            return 'message';
        } else if (path.includes('/notifications')) {
            return 'notifications';
        } else if (path.includes('/leaderboard')) {
            return 'leaderboard';
        } else if (path.includes('/quests')) {
            return 'quests';
        }

        return 'home'; // default
    }

    setupNavigationListeners() {
        // Desktop sidebar navigation
        document.querySelectorAll('#sidebar .nav-link').forEach(link => {
            link.addEventListener('click', (e) => {
                const href = link.getAttribute('href');
                const dataPage = link.getAttribute('data-page');

                // Handle special cases
                if (dataPage === 'create') {
                    e.preventDefault();
                    this.openCreateModal();
                } else if (dataPage === 'token') {
                    e.preventDefault();
                    this.showTokenModal();
                } else if (href && href !== '#') {
                    // Allow normal navigation for valid hrefs
                    this.setActiveNavItem(dataPage);
                }
            });
        });

        // Mobile dock navigation
        document.querySelectorAll('.dock-item').forEach(item => {
            item.addEventListener('click', (e) => {
                const dataPage = item.getAttribute('data-page');
                const onclickAttr = item.getAttribute('onclick');

                // Handle special cases
                if (dataPage === 'home') {
                    window.location.href = '/Home/Main';
                } else if (dataPage === 'settings') {
                    window.location.href = '/Home/Settings';
                } else if (dataPage === 'create') {
                    this.openCreateModal();
                } else if (dataPage === 'token') {
                    this.showTokenModal();
                } else if (onclickAttr) {
                    // Execute existing onclick if present
                    return;
                }

                this.setActiveNavItem(dataPage);
            });
        });
    }

    setActiveStates() {
        this.setActiveNavItem(this.currentPage);
    }

    setActiveNavItem(pageName) {
        // Desktop sidebar
        document.querySelectorAll('#sidebar .nav-link').forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('data-page') === pageName) {
                link.classList.add('active');
            }
        });

        // Mobile dock
        document.querySelectorAll('.dock-item').forEach(item => {
            item.classList.remove('active');
            if (item.getAttribute('data-page') === pageName) {
                item.classList.add('active');
            }
        });
    }

    openCreateModal() {
        // Check if page-specific function exists
        if (typeof window.openCreateModal === 'function' && window.openCreateModal !== this.openCreateModal) {
            return window.openCreateModal();
        }

        const modal = document.getElementById('createPostModal');
        if (modal) {
            modal.style.display = 'flex';
            console.log('Create modal opened from NavigationManager');
        } else {
            console.warn('Create modal not found');
        }
    }

    showTokenModal() {
        const tokenModal = document.getElementById('tokenModal');
        if (tokenModal) {
            tokenModal.classList.add('show');

            // Auto-close after 5 seconds
            setTimeout(() => {
                this.hideTokenModal();
            }, 5000);

            // Add backdrop click to close
            const closeModal = (e) => {
                if (e.target === tokenModal || !tokenModal.contains(e.target)) {
                    tokenModal.classList.remove('show');
                    document.removeEventListener('click', closeModal);
                }
            };

            setTimeout(() => {
                document.addEventListener('click', closeModal);
            }, 100);
        } else {
            console.warn('Token modal not found');
        }
    }

    hideTokenModal() {
        const tokenModal = document.getElementById('tokenModal');
        if (tokenModal) {
            tokenModal.classList.remove('show');
        }
    }

    setupTokenModal() {
        const tokenModal = document.getElementById('tokenModal');
        if (!tokenModal) return;

        // Enhanced token modal interactions
        tokenModal.addEventListener('mousemove', (e) => {
            const rect = tokenModal.getBoundingClientRect();
            tokenModal.style.setProperty('--mouse-x', `${e.clientX - rect.left}px`);
            tokenModal.style.setProperty('--mouse-y', `${e.clientY - rect.top}px`);
        });

        // Close on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && tokenModal.classList.contains('show')) {
                tokenModal.classList.remove('show');
            }
        });

        // All token triggers
        document.querySelectorAll('.open-token').forEach(trigger => {
            trigger.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.showTokenModal();
            });
        });
    }
}

// Enhanced Messaging System
document.addEventListener('DOMContentLoaded', function () {
    console.log('Site.js DOM loaded');

    // Initialize navigation manager
    window.navigationManager = new NavigationManager();

    // Enhanced messaging overlay
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');
    const newMessageBtn = document.getElementById('newMessageBtn');
    const messageItems = document.querySelectorAll('.message-item');
    const closeBtn = document.getElementById('closeAppBtn');
    const closeChatBtn = document.getElementById('closeChatBtn');

    if (newMessageBtn && appContainer) {
        // Hide both on load
        appContainer.classList.remove('active');
        if (chatWindow) chatWindow.classList.remove('active');

        // Show app-container when new-message-btn is clicked
        newMessageBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            appContainer.classList.add('active');
            if (chatWindow) chatWindow.classList.remove('active');

            // Add smooth animation
            appContainer.style.transform = 'translateX(0)';
            appContainer.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        });

        // Show chat-window when a message-item is clicked
        messageItems.forEach(function (item) {
            item.addEventListener('click', function () {
                appContainer.classList.remove('active');
                if (chatWindow) {
                    chatWindow.classList.add('active');
                    chatWindow.style.transform = 'translateX(0)';
                    chatWindow.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
                }
            });
        });

        // Close handlers with enhanced animations
        if (closeBtn) {
            closeBtn.addEventListener('click', function () {
                appContainer.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    appContainer.classList.remove('active');
                }, 300);
            });
        }

        if (closeChatBtn) {
            closeChatBtn.addEventListener('click', function () {
                if (chatWindow) {
                    chatWindow.style.transform = 'translateX(100%)';
                    setTimeout(() => {
                        chatWindow.classList.remove('active');
                        appContainer.classList.add('active');
                    }, 300);
                }
            });
        }
    }

    // Enhanced dropdown functionality
    setupEnhancedDropdowns();

    // Enhanced form interactions
    setupFormEnhancements();

    // Enhanced loading states
    setupLoadingStates();
});

// Enhanced Dropdown System
function setupEnhancedDropdowns() {
    // Global click handler for dropdowns
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown-menu')) {
            document.querySelectorAll('.dropdown-menu.active').forEach(menu => {
                menu.classList.remove('active');
                const btn = menu.querySelector('.dropdown-btn');
                if (btn) btn.setAttribute('aria-expanded', 'false');
            });
        }
    });

    // Keyboard navigation for dropdowns
    document.addEventListener('keydown', function (e) {
        const activeDropdown = document.querySelector('.dropdown-menu.active');
        if (!activeDropdown) return;

        const items = activeDropdown.querySelectorAll('.dropdown-item');
        const currentFocus = document.activeElement;

        switch (e.key) {
            case 'Escape':
                activeDropdown.classList.remove('active');
                const btn = activeDropdown.querySelector('.dropdown-btn');
                if (btn) {
                    btn.setAttribute('aria-expanded', 'false');
                    btn.focus();
                }
                break;
            case 'ArrowDown':
                e.preventDefault();
                const nextIndex = Array.from(items).indexOf(currentFocus) + 1;
                if (items[nextIndex]) items[nextIndex].focus();
                else if (items[0]) items[0].focus();
                break;
            case 'ArrowUp':
                e.preventDefault();
                const prevIndex = Array.from(items).indexOf(currentFocus) - 1;
                if (items[prevIndex]) items[prevIndex].focus();
                else if (items[items.length - 1]) items[items.length - 1].focus();
                break;
        }
    });
}

// Enhanced Form Interactions
function setupFormEnhancements() {
    // Enhanced input focus effects
    document.querySelectorAll('.input-form, .post-input').forEach(input => {
        input.addEventListener('focus', function () {
            this.parentElement.classList.add('focused');
        });

        input.addEventListener('blur', function () {
            this.parentElement.classList.remove('focused');
        });

        // Auto-resize textareas
        if (input.tagName === 'TEXTAREA') {
            input.addEventListener('input', function () {
                this.style.height = 'auto';
                this.style.height = this.scrollHeight + 'px';
            });
        }
    });

    // Enhanced button interactions
    document.querySelectorAll('.post-btn, .interested-btn, .floating-add-btn').forEach(btn => {
        btn.addEventListener('mousedown', function () {
            this.style.transform = 'scale(0.95)';
        });

        btn.addEventListener('mouseup', function () {
            this.style.transform = '';
        });

        btn.addEventListener('mouseleave', function () {
            this.style.transform = '';
        });
    });
}

// Enhanced Loading States
function setupLoadingStates() {
    // Add loading shimmer to images while they load
    document.querySelectorAll('img').forEach(img => {
        if (!img.complete) {
            img.classList.add('loading-state');

            img.addEventListener('load', function () {
                this.classList.remove('loading-state');
            });
        }
    });

    // Enhanced form submission loading
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function () {
            const submitBtn = this.querySelector('[type="submit"], .post-btn');
            if (submitBtn && !submitBtn.disabled) {
                showButtonLoading(submitBtn);
            }
        });
    });
}

// Utility Functions
function showButtonLoading(button) {
    const originalText = button.textContent;
    const spinner = button.querySelector('.loading-spinner');

    button.disabled = true;

    if (spinner) {
        spinner.style.display = 'inline-block';
    } else {
        button.innerHTML = '<span class="loading-spinner" style="display: inline-block;"></span> Loading...';
    }

    button.classList.add('loading');
}

function hideButtonLoading(button, originalText = 'Submit') {
    button.disabled = false;
    button.classList.remove('loading');

    const spinner = button.querySelector('.loading-spinner');
    if (spinner) {
        spinner.style.display = 'none';
        button.textContent = originalText;
    } else {
        button.innerHTML = originalText;
    }
}

// FIXED: Enhanced notification system
function showNotification(message, type = 'info', duration = 3000) {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(n => n.remove());

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        border-radius: 12px;
        color: white;
        font-weight: 600;
        z-index: 10000;
        max-width: 400px;
        box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2);
        backdrop-filter: blur(10px);
        transform: translateX(100%);
        transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        display: flex;
        align-items: center;
        gap: 12px;
    `;

    // Set colors based on type
    const colors = {
        success: 'linear-gradient(135deg, #10b981, #059669)',
        error: 'linear-gradient(135deg, #ef4444, #dc2626)',
        warning: 'linear-gradient(135deg, #f59e0b, #d97706)',
        info: 'linear-gradient(135deg, #3b82f6, #2563eb)'
    };

    const icons = {
        success: '<i class="bx bx-check-circle"></i>',
        error: '<i class="bx bx-error-circle"></i>',
        warning: '<i class="bx bx-error"></i>',
        info: '<i class="bx bx-info-circle"></i>'
    };

    notification.style.background = colors[type] || colors.info;
    notification.innerHTML = `${icons[type] || icons.info}<span>${message}</span>`;

    document.body.appendChild(notification);

    // Animate in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);

    // Auto-remove
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, duration);

    return notification;
}

// Navigation functions
function navigateToPage(url) {
    if (url && url !== '#') {
        window.location.href = url;
    } else {
        console.warn('Invalid navigation URL:', url);
    }
}

function setActiveNavItem(pageName) {
    // Desktop sidebar
    document.querySelectorAll('#sidebar .nav-link').forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('data-page') === pageName) {
            link.classList.add('active');
        }
    });

    // Mobile dock
    document.querySelectorAll('.dock-item').forEach(item => {
        item.classList.remove('active');
        if (item.getAttribute('data-page') === pageName) {
            item.classList.add('active');
        }
    });
}

// FIXED: Token modal functions
function showTokenModal() {
    console.log('showTokenModal called from site.js');

    const tokenModal = document.getElementById('tokenModal');
    if (!tokenModal) {
        console.error('Token modal not found');
        return false;
    }

    tokenModal.classList.add('show');
    console.log('Token modal shown');

    // Auto-close after 5 seconds
    setTimeout(() => {
        hideTokenModal();
    }, 5000);

    return true;
}

function hideTokenModal() {
    const tokenModal = document.getElementById('tokenModal');
    if (tokenModal) {
        tokenModal.classList.remove('show');
    }
}

// FIXED: Create modal function
function openCreateModal() {
    console.log('openCreateModal called from site.js');

    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'flex';
        setTimeout(() => modal.classList.add('show'), 10);
        console.log('Create modal opened');
    } else {
        console.warn('Create modal not found');
    }
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

function throttle(func, limit) {
    let inThrottle;
    return function () {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    }
}

// Smooth scrolling for better UX
function smoothScrollTo(target, duration = 1000) {
    const targetElement = typeof target === 'string' ? document.querySelector(target) : target;
    if (!targetElement) return;

    const targetPosition = targetElement.getBoundingClientRect().top + window.pageYOffset;
    const startPosition = window.pageYOffset;
    const distance = targetPosition - startPosition;
    let startTime = null;

    function animation(currentTime) {
        if (startTime === null) startTime = currentTime;
        const timeElapsed = currentTime - startTime;
        const run = easeInOutQuad(timeElapsed, startPosition, distance, duration);
        window.scrollTo(0, run);
        if (timeElapsed < duration) requestAnimationFrame(animation);
    }

    function easeInOutQuad(t, b, c, d) {
        t /= d / 2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t - 2) - 1) + b;
    }

    requestAnimationFrame(animation);
}

// Export functions for global use
window.toggleSidebar = toggleSidebar;
window.showTokenModal = showTokenModal;
window.hideTokenModal = hideTokenModal;
window.openCreateModal = openCreateModal;
window.showNotification = showNotification;
window.showButtonLoading = showButtonLoading;
window.hideButtonLoading = hideButtonLoading;
window.smoothScrollTo = smoothScrollTo;
window.debounce = debounce;
window.throttle = throttle;
window.navigateToPage = navigateToPage;
window.setActiveNavItem = setActiveNavItem;

console.log('Site.js loaded successfully');