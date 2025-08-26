// Enhanced NotificationPage.js - Improved Responsiveness
document.addEventListener('DOMContentLoaded', function () {
    initializeNotificationPage();
    initializeSidebarResponsiveness();
    initializeResponsiveHandlers();
    initializeIntersectionObserver();
    initializeTouchHandlers();
});

// Enhanced global state with better viewport detection
let currentTab = 'all';
let isModalOpen = false;
let viewportState = {
    width: window.innerWidth,
    height: window.innerHeight,
    isMobile: window.innerWidth <= 768,
    isTablet: window.innerWidth > 768 && window.innerWidth <= 1024,
    isDesktop: window.innerWidth > 1024,
    orientation: window.innerHeight > window.innerWidth ? 'portrait' : 'landscape'
};

// Debounce utility for performance
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

function initializeNotificationPage() {
    console.log('Initializing notification page...');

    // Set default tab
    switchTab('all');

    // Enhanced notification setup with better event delegation
    setupNotificationEventDelegation();

    // Initialize advanced keyboard navigation
    setupAdvancedKeyboardNavigation();

    // Update initial unread count
    updateUnreadCount();

    // Set up notification type handlers with ARIA improvements
    setupEnhancedNotificationTypeHandlers();

    // Initialize lazy loading for performance
    initializeLazyLoading();
}

function setupNotificationEventDelegation() {
    const container = document.getElementById('notifications-container');
    if (!container) return;

    // Use event delegation for better performance
    container.addEventListener('click', function (e) {
        const notification = e.target.closest('.notification-card');
        if (notification) {
            handleNotificationClick(notification, e);
        }
    });

    container.addEventListener('keydown', function (e) {
        const notification = e.target.closest('.notification-card');
        if (notification && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            handleNotificationClick(notification, e);
        }
    });

    // Make all notifications focusable and accessible
    const notifications = container.querySelectorAll('.notification-card');
    notifications.forEach((notification, index) => {
        notification.setAttribute('tabindex', '0');
        notification.setAttribute('role', 'button');
        notification.setAttribute('aria-describedby', `notification-${index}-desc`);
    });
}

function initializeResponsiveHandlers() {
    // Enhanced viewport checking
    updateViewportState();

    // Optimized resize handler with better debouncing
    const handleResize = debounce(() => {
        const oldState = { ...viewportState };
        updateViewportState();

        if (hasViewportChanged(oldState)) {
            handleViewportTransition(oldState);
        }

        updateLayoutForCurrentViewport();
        updateNotificationContainerHeight();
        adjustModalPositioning();
    }, 150);

    const handleOrientationChange = debounce(() => {
        setTimeout(() => {
            updateViewportState();
            updateLayoutForCurrentViewport();
            updateNotificationContainerHeight();
            handleOrientationSpecificAdjustments();
        }, 300);
    }, 200);

    window.addEventListener('resize', handleResize);
    window.addEventListener('orientationchange', handleOrientationChange);

    // Handle visibility changes for better performance
    document.addEventListener('visibilitychange', () => {
        if (!document.hidden) {
            updateLayoutForCurrentViewport();
            updateNotificationContainerHeight();
        }
    });

    // Initial setup
    updateNotificationContainerHeight();
}

function updateViewportState() {
    const oldState = { ...viewportState };

    viewportState = {
        width: window.innerWidth,
        height: window.innerHeight,
        isMobile: window.innerWidth <= 768,
        isTablet: window.innerWidth > 768 && window.innerWidth <= 1024,
        isDesktop: window.innerWidth > 1024,
        orientation: window.innerHeight > window.innerWidth ? 'portrait' : 'landscape'
    };

    // Log viewport changes for debugging
    if (oldState.isMobile !== viewportState.isMobile ||
        oldState.isTablet !== viewportState.isTablet ||
        oldState.isDesktop !== viewportState.isDesktop) {
        console.log('Viewport state changed:', viewportState);
    }
}

function hasViewportChanged(oldState) {
    return oldState.isMobile !== viewportState.isMobile ||
        oldState.isTablet !== viewportState.isTablet ||
        oldState.isDesktop !== viewportState.isDesktop ||
        oldState.orientation !== viewportState.orientation;
}

