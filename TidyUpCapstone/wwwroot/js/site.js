// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Add this to your existing site.js file

// Page theme management
function initializePageTheme() {
    // Get the current page type from body class
    const bodyClasses = document.body.classList;
    let pageType = 'main'; // default

    // Check for page-specific classes
    if (bodyClasses.contains('quest-page')) {
        pageType = 'quest';
    } else if (bodyClasses.contains('shop-page')) {
        pageType = 'shop';
    } else if (bodyClasses.contains('main-page')) {
        pageType = 'main';
    }

    // Apply theme-specific behaviors
    applyPageTheme(pageType);
}

function applyPageTheme(pageType) {
    const sidebar = document.getElementById('sidebar');
    const dockPanel = document.getElementById('dockPanel');
    const mainContent = document.querySelector('.main-content');

    // Add smooth transitions
    if (sidebar) {
        sidebar.style.transition = 'all 0.3s ease';
    }

    if (dockPanel) {
        dockPanel.style.transition = 'all 0.3s ease';
    }

    if (mainContent) {
        mainContent.style.transition = 'all 0.3s ease';
    }

    // Apply page-specific adjustments
    switch (pageType) {
        case 'quest':
            // Quest page: green body, light sidebar
            console.log('Quest page theme applied');
            break;
        case 'main':
            // Main page: light body, green sidebar
            console.log('Main page theme applied');
            break;
        case 'shop':
            // Shop page: custom theme
            console.log('Shop page theme applied');
            break;
        default:
            console.log('Default theme applied');
    }
}

// Function to change page theme dynamically (useful for navigation)
function changePageTheme(newPageType) {
    // Remove existing page type classes
    const bodyClasses = document.body.classList;
    bodyClasses.remove('main-page', 'quest-page', 'shop-page');

    // Add new page type class
    bodyClasses.add(`${newPageType}-page`);

    // Apply the new theme
    applyPageTheme(newPageType);
}

// Enhanced sidebar toggle function that respects themes
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const toggle = document.getElementById('icon-toggle');

    if (sidebar && toggle) {
        sidebar.classList.toggle('close');

        // Rotate toggle icon
        const toggleImg = toggle.querySelector('.toggle');
        if (toggleImg) {
            toggleImg.classList.toggle('rotate');
        }

        // Maintain theme consistency after toggle
        const currentPageType = getCurrentPageType();
        setTimeout(() => {
            applyPageTheme(currentPageType);
        }, 300); // Wait for transition to complete
    }
}

// Helper function to get current page type
function getCurrentPageType() {
    const bodyClasses = document.body.classList;
    if (bodyClasses.contains('quest-page')) return 'quest';
    if (bodyClasses.contains('shop-page')) return 'shop';
    return 'main';
}

// Enhanced navigation handling with theme switching
function handleNavigation(pageType, url) {
    // Change theme immediately for smooth transition
    changePageTheme(pageType);

    // Navigate to the new page
    if (url) {
        window.location.href = url;
    }
}

// Add navigation event listeners
function setupThemeNavigation() {
    // Setup sidebar navigation links
    const navLinks = document.querySelectorAll('#sidebar a[href]');
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');

            // Determine page type based on URL or data attribute
            let pageType = 'main';
            if (href.includes('Quest') || this.dataset.pageType === 'quest') {
                pageType = 'quest';
            } else if (href.includes('Shop') || this.dataset.pageType === 'shop') {
                pageType = 'shop';
            }

            // Don't prevent default, let normal navigation happen
            // The new page will have the correct class in its ViewData
        });
    });

    // Setup dock navigation links (mobile)
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

// Quest page specific interactivity
function initializeQuestPageInteractions() {
    if (!document.body.classList.contains('quest-page')) return;

    // Quest card interactions
    const questCards = document.querySelectorAll('.quest-card');
    questCards.forEach(card => {
        card.addEventListener('click', function () {
            this.style.transform = 'scale(1.02)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });
    });

    // Category tag interactions
    const categoryTags = document.querySelectorAll('.category-tag');
    categoryTags.forEach(tag => {
        tag.addEventListener('click', function () {
            this.style.background = 'rgba(255, 255, 255, 0.4)';
            setTimeout(() => {
                this.style.background = 'rgba(255, 255, 255, 0.2)';
            }, 200);
        });
    });

    // Quest button interactions
    const questButtons = document.querySelectorAll('.quest-card button');
    questButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();
            console.log('Quest button clicked:', this.textContent);

            // Add your ASP.NET Core MVC action calls here
            // Example: 
            // if (this.textContent === 'Start') {
            //     startQuest(questId);
            // } else if (this.textContent === 'Claim') {
            //     claimQuest(questId);
            // }
        });
    });
}

// Initialize everything when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializePageTheme();
    setupThemeNavigation();
    initializeQuestPageInteractions();
});

// Handle page visibility changes to maintain theme consistency
document.addEventListener('visibilitychange', function () {
    if (!document.hidden) {
        // Reapply theme when page becomes visible
        const currentPageType = getCurrentPageType();
        applyPageTheme(currentPageType);
    }
});

// Utility functions for ASP.NET Core integration
function startQuest(questId) {
    // Example AJAX call to start a quest
    fetch('/Home/StartQuest', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify({ questId: questId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Quest started successfully');
                // Update UI accordingly
            }
        })
        .catch(error => {
            console.error('Error starting quest:', error);
        });
}

function claimQuest(questId) {
    // Example AJAX call to claim a quest reward
    fetch('/Home/ClaimQuest', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify({ questId: questId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Quest claimed successfully');
                // Update UI to show claimed state
            }
        })
        .catch(error => {
            console.error('Error claiming quest:', error);
        });
}