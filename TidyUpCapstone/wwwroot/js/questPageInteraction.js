// questInteractions.js - Quest page specific functionality
// Handles quest cards, achievements, and quest-related interactions

document.addEventListener('DOMContentLoaded', function () {
    // Only initialize if we're on the quest page
    if (document.body.classList.contains('quest-page')) {
        initializeQuestPageInteractions();
    }
});

function initializeQuestPageInteractions() {
    console.log('Initializing quest page interactions');

    setupQuestCards();
    setupQuestButtons();
    setupCategoryTags();
    setupAchievementAnimations();
    setupQuestKeyboardNavigation();
    initializeQuestObservers();
}

// Quest Card Interactions
function setupQuestCards() {
    const questCards = document.querySelectorAll('.quest-card');

    questCards.forEach((card, index) => {
        // Add accessibility attributes
        card.setAttribute('tabindex', '0');
        card.setAttribute('role', 'button');
        card.setAttribute('aria-label', `Quest card ${index + 1}`);

        // Mouse interactions
        card.addEventListener('mouseenter', function () {
            if (window.innerWidth > 768) {
                this.style.transform = 'translateY(-3px)';
                this.style.boxShadow = '0 8px 25px rgba(0,0,0,0.15)';
            }
        });

        card.addEventListener('mouseleave', function () {
            if (window.innerWidth > 768) {
                this.style.transform = 'translateY(0)';
                this.style.boxShadow = '0 2px 8px rgba(0,0,0,0.1)';
            }
        });

        // Click interactions
        card.addEventListener('click', function (e) {
            // Don't trigger if clicking on a button
            if (e.target.tagName === 'BUTTON') return;

            this.style.transform = 'scale(1.02)';
            setTimeout(() => {
                this.style.transform = window.innerWidth > 768 ? 'translateY(-3px)' : 'scale(1)';
            }, 150);

            // Optional: Show quest details
            showQuestDetails(this);
        });

        // Keyboard interactions
        card.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });

        // Touch interactions for mobile
        card.addEventListener('touchstart', function () {
            this.style.opacity = '0.9';
        }, { passive: true });

        card.addEventListener('touchend', function () {
            this.style.opacity = '1';
        }, { passive: true });
    });
}

// Quest Button Interactions
function setupQuestButtons() {
    const questButtons = document.querySelectorAll('.quest-card button, .claim-button');

    questButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();

            // Visual feedback
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 100);

            const buttonText = this.textContent.trim().toLowerCase();
            const questCard = this.closest('.quest-card');
            const questId = questCard?.dataset.questId || generateQuestId(questCard);

            console.log('Quest button clicked:', buttonText, 'Quest ID:', questId);

            // Handle different button types
            switch (buttonText) {
                case 'start':
                    startQuest(questId);
                    break;
                case 'claim':
                    claimQuest(questId);
                    break;
                case 'continue':
                    continueQuest(questId);
                    break;
                default:
                    console.log('Unknown button type:', buttonText);
            }
        });

        // Keyboard accessibility
        button.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });
}

// Category Tag Interactions
function setupCategoryTags() {
    const categoryTags = document.querySelectorAll('.category-tag');

    categoryTags.forEach(tag => {
        tag.addEventListener('click', function () {
            // Visual feedback
            this.style.background = 'rgba(255, 255, 255, 0.4)';
            setTimeout(() => {
                this.style.background = 'rgba(255, 255, 255, 0.2)';
            }, 200);

            // Filter quests by category
            const category = this.textContent.trim();
            filterQuestsByCategory(category);
        });

        // Keyboard support
        tag.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });
}

// Achievement Animations
function setupAchievementAnimations() {
    if ('IntersectionObserver' in window) {
        const achievementObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';

                    // Add achievement unlock animation
                    entry.target.classList.add('achievement-unlocked');
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        const achievementElements = document.querySelectorAll(
            '.achievement-title-section, .achievement-main-section, .achievement-stats-section, .profile-avatar-section'
        );

        achievementElements.forEach(item => {
            item.style.opacity = '0';
            item.style.transform = 'translateY(20px)';
            item.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            achievementObserver.observe(item);
        });
    }
}

// Quest Keyboard Navigation
function setupQuestKeyboardNavigation() {
    document.addEventListener('keydown', function (e) {
        if (!document.body.classList.contains('quest-page')) return;

        const questCards = document.querySelectorAll('.quest-card');
        const currentIndex = Array.from(questCards).findIndex(card =>
            card === document.activeElement
        );

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                navigateToQuest(questCards, currentIndex + 1);
                break;
            case 'ArrowUp':
                e.preventDefault();
                navigateToQuest(questCards, currentIndex - 1);
                break;
            case 'Home':
                e.preventDefault();
                navigateToQuest(questCards, 0);
                break;
            case 'End':
                e.preventDefault();
                navigateToQuest(questCards, questCards.length - 1);
                break;
        }
    });
}

