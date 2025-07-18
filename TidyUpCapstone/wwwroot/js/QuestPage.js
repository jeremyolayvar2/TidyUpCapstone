document.addEventListener('DOMContentLoaded', function () {
    // Quest card interactions
    document.querySelectorAll('.quest-card').forEach(card => {
        // Mouse events for desktop
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

        // Click events for all devices
        card.addEventListener('click', function () {
            this.style.transform = 'scale(1.02)';
            setTimeout(() => {
                this.style.transform = window.innerWidth > 768 ? 'translateY(-3px)' : 'scale(1)';
            }, 150);
        });

        // Touch events for mobile
        card.addEventListener('touchstart', function () {
            this.style.opacity = '0.9';
        }, { passive: true });

        card.addEventListener('touchend', function () {
            this.style.opacity = '1';
        }, { passive: true });
    });

    // Button interactions
    document.querySelectorAll('.claim-button').forEach(button => {
        button.addEventListener('click', function (e) {
            e.stopPropagation();

            // Visual feedback
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 100);

            // Add your ASP.NET Core MVC action calls here
            console.log('Button clicked:', this.textContent);

            // Example claim logic
            if (this.classList.contains('claim-active')) {
                this.textContent = 'Claimed!';
                this.classList.remove('claim-active');
                this.classList.add('claim-disabled');

                // Reset after demo
                setTimeout(() => {
                    this.textContent = 'Claim';
                    this.classList.remove('claim-disabled');
                    this.classList.add('claim-active');
                }, 3000);
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

    // Smooth scrolling for quest cards on mobile
    const questContainer = document.querySelector('.quest-cards-container');
    if (questContainer && window.innerWidth <= 768) {
        questContainer.style.scrollBehavior = 'smooth';
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

    // Handle window resize for responsive behavior
    let resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            // Reset styles on resize
            document.querySelectorAll('.quest-card').forEach(card => {
                card.style.transform = '';
                card.style.boxShadow = '';
            });
        }, 250);
    });
});