function handleViewportTransition(oldState) {
    console.log('Handling viewport transition from:', oldState, 'to:', viewportState);

    // Handle modal transitions gracefully
    if (isModalOpen) {
        const wasModalVisible = !document.getElementById('confirm-meetup-modal')?.classList.contains('hidden') ||
            !document.getElementById('mobile-confirm-modal')?.classList.contains('hidden');

        if (wasModalVisible) {
            closeAllModals();
            setTimeout(() => {
                const transactionCard = document.querySelector('.transaction-notification[data-read="false"]');
                if (transactionCard) {
                    openTransactionModal();
                }
            }, 200);
        }
    }

    // Trigger layout recalculation
    requestAnimationFrame(() => {
        updateLayoutForCurrentViewport();
        adjustTabletSpecificLayout();
    });
}

function updateLayoutForCurrentViewport() {
    const container = document.querySelector('.notification-page-container');
    const child2 = document.querySelector('.notification-page-container-child2');

    if (!container) return;

    // Apply viewport-specific classes for better CSS targeting
    document.body.classList.remove('mobile-view', 'tablet-view', 'desktop-view');

    if (viewportState.isMobile) {
        document.body.classList.add('mobile-view');
        if (child2) child2.style.display = 'none';
        // Remove sidebar adjustments on mobile
        document.body.classList.remove('sidebar-open', 'sidebar-closed');
    } else if (viewportState.isTablet) {
        document.body.classList.add('tablet-view');
        adjustTabletLayout(container, child2);
        // Maintain sidebar functionality on tablet
        updateSidebarStateForTablet();
    } else {
        document.body.classList.add('desktop-view');
        adjustDesktopLayout(container, child2);
        // Maintain sidebar functionality on desktop
        updatePageLayout();
    }
}

function updateSidebarStateForTablet() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    // Apply sidebar state classes for tablet as well
    document.body.classList.remove('sidebar-open', 'sidebar-closed');

    if (sidebar.classList.contains('close')) {
        document.body.classList.add('sidebar-closed');
    } else {
        document.body.classList.add('sidebar-open');
    }
}

function adjustTabletLayout(container, child2) {
    if (!container || !child2) return;

    container.style.display = 'flex';
    container.style.flexDirection = 'row';
    child2.style.display = 'flex';

    // Optimize tablet layout based on orientation
    if (viewportState.orientation === 'portrait') {
        container.style.gap = '16px';
        child2.style.maxWidth = '280px';
    } else {
        container.style.gap = '24px';
        child2.style.maxWidth = '320px';
    }
}

function adjustDesktopLayout(container, child2) {
    if (!container || !child2) return;

    container.style.display = 'flex';
    container.style.flexDirection = 'row';
    child2.style.display = 'flex';
    child2.style.maxWidth = viewportState.width >= 1440 ? '500px' : '450px';
}

function adjustTabletSpecificLayout() {
    if (!viewportState.isTablet) return;

    const notifications = document.querySelectorAll('.notification-card');
    notifications.forEach(card => {
        // Adjust notification card spacing for tablet
        if (viewportState.orientation === 'portrait') {
            card.style.marginBottom = '10px';
        } else {
            card.style.marginBottom = '12px';
        }
    });
}

function handleOrientationSpecificAdjustments() {
    // Handle orientation-specific adjustments
    if (viewportState.isMobile || viewportState.isTablet) {
        const container = document.querySelector('.notifications-container');
        if (container) {
            // Adjust scrolling area based on orientation
            const baseHeight = viewportState.orientation === 'portrait' ? '60vh' : '50vh';
            container.style.maxHeight = baseHeight;
        }
    }
}

