

// CORE SITE INITIALIZATION -----------------------------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded - initializing site');

    // Initialize core functionality based on page type
    const layoutWrapper = document.querySelector('.layout-wrapper');

    if (layoutWrapper) {
        // Pages WITH navigation
        initializeNavigationFeatures();
        initializePageTheme();
        setupThemeNavigation();
    } else {
        // Pages WITHOUT navigation (like landing/login pages)
        initializeStandalonePageFeatures();
    }

    // Initialize global features
    initializeGlobalFeatures();
});

// NAVIGATION FEATURES -----------------------------------------------------------------------------------------
function initializeNavigationFeatures() {
    initializeSidebar();
    initializeMobileDock();
}

// Sidebar functionality - CLEANED UP
function initializeSidebar() {
    const toggleButton = document.getElementById('icon-toggle');
    const sidebar = document.getElementById('sidebar');

    console.log('Toggle button:', toggleButton);
    console.log('Sidebar:', sidebar);

    if (toggleButton && sidebar) {
        // Remove any existing onclick attribute to avoid conflicts
        toggleButton.removeAttribute('onclick');

        // Add click event listener
        toggleButton.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('Toggle button clicked');

            // Toggle sidebar
            sidebar.classList.toggle('close');

            // Update layout wrapper and body classes
            const layoutWrapper = document.querySelector('.layout-wrapper');
            if (layoutWrapper) {
                if (sidebar.classList.contains('close')) {
                    layoutWrapper.classList.add('sidebar-closed');
                    document.body.classList.add('sidebar-closed');
                    document.body.classList.remove('sidebar-open');
                    console.log('Sidebar closed');
                } else {
                    layoutWrapper.classList.remove('sidebar-closed');
                    document.body.classList.remove('sidebar-closed');
                    document.body.classList.add('sidebar-open');
                    console.log('Sidebar opened');
                }
            }

            // Toggle icon rotation
            const toggleIcon = document.querySelector('#icon-toggle img');
            if (toggleIcon) {
                toggleIcon.classList.toggle('rotate');
                console.log('Icon rotation toggled');
            }

            // Dispatch custom event for LeaderboardPage.js
            const event = new CustomEvent('sidebarToggled', {
                detail: {
                    isOpen: !sidebar.classList.contains('close'),
                    sidebar: sidebar
                }
            });
            document.dispatchEvent(event);
            console.log('Custom event dispatched');
        });

        console.log('Sidebar toggle initialized successfully');
    } else {
        console.error('Sidebar or toggle button not found');
        if (!toggleButton) console.error('Toggle button with id="icon-toggle" not found');
        if (!sidebar) console.error('Sidebar with id="sidebar" not found');
    }
}

// Global sidebar toggle function for fallback
window.toggleSidebar = function () {
    console.log('Global toggleSidebar called');
    const sidebar = document.getElementById('sidebar');
    const layoutWrapper = document.querySelector('.layout-wrapper');
    const toggleIcon = document.querySelector('#icon-toggle img');

    if (sidebar && layoutWrapper) {
        sidebar.classList.toggle('close');

        if (sidebar.classList.contains('close')) {
            layoutWrapper.classList.add('sidebar-closed');
            document.body.classList.add('sidebar-closed');
            document.body.classList.remove('sidebar-open');
        } else {
            layoutWrapper.classList.remove('sidebar-closed');
            document.body.classList.remove('sidebar-closed');
            document.body.classList.add('sidebar-open');
        }

        if (toggleIcon) {
            toggleIcon.classList.toggle('rotate');
        }

        // Dispatch custom event
        const event = new CustomEvent('sidebarToggled', {
            detail: {
                isOpen: !sidebar.classList.contains('close'),
                sidebar: sidebar
            }
        });
        document.dispatchEvent(event);
    }
};

// Mobile dock functionality
function initializeMobileDock() {
    const dockPanel = document.getElementById("dockPanel");
    const dockItems = document.querySelectorAll(".dock-item");

    if (dockPanel && dockItems.length > 0) {
        setupDockInteractions(dockPanel, dockItems);
        setupDockLabels(dockItems);
    }
}

