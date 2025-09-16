// Enhanced toggleEditDelete.js - Improved Dropdown UX & Consistency
// ============================================================================

// Enhanced dropdown functionality with better UX
function toggleDropdown(button) {
    const dropdown = button.closest('.dropdown-menu');
    const dropdownContent = dropdown.querySelector('.dropdown-content');
    const isActive = dropdown.classList.contains('active');

    // Close all other dropdowns first
    document.querySelectorAll('.dropdown-menu.active').forEach(menu => {
        if (menu !== dropdown) {
            menu.classList.remove('active');
        }
    });

    // Toggle current dropdown with animation
    if (!isActive) {
        dropdown.classList.add('active');

        // Add entrance animation
        if (dropdownContent) {
            dropdownContent.style.animation = 'dropdownSlide 0.3s ease-out';
        }

        // Add accessibility attributes
        button.setAttribute('aria-expanded', 'true');

        console.log('📋 Dropdown opened');
    } else {
        dropdown.classList.remove('active');
        button.setAttribute('aria-expanded', 'false');

        console.log('📋 Dropdown closed');
    }
}

// Enhanced click outside handler with better performance
let dropdownClickHandler = null;

function setupDropdownHandler() {
    // Remove existing handler to prevent duplicates
    if (dropdownClickHandler) {
        document.removeEventListener('click', dropdownClickHandler);
    }

    dropdownClickHandler = function (event) {
        // Check if click is outside any dropdown
        if (!event.target.closest('.dropdown-menu')) {
            const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');

            activeDropdowns.forEach(menu => {
                menu.classList.remove('active');
                const button = menu.querySelector('.dropdown-btn');
                if (button) {
                    button.setAttribute('aria-expanded', 'false');
                }
            });

            if (activeDropdowns.length > 0) {
                console.log('📋 Dropdowns closed (click outside)');
            }
        }
    };

    document.addEventListener('click', dropdownClickHandler);
}

// Enhanced keyboard navigation for dropdowns
function setupKeyboardNavigation() {
    document.addEventListener('keydown', function (event) {
        const activeDropdown = document.querySelector('.dropdown-menu.active');

        if (!activeDropdown) return;

        const dropdownItems = activeDropdown.querySelectorAll('.dropdown-item');
        const currentFocus = document.activeElement;

        switch (event.key) {
            case 'Escape':
                activeDropdown.classList.remove('active');
                const button = activeDropdown.querySelector('.dropdown-btn');
                if (button) {
                    button.setAttribute('aria-expanded', 'false');
                    button.focus();
                }
                event.preventDefault();
                break;

            case 'ArrowDown':
                event.preventDefault();
                if (dropdownItems.length > 0) {
                    const currentIndex = Array.from(dropdownItems).indexOf(currentFocus);
                    const nextIndex = currentIndex < dropdownItems.length - 1 ? currentIndex + 1 : 0;
                    dropdownItems[nextIndex].focus();
                }
                break;

            case 'ArrowUp':
                event.preventDefault();
                if (dropdownItems.length > 0) {
                    const currentIndex = Array.from(dropdownItems).indexOf(currentFocus);
                    const prevIndex = currentIndex > 0 ? currentIndex - 1 : dropdownItems.length - 1;
                    dropdownItems[prevIndex].focus();
                }
                break;

            case 'Enter':
            case ' ':
                if (currentFocus && currentFocus.classList.contains('dropdown-item')) {
                    event.preventDefault();
                    currentFocus.click();
                }
                break;
        }
    });
}

// Enhanced dropdown initialization with accessibility
function initializeDropdowns() {
    const dropdownButtons = document.querySelectorAll('.dropdown-btn');

    dropdownButtons.forEach(button => {
        // Add ARIA attributes
        button.setAttribute('aria-haspopup', 'true');
        button.setAttribute('aria-expanded', 'false');

        // Add keyboard support
        button.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                toggleDropdown(button);
            }
        });

        // Ensure proper button type
        if (!button.type) {
            button.type = 'button';
        }
    });

    // Setup dropdown items with proper ARIA roles
    const dropdownItems = document.querySelectorAll('.dropdown-item');
    dropdownItems.forEach(item => {
        item.setAttribute('role', 'menuitem');
        item.setAttribute('tabindex', '-1');

        // Add focus styling
        item.addEventListener('focus', function () {
            this.style.outline = '2px solid var(--primary-color)';
            this.style.outlineOffset = '-2px';
        });

        item.addEventListener('blur', function () {
            this.style.outline = '';
            this.style.outlineOffset = '';
        });
    });

    console.log('📋 Enhanced dropdowns initialized with accessibility features');
}