function updateNotificationContainerHeight() {
    const container = document.querySelector('.notifications-container');
    if (!container) return;

    try {
        const containerRect = container.getBoundingClientRect();
        const viewportHeight = viewportState.height;
        const containerTop = containerRect.top;

        let bottomPadding;
        if (viewportState.isMobile) {
            bottomPadding = viewportState.orientation === 'portrait' ? 140 : 100;
        } else if (viewportState.isTablet) {
            bottomPadding = viewportState.orientation === 'portrait' ? 120 : 80;
        } else {
            bottomPadding = 80;
        }

        const maxHeight = Math.max(300, viewportHeight - containerTop - bottomPadding);
        container.style.maxHeight = maxHeight + 'px';

        console.log(`Container height updated: ${maxHeight}px for ${viewportState.isMobile ? 'mobile' : viewportState.isTablet ? 'tablet' : 'desktop'} ${viewportState.orientation}`);
    } catch (error) {
        console.warn('Error updating container height:', error);
    }
}

function adjustModalPositioning() {
    if (!isModalOpen) return;

    const desktopModal = document.getElementById('confirm-meetup-modal');
    const mobileModal = document.getElementById('mobile-confirm-modal');

    if (viewportState.isTablet && desktopModal && !desktopModal.classList.contains('hidden')) {
        const modalContent = desktopModal.querySelector('.modal-content');
        if (modalContent) {
            modalContent.style.maxWidth = viewportState.orientation === 'portrait' ? '260px' : '300px';
            modalContent.style.padding = viewportState.orientation === 'portrait' ? '16px' : '20px';
        }
    }
}

function initializeTouchHandlers() {
    if (!('ontouchstart' in window)) return;

    const container = document.getElementById('notifications-container');
    if (!container) return;

    let touchStartY = 0;
    let touchEndY = 0;

    container.addEventListener('touchstart', function (e) {
        touchStartY = e.touches[0].clientY;
    }, { passive: true });

    container.addEventListener('touchend', function (e) {
        touchEndY = e.changedTouches[0].clientY;
        handleSwipeGesture();
    }, { passive: true });

    function handleSwipeGesture() {
        const swipeDistance = touchEndY - touchStartY;
        const minSwipeDistance = 50;

        if (Math.abs(swipeDistance) > minSwipeDistance) {
            // Add subtle haptic feedback if available
            if (navigator.vibrate) {
                navigator.vibrate(10);
            }
        }
    }
}

function initializeIntersectionObserver() {
    const observerOptions = {
        root: document.querySelector('.notifications-container'),
        rootMargin: '20px',
        threshold: 0.1
    };

    const notificationObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                // Lazy load any heavy content here if needed
            }
        });
    }, observerOptions);

    // Observe all notification cards
    const notifications = document.querySelectorAll('.notification-card');
    notifications.forEach(card => {
        notificationObserver.observe(card);
    });
}

function initializeLazyLoading() {
    // Implement lazy loading for notifications if there are many
    const notifications = document.querySelectorAll('.notification-card');

    if (notifications.length > 20) {
        // Hide notifications beyond the first 10 initially
        notifications.forEach((card, index) => {
            if (index >= 10) {
                card.style.display = 'none';
                card.setAttribute('data-lazy', 'true');
            }
        });

        // Add "Load More" functionality
        addLoadMoreButton();
    }
}

function addLoadMoreButton() {
    const container = document.getElementById('notifications-container');
    const loadMoreBtn = document.createElement('button');

    loadMoreBtn.className = 'load-more-btn';
    loadMoreBtn.textContent = 'Load More Notifications';
    loadMoreBtn.style.cssText = `
        width: 100%;
        padding: 12px;
        margin-top: 16px;
        border: 2px solid var(--primary-color);
        background: white;
        color: var(--primary-color);
        border-radius: 8px;
        cursor: pointer;
        font-family: 'Montserrat', sans-serif;
        font-weight: 500;
        transition: all 0.2s ease;
    `;

    loadMoreBtn.addEventListener('click', () => {
        const hiddenCards = document.querySelectorAll('[data-lazy="true"]');
        const toShow = Array.from(hiddenCards).slice(0, 10);

        toShow.forEach(card => {
            card.style.display = 'block';
            card.removeAttribute('data-lazy');
        });

        if (document.querySelectorAll('[data-lazy="true"]').length === 0) {
            loadMoreBtn.remove();
        }
    });

    container.appendChild(loadMoreBtn);
}

