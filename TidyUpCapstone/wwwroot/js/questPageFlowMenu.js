class FlowingMenu {
    constructor(containerId, items = []) {
        this.container = document.getElementById(containerId);
        this.items = items;
        this.animationDefaults = { duration: 0.6, ease: 'expo' };
        this.isMobile = this.checkMobile();
        this.isTouch = this.checkTouch();
        this.init();
        this.bindResize();
    }

    checkMobile() {
        return window.innerWidth <= 768;
    }

    checkTouch() {
        return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
    }

    init() {
        if (!this.container) {
            console.warn('Flowing menu container not found');
            return;
        }
        this.render();
        this.bindEvents();
    }

    render() {
        this.container.innerHTML = '';

        this.items.forEach((item, index) => {
            const menuItem = this.createMenuItem(item, index);
            this.container.appendChild(menuItem);
        });
    }

    createMenuItem(item, index) {
        const menuItem = document.createElement('div');
        menuItem.className = 'menu-item';
        menuItem.setAttribute('data-index', index);
        menuItem.setAttribute('role', 'menuitem');
        menuItem.setAttribute('tabindex', '0');

        // Create repeated content for marquee effect
        const repeatedContent = Array.from({ length: 4 }, () => `
            <span class="marquee-text">${item.text}</span>
            <div class="marquee-image" style="background-image: url(${item.image})" aria-hidden="true"></div>
        `).join('');

        menuItem.innerHTML = `
            <a class="menu-link" href="${item.link}" ${this.isTouch ? 'ontouchstart=""' : ''} 
               aria-label="Navigate to ${item.text}">
                ${item.text}
            </a>
            <div class="marquee-container" aria-hidden="true">
                <div class="marquee-inner">
                    <div class="marquee-content animate-marquee">
                        ${repeatedContent}
                    </div>
                </div>
            </div>
        `;

        return menuItem;
    }

    findClosestEdge(clientX, clientY, rect) {
        const mouseX = clientX - rect.left;
        const mouseY = clientY - rect.top;
        const width = rect.width;
        const height = rect.height;

        const topEdgeDist = Math.pow(mouseX - width / 2, 2) + Math.pow(mouseY, 2);
        const bottomEdgeDist = Math.pow(mouseX - width / 2, 2) + Math.pow(mouseY - height, 2);

        return topEdgeDist < bottomEdgeDist ? 'top' : 'bottom';
    }

    handleMouseEnter(event, menuItem) {
        // Disable complex animations on mobile for performance
        if (this.isMobile || this.isTouch) return;

        const marqueeContainer = menuItem.querySelector('.marquee-container');
        const marqueeInner = menuItem.querySelector('.marquee-inner');

        if (!marqueeContainer || !marqueeInner) return;

        const rect = menuItem.getBoundingClientRect();
        const edge = this.findClosestEdge(event.clientX, event.clientY, rect);

        // Check if GSAP is available
        if (typeof gsap !== 'undefined') {
            const timeline = gsap.timeline({ defaults: this.animationDefaults });

            timeline
                .set(marqueeContainer, { y: edge === 'top' ? '-101%' : '101%' })
                .set(marqueeInner, { y: edge === 'top' ? '101%' : '-101%' })
                .to([marqueeContainer, marqueeInner], { y: '0%' });
        } else {
            // Fallback without GSAP
            marqueeContainer.style.transform = 'translateY(0%)';
            marqueeInner.style.transform = 'translateY(0%)';
        }
    }

    handleMouseLeave(event, menuItem) {
        // Disable complex animations on mobile for performance
        if (this.isMobile || this.isTouch) return;

        const marqueeContainer = menuItem.querySelector('.marquee-container');
        const marqueeInner = menuItem.querySelector('.marquee-inner');

        if (!marqueeContainer || !marqueeInner) return;

        const rect = menuItem.getBoundingClientRect();
        const edge = this.findClosestEdge(event.clientX, event.clientY, rect);

        // Check if GSAP is available
        if (typeof gsap !== 'undefined') {
            const timeline = gsap.timeline({ defaults: this.animationDefaults });

            timeline
                .to(marqueeContainer, { y: edge === 'top' ? '-101%' : '101%' })
                .to(marqueeInner, { y: edge === 'top' ? '101%' : '-101%' });
        } else {
            // Fallback without GSAP
            marqueeContainer.style.transform = edge === 'top' ? 'translateY(-101%)' : 'translateY(101%)';
            marqueeInner.style.transform = edge === 'top' ? 'translateY(101%)' : 'translateY(-101%)';
        }
    }

    handleTouchStart(event, menuItem) {
        // Simple touch feedback for mobile
        menuItem.style.opacity = '0.8';
        setTimeout(() => {
            menuItem.style.opacity = '1';
        }, 150);
    }

    handleKeyDown(event, menuItem) {
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            const link = menuItem.querySelector('.menu-link');
            if (link) {
                link.click();
            }
        }
    }

    bindEvents() {
        if (!this.container) return;

        // Mouse events for desktop (non-touch devices)
        if (!this.isMobile && !this.isTouch) {
            this.container.addEventListener('mouseenter', (event) => {
                const menuItem = event.target.closest('.menu-item');
                if (menuItem) {
                    this.handleMouseEnter(event, menuItem);
                }
            }, true);

            this.container.addEventListener('mouseleave', (event) => {
                const menuItem = event.target.closest('.menu-item');
                if (menuItem) {
                    this.handleMouseLeave(event, menuItem);
                }
            }, true);
        }

        // Touch events for mobile/touch devices
        this.container.addEventListener('touchstart', (event) => {
            const menuItem = event.target.closest('.menu-item');
            if (menuItem) {
                this.handleTouchStart(event, menuItem);
            }
        }, { passive: true });

        // Keyboard navigation
        this.container.addEventListener('keydown', (event) => {
            const menuItem = event.target.closest('.menu-item');
            if (menuItem) {
                this.handleKeyDown(event, menuItem);
            }
        });

        // Click/tap events for all devices
        this.container.addEventListener('click', (event) => {
            const menuLink = event.target.closest('.menu-link');
            if (menuLink) {
                // Add click feedback
                menuLink.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    menuLink.style.transform = 'scale(1)';
                }, 100);
            }
        });

        // Focus events for accessibility
        this.container.addEventListener('focus', (event) => {
            const menuItem = event.target.closest('.menu-item');
            if (menuItem) {
                menuItem.style.outline = '2px solid white';
                menuItem.style.outlineOffset = '2px';
            }
        }, true);

        this.container.addEventListener('blur', (event) => {
            const menuItem = event.target.closest('.menu-item');
            if (menuItem) {
                menuItem.style.outline = '';
                menuItem.style.outlineOffset = '';
            }
        }, true);
    }

    bindResize() {
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                const newIsMobile = this.checkMobile();
                const newIsTouch = this.checkTouch();

                if (newIsMobile !== this.isMobile || newIsTouch !== this.isTouch) {
                    this.isMobile = newIsMobile;
                    this.isTouch = newIsTouch;
                    this.render(); // Re-render with new mobile/touch state
                }
            }, 250);
        });
    }

    // Method to update items dynamically
    updateItems(newItems) {
        this.items = newItems;
        this.render();
    }

    // Method to destroy the menu
    destroy() {
        if (this.container) {
            this.container.innerHTML = '';
        }
    }
}

