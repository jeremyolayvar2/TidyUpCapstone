// External dropdown system - completely isolated from post layout
let currentActiveButton = null;

function showExternalDropdown(button) {
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

    // Calculate button position
    const buttonRect = button.getBoundingClientRect();
    const position = calculateOptimalPosition(buttonRect, 200, 150);

    // Create dropdown content based on ownership
    const dropdownContent = createDropdownContent(itemId, isOwner);

    // Set up container positioning
    container.style.position = 'fixed';
    container.style.top = position.top + 'px';
    container.style.left = position.left + 'px';
    container.style.zIndex = '2100';
    container.style.display = 'block';
    container.style.opacity = '0';
    container.style.transform = 'scale(0.95) translateY(-10px)';
    container.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
    container.style.pointerEvents = 'auto';
    container.innerHTML = dropdownContent;

    // Show backdrop
    backdrop.style.display = 'block';
    backdrop.style.opacity = '0';
    backdrop.style.transition = 'opacity 0.3s ease';

    // Animate in
    requestAnimationFrame(() => {
        backdrop.style.opacity = '1';
        container.style.opacity = '1';
        container.style.transform = 'scale(1) translateY(0)';
    });

    // Set active state
    button.classList.add('active');
    button.style.transform = 'scale(1.05)';
    const img = button.querySelector('img');
    if (img) {
        img.style.transform = 'rotate(90deg)';
        img.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
    }

    currentActiveButton = button;

    // Add event listeners
    backdrop.addEventListener('click', hideExternalDropdown);
    document.addEventListener('click', handleOutsideClick);
    document.addEventListener('keydown', handleEscapeKey);
}

function hideExternalDropdown() {
    const backdrop = document.getElementById('dropdown-backdrop');
    const container = document.getElementById('dropdown-container');

    if (backdrop && container) {
        // Animate out
        backdrop.style.opacity = '0';
        container.style.opacity = '0';
        container.style.transform = 'scale(0.95) translateY(-10px)';

        setTimeout(() => {
            backdrop.style.display = 'none';
            container.style.display = 'none';
            container.innerHTML = '';
        }, 300);
    }

    // Reset active button
    if (currentActiveButton) {
        currentActiveButton.classList.remove('active');
        currentActiveButton.style.transform = '';
        const img = currentActiveButton.querySelector('img');
        if (img) {
            img.style.transform = '';
        }
        currentActiveButton = null;
    }

    // Remove event listeners
    document.removeEventListener('click', handleOutsideClick);
    document.removeEventListener('keydown', handleEscapeKey);
}

function createDropdownContent(itemId, isOwner) {
    const baseClasses = 'dropdown-item';
    const iconClasses = 'bx';

    if (isOwner) {
        return `
            <div class="external-dropdown-content">
                <button class="${baseClasses}" onclick="handleEditItem(${itemId}); hideExternalDropdown();">
                    <i class="${iconClasses} bx-edit"></i>
                    Edit
                </button>
                <button class="${baseClasses} delete" onclick="handleDeleteItem(${itemId}); hideExternalDropdown();">
                    <i class="${iconClasses} bx-trash"></i>
                    Delete
                </button>
            </div>
        `;
    } else {
        return `
            <div class="external-dropdown-content">
                <button class="${baseClasses} report" onclick="handleReportItem(${itemId}); hideExternalDropdown();">
                    <i class="${iconClasses} bx-flag"></i>
                    Report
                </button>
            </div>
        `;
    }
}

function handleOutsideClick(event) {
    // Don't close if clicking inside the external dropdown system
    if (event.target.closest('#external-dropdown-system')) {
        return;
    }

    // Don't close if clicking on an external dropdown button
    if (event.target.closest('.external-dropdown-btn')) {
        return;
    }

    hideExternalDropdown();
}

function handleEscapeKey(event) {
    if (event.key === 'Escape') {
        hideExternalDropdown();
        // Focus the button that opened the dropdown
        if (currentActiveButton) {
            currentActiveButton.focus();
        }
    }
}

function calculateOptimalPosition(buttonRect, dropdownWidth, dropdownHeight) {
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    let position = {
        top: buttonRect.bottom + 8,
        left: buttonRect.right - dropdownWidth,
        placement: 'bottom-right'
    };

    // Check if dropdown fits below button
    if (position.top + dropdownHeight > viewportHeight - 10) {
        position.top = buttonRect.top - dropdownHeight - 8;
        position.placement = position.placement.replace('bottom', 'top');
    }

    // Check if dropdown fits to the right
    if (position.left + dropdownWidth > viewportWidth - 10) {
        position.left = buttonRect.left - 8;
        position.placement = position.placement.replace('right', 'left');
    }

    // Ensure dropdown doesn't go off the left edge
    if (position.left < 10) {
        position.left = 10;
    }

    // Ensure dropdown doesn't go off the top edge
    if (position.top < 10) {
        position.top = 10;
    }

    return position;
}

// Action handlers - these call your existing functions
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

// Close dropdowns on various events
window.addEventListener('scroll', hideExternalDropdown);
window.addEventListener('resize', hideExternalDropdown);
window.addEventListener('orientationchange', () => {
    setTimeout(hideExternalDropdown, 100);
});
window.addEventListener('blur', hideExternalDropdown);

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    // Ensure external dropdown system exists
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

    // Add keyboard navigation support
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

    // Add ARIA attributes to external dropdown buttons
    document.querySelectorAll('.external-dropdown-btn').forEach(button => {
        button.setAttribute('aria-haspopup', 'true');
        button.setAttribute('aria-expanded', 'false');
    });
});

// Touch handling for mobile
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

// Export functions for global access
if (typeof window !== 'undefined') {
    window.showExternalDropdown = showExternalDropdown;
    window.hideExternalDropdown = hideExternalDropdown;
}

// Debug function (remove in production)
function debugExternalDropdown(message, data = {}) {
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        console.log(`[External Dropdown Debug] ${message}`, data);
    }
}