function setupDockInteractions(dockPanel, dockItems) {
    const distance = 100;
    const maxScale = 1.8;

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

function setupDockLabels(dockItems) {
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
}

// PAGE THEME MANAGEMENT -----------------------------------------------------------------------------------------
function initializePageTheme() {
    const layoutWrapper = document.querySelector('.layout-wrapper');
    if (!layoutWrapper) return;

    // Get the current page type from body class
    const pageType = getCurrentPageType();
    console.log('Current page type:', pageType);

    // Apply theme-specific behaviors
    applyPageTheme(pageType);
}

function getCurrentPageType() {
    const bodyClasses = document.body.classList;

    if (bodyClasses.contains('quest-page')) return 'quest';
    if (bodyClasses.contains('leaderboard-page')) return 'leaderboard';
    if (bodyClasses.contains('shop-page')) return 'shop';
    if (bodyClasses.contains('main-page')) return 'main';

    return 'main'; // default
}

function applyPageTheme(pageType) {
    const sidebar = document.getElementById('sidebar');
    const dockPanel = document.getElementById('dockPanel');
    const mainContent = document.querySelector('.main-content');

    // Add smooth transitions
    [sidebar, dockPanel, mainContent].forEach(element => {
        if (element) {
            element.style.transition = 'all 0.3s ease';
        }
    });

    // Apply page-specific adjustments
    switch (pageType) {
        case 'quest':
            console.log('Quest page theme applied');
            break;
        case 'leaderboard':
            console.log('Leaderboard page theme applied');
            break;
        case 'main':
            console.log('Main page theme applied');
            break;
        case 'shop':
            console.log('Shop page theme applied');
            break;
        default:
            console.log('Default theme applied');
    }
}

function changePageTheme(newPageType) {
    // Remove existing page type classes
    const bodyClasses = document.body.classList;
    bodyClasses.remove('main-page', 'quest-page', 'shop-page', 'leaderboard-page');

    // Add new page type class
    bodyClasses.add(`${newPageType}-page`);

    // Apply the new theme
    applyPageTheme(newPageType);
}

// Navigation theme handling
function setupThemeNavigation() {
    setupSidebarNavigation();
    setupMobileNavigation();
}

function setupSidebarNavigation() {
    const navLinks = document.querySelectorAll('#sidebar a[href]');
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            const href = this.getAttribute('href');
            let pageType = determinePageType(href, this.dataset.pageType);

            console.log(`Navigating to ${pageType} page`);
        });
    });
}

function setupMobileNavigation() {
    const dockItems = document.querySelectorAll('.dock-item');
    dockItems.forEach(item => {
        item.addEventListener('click', function () {
            const label = this.dataset.label?.toLowerCase();
            let pageType = 'main';

            if (label === 'quests/milestone') {
                pageType = 'quest';
            } else if (label === 'shop') {
                pageType = 'shop';
            } else if (label === 'leaderboards') {
                pageType = 'leaderboard';
            }

            // Apply theme change for visual feedback
            changePageTheme(pageType);
        });
    });
}

function determinePageType(href, dataPageType) {
    if (dataPageType) return dataPageType;

    if (href && href.includes('Quest')) return 'quest';
    if (href && href.includes('Shop')) return 'shop';
    if (href && href.includes('Leaderboard')) return 'leaderboard';

    return 'main';
}

// GLOBAL FEATURES -----------------------------------------------------------------------------------------
function initializeGlobalFeatures() {
    initializeAccessibility();
    initializePerformanceOptimizations();
    setupGlobalEventListeners();
}

function initializeAccessibility() {
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeActiveModals();
        }
    });
}

function initializePerformanceOptimizations() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                        imageObserver.unobserve(img);
                    }
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

function setupGlobalEventListeners() {
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) {
            const currentPageType = getCurrentPageType();
            applyPageTheme(currentPageType);
        }
    });

    let resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            handleWindowResize();
        }, 250);
    });
}

function handleWindowResize() {
    const dockItems = document.querySelectorAll('.dock-item');
    dockItems.forEach(item => {
        item.style.transform = 'scale(1)';
    });
}

// STANDALONE PAGE FEATURES -----------------------------------------------------------------------------------------
function initializeStandalonePageFeatures() {
    console.log('Initializing standalone page features');
    initializeStandaloneInteractions();
}

function initializeStandaloneInteractions() {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            console.log('Form submitted:', form.id || form.className);
        });
    });
}

// UTILITY FUNCTIONS -----------------------------------------------------------------------------------------
function closeActiveModals() {
    const activeModals = document.querySelectorAll('.modal.active, .overlay.active');
    activeModals.forEach(modal => {
        modal.classList.remove('active');
    });
}

// Error handling
window.addEventListener('error', function (e) {
    console.error('Global error:', e.error);
});

// Export functions for use in other files
window.TidyUpSite = {
    toggleSidebar: window.toggleSidebar,
    getCurrentPageType,
    changePageTheme,
    applyPageTheme,
    closeActiveModals
};