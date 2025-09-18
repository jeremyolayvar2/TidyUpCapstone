// Complete enhanced dropdown system - replaces toggleEditDelete.js entirely
// Preserves all original functionality while fixing positioning and event issues
let currentActiveButton = null;

function showExternalDropdown(button, event) {
    // Prevent event bubbling to avoid post-header reactions
    if (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
    }

    // Close any existing dropdown first
    hideExternalDropdown();

    const itemId = button.getAttribute('data-item-id');
    const isOwner = button.getAttribute('data-is-owner') === 'true';

    // Get external dropdown system elements
    const dropdownSystem = document.getElementById('external-dropdown-system');
    const backdrop = document.getElementById('dropdown-backdrop');
    const container = document.getElementById('dropdown-container');

    if (!dropdownSystem || !backdrop || !container) {
        console.error('External dropdown system not found');
        return;
    }

    // Enhanced position calculation with better viewport handling
    const buttonRect = button.getBoundingClientRect();
    const dropdownWidth = 200;
    const dropdownHeight = 120; // Estimated height

    const position = calculateOptimalPosition(buttonRect, dropdownWidth, dropdownHeight);

    // Create dropdown content
    const dropdownContent = createDropdownContent(itemId, isOwner);

    // Set up container with enhanced positioning
    container.style.cssText = `
        position: fixed;
        top: ${position.top}px;
        left: ${position.left}px;
        z-index: 99999;
        display: block;
        opacity: 0;
        transform: scale(0.95) translateY(-10px);
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        pointer-events: auto;
        will-change: transform, opacity;
    `;

    container.innerHTML = dropdownContent;

    // Show backdrop with enhanced styling
    backdrop.style.cssText = `
        display: block;
        opacity: 0;
        transition: opacity 0.3s ease;
        z-index: 99998;
        pointer-events: auto;
    `;

    // Force reflow and animate in
    requestAnimationFrame(() => {
        backdrop.style.opacity = '1';
        container.style.opacity = '1';
        container.style.transform = 'scale(1) translateY(0)';
    });

    // Set active state on button
    button.classList.add('active');
    button.style.transform = 'scale(1.05)';
    const img = button.querySelector('img');
    if (img) {
        img.style.transform = 'rotate(90deg)';
        img.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
    }

    currentActiveButton = button;

    // Enhanced event listeners with better cleanup
    const hideHandler = (e) => {
        hideExternalDropdown();
    };

    const outsideClickHandler = (e) => {
        handleOutsideClick(e);
    };

    const keyHandler = (e) => {
        handleEscapeKey(e);
    };

    backdrop.addEventListener('click', hideHandler, { once: true });
    document.addEventListener('click', outsideClickHandler);
    document.addEventListener('keydown', keyHandler);

    // Store handlers for cleanup
    container._eventHandlers = { outsideClickHandler, keyHandler };
}

function hideExternalDropdown() {
    const backdrop = document.getElementById('dropdown-backdrop');
    const container = document.getElementById('dropdown-container');

    if (backdrop && container) {
        // Animate out with better timing
        backdrop.style.transition = 'opacity 0.2s ease';
        backdrop.style.opacity = '0';

        container.style.transition = 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)';
        container.style.opacity = '0';
        container.style.transform = 'scale(0.95) translateY(-5px)';

        setTimeout(() => {
            backdrop.style.display = 'none';
            container.style.display = 'none';
            container.innerHTML = '';
        }, 200);
    }

    // Reset active button state
    if (currentActiveButton) {
        currentActiveButton.classList.remove('active');
        currentActiveButton.style.transform = '';
        const img = currentActiveButton.querySelector('img');
        if (img) {
            img.style.transform = '';
        }
        currentActiveButton = null;
    }

    // Clean up event listeners
    if (container && container._eventHandlers) {
        document.removeEventListener('click', container._eventHandlers.outsideClickHandler);
        document.removeEventListener('keydown', container._eventHandlers.keyHandler);
        delete container._eventHandlers;
    }
}

function createDropdownContent(itemId, isOwner) {
    const baseClasses = 'dropdown-item';
    const iconClasses = 'bx';

    if (isOwner) {
        return `
            <div class="external-dropdown-content">
                <button class="${baseClasses}" onclick="handleEditItem(${itemId}); hideExternalDropdown();" tabindex="0">
                    <i class="${iconClasses} bx-edit"></i>
                    Edit
                </button>
                <button class="${baseClasses} delete" onclick="handleDeleteItem(${itemId}); hideExternalDropdown();" tabindex="0">
                    <i class="${iconClasses} bx-trash"></i>
                    Delete
                </button>
            </div>
        `;
    } else {
        return `
            <div class="external-dropdown-content">
                <button class="${baseClasses} report" onclick="handleReportItem(${itemId}); hideExternalDropdown();" tabindex="0">
                    <i class="${iconClasses} bx-flag"></i>
                    Report
                </button>
            </div>
        `;
    }
}

function handleOutsideClick(event) {
    // More precise outside click detection
    const dropdownSystem = document.getElementById('external-dropdown-system');
    const dropdownBtn = event.target.closest('.external-dropdown-btn');

    // Don't close if clicking inside dropdown or on dropdown button
    if (dropdownSystem && dropdownSystem.contains(event.target)) {
        return;
    }

    if (dropdownBtn) {
        return;
    }

    hideExternalDropdown();
}