// Enhanced initialization that works with your layout system
document.addEventListener('DOMContentLoaded', function () {
    // Wait for layout to be ready
    setTimeout(() => {
        initializeQuestPage();
    }, 100);
});

function initializeQuestPage() {
    // Demo items for flowing menu - using your navigation structure
    const demoItems = [
        { link: '#', text: 'My Items', image: 'https://picsum.photos/600/400?random=1' },
        { link: '#', text: 'Leaderboard', image: 'https://picsum.photos/600/400?random=2' },
        { link: '#', text: 'Profile', image: 'https://picsum.photos/600/400?random=3' },
        { link: '#', text: 'Shop', image: 'https://picsum.photos/600/400?random=4' }
    ];

    // Initialize flowing menu
    let flowingMenu;
    try {
        flowingMenu = new FlowingMenu('flowing-menu', demoItems);
    } catch (error) {
        console.warn('Could not initialize flowing menu:', error);
    }

    // Enhanced quest card interactions that work with your layout
    document.querySelectorAll('.quest-card').forEach((card, index) => {
        // Add accessibility attributes
        card.setAttribute('role', 'button');
        card.setAttribute('tabindex', '0');
        card.setAttribute('aria-label', `Quest card ${index + 1}`);

        // Mouse events for desktop
        card.addEventListener('mouseenter', function () {
            if (window.innerWidth > 768 && !('ontouchstart' in window)) {
                this.style.transform = 'translateY(-3px)';
                this.style.boxShadow = '0 8px 25px rgba(0,0,0,0.15)';
            }
        });

        card.addEventListener('mouseleave', function () {
            if (window.innerWidth > 768 && !('ontouchstart' in window)) {
                this.style.transform = 'translateY(0)';
                this.style.boxShadow = '0 2px 8px rgba(0,0,0,0.1)';
            }
        });

        // Click/tap events
        card.addEventListener('click', function () {
            this.style.transform = 'scale(1.02)';
            setTimeout(() => {
                this.style.transform = window.innerWidth > 768 ? 'translateY(-3px)' : 'scale(1)';
            }, 150);
        });

        // Keyboard navigation
        card.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });

        // Touch events for mobile feedback
        card.addEventListener('touchstart', function () {
            this.style.opacity = '0.9';
        }, { passive: true });

        card.addEventListener('touchend', function () {
            this.style.opacity = '1';
        }, { passive: true });
    });

    // Enhanced button interactions that integrate with your MVC actions
    document.querySelectorAll('.claim-button').forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();

            // Visual feedback
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 100);

            // Integration point for your ASP.NET Core MVC actions
            console.log('Button clicked:', this.textContent);

            // Example: Call your controller action
            // This is where you'd integrate with your existing MVC structure
            if (this.classList.contains('claim-active')) {
                handleQuestClaim(this);
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

    // Smooth scrolling for quest cards container
    const questContainer = document.querySelector('.quest-cards-container');
    if (questContainer) {
        questContainer.style.scrollBehavior = 'smooth';

        // Add scroll indicators for mobile
        if (window.innerWidth <= 768) {
            addScrollIndicators(questContainer);
        }
    }

    // Intersection observer for achievement animations
    if ('IntersectionObserver' in window) {
        const achievementObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        document.querySelectorAll('.achievement-title-section, .achievement-main-section, .achievement-stats-section').forEach(item => {
            item.style.opacity = '0';
            item.style.transform = 'translateY(20px)';
            item.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            achievementObserver.observe(item);
        });
    }

    // Handle orientation change for mobile devices
    window.addEventListener('orientationchange', function () {
        setTimeout(() => {
            if (flowingMenu) {
                flowingMenu.isMobile = flowingMenu.checkMobile();
                flowingMenu.isTouch = flowingMenu.checkTouch();
            }

            // Recalculate layout
            handleResponsiveLayout();
        }, 100);
    });

    // Performance optimization: Debounced scroll handler
    let scrollTimeout;
    window.addEventListener('scroll', function () {
        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(() => {
            handleScrollEffects();
        }, 10);
    }, { passive: true });

    // Initialize responsive layout
    handleResponsiveLayout();
}

