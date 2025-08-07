document.addEventListener('DOMContentLoaded', function () {
    const sections = document.querySelectorAll('.section');
    const scrollDots = document.querySelectorAll('.scroll-dot');
    const scrollContainer = document.querySelector('.scroll-container');
    let isScrolling = false;
    let currentSection = 0;
    let scrollTimeout;
    let wheelAccumulator = 0;
    let lastWheelTime = Date.now();

    // Modern easing functions
    const easings = {
        easeOutCubic: t => 1 - Math.pow(1 - t, 3),
        easeInOutQuart: t => t < 0.5 ? 8 * t * t * t * t : 1 - Math.pow(-2 * t + 2, 4) / 2,
        easeOutExpo: t => t === 1 ? 1 : 1 - Math.pow(2, -10 * t),
        easeInOutBack: t => {
            const c1 = 1.70158;
            const c2 = c1 * 1.525;
            return t < 0.5
                ? (Math.pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                : (Math.pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
        }
    };

    // Enhanced smooth scroll with momentum and spring physics
    function smoothScrollTo(targetSection, options = {}) {
        if (isScrolling) return Promise.resolve();

        const {
            duration = 1200,
            easing = 'easeInOutQuart',
            momentum = true
        } = options;

        return new Promise(resolve => {
            isScrolling = true;
            const targetPosition = targetSection.offsetTop;
            const startPosition = scrollContainer.scrollTop;
            const distance = targetPosition - startPosition;

            // Add momentum-based duration adjustment
            const adjustedDuration = momentum ?
                Math.max(800, Math.min(1500, duration + Math.abs(distance) * 0.3)) :
                duration;

            let start = null;

            // Add subtle spring effect at the end
            function springAnimation(currentTime) {
                if (start === null) start = currentTime;
                const timeElapsed = currentTime - start;
                let progress = Math.min(timeElapsed / adjustedDuration, 1);

                // Apply easing
                const easedProgress = easings[easing](progress);

                // Add subtle spring effect in the last 20% of animation
                let finalPosition = startPosition + distance * easedProgress;
                if (progress > 0.8) {
                    const springProgress = (progress - 0.8) / 0.2;
                    const springOffset = Math.sin(springProgress * Math.PI * 3) *
                        (1 - springProgress) * 8;
                    finalPosition += springOffset;
                }

                scrollContainer.scrollTop = finalPosition;

                if (timeElapsed < adjustedDuration) {
                    requestAnimationFrame(springAnimation);
                } else {
                    // Ensure exact position
                    scrollContainer.scrollTop = targetPosition;
                    isScrolling = false;
                    resolve();
                }
            }

            requestAnimationFrame(springAnimation);
        });
    }

    // Enhanced intersection observer for smoother section detection
    const observerOptions = {
        root: scrollContainer,
        rootMargin: '-10% 0px -10% 0px',
        threshold: [0, 0.25, 0.5, 0.75, 1]
    };

    const sectionObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const sectionIndex = Array.from(sections).indexOf(entry.target);

            if (entry.isIntersecting && entry.intersectionRatio > 0.5) {
                updateActiveDot(sectionIndex);
                currentSection = sectionIndex;

                // Trigger section-specific animations
                triggerSectionAnimations(entry.target, sectionIndex);
            }
        });
    }, observerOptions);

    // Observe all sections
    sections.forEach(section => sectionObserver.observe(section));

    // Enhanced active dot update with smooth transitions
    function updateActiveDot(activeIndex) {
        scrollDots.forEach((dot, index) => {
            dot.classList.toggle('active', index === activeIndex);

            // Add ripple effect on activation
            if (index === activeIndex) {
                addRippleEffect(dot);
            }
        });
    }

    // Add ripple effect to dots
    function addRippleEffect(element) {
        const ripple = element.querySelector('.ripple');
        if (ripple) ripple.remove();

        const newRipple = document.createElement('div');
        newRipple.className = 'ripple';
        element.appendChild(newRipple);

        // Remove ripple after animation
        setTimeout(() => newRipple.remove(), 600);
    }

    // Section-specific animation triggers
    function triggerSectionAnimations(section, index) {
        // Community section avatar re-animation
        if (section.classList.contains('community-section')) {
            const avatars = section.querySelectorAll('.avatar');
            avatars.forEach((avatar, avatarIndex) => {
                // Reset and retrigger animations with stagger
                avatar.style.animation = 'none';
                requestAnimationFrame(() => {
                    avatar.style.animation = `
                        modernPopIn 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275) forwards ${avatarIndex * 0.1}s,
                        modernFloat 8s ease-in-out infinite ${0.8 + avatarIndex * 0.1}s,
                        pulseGlow 4s ease-in-out infinite ${1.2 + avatarIndex * 0.1}s
                    `;
                });
            });
        }

        // Add parallax effect to background elements
        const parallaxElements = section.querySelectorAll('[data-parallax]');
        parallaxElements.forEach(el => {
            const speed = el.dataset.parallax || 0.5;
            const yPos = -(window.pageYOffset * speed);
            el.style.transform = `translateY(${yPos}px)`;
        });
    }

    // Enhanced wheel handling with momentum and direction detection
    let wheelDirection = 0;
    let wheelMomentum = 0;

    function handleWheel(e) {
        e.preventDefault();

        if (isScrolling) return;

        const now = Date.now();
        const timeDelta = now - lastWheelTime;
        lastWheelTime = now;

        // Accumulate wheel delta for smoother detection
        wheelAccumulator += e.deltaY;

        // Reset accumulator if too much time passed
        if (timeDelta > 200) {
            wheelAccumulator = e.deltaY;
        }

        // Clear existing timeout
        clearTimeout(scrollTimeout);

        // Determine scroll direction and momentum
        wheelDirection = wheelAccumulator > 0 ? 1 : -1;
        wheelMomentum = Math.min(Math.abs(wheelAccumulator) / 100, 2);

        scrollTimeout = setTimeout(() => {
            if (Math.abs(wheelAccumulator) > 50) { // Threshold for intentional scroll
                const newSection = Math.max(0, Math.min(sections.length - 1,
                    currentSection + wheelDirection));

                if (newSection !== currentSection) {
                    smoothScrollTo(sections[newSection], {
                        duration: 1000 + (wheelMomentum * 200),
                        easing: 'easeInOutBack'
                    });
                }
            }
            wheelAccumulator = 0;
        }, 100);
    }

    // Enhanced touch handling for mobile
    let touchStartY = 0;
    let touchEndY = 0;
    let touchStartTime = 0;
    let touchMomentum = 0;

    function handleTouchStart(e) {
        touchStartY = e.touches[0].clientY;
        touchStartTime = Date.now();
    }

    function handleTouchEnd(e) {
        if (isScrolling) return;

        touchEndY = e.changedTouches[0].clientY;
        const touchDistance = touchStartY - touchEndY;
        const touchTime = Date.now() - touchStartTime;
        touchMomentum = Math.abs(touchDistance) / touchTime;

        // Minimum swipe distance and maximum swipe time
        if (Math.abs(touchDistance) > 50 && touchTime < 500) {
            const direction = touchDistance > 0 ? 1 : -1;
            const newSection = Math.max(0, Math.min(sections.length - 1,
                currentSection + direction));

            if (newSection !== currentSection) {
                smoothScrollTo(sections[newSection], {
                    duration: 900 + (touchMomentum * 100),
                    easing: 'easeOutExpo'
                });
            }
        }
    }

    // Enhanced keyboard navigation
    function handleKeydown(e) {
        if (isScrolling) return;

        let targetSection = currentSection;
        let shouldScroll = false;

        switch (e.key) {
            case 'ArrowDown':
            case 'PageDown':
            case ' ': // Spacebar
                if (currentSection < sections.length - 1) {
                    targetSection = currentSection + 1;
                    shouldScroll = true;
                }
                break;
            case 'ArrowUp':
            case 'PageUp':
                if (currentSection > 0) {
                    targetSection = currentSection - 1;
                    shouldScroll = true;
                }
                break;
            case 'Home':
                targetSection = 0;
                shouldScroll = true;
                break;
            case 'End':
                targetSection = sections.length - 1;
                shouldScroll = true;
                break;
            case 'Tab':
                return; // Allow default tab behavior
        }

        if (shouldScroll) {
            e.preventDefault();
            smoothScrollTo(sections[targetSection], {
                duration: 1000,
                easing: 'easeInOutQuart'
            });
        }
    }

    // Dot click handlers with enhanced feedback
    scrollDots.forEach((dot, index) => {
        dot.addEventListener('click', (e) => {
            e.preventDefault();
            if (index !== currentSection) {
                smoothScrollTo(sections[index], {
                    duration: 1200,
                    easing: 'easeInOutBack'
                });
            }
        });

        // Add hover effects
        dot.addEventListener('mouseenter', () => {
            if (!dot.classList.contains('active')) {
                dot.style.transform = 'scale(1.3)';
            }
        });

        dot.addEventListener('mouseleave', () => {
            if (!dot.classList.contains('active')) {
                dot.style.transform = 'scale(1)';
            }
        });
    });

    // Event listeners with passive options for better performance
    scrollContainer.addEventListener('wheel', handleWheel, { passive: false });
    scrollContainer.addEventListener('touchstart', handleTouchStart, { passive: true });
    scrollContainer.addEventListener('touchend', handleTouchEnd, { passive: true });
    document.addEventListener('keydown', handleKeydown);

    // Optimized resize handler with debouncing
    let resizeTimeout;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            // Re-observe sections after resize
            sections.forEach(section => {
                sectionObserver.unobserve(section);
                sectionObserver.observe(section);
            });
        }, 250);
    });

    // Initialize - set first section as active
    updateActiveDot(0);
    currentSection = 0;

    // Performance monitoring (optional - remove in production)
    if (window.performance && window.performance.mark) {
        window.performance.mark('scroll-animation-initialized');
    }

    // Expose API for external control
    window.scrollAnimationAPI = {
        goToSection: (index) => {
            if (index >= 0 && index < sections.length && index !== currentSection) {
                return smoothScrollTo(sections[index]);
            }
            return Promise.resolve();
        },
        getCurrentSection: () => currentSection,
        isScrolling: () => isScrolling,
        getSectionCount: () => sections.length
    };
});    