function handleEscapeKey(event) {
    if (event.key === 'Escape') {
        event.preventDefault();
        hideExternalDropdown();

        // Return focus to button
        if (currentActiveButton) {
            currentActiveButton.focus();
        }
    }
}

function calculateOptimalPosition(buttonRect, dropdownWidth, dropdownHeight) {
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const scrollX = window.scrollX || window.pageXOffset;
    const scrollY = window.scrollY || window.pageYOffset;

    // Default position (bottom-right of button)
    let position = {
        top: buttonRect.bottom + scrollY + 8,
        left: buttonRect.right + scrollX - dropdownWidth,
        placement: 'bottom-right'
    };

    // Check if dropdown fits below button
    if (position.top + dropdownHeight > viewportHeight + scrollY - 20) {
        position.top = buttonRect.top + scrollY - dropdownHeight - 8;
        position.placement = position.placement.replace('bottom', 'top');
    }

    // Check if dropdown fits to the right
    if (position.left + dropdownWidth > viewportWidth + scrollX - 20) {
        position.left = buttonRect.left + scrollX - 8;
        position.placement = position.placement.replace('right', 'left');
    }

    // Ensure dropdown doesn't go off edges
    position.left = Math.max(20 + scrollX, Math.min(position.left, viewportWidth + scrollX - dropdownWidth - 20));
    position.top = Math.max(20 + scrollY, Math.min(position.top, viewportHeight + scrollY - dropdownHeight - 20));

    return position;
}

// Action handlers remain the same
function handleEditItem(itemId) {
    if (typeof openEditModal !== 'undefined') {
        openEditModal(itemId);
    } else {
        console.error('openEditModal function not found');
    }
}

function handleDeleteItem(itemId) {
    if (typeof deleteItem !== 'undefined') {
        deleteItem(itemId);
    } else {
        console.error('deleteItem function not found');
    }
}

function handleReportItem(itemId) {
    if (typeof reportItem !== 'undefined') {
        reportItem(itemId);
    } else {
        console.error('reportItem function not found');
    }
}

// Enhanced event listeners with all original functionality preserved
window.addEventListener('scroll', hideExternalDropdown, { passive: true });
window.addEventListener('resize', hideExternalDropdown);
window.addEventListener('orientationchange', () => {
    setTimeout(hideExternalDropdown, 100);
});
window.addEventListener('blur', hideExternalDropdown);

// Enhanced touch handling for mobile (from original)
let touchStartY = 0;
let touchStartX = 0;

document.addEventListener('touchstart', function (event) {
    touchStartY = event.touches[0].clientY;
    touchStartX = event.touches[0].clientX;
}, { passive: true });

document.addEventListener('touchmove', function (event) {
    if (!currentActiveButton) return;

    const touchY = event.touches[0].clientY;
    const touchX = event.touches[0].clientX;
    const deltaY = Math.abs(touchY - touchStartY);
    const deltaX = Math.abs(touchX - touchStartX);

    // If significant movement detected, close dropdown
    if (deltaY > 50 || deltaX > 50) {
        hideExternalDropdown();
    }
}, { passive: true });

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    // Create or ensure external dropdown system exists
    let dropdownSystem = document.getElementById('external-dropdown-system');
    if (!dropdownSystem) {
        dropdownSystem = document.createElement('div');
        dropdownSystem.id = 'external-dropdown-system';
        dropdownSystem.innerHTML = `
            <div id="dropdown-backdrop" class="dropdown-backdrop" style="display: none;"></div>
            <div id="dropdown-container" class="external-dropdown-container" style="display: none;"></div>
        `;
        document.body.appendChild(dropdownSystem);
    }

    // Enhanced keyboard navigation
    document.addEventListener('keydown', function (event) {
        const container = document.getElementById('dropdown-container');
        if (!container || container.style.display === 'none') return;

        const dropdownItems = container.querySelectorAll('.dropdown-item');
        const currentFocus = document.activeElement;
        let currentIndex = Array.from(dropdownItems).indexOf(currentFocus);

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                currentIndex = currentIndex < dropdownItems.length - 1 ? currentIndex + 1 : 0;
                dropdownItems[currentIndex].focus();
                break;
            case 'ArrowUp':
                event.preventDefault();
                currentIndex = currentIndex > 0 ? currentIndex - 1 : dropdownItems.length - 1;
                dropdownItems[currentIndex].focus();
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

    // Enhanced ARIA attributes and event handlers
    document.querySelectorAll('.external-dropdown-btn').forEach(button => {
        button.setAttribute('aria-haspopup', 'true');
        button.setAttribute('aria-expanded', 'false');
        button.setAttribute('role', 'button');

        // Add proper click handler with event object (replaces onclick)
        button.addEventListener('click', function (event) {
            showExternalDropdown(this, event);
        });
    });
});

// Debug function (preserved from original - remove in production)
function debugExternalDropdown(message, data = {}) {
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        console.log(`[External Dropdown Debug] ${message}`, data);
    }
}

// Export functions for global access
if (typeof window !== 'undefined') {
    window.showExternalDropdown = showExternalDropdown;
    window.hideExternalDropdown = hideExternalDropdown;
}