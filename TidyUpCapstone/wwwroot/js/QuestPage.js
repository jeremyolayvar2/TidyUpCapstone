$(document).ready(function () {
    // Initialize quest page
    initializeQuestPage();

    // Load check-in status and start timer
    loadCheckInStatus();

    // Set up event handlers
    setupEventHandlers();
});

let checkInTimer;

function initializeQuestPage() {
    // Add smooth scrolling behavior to quest containers
    $('.quests-container').each(function () {
        this.style.scrollBehavior = 'smooth';
    });

    // Initialize card interactions
    initializeCardInteractions();

    // Initialize progress bar animations
    initializeProgressAnimations();
}

function setupEventHandlers() {
    // Daily check-in handler
    $('#checkInBtn').click(function () {
        handleCheckIn($(this));
    });

    // Claim reward handlers
    $(document).on('click', '.claim-reward-btn', function () {
        handleClaimReward($(this));
    });

    // Quest item interactions
    $('.quest-item').on('click', function () {
        handleQuestItemClick($(this));
    });

    // Generation button handlers
    $('.generate-quest-btn').on('click', function () {
        const questType = $(this).closest('.quests-card').hasClass('daily-quests') ? 'daily' :
            $(this).closest('.quests-card').hasClass('weekly-quests') ? 'weekly' : 'special';
        handleGenerateQuest(questType);
    });
}

function initializeCardInteractions() {
    // Bento card hover effects
    $('.bento-card').each(function () {
        const card = $(this);

        card.on('mouseenter', function () {
            if (window.innerWidth > 768) {
                $(this).css('transform', 'translateY(-4px) scale(1.02)');
            }
        });

        card.on('mouseleave', function () {
            if (window.innerWidth > 768) {
                $(this).css('transform', 'translateY(-2px) scale(1)');
            }
        });

        // Touch interactions for mobile
        card.on('touchstart', function () {
            $(this).css('opacity', '0.95');
        });

        card.on('touchend', function () {
            $(this).css('opacity', '1');
        });
    });

    // Quest item interactions
    $('.quest-item').each(function () {
        const item = $(this);

        item.on('mouseenter', function () {
            if (window.innerWidth > 768) {
                $(this).css({
                    'transform': 'translateY(-2px)',
                    'box-shadow': '0 8px 25px rgba(107, 144, 128, 0.2)'
                });
            }
        });

        item.on('mouseleave', function () {
            if (window.innerWidth > 768) {
                $(this).css({
                    'transform': 'translateY(0)',
                    'box-shadow': 'none'
                });
            }
        });
    });
}

function initializeProgressAnimations() {
    // Animate progress bars on load
    setTimeout(() => {
        $('.progress-fill').each(function () {
            const width = $(this).css('width');
            $(this).css('width', '0').animate({
                width: width
            }, 1000, 'easeOutCubic');
        });

        // Animate level progress bar
        $('.level-progress-fill').each(function () {
            const width = $(this).css('width');
            $(this).css('width', '0').animate({
                width: width
            }, 1500, 'easeOutCubic');
        });
    }, 500);
}

function handleCheckIn(btn) {
    if (btn.prop('disabled')) return;

    btn.prop('disabled', true).html('Checking in...');

    $.post('/Quest/CheckIn')
        .done(function (response) {
            if (response && response.success) {
                showNotification('success', response.message || 'Successfully checked in!');
                $('#streakCount').text(response.streak || 0);
                btn.removeClass('checkin-button').addClass('checkin-button')
                    .html('✓ Checked In').prop('disabled', true);

                // Hide timer and show next check-in countdown
                $('#nextCheckInTimer').hide();

                // Show token/XP notification
                if (response.tokensEarned) {
                    setTimeout(() => {
                        showNotification('success', `+${response.tokensEarned} tokens, +${response.xpEarned || 0} XP!`);
                    }, 1000);
                }

                // Reload to show updated stats
                setTimeout(() => location.reload(), 2500);
            } else {
                showNotification('warning', response.message || 'Check-in failed');
                btn.prop('disabled', false).html('Check In');
            }
        })
        .fail(function (xhr, status, error) {
            console.error('Check-in error:', error);
            showNotification('error', 'An error occurred during check-in.');
            btn.prop('disabled', false).html('Check In');
        });
}

function handleClaimReward(btn) {
    const questId = btn.data('quest-id');

    if (!questId || btn.prop('disabled')) return;

    btn.prop('disabled', true).html('Claiming...');

    $.post('/Quest/ClaimReward', { questId: questId })
        .done(function (response) {
            if (response && response.success) {
                showNotification('success', response.message || 'Reward claimed successfully!');
                btn.removeClass('active').addClass('completed')
                    .html('✓ Claimed').prop('disabled', true);

                // Update the progress bar to 100%
                const questItem = btn.closest('.quest-item');
                questItem.find('.progress-fill').animate({ width: '100%' }, 500);

                // Add completion effect
                questItem.addClass('quest-completed');

                // Reload page to update stats
                setTimeout(() => location.reload(), 2000);
            } else {
                showNotification('error', response.message || 'Failed to claim reward');
                btn.prop('disabled', false).html('Claim Reward');
            }
        })
        .fail(function (xhr, status, error) {
            console.error('Claim error:', error);
            showNotification('error', 'An error occurred while claiming the reward.');
            btn.prop('disabled', false).html('Claim Reward');
        });
}

function handleQuestItemClick(item) {
    // Add ripple effect
    createRippleEffect(item, event);

    // Visual feedback
    item.css('transform', 'scale(1.02)');
    setTimeout(() => {
        item.css('transform', '');
    }, 150);
}