// Enhanced edit function with better error handling
function editItem(itemId) {
    if (!itemId) {
        console.error('❌ No item ID provided for edit');
        return;
    }

    console.log('✏️ Edit requested for item:', itemId);

    // Check if openEditModal function exists
    if (typeof window.openEditModal === 'function') {
        try {
            window.openEditModal(itemId);
        } catch (error) {
            console.error('❌ Error opening edit modal:', error);
            showNotificationMessage('Failed to open edit dialog. Please try again.', 'error');
        }
    } else {
        console.error('❌ openEditModal function not found');
        showNotificationMessage('Edit functionality is not available.', 'error');
    }
}

// Enhanced delete function with better confirmation
function deleteItemEnhanced(itemId) {
    if (!itemId) {
        console.error('❌ No item ID provided for delete');
        return;
    }

    console.log('🗑️ Delete requested for item:', itemId);

    // Check if deleteItem function exists
    if (typeof window.deleteItem === 'function') {
        try {
            window.deleteItem(itemId);
        } catch (error) {
            console.error('❌ Error deleting item:', error);
            showNotificationMessage('Failed to delete item. Please try again.', 'error');
        }
    } else {
        console.error('❌ deleteItem function not found');
        showNotificationMessage('Delete functionality is not available.', 'error');
    }
}

// Enhanced report function
function reportItemEnhanced(itemId) {
    if (!itemId) {
        console.error('❌ No item ID provided for report');
        return;
    }

    console.log('🚩 Report requested for item:', itemId);

    // Check if reportItem function exists
    if (typeof window.reportItem === 'function') {
        try {
            window.reportItem(itemId);
        } catch (error) {
            console.error('❌ Error reporting item:', error);
            showNotificationMessage('Failed to report item. Please try again.', 'error');
        }
    } else {
        // Fallback report functionality
        showReportDialog(itemId);
    }
}

// Fallback report dialog
function showReportDialog(itemId) {
    const reasons = [
        { value: 'inappropriate', text: 'Inappropriate content' },
        { value: 'spam', text: 'Spam or misleading' },
        { value: 'fraud', text: 'Fraudulent listing' },
        { value: 'copyright', text: 'Copyright violation' },
        { value: 'other', text: 'Other reason' }
    ];

    // Create modal overlay
    const overlay = document.createElement('div');
    overlay.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.6);
        backdrop-filter: blur(8px);
        z-index: 15000;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 20px;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;

    // Create modal content
    const modal = document.createElement('div');
    modal.style.cssText = `
        background: white;
        border-radius: 16px;
        padding: 24px;
        max-width: 400px;
        width: 100%;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
        transform: scale(0.9);
        transition: transform 0.3s ease;
    `;

    modal.innerHTML = `
        <div style="text-align: center; margin-bottom: 20px;">
            <div style="
                width: 48px;
                height: 48px;
                background: linear-gradient(135deg, #ef4444, #dc2626);
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                margin: 0 auto 16px;
                color: white;
                font-size: 24px;
            ">
                <i class='bx bx-flag'></i>
            </div>
            <h3 style="margin: 0 0 8px; color: #333; font-size: 20px;">Report Item</h3>
            <p style="margin: 0; color: #666; font-size: 14px;">Help us keep the community safe by reporting inappropriate content.</p>
        </div>
        
        <div style="margin-bottom: 20px;">
            <label style="display: block; margin-bottom: 8px; font-weight: 600; color: #333;">
                Why are you reporting this item?
            </label>
            <select id="reportReason" style="
                width: 100%;
                padding: 12px;
                border: 2px solid #ddd;
                border-radius: 8px;
                font-size: 14px;
                background: white;
                color: #333;
            ">
                <option value="">Select a reason...</option>
                ${reasons.map(reason => `<option value="${reason.value}">${reason.text}</option>`).join('')}
            </select>
        </div>
        
        <div style="display: flex; gap: 12px; justify-content: flex-end;">
            <button id="cancelReport" style="
                padding: 12px 20px;
                border: 2px solid #ddd;
                background: white;
                color: #666;
                border-radius: 8px;
                cursor: pointer;
                font-weight: 600;
                transition: all 0.3s ease;
            ">Cancel</button>
            <button id="submitReport" style="
                padding: 12px 20px;
                border: none;
                background: linear-gradient(135deg, #ef4444, #dc2626);
                color: white;
                border-radius: 8px;
                cursor: pointer;
                font-weight: 600;
                transition: all 0.3s ease;
            " disabled>Submit Report</button>
        </div>
    `;

    overlay.appendChild(modal);
    document.body.appendChild(overlay);

    // Animate in
    setTimeout(() => {
        overlay.style.opacity = '1';
        modal.style.transform = 'scale(1)';
    }, 10);

    // Handle form interactions
    const reasonSelect = modal.querySelector('#reportReason');
    const submitBtn = modal.querySelector('#submitReport');
    const cancelBtn = modal.querySelector('#cancelReport');

    reasonSelect.addEventListener('change', function () {
        submitBtn.disabled = !this.value;
        submitBtn.style.opacity = this.value ? '1' : '0.5';
    });

    const cleanup = () => {
        overlay.style.opacity = '0';
        modal.style.transform = 'scale(0.9)';
        setTimeout(() => {
            if (overlay.parentNode) {
                document.body.removeChild(overlay);
            }
        }, 300);
    };

    cancelBtn.addEventListener('click', cleanup);

    submitBtn.addEventListener('click', async function () {
        const reason = reasonSelect.value;
        if (!reason) return;

        this.disabled = true;
        this.innerHTML = '<i class="bx bx-loader-alt" style="animation: spin 1s linear infinite;"></i> Submitting...';

        try {
            // Send report (you would implement the actual API call here)
            await new Promise(resolve => setTimeout(resolve, 1000)); // Simulate API call

            cleanup();
            showNotificationMessage('Thank you for helping keep our community safe.', 'success');

        } catch (error) {
            showNotificationMessage('Failed to submit report. Please try again.', 'error');
            this.disabled = false;
            this.innerHTML = 'Submit Report';
        }
    });

    // Close on overlay click
    overlay.addEventListener('click', function (e) {
        if (e.target === overlay) {
            cleanup();
        }
    });

    // Close on escape key
    const escapeHandler = function (e) {
        if (e.key === 'Escape') {
            cleanup();
            document.removeEventListener('keydown', escapeHandler);
        }
    };
    document.addEventListener('keydown', escapeHandler);
}