function navigateToQuest(questCards, index) {
    if (index >= 0 && index < questCards.length) {
        questCards[index].focus();
    }
}

// Quest Observers for Performance
function initializeQuestObservers() {
    // Lazy load quest images
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

        document.querySelectorAll('.quest-card img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }

    // Quest progress tracking
    setupQuestProgressTracking();
}

function setupQuestProgressTracking() {
    const progressBars = document.querySelectorAll('.progress-fill');

    progressBars.forEach(bar => {
        const targetWidth = bar.style.width || bar.dataset.progress || '0%';
        bar.style.width = '0%';

        // Animate progress bar
        setTimeout(() => {
            bar.style.width = targetWidth;
        }, 500);
    });
}

// QUEST API INTEGRATION FUNCTIONS -----------------------------------------------------------------------------------------

function startQuest(questId) {
    if (!questId) {
        console.error('Quest ID is required');
        showQuestError('Invalid quest ID');
        return;
    }

    console.log('Starting quest:', questId);

    // Show loading state
    const questCard = document.querySelector(`[data-quest-id="${questId}"]`);
    const button = questCard?.querySelector('button');

    if (button) {
        button.disabled = true;
        button.textContent = 'Starting...';
    }

    // AJAX call to start quest
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
                updateQuestUI(questId, 'started');
                showQuestSuccess('Quest started successfully!');

                // Track quest start event
                trackQuestEvent('quest_started', questId);
            } else {
                console.error('Failed to start quest:', data.message);
                showQuestError(data.message || 'Failed to start quest');
                resetQuestButton(questId, 'Start');
            }
        })
        .catch(error => {
            console.error('Error starting quest:', error);
            showQuestError('Network error occurred');
            resetQuestButton(questId, 'Start');
        });
}

function claimQuest(questId) {
    if (!questId) {
        console.error('Quest ID is required');
        showQuestError('Invalid quest ID');
        return;
    }

    console.log('Claiming quest reward:', questId);

    // Show loading state
    const questCard = document.querySelector(`[data-quest-id="${questId}"]`);
    const button = questCard?.querySelector('button');

    if (button) {
        button.disabled = true;
        button.textContent = 'Claiming...';
    }

    // AJAX call to claim quest reward
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
                updateQuestUI(questId, 'claimed');
                showQuestSuccess('Quest reward claimed!');

                // Update token balance if provided
                if (data.newTokenBalance !== undefined) {
                    updateTokenBalance(data.newTokenBalance);
                }

                // Show reward animation
                if (data.reward) {
                    showRewardAnimation(data.reward);
                }

                // Track quest claim event
                trackQuestEvent('quest_claimed', questId);
            } else {
                console.error('Failed to claim quest:', data.message);
                showQuestError(data.message || 'Failed to claim quest');
                resetQuestButton(questId, 'Claim');
            }
        })
        .catch(error => {
            console.error('Error claiming quest:', error);
            showQuestError('Network error occurred');
            resetQuestButton(questId, 'Claim');
        });
}

function continueQuest(questId) {
    console.log('Continuing quest:', questId);

    // Navigate to quest details or progress page
    window.location.href = `/Quest/Details/${questId}`;
}

// UI UPDATE FUNCTIONS -----------------------------------------------------------------------------------------

function updateQuestUI(questId, status) {
    const questCard = document.querySelector(`[data-quest-id="${questId}"]`);
    if (!questCard) return;

    const button = questCard.querySelector('button');
    const progressBar = questCard.querySelector('.progress-fill');
    const questTimer = questCard.querySelector('.quest-timer');

    switch (status) {
        case 'started':
            if (button) {
                button.textContent = 'In Progress';
                button.disabled = true;
                button.className = 'claim-button bg-yellow-400 hover:bg-yellow-500';
            }
            if (progressBar) {
                progressBar.style.width = '25%';
            }
            if (questTimer) {
                questTimer.textContent = 'IN PROGRESS';
            }
            questCard.classList.add('quest-active');
            break;

        case 'claimed':
            if (button) {
                button.textContent = 'Completed';
                button.disabled = true;
                button.className = 'claim-button bg-green-500 hover:bg-green-600';
            }
            if (progressBar) {
                progressBar.style.width = '100%';
            }
            if (questTimer) {
                questTimer.textContent = 'COMPLETED';
            }
            questCard.classList.add('quest-completed');

            // Add completion animation
            questCard.style.animation = 'questComplete 0.6s ease-out';
            break;

        case 'failed':
            if (button) {
                button.textContent = 'Try Again';
                button.disabled = false;
                button.className = 'claim-button bg-red-500 hover:bg-red-600';
            }
            questCard.classList.add('quest-failed');
            break;
    }
}

function resetQuestButton(questId, originalText) {
    const questCard = document.querySelector(`[data-quest-id="${questId}"]`);
    const button = questCard?.querySelector('button');

    if (button) {
        button.disabled = false;
        button.textContent = originalText;
    }
}

