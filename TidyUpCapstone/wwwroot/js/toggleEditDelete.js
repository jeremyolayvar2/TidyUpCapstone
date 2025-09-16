// Enhanced toggleEditDelete.js - Premium UI with Improved Consistency & Animations
// ============================================================================

// Enhanced dropdown functionality with premium interactions
function toggleDropdown(button) {
    const dropdown = button.closest('.dropdown-menu');
    const dropdownContent = dropdown.querySelector('.dropdown-content');
    const isActive = dropdown.classList.contains('active');

    // Close all other dropdowns first with smooth animations
    document.querySelectorAll('.dropdown-menu.active').forEach(menu => {
        if (menu !== dropdown) {
            menu.classList.remove('active');
            const btn = menu.querySelector('.dropdown-btn');
            if (btn) {
                btn.setAttribute('aria-expanded', 'false');
                // Reset button styles
                btn.style.transform = '';
                const img = btn.querySelector('img');
                if (img) img.style.transform = '';
            }
        }
    });

    // Toggle current dropdown with enhanced animations
    if (!isActive) {
        dropdown.classList.add('active');
        button.setAttribute('aria-expanded', 'true');

        // Enhanced button animation
        button.style.transform = 'scale(1.05)';
        const img = button.querySelector('img');
        if (img) {
            img.style.transform = 'rotate(90deg)';
            img.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        }

        // Enhanced dropdown entrance animation
        if (dropdownContent) {
            dropdownContent.style.animation = 'dropdownSlideIn 0.4s cubic-bezier(0.4, 0, 0.2, 1)';
        }

        // Add backdrop blur effect
        setTimeout(() => {
            const backdrop = document.createElement('div');
            backdrop.className = 'dropdown-backdrop';
            backdrop.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.1);
                backdrop-filter: blur(2px);
                z-index: 999;
                opacity: 0;
                transition: opacity 0.3s ease;
            `;
            document.body.appendChild(backdrop);

            requestAnimationFrame(() => {
                backdrop.style.opacity = '1';
            });

            backdrop.addEventListener('click', () => {
                toggleDropdown(button);
            });
        }, 50);

        console.log('📋 Enhanced dropdown opened with premium animations');
    } else {
        dropdown.classList.remove('active');
        button.setAttribute('aria-expanded', 'false');

        // Reset button styles
        button.style.transform = '';
        const img = button.querySelector('img');
        if (img) {
            img.style.transform = '';
        }

        // Remove backdrop
        const backdrop = document.querySelector('.dropdown-backdrop');
        if (backdrop) {
            backdrop.style.opacity = '0';
            setTimeout(() => {
                if (backdrop.parentNode) {
                    document.body.removeChild(backdrop);
                }
            }, 300);
        }

        console.log('📋 Enhanced dropdown closed');
    }
}

// Enhanced click outside handler with better performance
let dropdownClickHandler = null;
let isDropdownHandlerActive = false;

function setupEnhancedDropdownHandler() {
    if (isDropdownHandlerActive) return;

    dropdownClickHandler = function (event) {
        // Performance optimization: only check if there are active dropdowns
        const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');
        if (activeDropdowns.length === 0) return;

        // Check if click is outside any dropdown
        if (!event.target.closest('.dropdown-menu')) {
            activeDropdowns.forEach(menu => {
                menu.classList.remove('active');
                const button = menu.querySelector('.dropdown-btn');
                if (button) {
                    button.setAttribute('aria-expanded', 'false');
                    // Reset button animations
                    button.style.transform = '';
                    const img = button.querySelector('img');
                    if (img) img.style.transform = '';
                }
            });

            // Remove backdrop
            const backdrop = document.querySelector('.dropdown-backdrop');
            if (backdrop) {
                backdrop.style.opacity = '0';
                setTimeout(() => {
                    if (backdrop.parentNode) {
                        document.body.removeChild(backdrop);
                    }
                }, 300);
            }

            if (activeDropdowns.length > 0) {
                console.log('📋 Enhanced dropdowns closed (click outside)');
            }
        }
    };

    document.addEventListener('click', dropdownClickHandler, { passive: true });
    isDropdownHandlerActive = true;
}

// Enhanced keyboard navigation with better accessibility
function setupEnhancedKeyboardNavigation() {
    document.addEventListener('keydown', function (event) {
        const activeDropdown = document.querySelector('.dropdown-menu.active');
        if (!activeDropdown) return;

        const dropdownItems = activeDropdown.querySelectorAll('.dropdown-item');
        const currentFocus = document.activeElement;

        switch (event.key) {
            case 'Escape':
                event.preventDefault();
                closeAllDropdowns();
                break;

            case 'ArrowDown':
                event.preventDefault();
                navigateDropdownItems(dropdownItems, currentFocus, 'next');
                break;

            case 'ArrowUp':
                event.preventDefault();
                navigateDropdownItems(dropdownItems, currentFocus, 'prev');
                break;

            case 'Enter':
            case ' ':
                if (currentFocus && currentFocus.classList.contains('dropdown-item')) {
                    event.preventDefault();
                    // Add ripple effect before click
                    addRippleEffect(currentFocus);
                    setTimeout(() => currentFocus.click(), 100);
                }
                break;

            case 'Tab':
                // Close dropdown when tabbing away
                closeAllDropdowns();
                break;
        }
    });
}

function navigateDropdownItems(items, currentFocus, direction) {
    const currentIndex = Array.from(items).indexOf(currentFocus);
    let nextIndex;

    if (direction === 'next') {
        nextIndex = currentIndex < items.length - 1 ? currentIndex + 1 : 0;
    } else {
        nextIndex = currentIndex > 0 ? currentIndex - 1 : items.length - 1;
    }

    if (items[nextIndex]) {
        items[nextIndex].focus();
        // Add focus highlight animation
        items[nextIndex].style.transform = 'translateX(8px)';
        setTimeout(() => {
            items[nextIndex].style.transform = '';
        }, 200);
    }
}

function closeAllDropdowns() {
    const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');
    activeDropdowns.forEach(menu => {
        menu.classList.remove('active');
        const button = menu.querySelector('.dropdown-btn');
        if (button) {
            button.setAttribute('aria-expanded', 'false');
            button.focus();
            // Reset animations
            button.style.transform = '';
            const img = button.querySelector('img');
            if (img) img.style.transform = '';
        }
    });

    // Remove backdrop
    const backdrop = document.querySelector('.dropdown-backdrop');
    if (backdrop) {
        backdrop.style.opacity = '0';
        setTimeout(() => {
            if (backdrop.parentNode) {
                document.body.removeChild(backdrop);
            }
        }, 300);
    }
}

// Enhanced ripple effect for better user feedback
function addRippleEffect(element) {
    const ripple = document.createElement('div');
    const rect = element.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);

    ripple.style.cssText = `
        position: absolute;
        width: ${size}px;
        height: ${size}px;
        border-radius: 50%;
        background: rgba(107, 144, 128, 0.3);
        transform: scale(0);
        animation: rippleEffect 0.6s ease-out;
        pointer-events: none;
        top: 50%;
        left: 50%;
        margin-top: -${size / 2}px;
        margin-left: -${size / 2}px;
    `;

    element.style.position = 'relative';
    element.style.overflow = 'hidden';
    element.appendChild(ripple);

    setTimeout(() => {
        if (ripple.parentNode) {
            ripple.parentNode.removeChild(ripple);
        }
    }, 600);
}

// Enhanced dropdown initialization with premium features
function initializeEnhancedDropdowns() {
    console.log('🚀 Initializing premium dropdown system...');

    const dropdownButtons = document.querySelectorAll('.dropdown-btn');

    dropdownButtons.forEach(button => {
        // Add enhanced ARIA attributes
        button.setAttribute('aria-haspopup', 'true');
        button.setAttribute('aria-expanded', 'false');
        button.setAttribute('role', 'button');

        // Add enhanced keyboard support
        button.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                addRippleEffect(button);
                setTimeout(() => toggleDropdown(button), 100);
            }
        });

        // Enhanced hover effects
        button.addEventListener('mouseenter', function () {
            if (!this.closest('.dropdown-menu').classList.contains('active')) {
                this.style.transform = 'translateY(-1px) scale(1.02)';
                this.style.boxShadow = '0 6px 20px rgba(107, 144, 128, 0.15)';
            }
        });

        button.addEventListener('mouseleave', function () {
            if (!this.closest('.dropdown-menu').classList.contains('active')) {
                this.style.transform = '';
                this.style.boxShadow = '';
            }
        });

        // Ensure proper button type
        if (!button.type) {
            button.type = 'button';
        }
    });

    // Setup enhanced dropdown items with premium interactions
    const dropdownItems = document.querySelectorAll('.dropdown-item');
    dropdownItems.forEach(item => {
        item.setAttribute('role', 'menuitem');
        item.setAttribute('tabindex', '-1');

        // Enhanced focus styling
        item.addEventListener('focus', function () {
            this.style.outline = '2px solid var(--primary-color)';
            this.style.outlineOffset = '2px';
            this.style.transform = 'translateX(4px)';
        });

        item.addEventListener('blur', function () {
            this.style.outline = '';
            this.style.outlineOffset = '';
            this.style.transform = '';
        });

        // Enhanced hover effects with sound feedback (if supported)
        item.addEventListener('mouseenter', function () {
            this.style.transform = 'translateX(6px)';
            this.style.background = 'rgba(107, 144, 128, 0.1)';

            // Subtle hover sound (modern browsers)
            if ('AudioContext' in window && this.getAttribute('data-sound-enabled') !== 'false') {
                playHoverSound();
            }
        });

        item.addEventListener('mouseleave', function () {
            this.style.transform = '';
            this.style.background = '';
        });

        // Click animation
        item.addEventListener('mousedown', function () {
            addRippleEffect(this);
        });
    });

    console.log('✅ Premium dropdown system initialized with enhanced features');
}

// Enhanced edit function with premium loading states
function editItem(itemId) {
    if (!itemId) {
        console.error('❌ No item ID provided for edit');
        showEnhancedNotification('No item selected for editing', 'error');
        return;
    }

    console.log('✏️ Enhanced edit requested for item:', itemId);

    // Close any open dropdowns first
    closeAllDropdowns();

    // Check if openEditModal function exists
    if (typeof window.openEditModal === 'function') {
        try {
            // Show premium loading indicator
            showPremiumLoader('Loading item details...');

            // Add delay for smooth UX
            setTimeout(() => {
                window.openEditModal(itemId);
                hidePremiumLoader();
            }, 300);

        } catch (error) {
            console.error('❌ Error opening edit modal:', error);
            hidePremiumLoader();
            showEnhancedNotification('Failed to open edit dialog. Please try again.', 'error');
        }
    } else {
        console.error('❌ openEditModal function not found');
        showEnhancedNotification('Edit functionality is not available.', 'error');
    }
}

// Enhanced delete function with premium confirmation
async function deleteItemEnhanced(itemId) {
    if (!itemId) {
        console.error('❌ No item ID provided for delete');
        showEnhancedNotification('No item selected for deletion', 'error');
        return;
    }

    console.log('🗑️ Enhanced delete requested for item:', itemId);

    // Close any open dropdowns first
    closeAllDropdowns();

    // Show premium confirmation dialog
    const confirmed = await showPremiumConfirmDialog({
        title: 'Delete Item',
        message: 'Are you sure you want to permanently delete this item? This action cannot be undone.',
        confirmText: 'Delete Forever',
        cancelText: 'Keep Item',
        type: 'danger',
        icon: 'trash'
    });

    if (!confirmed) return;

    // Check if deleteItem function exists
    if (typeof window.deleteItem === 'function') {
        try {
            showPremiumLoader('Deleting item...');
            await window.deleteItem(itemId);
            hidePremiumLoader();
        } catch (error) {
            console.error('❌ Error deleting item:', error);
            hidePremiumLoader();
            showEnhancedNotification('Failed to delete item. Please try again.', 'error');
        }
    } else {
        console.error('❌ deleteItem function not found');
        showEnhancedNotification('Delete functionality is not available.', 'error');
    }
}

// Enhanced report function with premium interface
async function reportItemEnhanced(itemId) {
    if (!itemId) {
        console.error('❌ No item ID provided for report');
        showEnhancedNotification('No item selected for reporting', 'error');
        return;
    }

    console.log('🚩 Enhanced report requested for item:', itemId);

    // Close any open dropdowns first
    closeAllDropdowns();

    // Check if reportItem function exists
    if (typeof window.reportItem === 'function') {
        try {
            window.reportItem(itemId);
        } catch (error) {
            console.error('❌ Error reporting item:', error);
            showEnhancedNotification('Failed to report item. Please try again.', 'error');
        }
    } else {
        // Show premium report dialog
        await showPremiumReportDialog(itemId);
    }
}

// Premium confirmation dialog
function showPremiumConfirmDialog({ title, message, confirmText, cancelText, type, icon }) {
    return new Promise((resolve) => {
        const overlay = document.createElement('div');
        overlay.className = 'premium-dialog-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.6);
            backdrop-filter: blur(12px);
            z-index: 15000;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;

        const dialog = document.createElement('div');
        dialog.className = 'premium-dialog';
        dialog.style.cssText = `
            background: white;
            border-radius: 20px;
            padding: 32px;
            max-width: 450px;
            width: 100%;
            box-shadow: 0 30px 80px rgba(0, 0, 0, 0.3);
            transform: scale(0.9) translateY(20px);
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
            text-align: center;
        `;

        const colors = {
            primary: '#6B9080',
            danger: '#dc3545',
            warning: '#ffc107',
            success: '#28a745'
        };

        const icons = {
            trash: 'bx-trash',
            flag: 'bx-flag',
            warning: 'bx-error',
            info: 'bx-info-circle'
        };

        dialog.innerHTML = `
            <div class="dialog-icon" style="
                width: 64px;
                height: 64px;
                background: ${type === 'danger' ? 'linear-gradient(135deg, #ef4444, #dc2626)' : colors[type] || colors.primary};
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                margin: 0 auto 24px;
                color: white;
                font-size: 28px;
                box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
            ">
                <i class='bx ${icons[icon] || icons.info}'></i>
            </div>
            <h3 style="margin: 0 0 12px; color: #333; font-size: 24px; font-weight: 700;">${title}</h3>
            <p style="margin: 0 0 32px; color: #666; font-size: 16px; line-height: 1.5;">${message}</p>
            <div style="display: flex; gap: 16px; justify-content: center;">
                <button class="cancel-btn" style="
                    padding: 14px 28px;
                    border: 2px solid #e2e8f0;
                    background: white;
                    color: #64748b;
                    border-radius: 12px;
                    cursor: pointer;
                    font-weight: 600;
                    font-size: 15px;
                    transition: all 0.3s ease;
                    min-width: 120px;
                ">${cancelText}</button>
                <button class="confirm-btn" style="
                    padding: 14px 28px;
                    border: none;
                    background: ${type === 'danger' ? 'linear-gradient(135deg, #ef4444, #dc2626)' : colors[type] || colors.primary};
                    color: white;
                    border-radius: 12px;
                    cursor: pointer;
                    font-weight: 600;
                    font-size: 15px;
                    transition: all 0.3s ease;
                    min-width: 120px;
                    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                ">${confirmText}</button>
            </div>
        `;

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        // Animate in
        requestAnimationFrame(() => {
            overlay.style.opacity = '1';
            dialog.style.transform = 'scale(1) translateY(0)';
        });

        const cleanup = () => {
            overlay.style.opacity = '0';
            dialog.style.transform = 'scale(0.9) translateY(20px)';
            setTimeout(() => {
                if (overlay.parentNode) {
                    document.body.removeChild(overlay);
                }
            }, 300);
        };

        // Event handlers
        dialog.querySelector('.cancel-btn').onclick = () => {
            cleanup();
            resolve(false);
        };

        dialog.querySelector('.confirm-btn').onclick = () => {
            cleanup();
            resolve(true);
        };

        overlay.onclick = (e) => {
            if (e.target === overlay) {
                cleanup();
                resolve(false);
            }
        };

        // Keyboard support
        const keyHandler = (e) => {
            if (e.key === 'Escape') {
                cleanup();
                resolve(false);
                document.removeEventListener('keydown', keyHandler);
            }
        };
        document.addEventListener('keydown', keyHandler);
    });
}

// Premium loader functions
function showPremiumLoader(message = 'Loading...') {
    const loader = document.createElement('div');
    loader.className = 'premium-loader';
    loader.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.6);
        backdrop-filter: blur(8px);
        z-index: 20000;
        display: flex;
        align-items: center;
        justify-content: center;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;

    loader.innerHTML = `
        <div style="
            background: white;
            border-radius: 16px;
            padding: 32px;
            text-align: center;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
        ">
            <div class="spinner" style="
                width: 40px;
                height: 40px;
                border: 4px solid rgba(107, 144, 128, 0.2);
                border-top: 4px solid var(--primary-color);
                border-radius: 50%;
                animation: spin 1s linear infinite;
                margin: 0 auto 16px;
            "></div>
            <p style="color: #666; margin: 0; font-weight: 500;">${message}</p>
        </div>
    `;

    document.body.appendChild(loader);
    requestAnimationFrame(() => {
        loader.style.opacity = '1';
    });
}

function hidePremiumLoader() {
    const loader = document.querySelector('.premium-loader');
    if (loader) {
        loader.style.opacity = '0';
        setTimeout(() => {
            if (loader.parentNode) {
                document.body.removeChild(loader);
            }
        }, 300);
    }
}

// Enhanced notification system
function showEnhancedNotification(message, type = 'info') {
    if (typeof window.showNotification === 'function') {
        window.showNotification(message, type);
    } else {
        // Fallback enhanced notification
        console.log(`${type.toUpperCase()}: ${message}`);
        alert(message);
    }
}

// Subtle hover sound effect (optional)
function playHoverSound() {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.1);
    } catch (e) {
        // Silently fail if audio context is not supported
    }
}

// Enhanced initialization
function initializePremiumDropdowns() {
    console.log('🚀 Initializing premium dropdown system...');

    // Setup main functionality
    setupEnhancedDropdownHandler();
    setupEnhancedKeyboardNavigation();
    initializeEnhancedDropdowns();

    // Add touch support for mobile with enhanced gestures
    if ('ontouchstart' in window) {
        let touchStartY = 0;
        let touchStartTime = 0;

        document.addEventListener('touchstart', function (e) {
            touchStartY = e.touches[0].clientY;
            touchStartTime = Date.now();

            if (!e.target.closest('.dropdown-menu')) {
                const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');
                activeDropdowns.forEach(menu => {
                    menu.classList.remove('active');
                    const button = menu.querySelector('.dropdown-btn');
                    if (button) {
                        button.setAttribute('aria-expanded', 'false');
                        button.style.transform = '';
                        const img = button.querySelector('img');
                        if (img) img.style.transform = '';
                    }
                });
            }
        }, { passive: true });

        // Enhanced swipe to close
        document.addEventListener('touchmove', function (e) {
            const touchCurrentY = e.touches[0].clientY;
            const swipeDistance = touchStartY - touchCurrentY;
            const swipeTime = Date.now() - touchStartTime;

            if (Math.abs(swipeDistance) > 50 && swipeTime < 300) {
                closeAllDropdowns();
            }
        }, { passive: true });
    }

    // Add premium CSS animations if not present
    if (!document.querySelector('#premium-dropdown-animations')) {
        const style = document.createElement('style');
        style.id = 'premium-dropdown-animations';
        style.textContent = `
            @keyframes dropdownSlideIn {
                0% {
                    opacity: 0;
                    transform: scale(0.9) translateY(-15px);
                }
                60% {
                    opacity: 0.9;
                    transform: scale(1.02) translateY(-2px);
                }
                100% {
                    opacity: 1;
                    transform: scale(1) translateY(0);
                }
            }
            
            @keyframes rippleEffect {
                0% {
                    transform: scale(0);
                    opacity: 1;
                }
                100% {
                    transform: scale(2);
                    opacity: 0;
                }
            }
            
            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
            
            .dropdown-btn:focus,
            .dropdown-item:focus {
                outline: 2px solid var(--primary-color);
                outline-offset: 2px;
            }
            
            .premium-dialog:hover {
                transform: scale(1) translateY(-2px);
            }
            
            .cancel-btn:hover {
                background: #f1f5f9 !important;
                border-color: #cbd5e1 !important;
                transform: translateY(-1px);
            }
            
            .confirm-btn:hover {
                transform: translateY(-2px);
                box-shadow: 0 6px 20px rgba(0, 0, 0, 0.2) !important;
            }
        `;
        document.head.appendChild(style);
    }

    console.log('✅ Premium dropdown system initialized with advanced features');
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializePremiumDropdowns();
});

// Also initialize if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializePremiumDropdowns);
} else {
    initializePremiumDropdowns();
}

// Global exports for backward compatibility
window.toggleDropdown = toggleDropdown;
window.editItem = editItem;
window.deleteItemEnhanced = deleteItemEnhanced;
window.reportItemEnhanced = reportItemEnhanced;
window.showPremiumConfirmDialog = showPremiumConfirmDialog;
window.showPremiumLoader = showPremiumLoader;
window.hidePremiumLoader = hidePremiumLoader;

console.log('✅ Premium toggleEditDelete.js loaded with enhanced UI consistency and animations');