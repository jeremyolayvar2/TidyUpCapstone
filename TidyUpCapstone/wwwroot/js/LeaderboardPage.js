document.addEventListener('DOMContentLoaded', function () {
    initializeLeaderboard();
});

function initializeLeaderboard() {
    initializeFilters();
    initializeAnimations();
    initializeProfileButtons();
    initializeResponsiveFeatures();
    initializeSidebarResponsiveness();
    enforceFirstPlacePositioning();
    setupResponsivePositioning();
}

// Enhanced function to ensure first place positioning based on screen size
function enforceFirstPlacePositioning() {
    const topThreeContainer = document.querySelector('.top-three-container');
    if (!topThreeContainer) return;

    const isMobile = window.innerWidth <= 768;

    if (isMobile) {
        // Mobile: 1st place should be first (order: 1)
        setMobilePositioning();
    } else {
        // Desktop: 1st place should be middle (order: 2)
        setDesktopPositioning();
    }
}

function setMobilePositioning() {
    const firstPlace = document.querySelector('.winner-card.first-place');
    const secondPlace = document.querySelector('.winner-card.second-place');
    const thirdPlace = document.querySelector('.winner-card.third-place');

    if (firstPlace && secondPlace && thirdPlace) {
        firstPlace.style.order = '1';  // First on mobile
        secondPlace.style.order = '2'; // Second on mobile
        thirdPlace.style.order = '3';  // Third on mobile

        // Ensure proper styling for mobile
        [firstPlace, secondPlace, thirdPlace].forEach(card => {
            card.style.margin = '0 auto';
            card.style.display = 'flex';
            card.style.flexDirection = 'column';
            card.style.alignItems = 'center';
            card.style.textAlign = 'center';
        });

        // Set appropriate scaling for mobile
        firstPlace.style.transform = 'scale(1.05)';
        secondPlace.style.transform = 'scale(0.95)';
        thirdPlace.style.transform = 'scale(0.95)';
    }
}

function setDesktopPositioning() {
    const firstPlace = document.querySelector('.winner-card.first-place');
    const secondPlace = document.querySelector('.winner-card.second-place');
    const thirdPlace = document.querySelector('.winner-card.third-place');

    if (firstPlace && secondPlace && thirdPlace) {
        firstPlace.style.order = '2';  // Middle on desktop
        secondPlace.style.order = '1'; // Left on desktop  
        thirdPlace.style.order = '3';  // Right on desktop

        // Reset mobile-specific styles
        [firstPlace, secondPlace, thirdPlace].forEach(card => {
            card.style.margin = '';
            card.style.display = '';
            card.style.flexDirection = '';
            card.style.alignItems = '';
            card.style.textAlign = '';
        });

        // Set appropriate scaling for desktop
        const scale = window.innerWidth <= 900 ? 'scale(1.02)' :
            window.innerWidth <= 1024 ? 'scale(1.05)' : 'scale(1.08)';
        firstPlace.style.transform = scale;

        const smallerScale = window.innerWidth <= 900 ? 'scale(0.90)' :
            window.innerWidth <= 1024 ? 'scale(0.92)' : 'scale(0.95)';
        secondPlace.style.transform = smallerScale;
        thirdPlace.style.transform = smallerScale;
    }
}

function setupResponsivePositioning() {
    let resizeTimeout;

    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            enforceFirstPlacePositioning();
        }, 100);
    });

    // Initial positioning
    enforceFirstPlacePositioning();
}

// Enhanced sidebar responsiveness handler
function initializeSidebarResponsiveness() {
    const sidebar = document.getElementById('sidebar');

    if (!sidebar) return;

    // Listen for the custom sidebar toggle event from site.js
    document.addEventListener('sidebarToggled', function (event) {
        handleSidebarStateChange(event.detail.isOpen);
    });

    // Handle window resize events
    window.addEventListener('resize', debounce(() => {
        adjustLayoutForSidebar();
        enforceFirstPlacePositioning(); // Ensure positioning is maintained
    }, 250));

    // Initial adjustment
    adjustLayoutForSidebar();
}

