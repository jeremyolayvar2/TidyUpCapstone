document.addEventListener('DOMContentLoaded', function () {
    initializeLeaderboard();
});

function initializeLeaderboard() {
    initializeFilters();
    initializeAnimations();
    initializeProfileButtons();
    initializeResponsiveFeatures();
    initializeSidebarResponsiveness();
}

// Enhanced sidebar responsiveness handler
function initializeSidebarResponsiveness() {
    const sidebar = document.getElementById('sidebar');
    const toggleButton = sidebar?.querySelector('#icon-toggle');

    if (!sidebar) return;

    // Set initial sidebar state class
    updateSidebarState();

    // Listen for sidebar toggle events
    if (toggleButton) {
        toggleButton.addEventListener('click', handleSidebarToggle);
    }

    // Monitor sidebar state changes using MutationObserver
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                handleSidebarStateChange();
            }
        });
    });

    observer.observe(sidebar, {
        attributes: true,
        attributeFilter: ['class']
    });

    // Handle window resize events
    window.addEventListener('resize', debounce(() => {
        updateSidebarState();
        adjustLayoutForSidebar();
    }, 250));

    // Initial adjustment
    adjustLayoutForSidebar();
}

function handleSidebarToggle() {
    // Small delay to ensure CSS transitions work smoothly
    setTimeout(() => {
        updateSidebarState();
        handleSidebarStateChange();
    }, 50);
}

function handleSidebarStateChange() {
    updateSidebarState();
    adjustLayoutForSidebar();
    
    // Add smooth transition feedback
    showSidebarTransitionFeedback();
}

function updateSidebarState() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar || window.innerWidth <= 768) return;

    const isClosed = sidebar.classList.contains('close');
    
    // Update body class for CSS targeting
    if (isClosed) {
        document.body.classList.add('sidebar-closed');
        document.body.classList.remove('sidebar-open');
    } else {
        document.body.classList.add('sidebar-open');
        document.body.classList.remove('sidebar-closed');
    }
}

function adjustLayoutForSidebar() {
    const sidebar = document.getElementById('sidebar');
    const backgroundText = document.querySelector('.background-text');
    const leaderboardContainer = document.querySelector('.leaderboard-container');
    const topThreeContainer = document.querySelector('.top-three-container');
    const tableContainer = document.querySelector('.leaderboard-table-container');

    // Only adjust for desktop screens
    if (!sidebar || window.innerWidth <= 768) {
        resetMobileLayout();
        return;
    }

    const isClosed = sidebar.classList.contains('close');
    const sidebarWidth = isClosed ? 82 : 250;

    // Adjust background text positioning
    if (backgroundText) {
        backgroundText.style.left = `calc(50% + ${sidebarWidth / 2}px)`;
        backgroundText.style.transition = 'all 300ms ease-in-out';
    }

    // Adjust container margins and padding
    if (leaderboardContainer) {
        leaderboardContainer.style.marginLeft = `${sidebarWidth}px`;
        leaderboardContainer.style.paddingLeft = isClosed ? '1.5rem' : '2rem';
        leaderboardContainer.style.transition = 'all 300ms ease-in-out';
    }

    // Optimize content width based on available space
    const availableWidth = window.innerWidth - sidebarWidth;
    
    if (topThreeContainer && availableWidth < 900) {
        // Adjust card layout for smaller available space
        topThreeContainer.style.gap = '1rem';
        
        const cards = topThreeContainer.querySelectorAll('.winner-card');
        cards.forEach(card => {
            card.style.padding = '1.5rem';
        });
    } else if (topThreeContainer) {
        // Reset to default for larger spaces
        topThreeContainer.style.gap = '2rem';
        
        const cards = topThreeContainer.querySelectorAll('.winner-card');
        cards.forEach(card => {
            card.style.padding = '2rem';
        });
    }

    // Adjust table container max-width
    if (tableContainer) {
        const maxWidth = Math.min(1200, availableWidth - 64); // 32px padding on each side
        tableContainer.style.maxWidth = `${maxWidth}px`;
        tableContainer.style.transition = 'all 300ms ease-in-out';
    }
}

function resetMobileLayout() {
    const backgroundText = document.querySelector('.background-text');
    const leaderboardContainer = document.querySelector('.leaderboard-container');
    const tableContainer = document.querySelector('.leaderboard-table-container');

    // Reset styles for mobile
    if (backgroundText) {
        backgroundText.style.left = '50%';
        backgroundText.style.transform = 'translate(-50%, -50%)';
    }

    if (leaderboardContainer) {
        leaderboardContainer.style.marginLeft = '0';
        leaderboardContainer.style.paddingLeft = '1rem';
    }

    if (tableContainer) {
        tableContainer.style.maxWidth = '1200px';
    }
}