function handleNotificationClick(notificationElement, event) {
    console.log('Notification clicked:', notificationElement.dataset.type);

    // Enhanced click animation with better performance
    requestAnimationFrame(() => {
        notificationElement.style.transform = 'scale(0.98)';
        notificationElement.style.transition = 'transform 0.1s ease';

        setTimeout(() => {
            notificationElement.style.transform = '';
            notificationElement.style.transition = '';
        }, 100);
    });

    // Mark as read with optimistic updates
    if (notificationElement.dataset.read === 'false') {
        markAsRead(notificationElement);
    }

    // Handle different notification types with enhanced feedback
    const notificationType = notificationElement.dataset.type;
    handleNotificationTypeAction(notificationType);
}

function handleNotificationTypeAction(type) {
    switch (type) {
        case 'transaction':
            console.log('Opening transaction modal');
            openTransactionModal();
            break;
        case 'reaction':
            showEnhancedNotificationToast('Viewing post reactions', 'info');
            break;
        case 'comment':
            showEnhancedNotificationToast('Opening comment thread', 'info');
            break;
        case 'delivery':
            showEnhancedNotificationToast('Viewing delivery details', 'success');
            break;
        default:
            console.log('Unknown notification type:', type);
            showEnhancedNotificationToast('Notification details', 'info');
    }
}

function openTransactionModal() {
    console.log('Opening transaction modal - Current viewport:', viewportState);
    isModalOpen = true;

    if (viewportState.isMobile) {
        openMobileModal();
    } else {
        openDesktopModal();
    }
}

function openMobileModal() {
    const mobileModal = document.getElementById('mobile-confirm-modal');
    if (mobileModal) {
        mobileModal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';

        // Enhanced focus management
        setTimeout(() => {
            const firstButton = mobileModal.querySelector('.confirm-button');
            if (firstButton) {
                firstButton.focus();
                // Announce to screen readers
                firstButton.setAttribute('aria-describedby', 'modal-announcement');
                const announcement = document.createElement('div');
                announcement.id = 'modal-announcement';
                announcement.className = 'sr-only';
                announcement.textContent = 'Transaction confirmation modal opened';
                mobileModal.appendChild(announcement);
            }
        }, 150);
    }
}

function openDesktopModal() {
    const desktopModal = document.getElementById('confirm-meetup-modal');
    if (desktopModal) {
        desktopModal.classList.remove('hidden');

        if (viewportState.isTablet) {
            adjustModalForTablet(desktopModal);
        }

        setTimeout(() => {
            const firstButton = desktopModal.querySelector('.confirm-button');
            if (firstButton) {
                firstButton.focus();
            }
        }, 150);
    }
}

function adjustModalForTablet(modal) {
    const modalContent = modal.querySelector('.modal-content');
    if (modalContent) {
        modalContent.style.maxWidth = viewportState.orientation === 'portrait' ? '260px' : '300px';
        modalContent.style.padding = viewportState.orientation === 'portrait' ? '16px' : '20px';
    }
}

function setupEnhancedNotificationTypeHandlers() {
    const notifications = document.querySelectorAll('.notification-card');

    notifications.forEach((card, index) => {
        const type = card.dataset.type;
        const isRead = card.dataset.read === 'true';

        let ariaLabel = '';
        switch (type) {
            case 'transaction':
                ariaLabel = `Transaction notification${!isRead ? ' (unread)' : ''}. Click to confirm transaction.`;
                break;
            case 'reaction':
                ariaLabel = `Reaction notification${!isRead ? ' (unread)' : ''}. Click to view post reactions.`;
                break;
            case 'comment':
                ariaLabel = `Comment notification${!isRead ? ' (unread)' : ''}. Click to view comments.`;
                break;
            case 'delivery':
                ariaLabel = `Delivery notification${!isRead ? ' (unread)' : ''}. Click to view delivery details.`;
                break;
            default:
                ariaLabel = `Notification${!isRead ? ' (unread)' : ''}. Click to view details.`;
        }

        card.setAttribute('aria-label', ariaLabel);
        card.setAttribute('aria-describedby', `notification-${index}-desc`);

        // Add hidden description for screen readers
        const description = document.createElement('div');
        description.id = `notification-${index}-desc`;
        description.className = 'sr-only';
        description.textContent = card.querySelector('.message-text')?.textContent || 'No description available';
        card.appendChild(description);
    });
}

