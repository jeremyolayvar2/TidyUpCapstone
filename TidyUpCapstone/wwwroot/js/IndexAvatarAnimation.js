// Balanced Avatar Animation System
(function () {
    'use strict';

    let isAnimated = false;
    let observer;

    // Animation configuration
    const animationConfig = {
        initialScale: 0,
        finalScale: 1,
        duration: 800,
        staggerDelay: 100,
        spreadRadius: {
            mobile: 35,
            tablet: 40,
            desktop: 45
        }
    };

    // Predefined balanced positions for different screen sizes
    const balancedPositions = {
        desktop: [
            { x: 25, y: 20, size: 'large' },   // Top left
            { x: 75, y: 25, size: 'medium' },  // Top right
            { x: 15, y: 50, size: 'medium' },  // Middle left
            { x: 85, y: 45, size: 'small' },   // Middle right
            { x: 30, y: 75, size: 'small' },   // Bottom left
            { x: 70, y: 80, size: 'large' },   // Bottom right
            { x: 50, y: 15, size: 'small' },   // Top center
            { x: 10, y: 30, size: 'medium' },  // Left center
            { x: 90, y: 70, size: 'medium' },  // Right bottom
            { x: 55, y: 85, size: 'large' }    // Bottom center
        ],
        tablet: [
            { x: 20, y: 25, size: 'large' },
            { x: 80, y: 20, size: 'medium' },
            { x: 15, y: 55, size: 'medium' },
            { x: 85, y: 50, size: 'small' },
            { x: 25, y: 80, size: 'small' },
            { x: 75, y: 75, size: 'large' },
            { x: 50, y: 10, size: 'small' },
            { x: 10, y: 35, size: 'medium' },
            { x: 90, y: 65, size: 'medium' },
            { x: 50, y: 90, size: 'large' }
        ],
        mobile: [
            { x: 25, y: 30, size: 'large' },
            { x: 75, y: 25, size: 'medium' },
            { x: 20, y: 60, size: 'medium' },
            { x: 80, y: 55, size: 'small' },
            { x: 30, y: 85, size: 'small' },
            { x: 70, y: 80, size: 'large' }
        ]
    };

    // Get viewport size category
    function getViewportCategory() {
        const width = window.innerWidth;
        if (width <= 767) return 'mobile';
        if (width <= 991) return 'tablet';
        return 'desktop';
    }

    // Get balanced position for avatar
    function getBalancedPosition(index, viewportCategory) {
        const positions = balancedPositions[viewportCategory];
        if (index < positions.length) {
            return positions[index];
        }

        // Fallback for extra avatars (shouldn't happen with proper responsive hiding)
        const fallback = positions[index % positions.length];
        return {
            x: fallback.x + (Math.random() - 0.5) * 10, // Small variation
            y: fallback.y + (Math.random() - 0.5) * 10,
            size: fallback.size
        };
    }

    // Reset avatars to center position
    function resetAvatarsToCenter() {
        const avatars = document.querySelectorAll('.floating-avatars .avatar');

        avatars.forEach((avatar, index) => {
            avatar.style.transition = 'none';
            avatar.style.left = '50%';
            avatar.style.top = '50%';
            avatar.style.transform = 'translate(-50%, -50%) scale(0)';
            avatar.style.opacity = '0';
            avatar.style.zIndex = '1';

            // Remove any existing animation classes
            avatar.classList.remove('avatar-spread');

            // Reset size classes
            avatar.classList.remove('small', 'medium', 'large');
        });

        // Reset animation state
        isAnimated = false;
    }

    // Apply size class to avatar
    function applySizeClass(avatar, size) {
        avatar.classList.remove('small', 'medium', 'large');
        avatar.classList.add(size);
    }

    // Animate avatars spreading out in balanced pattern
    function animateAvatarsSpread() {
        if (isAnimated) return;

        const avatars = document.querySelectorAll('.floating-avatars .avatar');
        const viewportCategory = getViewportCategory();

        avatars.forEach((avatar, index) => {
            const position = getBalancedPosition(index, viewportCategory);
            const delay = index * animationConfig.staggerDelay;

            // Set initial state
            avatar.style.transition = 'none';
            avatar.style.left = '50%';
            avatar.style.top = '50%';
            avatar.style.transform = 'translate(-50%, -50%) scale(0)';
            avatar.style.opacity = '0';

            // Apply size class immediately
            applySizeClass(avatar, position.size);

            // Animate to spread position
            setTimeout(() => {
                avatar.style.transition = `all ${animationConfig.duration}ms cubic-bezier(0.175, 0.885, 0.32, 1.275)`;
                avatar.style.left = `${position.x}%`;
                avatar.style.top = `${position.y}%`;
                avatar.style.transform = 'translate(-50%, -50%) scale(1)';
                avatar.style.opacity = '1';
                avatar.classList.add('avatar-spread');

                // Add floating animation after spread
                setTimeout(() => {
                    avatar.style.animation = `modernFloat 8s ease-in-out infinite, pulseGlow 4s ease-in-out infinite`;
                }, animationConfig.duration);

            }, delay);
        });

        isAnimated = true;
    }

    // Intersection Observer for community section
    function initializeObserver() {
        const communitySection = document.querySelector('.community-section');

        if (!communitySection) return;

        const observerOptions = {
            root: null,
            rootMargin: '-20% 0px -20% 0px',
            threshold: 0.3
        };

        observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateAvatarsSpread();
                } else if (entry.target === communitySection && !entry.isIntersecting) {
                    // Reset when completely out of view
                    if (entry.boundingClientRect.top > 0) {
                        resetAvatarsToCenter();
                    }
                }
            });
        }, observerOptions);

        observer.observe(communitySection);
    }

    // Handle resize events with balanced repositioning
    function handleResize() {
        if (isAnimated) {
            // Re-trigger animation with new balanced positions on resize
            const wasAnimated = isAnimated;
            resetAvatarsToCenter();

            setTimeout(() => {
                if (wasAnimated) {
                    animateAvatarsSpread();
                }
            }, 100);
        }
    }

    // Debounce resize handler
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

    // Initialize on DOM load
    function initialize() {
        // Wait for page load to ensure all elements are ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                setTimeout(initialize, 100);
            });
            return;
        }

        // Reset avatars to center initially
        resetAvatarsToCenter();

        // Initialize observer
        initializeObserver();

        // Add resize listener with debounce
        window.addEventListener('resize', debounce(handleResize, 250));

        // Handle browser back/forward navigation
        window.addEventListener('pageshow', () => {
            setTimeout(() => {
                resetAvatarsToCenter();
                const communitySection = document.querySelector('.community-section');
                if (communitySection) {
                    const rect = communitySection.getBoundingClientRect();
                    const isVisible = rect.top < window.innerHeight && rect.bottom > 0;
                    if (isVisible) {
                        setTimeout(animateAvatarsSpread, 300);
                    }
                }
            }, 100);
        });
    }

    // Cleanup function
    function cleanup() {
        if (observer) {
            observer.disconnect();
        }
        window.removeEventListener('resize', handleResize);
    }

    // Handle page unload
    window.addEventListener('beforeunload', cleanup);

    // Start initialization
    initialize();

    // Expose methods for external control if needed
    window.AvatarAnimation = {
        reset: resetAvatarsToCenter,
        spread: animateAvatarsSpread,
        isAnimated: () => isAnimated,
        getPositions: () => balancedPositions
    };

})();