// Function to handle quest claim - integrate with your MVC actions
function handleQuestClaim(button) {
    // This is where you'd make your AJAX call to your ASP.NET Core controller
    // Example:
    /*
    fetch('/Home/ClaimQuest', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({ questId: button.dataset.questId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            button.textContent = 'Claimed!';
            button.classList.remove('claim-active');
            button.classList.add('claim-disabled');
        }
    });
    */

    // Demo behavior
    button.textContent = 'Claimed!';
    button.classList.remove('claim-active');
    button.classList.add('claim-disabled');

    // Reset after demo
    setTimeout(() => {
        button.textContent = 'Claim';
        button.classList.remove('claim-disabled');
        button.classList.add('claim-active');
    }, 3000);
}

// Add scroll indicators for mobile quest cards
function addScrollIndicators(container) {
    const indicator = document.createElement('div');
    indicator.style.cssText = `
        position: absolute;
        bottom: 10px;
        right: 10px;
        background: rgba(255,255,255,0.3);
        border-radius: 10px;
        padding: 5px 10px;
        font-size: 12px;
        color: white;
        pointer-events: none;
        opacity: 0;
        transition: opacity 0.3s ease;
    `;
    indicator.textContent = 'Scroll for more';

    container.parentElement.style.position = 'relative';
    container.parentElement.appendChild(indicator);

    container.addEventListener('scroll', () => {
        const { scrollTop, scrollHeight, clientHeight } = container;
        const isAtBottom = scrollTop + clientHeight >= scrollHeight - 10;
        indicator.style.opacity = isAtBottom ? '0' : '1';
    });
}

// Handle responsive layout changes
function handleResponsiveLayout() {
    const questCards = document.querySelectorAll('.quest-card');
    const isTablet = window.innerWidth >= 768 && window.innerWidth <= 1023;
    const isMobile = window.innerWidth < 768;

    questCards.forEach(card => {
        if (isMobile) {
            // Mobile optimizations
            card.style.transform = '';
            card.style.boxShadow = '';
        } else if (isTablet) {
            // Tablet optimizations
            card.style.transition = 'all 0.2s ease';
        }
    });
}

// Handle scroll effects
function handleScrollEffects() {
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

    // Optional: Parallax effect for title (only on desktop)
    if (window.innerWidth > 1024) {
        const title = document.querySelector('.quest-title');
        if (title) {
            title.style.transform = `rotate(-3deg) translateY(${scrollTop * 0.05}px)`;
        }
    }
}

// Export for potential external use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { FlowingMenu, initializeQuestPage };
}