function setupAdvancedKeyboardNavigation() {
    document.addEventListener('keydown', function (event) {
        // Enhanced escape handling
        if (event.key === 'Escape' && isModalOpen) {
            event.preventDefault();
            closeAllModals();
            // Return focus to the notification that opened the modal
            const focusTarget = document.querySelector('.notification-card[data-read="false"]') ||
                document.querySelector('.notification-card');
            if (focusTarget) focusTarget.focus();
        }

        // Enhanced arrow navigation
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            handleAdvancedArrowNavigation(event);
        }

        // Tab switching shortcuts
        if (event.altKey && (event.key === '1' || event.key === '2')) {
            event.preventDefault();
            const tab = event.key === '1' ? 'all' : 'unread';
            switchTab(tab);
        }

        // Home/End keys for navigation
        if (event.key === 'Home' || event.key === 'End') {
            handleHomeEndNavigation(event);
        }
    });
}

function handleAdvancedArrowNavigation(event) {
    const focusedElement = document.activeElement;
    const visibleCards = Array.from(document.querySelectorAll('.notification-card')).filter(
        card => card.style.display !== 'none' && card.offsetParent !== null
    );

    if (visibleCards.length === 0) return;

    const currentIndex = visibleCards.indexOf(focusedElement);

    event.preventDefault();

    let nextIndex;
    if (event.key === 'ArrowDown') {
        nextIndex = currentIndex === -1 ? 0 : Math.min(currentIndex + 1, visibleCards.length - 1);
    } else {
        nextIndex = currentIndex === -1 ? visibleCards.length - 1 : Math.max(currentIndex - 1, 0);
    }

    const targetCard = visibleCards[nextIndex];
    targetCard.focus();

    // Scroll into view smoothly
    targetCard.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        inline: 'nearest'
    });
}

function handleHomeEndNavigation(event) {
    const visibleCards = Array.from(document.querySelectorAll('.notification-card')).filter(
        card => card.style.display !== 'none' && card.offsetParent !== null
    );

    if (visibleCards.length === 0) return;

    event.preventDefault();

    const targetCard = event.key === 'Home' ? visibleCards[0] : visibleCards[visibleCards.length - 1];
    targetCard.focus();
    targetCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function showEnhancedNotificationToast(message, type = 'info') {
    console.log('Showing enhanced toast:', message, type);

    // Remove existing toasts
    document.querySelectorAll('.notification-toast').forEach(toast => toast.remove());

    const toast = document.createElement('div');
    toast.className = 'notification-toast';
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'polite');

    // Enhanced positioning based on current viewport
    const isTop = viewportState.isMobile;
    const positions = getToastPosition(isTop);

    Object.assign(toast.style, {
        position: 'fixed',
        ...positions,
        zIndex: '2000',
        padding: viewportState.isTablet ? '14px 20px' : '16px 24px',
        borderRadius: '8px',
        color: 'white',
        fontFamily: '"Montserrat", sans-serif',
        fontWeight: '500',
        fontSize: viewportState.isTablet ? '13px' : '14px',
        transform: getInitialToastTransform(),
        transition: 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
        boxShadow: '0 10px 25px rgba(0, 0, 0, 0.2)',
        maxWidth: getToastMaxWidth(),
        wordWrap: 'break-word',
        backdropFilter: 'blur(8px)'
    });

    const colors = {
        success: '#10B981',
        error: '#EF4444',
        warning: '#F59E0B',
        info: '#3B82F6'
    };

    toast.style.backgroundColor = colors[type] || colors.info;
    toast.textContent = message;
    document.body.appendChild(toast);

    // Animate in
    requestAnimationFrame(() => {
        toast.style.transform = 'translate(0, 0)';
    });

    // Animate out
    setTimeout(() => {
        toast.style.transform = getInitialToastTransform();
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }, 3000);
}