function handleSidebarStateChange(isOpen) {
    adjustLayoutForSidebar();
    // Maintain positioning after sidebar changes
    setTimeout(enforceFirstPlacePositioning, 100);
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
    const sidebarWidth = isClosed ? 12 : 250;

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
        backgroundText.style.transform = 'translate(-50%, -200px)';
    }

    if (leaderboardContainer) {
        leaderboardContainer.style.marginLeft = '0';
        leaderboardContainer.style.paddingLeft = '1rem';
    }

    if (tableContainer) {
        tableContainer.style.maxWidth = '1200px';
    }

    // Ensure mobile positioning is applied
    setMobilePositioning();
}

// Filter functionality with enhanced positioning maintenance
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

            // Apply filter with animation and maintain positioning
            applyFilter(filterType);
        });
    });
}

// Apply filter with smooth transition and consistent positioning
function applyFilter(filterType) {
    const container = document.querySelector('.leaderboard-container');

    // Add loading state
    container.style.pointerEvents = 'none';

    // Use the new animation function for filtering
    animateFilterTransition(() => {
        updateLeaderboardData(filterType);
    });

    // Remove loading state and show feedback after animation completes
    setTimeout(() => {
        container.style.pointerEvents = 'auto';
        showFilterFeedback(filterType);
    }, 800);
}

function updateLeaderboardData(filterType) {
    // Sample data for different filters - position property helps maintain order
    const sampleData = {
        'all-time': [
            { name: 'Kate Gonzales', rank: 1, items: 251, streak: 24, level: 602, position: 'first' },
            { name: 'Deither Arias', rank: 2, items: 178, streak: 19, level: 472, position: 'second' },
            { name: 'Jiro Llaguno', rank: 3, items: 142, streak: 11, level: 401, position: 'third' }
        ],
        'weekly': [
            { name: 'Maria Santos', rank: 1, items: 45, streak: 7, level: 285, position: 'first' },
            { name: 'Juan Dela Cruz', rank: 2, items: 38, streak: 6, level: 268, position: 'second' },
            { name: 'Anna Reyes', rank: 3, items: 32, streak: 5, level: 245, position: 'third' }
        ],
        'daily': [
            { name: 'Pedro Garcia', rank: 1, items: 12, streak: 1, level: 180, position: 'first' },
            { name: 'Lisa Wong', rank: 2, items: 9, streak: 1, level: 165, position: 'second' },
            { name: 'Carlos Lopez', rank: 3, items: 7, streak: 1, level: 148, position: 'third' }
        ]
    };

    const data = sampleData[filterType] || sampleData['all-time'];

    // Update cards while maintaining their position classes and order
    data.forEach(cardData => {
        let targetCard;

        // Find the correct card based on position, not current rank
        if (cardData.position === 'first') {
            targetCard = document.querySelector('.winner-card.first-place');
        } else if (cardData.position === 'second') {
            targetCard = document.querySelector('.winner-card.second-place');
        } else if (cardData.position === 'third') {
            targetCard = document.querySelector('.winner-card.third-place');
        }

        if (targetCard) {
            // Add updating animation
            targetCard.style.opacity = '0.7';
            targetCard.style.transition = 'all 0.3s ease';

            setTimeout(() => {
                const nameElement = targetCard.querySelector('.winner-name');
                const statsNumbers = targetCard.querySelectorAll('.stat-number');

                // Update content
                nameElement.innerHTML = `${cardData.name} <span class="rank-number">#${cardData.rank}</span>`;
                statsNumbers[0].textContent = cardData.items;
                statsNumbers[1].textContent = `${cardData.streak}d`;
                statsNumbers[2].textContent = cardData.level;

                // Restore card appearance
                targetCard.style.opacity = '1';

                // Ensure proper positioning is maintained
                setTimeout(() => {
                    enforceFirstPlacePositioning();
                }, 50);

            }, 300);
        }
    });

    // Update table data as well
    updateTableData(filterType, data);
}