function handleGenerateQuest(questType) {
    let endpoint = '';
    let message = '';

    switch (questType) {
        case 'daily':
            endpoint = '/Quest/DebugGenerateQuests';
            message = 'Generating daily quests...';
            break;
        case 'weekly':
            endpoint = '/Quest/DebugWeeklyQuestGeneration';
            message = 'Generating weekly quest...';
            break;
        case 'special':
            endpoint = '/Quest/DebugGenerateSpecialQuest';
            message = 'Generating special quest...';
            break;
        default:
            return;
    }

    showNotification('info', message);

    $.post(endpoint)
        .done(function (response) {
            if (response && response.success) {
                showNotification('success', response.message || `${questType} quest generated successfully!`);
                setTimeout(() => location.reload(), 1500);
            } else {
                showNotification('error', response.message || 'Failed to generate quest');
            }
        })
        .fail(function () {
            showNotification('error', `Failed to generate ${questType} quest`);
        });
}

function loadCheckInStatus() {
    $.get('/Quest/CheckInStatus')
        .done(function (data) {
            if (data) {
                $('#streakCount').text(data.streak || 0);
                if (data.hasCheckedIn) {
                    $('#checkInBtn').addClass('checkin-button')
                        .html('✓ Checked In').prop('disabled', true);

                    // Show timer for next check-in
                    if (data.timeUntilNextCheckIn && data.timeUntilNextCheckIn.totalSeconds > 0) {
                        showNextCheckInTimer(data.timeUntilNextCheckIn.totalSeconds);
                    }
                } else {
                    $('#checkInBtn').removeClass('completed').addClass('checkin-button')
                        .html('Check In').prop('disabled', false);
                    $('#nextCheckInTimer').hide();
                }
            }
        })
        .fail(function (xhr, status, error) {
            console.log('Failed to load check-in status:', error);
        });
}

function showNextCheckInTimer(totalSeconds) {
    if (totalSeconds <= 0) {
        $('#nextCheckInTimer').hide();
        return;
    }

    $('#nextCheckInTimer').show();

    // Clear existing timer
    if (checkInTimer) {
        clearInterval(checkInTimer);
    }

    let remainingSeconds = totalSeconds;

    function updateTimer() {
        if (remainingSeconds <= 0) {
            clearInterval(checkInTimer);
            $('#nextCheckInTimer').hide();
            showNotification('info', 'New check-in available!');
            // Refresh to allow new check-in
            setTimeout(() => location.reload(), 1000);
            return;
        }

        const hours = Math.floor(remainingSeconds / 3600);
        const minutes = Math.floor((remainingSeconds % 3600) / 60);
        const seconds = remainingSeconds % 60;

        const timeString = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        $('#timerDisplay').text(timeString);

        remainingSeconds--;
    }

    // Update immediately and then every second
    updateTimer();
    checkInTimer = setInterval(updateTimer, 1000);
}

function showNotification(type, message) {
    // Remove existing notifications
    $('.quest-notification').remove();

    const notification = $(`
        <div class="quest-notification ${type}">
            <div class="notification-content">
                ${message}
            </div>
        </div>
    `);

    $('body').append(notification);

    // Trigger show animation
    setTimeout(function () {
        notification.addClass('show');
    }, 100);

    // Auto hide after 4 seconds
    setTimeout(function () {
        hideNotification(notification);
    }, 4000);
}

function hideNotification(notification) {
    notification.removeClass('show');
    setTimeout(() => {
        notification.remove();
    }, 300);
}

function createRippleEffect(element, event) {
    const ripple = $('<div class="ripple"></div>');
    element.append(ripple);

    // Position the ripple
    const rect = element[0].getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    ripple.css({
        left: x + 'px',
        top: y + 'px'
    });

    // Remove ripple after animation
    setTimeout(() => {
        ripple.remove();
    }, 600);
}

// Global quest generation functions for backward compatibility
function generateDailyQuests() {
    handleGenerateQuest('daily');
}

function generateWeeklyQuest() {
    handleGenerateQuest('weekly');
}

function generateSpecialQuest() {
    handleGenerateQuest('special');
}

// Handle window resize for responsive behavior
let resizeTimeout;
$(window).on('resize', function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(() => {
        // Reset styles on resize
        $('.bento-card, .quest-item').css({
            'transform': '',
            'box-shadow': ''
        });

        // Reinitialize interactions based on new screen size
        initializeCardInteractions();
    }, 250);
});

// Clean up timer when page unloads
$(window).on('beforeunload', function () {
    if (checkInTimer) {
        clearInterval(checkInTimer);
    }
});

// Intersection observer for scroll animations
if ('IntersectionObserver' in window) {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const cardObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Observe bento cards for scroll animations
    $('.bento-card').each(function () {
        this.style.opacity = '0';
        this.style.transform = 'translateY(20px)';
        this.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        cardObserver.observe(this);
    });
}

// Add CSS easing function for jQuery animations
$.easing.easeOutCubic = function (x, t, b, c, d) {
    return c * ((t = t / d - 1) * t * t + 1) + b;
};

// Keyboard accessibility
$(document).on('keydown', '.claim-button, .checkin-button, .generate-quest-btn', function (e) {
    if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        $(this).click();
    }
});

// Touch feedback for mobile devices
if ('ontouchstart' in window) {
    $('.bento-card, .quest-item, .claim-button').on('touchstart', function () {
        $(this).addClass('touch-feedback');
    }).on('touchend touchcancel', function () {
        const element = $(this);
        setTimeout(() => {
            element.removeClass('touch-feedback');
        }, 150);
    });
}