function getToastPosition(isTop) {
    if (viewportState.isMobile) {
        return {
            top: isTop ? '20px' : 'auto',
            bottom: isTop ? 'auto' : '100px',
            right: '20px',
            left: '20px'
        };
    } else if (viewportState.isTablet) {
        return {
            top: isTop ? '20px' : 'auto',
            bottom: isTop ? 'auto' : '20px',
            right: '30px',
            left: 'auto'
        };
    } else {
        return {
            top: 'auto',
            bottom: '20px',
            right: '20px',
            left: 'auto'
        };
    }
}

function getInitialToastTransform() {
    if (viewportState.isMobile) {
        return 'translateY(-100%)';
    }
    return 'translateX(100%)';
}

function getToastMaxWidth() {
    if (viewportState.isMobile) {
        return 'calc(100vw - 40px)';
    } else if (viewportState.isTablet) {
        return '250px';
    }
    return '300px';
}

// Enhanced tab switching with better state management
function switchTab(tab) {
    console.log('Switching to tab:', tab);

    const allTab = document.getElementById('tab-all');
    const unreadTab = document.getElementById('tab-unread');
    const notifications = document.querySelectorAll('.notification-card');
    const emptyState = document.getElementById('empty-state');

    if (!allTab || !unreadTab) {
        console.error('Tab elements not found');
        return;
    }

    currentTab = tab;

    // Enhanced tab state management
    resetTabStyles();

    if (tab === 'all') {
        activateTab(allTab);
        showAllNotifications(notifications, emptyState);
        announceToScreenReader('Showing all notifications');
    } else if (tab === 'unread') {
        activateTab(unreadTab);
        showUnreadNotifications(notifications, emptyState);
        const unreadCount = document.querySelectorAll('[data-read="false"]').length;
        announceToScreenReader(`Showing ${unreadCount} unread notifications`);
    }

    // Persist tab preference
    try {
        if (typeof Storage !== 'undefined') {
            sessionStorage.setItem('activeNotificationTab', tab);
        }
    } catch (e) {
        console.log('SessionStorage not available');
    }
}

function announceToScreenReader(message) {
    const announcement = document.createElement('div');
    announcement.setAttribute('aria-live', 'polite');
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'sr-only';
    announcement.textContent = message;

    document.body.appendChild(announcement);
    setTimeout(() => {
        document.body.removeChild(announcement);
    }, 1000);
}

// Enhanced close modal functionality
function closeAllModals() {
    const desktopModal = document.getElementById('confirm-meetup-modal');
    const mobileModal = document.getElementById('mobile-confirm-modal');

    if (desktopModal) desktopModal.classList.add('hidden');
    if (mobileModal) mobileModal.classList.add('hidden');

    isModalOpen = false;
    document.body.style.overflow = '';

    // Clean up any announcement elements
    const announcements = document.querySelectorAll('#modal-announcement');
    announcements.forEach(el => el.remove());
}

// Initialize sidebar responsiveness with enhanced detection
function initializeSidebarResponsiveness() {
    updatePageLayout();

    const toggleButton = document.getElementById('icon-toggle');
    if (toggleButton) {
        toggleButton.addEventListener('click', () => {
            // Wait for sidebar animation to complete before updating layout
            setTimeout(() => {
                updatePageLayout();
                if (viewportState.isTablet) {
                    updateSidebarStateForTablet();
                }
            }, 50);
        });
    }

    // Enhanced sidebar observer
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        const observer = new MutationObserver(debounce((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                    updatePageLayout();
                    if (viewportState.isTablet) {
                        updateSidebarStateForTablet();
                    }
                }
            });
        }, 100));

        observer.observe(sidebar, {
            attributes: true,
            attributeFilter: ['class']
        });
    }
}

function updatePageLayout() {
    if (viewportState.isMobile) {
        // Skip sidebar adjustments on mobile only
        document.body.classList.remove('sidebar-open', 'sidebar-closed');
        return;
    }

    const sidebar = document.getElementById('sidebar');
    const pageContainer = document.querySelector('.notification-page-container');

    if (!sidebar || !pageContainer) return;

    // Remove existing classes
    document.body.classList.remove('sidebar-open', 'sidebar-closed');

    // Add appropriate class based on sidebar state (desktop and tablet)
    if (sidebar.classList.contains('close')) {
        document.body.classList.add('sidebar-closed');
    } else {
        document.body.classList.add('sidebar-open');
    }
}