//function showSidebarTransitionFeedback() {
//    // Visual feedback for sidebar state change
//    const feedback = document.createElement('div');
//    const sidebar = document.getElementById('sidebar');
//    const isClosed = sidebar?.classList.contains('close');
    
//    feedback.textContent = isClosed ? 'Sidebar minimized' : 'Sidebar expanded';
//    feedback.className = 'sidebar-feedback';
//    feedback.style.cssText = `
//        position: fixed;
//        top: 1rem;
//        left: 50%;
//        transform: translateX(-50%);
//        background: var(--primary-color);
//        color: white;
//        padding: 0.5rem 1rem;
//        border-radius: 20px;
//        font-size: 0.875rem;
//        font-weight: 600;
//        z-index: 10000;
//        opacity: 0;
//        transition: all 0.3s ease;
//        pointer-events: none;
//    `;

//    document.body.appendChild(feedback);

//    // Animate in
//    setTimeout(() => {
//        feedback.style.opacity = '1';
//        feedback.style.transform = 'translateX(-50%) translateY(0)';
//    }, 100);

//    // Remove after delay
//    setTimeout(() => {
//        feedback.style.opacity = '0';
//        feedback.style.transform = 'translateX(-50%) translateY(-20px)';
//        setTimeout(() => {
//            if (feedback.parentNode) {
//                feedback.remove();
//            }
//        }, 300);
//    }, 1500);
//}

// Filter functionality
function initializeFilters() {
    const filterButtons = document.querySelectorAll('.filter-btn');

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Remove active class from all buttons
            filterButtons.forEach(btn => btn.classList.remove('active'));

            // Add active class to clicked button
            this.classList.add('active');

            // Get filter type
            const filterType = this.getAttribute('data-filter');

            // Apply filter with animation
            applyFilter(filterType);
        });
    });
}

// Apply filter with smooth transition
function applyFilter(filterType) {
    const container = document.querySelector('.leaderboard-container');

    // Add loading state
    container.style.opacity = '0.7';
    container.style.pointerEvents = 'none';

    // Show loading feedback
    showLoadingFeedback();

    // Simulate API call or data filtering
    setTimeout(() => {
        // Here you would typically make an AJAX call to get filtered data
        // For demo purposes, we'll just simulate the update
        updateLeaderboardData(filterType);

        // Remove loading state
        container.style.opacity = '1';
        container.style.pointerEvents = 'auto';

        // Show success feedback
        showFilterFeedback(filterType);
    }, 800);
}

function updateLeaderboardData(filterType) {
    // Sample data for different filters
    const sampleData = {
        'all-time': [
            { name: 'Kate Gonzales', rank: 1, items: 251, streak: 24, level: 'Expert' },
            { name: 'Deither Arias', rank: 2, items: 178, streak: 19, level: 'Declutter Champ' },
            { name: 'Jiro Llaguno', rank: 3, items: 142, streak: 11, level: 'Enthusiast' }
        ],
        'weekly': [
            { name: 'Maria Santos', rank: 1, items: 45, streak: 7, level: 'Expert' },
            { name: 'Juan Dela Cruz', rank: 2, items: 38, streak: 6, level: 'Enthusiast' },
            { name: 'Anna Reyes', rank: 3, items: 32, streak: 5, level: 'Enthusiast' }
        ],
        'daily': [
            { name: 'Pedro Garcia', rank: 1, items: 12, streak: 1, level: 'Enthusiast' },
            { name: 'Lisa Wong', rank: 2, items: 9, streak: 1, level: 'Beginner' },
            { name: 'Carlos Lopez', rank: 3, items: 7, streak: 1, level: 'Beginner' }
        ]
    };

    const data = sampleData[filterType] || sampleData['all-time'];

    // Update top 3 cards with animation
    const winnerCards = document.querySelectorAll('.winner-card');
    winnerCards.forEach((card, index) => {
        if (data[index]) {
            // Add updating animation
            card.style.transform = 'scale(0.95)';
            card.style.opacity = '0.7';

            setTimeout(() => {
                const nameElement = card.querySelector('.winner-name');
                const statsNumbers = card.querySelectorAll('.stat-number');

                nameElement.innerHTML = `${data[index].name} <span class="rank-number">#${data[index].rank}</span>`;
                statsNumbers[0].textContent = data[index].items;
                statsNumbers[1].textContent = `${data[index].streak}d`;
                statsNumbers[2].textContent = data[index].level;

                // Restore card appearance
                card.style.transform = '';
                card.style.opacity = '';
            }, 300);
        }
    });
}