function updateTokenBalance(newBalance) {
    const tokenDisplays = document.querySelectorAll('[data-token-balance], .token-number');

    tokenDisplays.forEach(display => {
        const currentBalance = parseInt(display.textContent) || 0;
        animateNumberChange(display, currentBalance, newBalance);
    });

    console.log('Token balance updated to:', newBalance);
}

function animateNumberChange(element, from, to) {
    const duration = 1000;
    const steps = 30;
    const stepValue = (to - from) / steps;
    let current = from;
    let step = 0;

    const timer = setInterval(() => {
        current += stepValue;
        step++;

        element.textContent = Math.round(current);

        if (step >= steps) {
            clearInterval(timer);
            element.textContent = to;
        }
    }, duration / steps);
}

// HELPER FUNCTIONS -----------------------------------------------------------------------------------------

function generateQuestId(questCard) {
    // Generate a temporary ID based on quest card position
    const questCards = Array.from(document.querySelectorAll('.quest-card'));
    const index = questCards.indexOf(questCard);
    return `quest-${index + 1}`;
}

function showQuestDetails(questCard) {
    const questName = questCard.querySelector('.quest-name')?.textContent;
    const questDescription = questCard.querySelector('.quest-description')?.textContent;

    console.log('Quest Details:', { name: questName, description: questDescription });

    // You can implement a modal or detailed view here
}

function filterQuestsByCategory(category) {
    const questCards = document.querySelectorAll('.quest-card');

    questCards.forEach(card => {
        const cardCategory = card.dataset.category || 'general';

        if (category === 'all' || cardCategory.toLowerCase() === category.toLowerCase()) {
            card.style.display = 'block';
            card.style.animation = 'fadeIn 0.3s ease-in';
        } else {
            card.style.display = 'none';
        }
    });

    console.log('Filtered quests by category:', category);
}

function showQuestSuccess(message) {
    showQuestNotification(message, 'success');
}

function showQuestError(message) {
    showQuestNotification(message, 'error');
}

function showQuestNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `quest-notification quest-notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <span class="notification-message">${message}</span>
            <button class="notification-close" aria-label="Close notification">&times;</button>
        </div>
    `;

    // Add to page
    document.body.appendChild(notification);

    // Show notification
    setTimeout(() => {
        notification.classList.add('show');
    }, 100);

    // Auto hide after 5 seconds
    const hideTimer = setTimeout(() => {
        hideQuestNotification(notification);
    }, 5000);

    // Close button functionality
    const closeBtn = notification.querySelector('.notification-close');
    closeBtn.addEventListener('click', () => {
        clearTimeout(hideTimer);
        hideQuestNotification(notification);
    });
}

function hideQuestNotification(notification) {
    notification.classList.remove('show');
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 300);
}

function showRewardAnimation(reward) {
    console.log('Showing reward animation for:', reward);

    // Create reward popup
    const rewardPopup = document.createElement('div');
    rewardPopup.className = 'reward-popup';
    rewardPopup.innerHTML = `
        <div class="reward-content">
            <h3>Reward Earned!</h3>
            <div class="reward-details">
                <span class="reward-amount">+${reward.amount}</span>
                <span class="reward-type">${reward.type}</span>
            </div>
        </div>
    `;

    document.body.appendChild(rewardPopup);

    // Animate in
    setTimeout(() => {
        rewardPopup.classList.add('show');
    }, 100);

    // Animate out
    setTimeout(() => {
        rewardPopup.classList.remove('show');
        setTimeout(() => {
            if (rewardPopup.parentNode) {
                rewardPopup.parentNode.removeChild(rewardPopup);
            }
        }, 500);
    }, 3000);
}

function trackQuestEvent(eventType, questId) {
    // Analytics tracking for quest events
    console.log('Quest event tracked:', eventType, questId);

    // You can integrate with Google Analytics, Mixpanel, etc.
    if (typeof gtag !== 'undefined') {
        gtag('event', eventType, {
            'custom_parameter_1': questId
        });
    }
}

// RESPONSIVE HANDLING -----------------------------------------------------------------------------------------

function handleQuestPageResize() {
    const questCards = document.querySelectorAll('.quest-card');

    questCards.forEach(card => {
        // Reset transforms on resize
        card.style.transform = '';
        card.style.boxShadow = '';
    });
}

// Window resize handler for quest page
window.addEventListener('resize', function () {
    if (document.body.classList.contains('quest-page')) {
        let resizeTimeout;
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(handleQuestPageResize, 250);
    }
});

// Export quest functions for external use
window.TidyUpQuests = {
    startQuest,
    claimQuest,
    continueQuest,
    updateQuestUI,
    updateTokenBalance,
    filterQuestsByCategory,
    showQuestSuccess,
    showQuestError
};