// Rest of the existing functions with minor enhancements...
function showAllNotifications(notifications, emptyState) {
    let hasNotifications = false;

    notifications.forEach(notification => {
        notification.style.display = 'block';
        hasNotifications = true;
    });

    if (emptyState) {
        emptyState.classList.toggle('hidden', hasNotifications);
    }
}

function showUnreadNotifications(notifications, emptyState) {
    let hasUnread = false;

    notifications.forEach(notification => {
        const isUnread = notification.dataset.read === 'false';
        notification.style.display = isUnread ? 'block' : 'none';
        if (isUnread) hasUnread = true;
    });

    if (emptyState) {
        emptyState.classList.toggle('hidden', hasUnread);
    }
}

function resetTabStyles() {
    const tabs = [document.getElementById('tab-all'), document.getElementById('tab-unread')];
    tabs.forEach(tab => {
        if (tab) {
            tab.classList.remove('active');
            tab.setAttribute('aria-selected', 'false');
        }
    });
}

function activateTab(tab) {
    if (tab) {
        tab.classList.add('active');
        tab.setAttribute('aria-selected', 'true');
    }
}

function openModal(modalId) {
    if (modalId === 'confirm-meetup-modal' || modalId === 'mobile-confirm-modal') {
        openTransactionModal();
    } else {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('hidden');
            isModalOpen = true;
            if (viewportState.isMobile) {
                document.body.style.overflow = 'hidden';
            }
        }
    }
}

function closeModal(modalId) {
    isModalOpen = false;
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
    }

    closeAllModals();
    document.body.style.overflow = '';
    console.log('Modal closed:', modalId);
}

function confirmTransaction() {
    console.log('Confirming transaction...');

    const confirmButtons = document.querySelectorAll('.confirm-button');
    const activeButton = Array.from(confirmButtons).find(btn => {
        const modal = btn.closest('.modal-wrapper, .modal-overlay');
        return modal && !modal.classList.contains('hidden');
    });

    if (!activeButton) {
        console.error('No active confirm button found');
        return;
    }

    // Enhanced loading state with better UX
    const originalText = activeButton.textContent;
    activeButton.textContent = 'Processing...';
    activeButton.disabled = true;
    activeButton.style.opacity = '0.7';

    // Add loading spinner if available
    const spinner = document.createElement('span');
    spinner.innerHTML = '⟳';
    spinner.style.animation = 'spin 1s linear infinite';
    activeButton.prepend(spinner);

    // Simulate API call with realistic delay
    setTimeout(() => {
        const notification = document.querySelector('.transaction-notification[data-read="false"]');
        if (notification) {
            markAsRead(notification);
        }

        showEnhancedNotificationToast('Transaction confirmed successfully!', 'success');
        closeAllModals();

        // Reset button state
        activeButton.textContent = originalText;
        activeButton.disabled = false;
        activeButton.style.opacity = '1';
        if (spinner.parentNode) {
            spinner.remove();
        }

        // Provide haptic feedback if available
        if (navigator.vibrate) {
            navigator.vibrate(50);
        }

        console.log('Transaction confirmed successfully');
    }, 1500);
}

function markAsRead(notificationElement) {
    console.log('Marking notification as read');

    // Optimistic update
    notificationElement.dataset.read = 'true';
    notificationElement.classList.add('read');

    // Update ARIA label
    const currentLabel = notificationElement.getAttribute('aria-label');
    if (currentLabel) {
        notificationElement.setAttribute('aria-label', currentLabel.replace(' (unread)', ''));
    }

    updateUnreadCount();

    // Refresh current tab view
    if (currentTab === 'unread') {
        setTimeout(() => switchTab('unread'), 100);
    }
}