function updateTableData(filterType, topThreeData) {
    // Sample table data for different filters
    const tableData = {
        'all-time': [
            { rank: 4, name: 'Russel Saldivar', items: 98, streak: 10, level: 373 },
            { rank: 5, name: 'Joaquin Bordado', items: 80, streak: 11, level: 371 },
            { rank: 6, name: 'Nardong Putik', items: 78, streak: 8, level: 302 },
            { rank: 7, name: 'Tonyong Bayawak', items: 58, streak: 8, level: 290 }
        ],
        'weekly': [
            { rank: 4, name: 'Mike Johnson', items: 28, streak: 4, level: 285 },
            { rank: 5, name: 'Sarah Lee', items: 25, streak: 3, level: 268 },
            { rank: 6, name: 'Tom Wilson', items: 22, streak: 3, level: 245 },
            { rank: 7, name: 'Emma Davis', items: 20, streak: 2, level: 230 }
        ],
        'daily': [
            { rank: 4, name: 'Alex Chen', items: 5, streak: 1, level: 180 },
            { rank: 5, name: 'Maya Patel', items: 4, streak: 1, level: 165 },
            { rank: 6, name: 'Jake Miller', items: 3, streak: 1, level: 150 },
            { rank: 7, name: 'Zoe Taylor', items: 3, streak: 1, level: 148 }
        ]
    };

    const data = tableData[filterType] || tableData['all-time'];
    const tableBody = document.querySelector('.leaderboard-table tbody');

    if (tableBody) {
        // Add fade out animation
        tableBody.style.opacity = '0.7';
        tableBody.style.transition = 'opacity 0.3s ease';

        setTimeout(() => {
            // Clear existing rows
            tableBody.innerHTML = '';

            // Add new rows
            data.forEach(rowData => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td><span class="rank-badge">#${rowData.rank}</span></td>
                    <td>${rowData.name}</td>
                    <td>${rowData.items}</td>
                    <td>${rowData.streak}</td>
                    <td>${rowData.level}</td>
                `;
                tableBody.appendChild(row);
            });

            // Fade back in
            tableBody.style.opacity = '1';
        }, 300);
    }
}

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

    // Adjust position for mobile
    if (window.innerWidth <= 768) {
        feedback.style.top = 'auto';
        feedback.style.bottom = '2rem';
        feedback.style.left = '50%';
        feedback.style.right = 'auto';
        feedback.style.transform = 'translateX(-50%) translateY(50px)';
        feedback.style.fontSize = '0.9rem';
        feedback.style.padding = '0.5rem 1rem';
    }

    document.body.appendChild(feedback);

    // Animate in
    setTimeout(() => {
        feedback.style.opacity = '1';
        if (window.innerWidth <= 768) {
            feedback.style.transform = 'translateX(-50%) translateY(0)';
        } else {
            feedback.style.transform = 'translateX(0)';
        }
    }, 100);

    // Remove after delay
    setTimeout(() => {
        feedback.style.opacity = '0';
        if (window.innerWidth <= 768) {
            feedback.style.transform = 'translateX(-50%) translateY(50px)';
        } else {
            feedback.style.transform = 'translateX(100px)';
        }
        setTimeout(() => {
            if (feedback.parentNode) {
                feedback.remove();
            }
        }, 300);
    }, 2500);
}

// Initialize animations with positioning enforcement
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
        animatedElements.forEach((el, index) => {
            el.style.opacity = '1';
            el.style.transform = 'translateY(0)';
        });

        // Enforce positioning after initial animations
        setTimeout(() => {
            enforceFirstPlacePositioning();
        }, 600);
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

// Responsive features with enhanced positioning
function initializeResponsiveFeatures() {
    initializeResponsiveTable();
    initializeTouchSupport();
    handleOrientationChange();
    initializeKeyboardSupport();
}

// Responsive table handling
function initializeResponsiveTable() {
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
        enforceFirstPlacePositioning(); // Maintain positioning on resize
    }, 250));

    // Initial check
    updateScrollIndicators();
}

// Touch support for mobile devices
function initializeTouchSupport() {
    const touchElements = document.querySelectorAll('.filter-btn, .profile-btn, .winner-card');

    touchElements.forEach(element => {
        element.addEventListener('touchstart', function () {
            // Add touch feedback while preserving transforms
            this.style.opacity = '0.8';
        });

        element.addEventListener('touchend', function () {
            setTimeout(() => {
                this.style.opacity = '1';
                // Ensure positioning is maintained after touch
                enforceFirstPlacePositioning();
            }, 150);
        });
    });
}

// Handle orientation changes with positioning maintenance
function handleOrientationChange() {
    window.addEventListener('orientationchange', function () {
        setTimeout(() => {
            initializeResponsiveTable();
            adjustLayoutForSidebar();
            enforceFirstPlacePositioning(); // Critical for orientation changes
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

// Export functions for external use
window.LeaderboardPage = {
    applyFilter,
    initializeLeaderboard,
    updateLeaderboardData,
    showFilterFeedback,
    adjustLayoutForSidebar,
    enforceFirstPlacePositioning,
    setMobilePositioning,
    setDesktopPositioning
};