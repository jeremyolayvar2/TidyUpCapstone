// FIXED: Enhanced Token Modal with proper error handling and debugging
document.addEventListener('DOMContentLoaded', function () {
    console.log('TokenModal.js loaded');

    // Check if token modal exists
    const modal = document.getElementById('tokenModal');
    if (!modal) {
        console.warn('Token modal not found in DOM');
        return;
    }

    console.log('Token modal found:', modal);

    // FIXED: Enhanced click handlers with better event management
    function setupTokenTriggers() {
        const triggers = document.querySelectorAll('.open-token');
        console.log('Found token triggers:', triggers.length);

        triggers.forEach((el, index) => {
            console.log(`Setting up trigger ${index}:`, el);

            // Remove existing listeners to prevent duplicates
            el.removeEventListener('click', handleTokenClick);

            // Add new listener
            el.addEventListener('click', handleTokenClick);
        });
    }

    // FIXED: Centralized click handler
    function handleTokenClick(e) {
        console.log('Token trigger clicked:', e.target);
        e.preventDefault();
        e.stopPropagation();

        if (modal) {
            modal.classList.add('show');
            console.log('Token modal shown');

            // Auto-hide after 5 seconds
            setTimeout(() => {
                hideTokenModal();
            }, 5000);
        } else {
            console.error('Modal not available when trigger clicked');
        }
    }

    // FIXED: Enhanced mouse move handler with better performance
    function setupMouseMove() {
        if (modal) {
            modal.addEventListener('mousemove', throttle((e) => {
                const rect = modal.getBoundingClientRect();
                modal.style.setProperty('--mouse-x', `${e.clientX - rect.left}px`);
                modal.style.setProperty('--mouse-y', `${e.clientY - rect.top}px`);
            }, 16)); // 60fps throttling
        }
    }

    // FIXED: Enhanced outside click handler
    function setupOutsideClick() {
        document.addEventListener('click', (e) => {
            if (modal && modal.classList.contains('show')) {
                // Check if click is outside modal and not on trigger
                if (!modal.contains(e.target) && !e.target.closest('.open-token')) {
                    hideTokenModal();
                }
            }
        });
    }

    // FIXED: Keyboard support
    function setupKeyboardSupport() {
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && modal && modal.classList.contains('show')) {
                hideTokenModal();
            }
        });
    }

    // FIXED: Hide function with proper cleanup
    function hideTokenModal() {
        if (modal && modal.classList.contains('show')) {
            modal.classList.remove('show');
            console.log('Token modal hidden');
        }
    }

    // FIXED: Show function for external use
    function showTokenModal() {
        if (modal) {
            modal.classList.add('show');
            console.log('Token modal shown programmatically');

            // Auto-hide after 5 seconds
            setTimeout(() => {
                hideTokenModal();
            }, 5000);
            return true;
        }
        console.error('Cannot show token modal - not found');
        return false;
    }

    // Throttle utility function
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
        };
    }

    // FIXED: Initialize all functionality
    function initialize() {
        console.log('Initializing token modal functionality');

        setupTokenTriggers();
        setupMouseMove();
        setupOutsideClick();
        setupKeyboardSupport();

        // Export functions globally
        window.showTokenModal = showTokenModal;
        window.hideTokenModal = hideTokenModal;

        console.log('Token modal initialized successfully');
    }

    // FIXED: Observe DOM changes to catch dynamically added triggers
    function observeDOM() {
        const observer = new MutationObserver((mutations) => {
            let shouldReinitialize = false;

            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1) { // Element node
                        if (node.matches('.open-token') || node.querySelector('.open-token')) {
                            shouldReinitialize = true;
                        }
                    }
                });
            });

            if (shouldReinitialize) {
                console.log('DOM changed, re-initializing token triggers');
                setupTokenTriggers();
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    // Start initialization
    initialize();

    // Watch for DOM changes
    observeDOM();

    // FIXED: Reinitialize after a delay to catch any late-loading elements
    setTimeout(() => {
        console.log('Re-initializing token modal after delay');
        setupTokenTriggers();
    }, 1000);
});

// FIXED: Export functions for use by other scripts
window.TokenModal = {
    show: () => window.showTokenModal && window.showTokenModal(),
    hide: () => window.hideTokenModal && window.hideTokenModal()
};

console.log('TokenModal.js loaded successfully');