function updateUnreadCount() {
    const unreadNotifications = document.querySelectorAll('[data-read="false"]');
    const unreadCount = unreadNotifications.length;

    console.log('Updating unread count:', unreadCount);

    const unreadTab = document.getElementById('tab-unread');
    if (unreadTab) {
        const tabText = unreadCount > 0 ? `Unread (${unreadCount})` : 'Unread';
        unreadTab.textContent = tabText;
        unreadTab.setAttribute('aria-label', `${tabText} notifications`);

        // Add visual indicator for unread count
        if (unreadCount > 0) {
            unreadTab.setAttribute('data-count', unreadCount);
        } else {
            unreadTab.removeAttribute('data-count');
        }
    }
}

// Enhanced performance monitoring
function initializePerformanceMonitoring() {
    if (typeof PerformanceObserver !== 'undefined') {
        const observer = new PerformanceObserver((list) => {
            for (const entry of list.getEntries()) {
                if (entry.entryType === 'largest-contentful-paint') {
                    console.log('LCP:', entry.startTime);
                }
            }
        });

        try {
            observer.observe({ entryTypes: ['largest-contentful-paint'] });
        } catch (e) {
            console.log('Performance observer not supported');
        }
    }
}

// Initialize page state with enhanced error handling
window.addEventListener('load', function () {
    console.log('Page loaded, initializing enhanced notification system...');

    try {
        let activeTab = 'all';
        if (typeof Storage !== 'undefined') {
            activeTab = sessionStorage.getItem('activeNotificationTab') || 'all';
        }

        switchTab(activeTab);

        // Enhanced initialization sequence
        setTimeout(() => {
            updateViewportState();
            updatePageLayout();
            updateLayoutForCurrentViewport();
            updateNotificationContainerHeight();

            // Ensure sidebar state is properly applied for tablets
            if (viewportState.isTablet) {
                updateSidebarStateForTablet();
                adjustTabletSpecificLayout();
            }

            initializePerformanceMonitoring();
        }, 100);

    } catch (error) {
        console.error('Error during initialization:', error);
        // Fallback to basic functionality
        switchTab('all');
    }
});

// Enhanced page visibility handling
document.addEventListener('visibilitychange', function () {
    if (!document.hidden) {
        // Refresh layout when page becomes visible
        updatePageLayout();
        updateNotificationContainerHeight();
        updateViewportState();

        // Check for any layout shifts that might have occurred
        requestAnimationFrame(() => {
            updateLayoutForCurrentViewport();
        });
    }
});

// Service Worker registration for enhanced offline support
function registerServiceWorker() {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/sw.js')
            .then(() => console.log('Service Worker registered'))
            .catch(() => console.log('Service Worker registration failed'));
    }
}

// Enhanced error boundary
window.addEventListener('error', function (event) {
    console.error('Global error caught:', event.error);

    // Try to recover gracefully
    setTimeout(() => {
        try {
            updateViewportState();
            updateLayoutForCurrentViewport();
        } catch (recoveryError) {
            console.error('Recovery failed:', recoveryError);
        }
    }, 100);
});

// Export enhanced functions for global access
window.switchTab = switchTab;
window.openModal = openModal;
window.closeModal = closeModal;
window.confirmTransaction = confirmTransaction;
window.debugNotifications = function () {
    console.log('=== Enhanced Notification Debug Info ===');
    console.log('Current tab:', currentTab);
    console.log('Viewport state:', viewportState);
    console.log('Is modal open:', isModalOpen);
    console.log('Unread count:', document.querySelectorAll('[data-read="false"]').length);
    console.log('Total notifications:', document.querySelectorAll('.notification-card').length);
    console.log('Performance info:', performance.now());
};

// Add CSS for spinner animation
const style = document.createElement('style');
style.textContent = `
    @keyframes spin {
        0% { transform: rotate(0deg); }
        100% { transform: rotate(360deg); }
    }
    .sr-only {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        border: 0;
    }
    .load-more-btn:hover {
        background-color: var(--primary-color) !important;
        color: white !important;
        transform: translateY(-1px);
    }
    .notification-card.visible {
        animation: fadeInUp 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    }
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(20px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
`;
document.head.appendChild(style);

console.log('Enhanced NotificationPage.js loaded successfully with improved responsiveness');