//function showLoadingFeedback() {
//    const loading = document.createElement('div');
//    loading.id = 'loading-indicator';
//    loading.innerHTML = `
//        <div class="loading-content">
//            <div class="loading-spinner"></div>
//            <span>Updating rankings...</span>
//        </div>
//    `;
//    loading.style.cssText = `
//        position: fixed;
//        top: 0;
//        left: 0;
//        right: 0;
//        bottom: 0;
//        background: rgba(107, 144, 128, 0.9);
//        display: flex;
//        justify-content: center;
//        align-items: center;
//        z-index: 9999;
//        backdrop-filter: blur(10px);
//    `;

//    const loadingContent = loading.querySelector('.loading-content');
//    loadingContent.style.cssText = `
//        text-align: center;
//        color: white;
//        font-weight: 600;
//        font-size: 1.1rem;
//    `;

//    const spinner = loading.querySelector('.loading-spinner');
//    spinner.style.cssText = `
//        width: 40px;
//        height: 40px;
//        border: 4px solid rgba(255, 255, 255, 0.3);
//        border-top: 4px solid white;
//        border-radius: 50%;
//        animation: spin 1s linear infinite;
//        margin: 0 auto 1rem;
//    `;

//    // Add spinner animation
//    if (!document.querySelector('#spinner-keyframes')) {
//        const style = document.createElement('style');
//        style.id = 'spinner-keyframes';
//        style.textContent = `
//            @keyframes spin {
//                0% { transform: rotate(0deg); }
//                100% { transform: rotate(360deg); }
//            }
//        `;
//        document.head.appendChild(style);
//    }

//    document.body.appendChild(loading);

//    // Remove loading after delay
//    setTimeout(() => {
//        if (loading.parentNode) {
//            loading.remove();
//        }
//    }, 700);
//}

function showFilterFeedback(filterType) {
    const feedback = document.createElement('div');
    feedback.textContent = `Showing ${filterType.replace('-', ' ')} rankings`;
    feedback.style.cssText = `
        position: fixed;
        top: 2rem;
        right: 2rem;
        background: var(--primary-color);
        color: white;
        padding: 0.75rem 1.5rem;
        border-radius: 50px;
        font-weight: 600;
        z-index: 1000;
        opacity: 0;
        transform: translateX(100px);
        transition: all 0.3s ease;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
    `;

    document.body.appendChild(feedback);

    // Animate in
    setTimeout(() => {
        feedback.style.opacity = '1';
        feedback.style.transform = 'translateX(0)';
    }, 100);

    // Remove after delay
    setTimeout(() => {
        feedback.style.opacity = '0';
        feedback.style.transform = 'translateX(100px)';
        setTimeout(() => {
            if (feedback.parentNode) {
                feedback.remove();
            }
        }, 300);
    }, 2500);
}

// Initialize animations
function initializeAnimations() {
    // Entrance animations
    const animatedElements = document.querySelectorAll('.winner-card, .leaderboard-table-container');

    animatedElements.forEach((el, index) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px)';
        el.style.transition = `all 0.6s ease ${index * 0.1}s`;
    });

    // Trigger animations
    setTimeout(() => {
        animatedElements.forEach(el => {
            el.style.opacity = '1';
            el.style.transform = 'translateY(0)';
        });
    }, 200);

    // Intersection Observer for scroll-based animations
    if ('IntersectionObserver' in window) {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                }
            });
        }, observerOptions);

        document.querySelectorAll('.winner-card, .leaderboard-table-container').forEach(el => {
            observer.observe(el);
        });
    }
}

// Profile button interactions
function initializeProfileButtons() {
    const profileButtons = document.querySelectorAll('.profile-btn');

    profileButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            // Get user info from parent card
            const card = this.closest('.winner-card');
            const userName = card.querySelector('.winner-name').textContent.split('#')[0].trim();

            // Add loading state
            const originalText = this.textContent;
            this.textContent = 'Loading...';
            this.disabled = true;

            // Add pulse animation
            this.style.animation = 'pulse 1s infinite';

            // Simulate profile loading
            setTimeout(() => {
                console.log(`Navigating to profile for: ${userName}`);

                // Reset button
                this.textContent = originalText;
                this.disabled = false;
                this.style.animation = '';

                // Show success message
                showProfileMessage(userName);

                // Here you would typically navigate:
                // window.location.href = `/Profile/${userId}`;
            }, 1200);
        });

        // Add ripple effect
        button.addEventListener('click', createRippleEffect);
    });

    // Add pulse animation keyframes
    if (!document.querySelector('#pulse-keyframes')) {
        const style = document.createElement('style');
        style.id = 'pulse-keyframes';
        style.textContent = `
            @keyframes pulse {
                0% { opacity: 1; }
                50% { opacity: 0.7; }
                100% { opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    }
}

function showProfileMessage(userName) {
    const message = document.createElement('div');
    message.textContent = `Opening ${userName}'s profile...`;
    message.style.cssText = `
        position: fixed;
        bottom: 2rem;
        left: 50%;
        transform: translateX(-50%);
        background: var(--secondary-color);
        color: white;
        padding: 1rem 2rem;
        border-radius: 50px;
        font-weight: 600;
        z-index: 1000;
        opacity: 0;
        transform: translateX(-50%) translateY(20px);
        transition: all 0.3s ease;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
    `;

    document.body.appendChild(message);

    setTimeout(() => {
        message.style.opacity = '1';
        message.style.transform = 'translateX(-50%) translateY(0)';
    }, 100);

    setTimeout(() => {
        message.style.opacity = '0';
        message.style.transform = 'translateX(-50%) translateY(20px)';
        setTimeout(() => {
            if (message.parentNode) {
                message.remove();
            }
        }, 300);
    }, 2000);
}

