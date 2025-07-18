// site.js - Main site functionality and core features
// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// CORE SITE INITIALIZATION -----------------------------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
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
    initializeMessages();
}

// Sidebar functionality
function initializeSidebar() {
    const toggleButton = document.getElementById('icon-toggle');
    const sidebar = document.getElementById('sidebar');

    if (toggleButton && sidebar) {
        // Initialize sidebar state
        setupSidebarToggle();
    }
}

function setupSidebarToggle() {
    // Sidebar toggle is handled by global toggleSidebar function
    // This ensures consistent behavior across all pages
}

// Global sidebar toggle function
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const layoutWrapper = document.querySelector('.layout-wrapper');
    const toggleIcon = document.querySelector('#icon-toggle img');

    if (sidebar && layoutWrapper) {
        sidebar.classList.toggle('close');

        // Add/remove class to layout wrapper for CSS targeting
        if (sidebar.classList.contains('close')) {
            layoutWrapper.classList.add('sidebar-closed');
        } else {
            layoutWrapper.classList.remove('sidebar-closed');
        }

        // Toggle icon rotation
        if (toggleIcon) {
            toggleIcon.classList.toggle('rotate');
        }

        // Maintain theme consistency after toggle
        const currentPageType = getCurrentPageType();
        setTimeout(() => {
            applyPageTheme(currentPageType);
        }, 300); // Wait for transition to complete
    }
}

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

    // Apply theme-specific behaviors
    applyPageTheme(pageType);
}

function getCurrentPageType() {
    const bodyClasses = document.body.classList;

    if (bodyClasses.contains('quest-page')) return 'quest';
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
    bodyClasses.remove('main-page', 'quest-page', 'shop-page');

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

            // Theme will be handled by the new page
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

    return 'main';
}

// GLOBAL FEATURES -----------------------------------------------------------------------------------------
function initializeGlobalFeatures() {
    // Features that work on all pages
    initializeAccessibility();
    initializePerformanceOptimizations();
    setupGlobalEventListeners();
}

function initializeAccessibility() {
    // Add keyboard navigation support
    document.addEventListener('keydown', function (e) {
        // ESC key to close modals/overlays
        if (e.key === 'Escape') {
            closeActiveModals();
        }
    });
}

function initializePerformanceOptimizations() {
    // Lazy loading for images
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
    // Handle page visibility changes
    document.addEventListener('visibilitychange', function () {
        if (!document.hidden) {
            // Reapply theme when page becomes visible
            const currentPageType = getCurrentPageType();
            applyPageTheme(currentPageType);
        }
    });

    // Handle window resize
    let resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            handleWindowResize();
        }, 250);
    });
}

function handleWindowResize() {
    // Handle responsive behavior on resize
    const dockItems = document.querySelectorAll('.dock-item');
    dockItems.forEach(item => {
        item.style.transform = 'scale(1)';
    });
}

// STANDALONE PAGE FEATURES -----------------------------------------------------------------------------------------
function initializeStandalonePageFeatures() {
    // Features for pages without navigation (login, landing, etc.)
    console.log('Initializing standalone page features');

    // Add any standalone page specific functionality here
    initializeStandaloneInteractions();
}

function initializeStandaloneInteractions() {
    // Handle standalone page interactions
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            // Add form validation or loading states
            console.log('Form submitted:', form.id || form.className);
        });
    });
}

// UTILITY FUNCTIONS -----------------------------------------------------------------------------------------
function closeActiveModals() {
    // Close any open modals or overlays
    const activeModals = document.querySelectorAll('.modal.active, .overlay.active');
    activeModals.forEach(modal => {
        modal.classList.remove('active');
    });
}

// Error handling
window.addEventListener('error', function (e) {
    console.error('Global error:', e.error);
    // Add error reporting here if needed
});

// Export functions for use in other files
window.TidyUpSite = {
    toggleSidebar,
    getCurrentPageType,
    changePageTheme,
    applyPageTheme,
    closeActiveModals
};