// Complete function - replace your entire toggleEditDelete.js content with this:
function toggleDropdown(button) {
    const dropdown = button.closest('.dropdown-menu');
    const dropdownContent = dropdown.querySelector('.dropdown-content');
    const isActive = dropdown.classList.contains('active');

    // Close all other dropdowns first
    document.querySelectorAll('.dropdown-menu.active').forEach(menu => {
        if (menu !== dropdown) {
            menu.classList.remove('active');
            const btn = menu.querySelector('.dropdown-btn');
            if (btn) {
                btn.setAttribute('aria-expanded', 'false');
                btn.style.transform = '';
                const img = btn.querySelector('img');
                if (img) img.style.transform = '';
            }
            const content = menu.querySelector('.dropdown-content');
            if (content) {
                content.style.position = '';
                content.style.top = '';
                content.style.left = '';
                content.style.zIndex = '';
                content.style.display = '';
                content.style.visibility = '';
            }
        }
    });

    if (!isActive) {
        // Calculate button position BEFORE showing dropdown
        const buttonRect = button.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        // Apply fixed positioning IMMEDIATELY before showing
        dropdownContent.style.position = 'fixed';
        dropdownContent.style.display = 'block';
        dropdownContent.style.visibility = 'hidden'; // Hidden while positioning

        // Calculate position
        let top = buttonRect.bottom + scrollTop + 8;
        let left = buttonRect.right + scrollLeft - 200;

        // Viewport boundaries
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;

        // Adjust positioning
        if (left < 10) {
            left = 10;
        } else if (left + 200 > viewportWidth - 10) {
            left = buttonRect.left + scrollLeft - 8;
        }

        if (top + 180 > viewportHeight + scrollTop - 10) {
            top = buttonRect.top + scrollTop - 180 - 8;
        }

        // Apply final position and show
        dropdownContent.style.top = top + 'px';
        dropdownContent.style.left = left + 'px';
        dropdownContent.style.zIndex = '1500';
        dropdownContent.style.visibility = 'visible'; // Now show

        dropdown.classList.add('active');
        button.setAttribute('aria-expanded', 'true');

        // Animations
        button.style.transform = 'scale(1.05)';
        const img = button.querySelector('img');
        if (img) {
            img.style.transform = 'rotate(90deg)';
            img.style.transition = 'transform 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
        }

        if (dropdownContent) {
            dropdownContent.style.animation = 'dropdownSlideIn 0.4s cubic-bezier(0.4, 0, 0.2, 1)';
        }

        // Add backdrop
        setTimeout(() => {
            const backdrop = document.createElement('div');
            backdrop.className = 'dropdown-backdrop';
            backdrop.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.1);
                backdrop-filter: blur(2px);
                z-index: 1400;
                opacity: 0;
                transition: opacity 0.3s ease;
            `;
            document.body.appendChild(backdrop);

            requestAnimationFrame(() => {
                backdrop.style.opacity = '1';
            });

            backdrop.addEventListener('click', () => {
                toggleDropdown(button);
            });
        }, 50);

    } else {
        dropdown.classList.remove('active');
        button.setAttribute('aria-expanded', 'false');

        // Reset ALL positioning and display properties
        dropdownContent.style.position = '';
        dropdownContent.style.top = '';
        dropdownContent.style.left = '';
        dropdownContent.style.zIndex = '';
        dropdownContent.style.display = '';
        dropdownContent.style.visibility = '';

        // Reset button styles
        button.style.transform = '';
        const img = button.querySelector('img');
        if (img) {
            img.style.transform = '';
        }

        // Remove backdrop
        const backdrop = document.querySelector('.dropdown-backdrop');
        if (backdrop) {
            backdrop.style.opacity = '0';
            setTimeout(() => {
                if (backdrop.parentNode) {
                    document.body.removeChild(backdrop);
                }
            }, 300);
        }
    }
}