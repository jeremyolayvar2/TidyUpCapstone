// Complete Fixed Avatar Animation System
(function () {
    'use strict';

    let isAnimated = false;
    let observer;
    let resizeTimeout;

    // Animation configuration
    const animationConfig = {
        duration: 1200,
        staggerDelay: 200,
        observerThreshold: 0.1
    };

    // Balanced positions for different screen sizes
    const balancedPositions = {
        desktop: [
            { x: 15, y: 15, size: 'large' },
            { x: 85, y: 20, size: 'medium' },
            { x: 10, y: 45, size: 'medium' },
            { x: 90, y: 40, size: 'small' },
            { x: 25, y: 75, size: 'small' },
            { x: 75, y: 80, size: 'large' },
            { x: 45, y: 10, size: 'small' },
            { x: 5, y: 25, size: 'medium' },
            { x: 95, y: 65, size: 'medium' },
            { x: 55, y: 85, size: 'large' }
        ],
        tablet: [
            { x: 20, y: 20, size: 'large' },
            { x: 80, y: 25, size: 'medium' },
            { x: 15, y: 55, size: 'medium' },
            { x: 85, y: 50, size: 'small' },
            { x: 30, y: 80, size: 'small' },
            { x: 70, y: 75, size: 'large' }
        ],
        mobile: [
            { x: 25, y: 25, size: 'large' },
            { x: 75, y: 30, size: 'medium' },
            { x: 20, y: 70, size: 'medium' },
            { x: 80, y: 65, size: 'small' }
        ]
    };

    // Get viewport category
    function getViewportCategory() {
        const width = window.innerWidth;
        if (width <= 767) return 'mobile';
        if (width <= 991) return 'tablet';
        return 'desktop';
    }

    // Get position for avatar
    function getBalancedPosition(index, viewportCategory) {
        const positions = balancedPositions[viewportCategory];
        return positions[index] || null;
    }

    // Add required CSS animations
    function injectAvatarCSS() {
        if (document.getElementById('avatar-animations-fixed')) return;

        const style = document.createElement('style');
        style.id = 'avatar-animations-fixed';
        style.textContent = `
            .floating-avatars .avatar {
                transition: all 1000ms cubic-bezier(0.175, 0.885, 0.32, 1.275) !important;
            }
            
            @keyframes avatarFloat {
                0%, 100% { 
                    transform: translate(-50%, -50%) translateY(0px) translateX(0px) rotate(0deg) scale(1); 
                }
                25% { 
                    transform: translate(-50%, -50%) translateY(-8px) translateX(3px) rotate(1deg) scale(1.02); 
                }
                50% { 
                    transform: translate(-50%, -50%) translateY(-4px) translateX(-2px) rotate(-0.5deg) scale(1); 
                }
                75% { 
                    transform: translate(-50%, -50%) translateY(-12px) translateX(1px) rotate(0.8deg) scale(1.01); 
                }
            }
            
            .avatar-floating {
                animation: avatarFloat 6s ease-in-out infinite !important;
            }
        `;
        document.head.appendChild(style);
        console.log('✅ Avatar CSS animations injected');
    }

    // Reset all avatars to center
    function resetAvatarsToCenter() {
        const avatars = document.querySelectorAll('.floating-avatars .avatar');

        if (avatars.length === 0) {
            console.warn('⚠️ No avatars found');
            return;
        }

        console.log(`🔄 Resetting ${avatars.length} avatars to center`);

        avatars.forEach((avatar) => {
            // Clear all animations and transitions
            avatar.style.animation = 'none';
            avatar.style.transition = 'none';

            // Reset position and visibility
            avatar.style.left = '50%';
            avatar.style.top = '50%';
            avatar.style.transform = 'translate(-50%, -50%) scale(0)';
            avatar.style.opacity = '0';
            avatar.style.zIndex = '1';
            avatar.style.display = 'block';

            // Remove classes
            avatar.classList.remove('avatar-spread', 'avatar-floating', 'small', 'medium', 'large');

            // Force reflow
            void avatar.offsetHeight;
        });

        isAnimated = false;
    }

    // Animate avatars spreading out
    function animateAvatarsSpread() {
        if (isAnimated) {
            console.log('⏭️ Animation already running, skipping');
            return;
        }

        const avatars = document.querySelectorAll('.floating-avatars .avatar');

        if (avatars.length === 0) {
            console.error('❌ No avatars found for animation');
            return;
        }

        const viewportCategory = getViewportCategory();
        console.log(`🎬 Starting avatar animation for ${viewportCategory} with ${avatars.length} avatars`);

        // Reset first
        resetAvatarsToCenter();

        // Animate each avatar
        let animatedCount = 0;
        avatars.forEach((avatar, index) => {
            const position = getBalancedPosition(index, viewportCategory);

            if (!position) {
                avatar.style.display = 'none';
                return;
            }

            animatedCount++;
            const delay = index * animationConfig.staggerDelay;

            // Apply size class immediately
            avatar.classList.add(position.size);

            setTimeout(() => {
                // Enable transitions
                avatar.style.transition = `all ${animationConfig.duration}ms cubic-bezier(0.175, 0.885, 0.32, 1.275)`;

                // Move to position
                avatar.style.left = `${position.x}%`;
                avatar.style.top = `${position.y}%`;
                avatar.style.transform = 'translate(-50%, -50%) scale(1)';
                avatar.style.opacity = '1';
                avatar.style.zIndex = '2';

                // Add spread class
                avatar.classList.add('avatar-spread');

                // Add floating animation after spread completes
                setTimeout(() => {
                    if (avatar.classList.contains('avatar-spread')) {
                        avatar.classList.add('avatar-floating');
                    }
                }, animationConfig.duration + 200);

            }, delay);
        });

        console.log(`✨ Animating ${animatedCount} avatars`);
        isAnimated = true;
    }

    // Initialize intersection observer
    function initializeObserver() {
        const communitySection = document.querySelector('.community-section');

        if (!communitySection) {
            console.error('❌ Community section not found');
            return false;
        }

        // Clean up existing observer
        if (observer) {
            observer.disconnect();
        }

        const observerOptions = {
            root: null,
            rootMargin: '50px 0px -50px 0px',
            threshold: animationConfig.observerThreshold
        };

        observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                const isVisible = entry.isIntersecting && entry.intersectionRatio >= animationConfig.observerThreshold;

                console.log('👁️ Observer triggered:', {
                    isIntersecting: entry.isIntersecting,
                    intersectionRatio: entry.intersectionRatio.toFixed(3),
                    isVisible
                });

                if (isVisible) {
                    console.log('🎯 Community section visible - triggering animation');
                    setTimeout(() => animateAvatarsSpread(), 300);
                } else if (!isVisible && isAnimated) {
                    console.log('🔄 Community section not visible - resetting');
                    resetAvatarsToCenter();
                }
            });
        }, observerOptions);

        observer.observe(communitySection);
        console.log('👁️ Intersection observer initialized');
        return true;
    }

    // Handle resize
    function handleResize() {
        console.log('📱 Window resized');

        if (isAnimated) {
            resetAvatarsToCenter();

            setTimeout(() => {
                const communitySection = document.querySelector('.community-section');
                if (communitySection) {
                    const rect = communitySection.getBoundingClientRect();
                    const isVisible = rect.top < window.innerHeight * 0.8 && rect.bottom > window.innerHeight * 0.2;

                    if (isVisible) {
                        console.log('🔄 Re-triggering animation after resize');
                        animateAvatarsSpread();
                    }
                }
            }, 500);
        }
    }

    // Debounce function
    function debounce(func, wait) {
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(resizeTimeout);
                func(...args);
            };
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(later, wait);
        };
    }

    // Initialize system
    function initialize() {
        console.log('🚀 Initializing Avatar Animation System...');

        // Wait for DOM
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initialize);
            return;
        }

        // Inject CSS
        injectAvatarCSS();

        // Wait for elements
        setTimeout(() => {
            const communitySection = document.querySelector('.community-section');
            const avatars = document.querySelectorAll('.floating-avatars .avatar');

            console.log('🔍 Element check:', {
                communitySection: !!communitySection,
                avatarCount: avatars.length,
                hasFloatingContainer: !!document.querySelector('.floating-avatars')
            });

            if (!communitySection) {
                console.error('❌ Community section missing - cannot initialize');
                return;
            }

            if (avatars.length === 0) {
                console.error('❌ No avatars found - cannot initialize');
                return;
            }

            // Initialize
            resetAvatarsToCenter();

            if (!initializeObserver()) {
                console.error('❌ Observer initialization failed');
                return;
            }

            // Add resize handler
            window.addEventListener('resize', debounce(handleResize, 400));

            // Manual trigger function
            window.triggerAvatarAnimation = () => {
                console.log('🎯 Manual trigger called');
                resetAvatarsToCenter();
                setTimeout(() => animateAvatarsSpread(), 100);
            };

            // Check initial visibility
            setTimeout(() => {
                const rect = communitySection.getBoundingClientRect();
                const isInitiallyVisible = rect.top < window.innerHeight * 0.8 && rect.bottom > window.innerHeight * 0.2;

                console.log('🔍 Initial visibility check:', {
                    isVisible: isInitiallyVisible,
                    top: Math.round(rect.top),
                    bottom: Math.round(rect.bottom),
                    windowHeight: window.innerHeight
                });

                if (isInitiallyVisible) {
                    console.log('✨ Triggering initial animation');
                    setTimeout(() => animateAvatarsSpread(), 800);
                }
            }, 200);

            console.log('✅ Avatar Animation System initialized successfully');

        }, 200);
    }

    // Cleanup
    function cleanup() {
        if (observer) observer.disconnect();
        clearTimeout(resizeTimeout);
        window.removeEventListener('resize', handleResize);
        console.log('🧹 Avatar animation system cleaned up');
    }

    // Prevent conflicts with other scripts
    function preventScrollAnimationConflict() {
        // Override the conflicting function from IndexPageScroll.js
        const originalTriggerSectionAnimations = window.triggerSectionAnimations;
        window.triggerSectionAnimations = function (section, index) {
            if (section && section.classList.contains('community-section')) {
                // Don't let scroll script interfere with avatar animation
                console.log('🚫 Preventing scroll script avatar interference');
                return;
            }
            if (originalTriggerSectionAnimations) {
                originalTriggerSectionAnimations(section, index);
            }
        };
    }

    // Start everything
    window.addEventListener('beforeunload', cleanup);

    // Prevent conflicts
    setTimeout(preventScrollAnimationConflict, 100);

    // Initialize
    initialize();

    // Expose API
    window.AvatarAnimation = {
        reset: resetAvatarsToCenter,
        spread: animateAvatarsSpread,
        isAnimated: () => isAnimated,
        trigger: () => {
            resetAvatarsToCenter();
            setTimeout(animateAvatarsSpread, 100);
        },
        reinitialize: initialize,
        getStatus: () => ({
            isAnimated,
            avatarCount: document.querySelectorAll('.floating-avatars .avatar').length,
            hasObserver: !!observer
        })
    };

    console.log('🎭 Avatar Animation API loaded:', window.AvatarAnimation);

})();