// Create ripple effect for buttons
function createRippleEffect(e) {
    const button = e.currentTarget;
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = e.clientX - rect.left - size / 2;
    const y = e.clientY - rect.top - size / 2;

    const ripple = document.createElement('span');
    ripple.style.cssText = `
        position: absolute;
        width: ${size}px;
        height: ${size}px;
        left: ${x}px;
        top: ${y}px;
        background: rgba(255, 255, 255, 0.3);
        border-radius: 50%;
        transform: scale(0);
        animation: ripple 0.6s linear;
        pointer-events: none;
    `;

    // Add ripple animation keyframes if not already added
    if (!document.querySelector('#ripple-keyframes')) {
        const style = document.createElement('style');
        style.id = 'ripple-keyframes';
        style.textContent = `
            @keyframes ripple {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);
    }

    button.style.position = 'relative';
    button.style.overflow = 'hidden';
    button.appendChild(ripple);

    setTimeout(() => ripple.remove(), 600);
}

// Responsive features
function initializeResponsiveFeatures() {
    initializeResponsiveTable();
    initializeTouchSupport();
    handleOrientationChange();
    initializeKeyboardSupport();
}

// Responsive table handling
function initializeResponsiveTable() {
    const table = document.querySelector('.leaderboard-table');
    const container = document.querySelector('.leaderboard-table-container');

    if (!container) return;

    // Add scroll indicators for mobile
    function updateScrollIndicators() {
        const canScrollLeft = container.scrollLeft > 0;
        const canScrollRight = container.scrollLeft < (container.scrollWidth - container.clientWidth);

        container.classList.toggle('can-scroll-left', canScrollLeft);
        container.classList.toggle('can-scroll-right', canScrollRight);
    }

    container.addEventListener('scroll', updateScrollIndicators);
    window.addEventListener('resize', debounce(() => {
        updateScrollIndicators();
        adjustLayoutForSidebar();
    }, 250));

    // Initial check
    updateScrollIndicators();
}

// Touch support for mobile devices
function initializeTouchSupport() {
    const touchElements = document.querySelectorAll('.filter-btn, .profile-btn, .winner-card');

    touchElements.forEach(element => {
        element.addEventListener('touchstart', function () {
            this.style.transform = 'scale(0.98)';
        });

        element.addEventListener('touchend', function () {
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
    });
}

// Handle orientation changes
function handleOrientationChange() {
    window.addEventListener('orientationchange', function () {
        setTimeout(() => {
            initializeResponsiveTable();
            adjustLayoutForSidebar();

            // Update background text positioning
            const backgroundText = document.querySelector('.background-text');
            if (backgroundText && window.innerWidth < 480) {
                backgroundText.style.writingMode = window.innerHeight > window.innerWidth ? 'vertical-lr' : 'horizontal-tb';
            }
        }, 300);
    });
}

// Keyboard navigation support
function initializeKeyboardSupport() {
    document.addEventListener('keydown', function (e) {
        // Filter navigation with keyboard
        const filterButtons = Array.from(document.querySelectorAll('.filter-btn'));
        const activeFilter = document.querySelector('.filter-btn.active');

        if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') {
            e.preventDefault();
            const currentIndex = filterButtons.indexOf(activeFilter);
            let nextIndex;

            if (e.key === 'ArrowLeft') {
                nextIndex = currentIndex > 0 ? currentIndex - 1 : filterButtons.length - 1;
            } else {
                nextIndex = currentIndex < filterButtons.length - 1 ? currentIndex + 1 : 0;
            }

            filterButtons[nextIndex].click();
            filterButtons[nextIndex].focus();
        }

        // Profile buttons with Enter/Space
        if ((e.key === 'Enter' || e.key === ' ') && e.target.classList.contains('profile-btn')) {
            e.preventDefault();
            e.target.click();
        }
    });

    // Make filter buttons focusable
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.setAttribute('tabindex', '0');
    });
}

// Utility functions
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

// Export functions for external use
window.LeaderboardPage = {
    applyFilter,
    initializeLeaderboard,
    updateLeaderboardData,
    showFilterFeedback,
    adjustLayoutForSidebar,
    updateSidebarState
};