// Utility function for notifications
function showNotificationMessage(message, type = 'info') {
    // Check if the enhanced notification function exists
    if (typeof window.showNotification === 'function') {
        window.showNotification(message, type);
        return;
    }

    // Fallback simple notification
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        border-radius: 8px;
        color: white;
        font-weight: 600;
        z-index: 20000;
        max-width: 350px;
        font-family: 'Montserrat', sans-serif;
        transform: translateX(100%);
        transition: transform 0.3s ease;
    `;

    const backgrounds = {
        success: 'linear-gradient(135deg, #10b981, #059669)',
        error: 'linear-gradient(135deg, #ef4444, #dc2626)',
        info: 'linear-gradient(135deg, #3b82f6, #2563eb)'
    };

    notification.style.background = backgrounds[type] || backgrounds.info;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);

    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

// Enhanced initialization
function initializeEnhancedDropdowns() {
    console.log('🚀 Initializing enhanced dropdown system...');

    // Setup main functionality
    setupDropdownHandler();
    setupKeyboardNavigation();
    initializeDropdowns();

    // Add touch support for mobile
    if ('ontouchstart' in window) {
        document.addEventListener('touchstart', function (e) {
            // Handle touch events similar to click events
            if (!e.target.closest('.dropdown-menu')) {
                const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');
                activeDropdowns.forEach(menu => {
                    menu.classList.remove('active');
                    const button = menu.querySelector('.dropdown-btn');
                    if (button) {
                        button.setAttribute('aria-expanded', 'false');
                    }
                });
            }
        });
    }

    // Add custom CSS for animations if not present
    if (!document.querySelector('#dropdown-animations')) {
        const style = document.createElement('style');
        style.id = 'dropdown-animations';
        style.textContent = `
            @keyframes dropdownSlide {
                from {
                    opacity: 0;
                    transform: translateY(-8px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }
            
            .dropdown-content {
                animation-fill-mode: both;
            }
            
            .dropdown-btn:focus {
                outline: 2px solid var(--primary-color);
                outline-offset: 2px;
            }
            
            .dropdown-item:focus {
                outline: 2px solid var(--primary-color);
                outline-offset: -2px;
            }
        `;
        document.head.appendChild(style);
    }

    console.log('✅ Enhanced dropdown system initialized');
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeEnhancedDropdowns();
});

// Also initialize if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeEnhancedDropdowns);
} else {
    initializeEnhancedDropdowns();
}

// Global exports for backward compatibility
window.toggleDropdown = toggleDropdown;
window.editItem = editItem;
window.deleteItemEnhanced = deleteItemEnhanced;
window.reportItemEnhanced = reportItemEnhanced;

console.log('✅ Enhanced toggleEditDelete.js loaded with improved UX and accessibility');