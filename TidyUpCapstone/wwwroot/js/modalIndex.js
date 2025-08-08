(function () {
    'use strict';

    // Get modal elements
    const loginModal = document.getElementById('loginModal');
    const registerModal = document.getElementById('registerModal');
    const closeLoginModal = document.getElementById('closeLoginModal');
    const closeRegisterModal = document.getElementById('closeRegisterModal');
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const loginSubmitBtn = document.getElementById('loginSubmitBtn');
    const registerSubmitBtn = document.getElementById('registerSubmitBtn');
    const loginErrorAlert = document.getElementById('loginErrorAlert');
    const loginErrorMessage = document.getElementById('loginErrorMessage');
    const registerErrorAlert = document.getElementById('registerErrorAlert');
    const registerErrorMessage = document.getElementById('registerErrorMessage');

    // Modal management functions
    function openLoginModal() {
        console.log('Opening login modal...');
        if (loginModal) {
            loginModal.classList.add('active');
            document.body.style.overflow = 'hidden';
        } else {
            console.error('Login modal not found!');
        }
    }

    function openRegisterModal() {
        console.log('Opening register modal...');
        if (registerModal) {
            registerModal.classList.add('active');
            document.body.style.overflow = 'hidden';
        } else {
            console.error('Register modal not found!');
        }
    }

    function closeAllModals() {
        console.log('Closing all modals...');
        if (loginModal) loginModal.classList.remove('active');
        if (registerModal) registerModal.classList.remove('active');
        document.body.style.overflow = '';
    }

    // Show error message functions
    function showLoginError(message) {
        console.log('Login error:', message);
        if (loginErrorMessage && loginErrorAlert) {
            loginErrorMessage.textContent = message;
            loginErrorAlert.style.display = 'flex';
        }
        if (loginSubmitBtn) {
            loginSubmitBtn.disabled = false;
            loginSubmitBtn.innerHTML = 'Sign In';
        }
    }

    function showRegisterError(message) {
        console.log('Register error:', message);
        if (registerErrorMessage && registerErrorAlert) {
            registerErrorMessage.textContent = message;
            registerErrorAlert.style.display = 'flex';
        }
        if (registerSubmitBtn) {
            registerSubmitBtn.disabled = false;
            registerSubmitBtn.innerHTML = 'Start Your Journey';
        }
    }

    function showSuccess(message) {
        console.log('Success:', message);

        // Use the custom top alert instead of browser alert
        topAlert.success(
            'Account Created Successfully!',
            'Please check your email to verify your account before signing in.'
        );
    }
    // Event listeners for opening modals
    document.addEventListener('click', function (e) {
        console.log('Click detected on:', e.target);

        // Open login modal
        if (e.target.closest('.btn-sign-in') || e.target.id === 'openLoginModal') {
            console.log('Login button clicked');
            e.preventDefault();
            openLoginModal();
        }

        // Open register modal
        if (e.target.closest('.btn-get-started') || e.target.id === 'openRegisterModal') {
            console.log('Register button clicked');
            e.preventDefault();
            openRegisterModal();
        }
    });

    // Close modal events
    if (closeLoginModal) {
        closeLoginModal.addEventListener('click', function (e) {
            console.log('Close login modal clicked');
            closeAllModals();
        });
    }
    if (closeRegisterModal) {
        closeRegisterModal.addEventListener('click', function (e) {
            console.log('Close register modal clicked');
            closeAllModals();
        });
    }

    // Close on overlay click
    if (loginModal) {
        loginModal.addEventListener('click', function (e) {
            if (e.target === loginModal) {
                console.log('Login overlay clicked');
                closeAllModals();
            }
        });
    }
    if (registerModal) {
        registerModal.addEventListener('click', function (e) {
            if (e.target === registerModal) {
                console.log('Register overlay clicked');
                closeAllModals();
            }
        });
    }

    // Close on Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            console.log('Escape key pressed');
            closeAllModals();
        }
    });

    // Handle login form submission
    if (loginForm) {
        loginForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            console.log('Login form submitted');

            // Show loading state
            if (loginSubmitBtn) {
                loginSubmitBtn.disabled = true;
                loginSubmitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Signing In...';
            }

            // Hide previous errors
            if (loginErrorAlert) {
                loginErrorAlert.style.display = 'none';
            }

            try {
                // Get form data
                const formData = new FormData(loginForm);

                // Submit form via AJAX
                const response = await fetch('/Account/ModalLogin', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();
                console.log('Login response:', result);

                if (result.success) {
                    // Success - redirect to main page
                    window.location.href = result.redirectUrl || '/Home/Main';
                } else {
                    // Show error
                    showLoginError(result.message || 'An error occurred during login.');
                }
            } catch (error) {
                console.error('Login error:', error);
                showLoginError('An unexpected error occurred. Please try again.');
            }
        });
    }

    // Handle register form submission
    // Replace the register form submission part in your JavaScript with this updated version

    // Handle register form submission
    if (registerForm) {
        registerForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            console.log('Register form submitted');

            // Show loading state
            if (registerSubmitBtn) {
                registerSubmitBtn.disabled = true;
                registerSubmitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating Account...';
            }

            // Hide previous errors
            if (registerErrorAlert) {
                registerErrorAlert.style.display = 'none';
            }

            try {
                // Get form data
                const formData = new FormData(registerForm);

                // Fix checkbox values - convert to proper boolean strings
                const acceptTermsCheckbox = document.getElementById('acceptTerms');
                const acceptPrivacyCheckbox = document.getElementById('acceptPrivacy');
                const marketingEmailsCheckbox = document.getElementById('marketingEmails');

                // Remove existing checkbox values and set proper ones
                formData.delete('AcceptTerms');
                formData.delete('AcceptPrivacy');
                formData.delete('MarketingEmails');

                // Set boolean values as strings
                formData.append('AcceptTerms', acceptTermsCheckbox.checked ? 'true' : 'false');
                formData.append('AcceptPrivacy', acceptPrivacyCheckbox.checked ? 'true' : 'false');
                formData.append('MarketingEmails', marketingEmailsCheckbox.checked ? 'true' : 'false');

                console.log('Form data being sent:');
                for (let [key, value] of formData.entries()) {
                    console.log(key + ': ' + value);
                }

                // Submit form via AJAX
                const response = await fetch('/Account/ModalRegister', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();
                console.log('Register response:', result);

                if (result.success) {
                    // Success - show message and redirect
                    closeAllModals();
                    showSuccess(result.message);
                    if (result.redirectUrl) {
                        setTimeout(() => {
                            window.location.href = result.redirectUrl;
                        }, 2000);
                    }
                } else {
                    // Show error
                    showRegisterError(result.message || 'An error occurred during registration.');
                }
            } catch (error) {
                console.error('Registration error:', error);
                showRegisterError('An unexpected error occurred. Please try again.');
            }
        });
    }
    // Input focus animations
    const formControls = document.querySelectorAll('.login-form-control, .register-form-control');
    formControls.forEach(input => {
        input.addEventListener('focus', function () {
            const inputGroup = this.closest('.login-input-group, .register-input-group');
            if (inputGroup) {
                inputGroup.style.transform = 'translateY(-2px)';
            }
        });

        input.addEventListener('blur', function () {
            const inputGroup = this.closest('.login-input-group, .register-input-group');
            if (inputGroup) {
                inputGroup.style.transform = 'translateY(0)';
            }
        });
    });

    // Ripple effect for buttons
    function createRipple(event) {
        const button = event.currentTarget;
        const circle = document.createElement('span');
        const diameter = Math.max(button.clientWidth, button.clientHeight);
        const radius = diameter / 2;

        circle.style.width = circle.style.height = `${diameter}px`;
        circle.style.left = `${event.clientX - button.offsetLeft - radius}px`;
        circle.style.top = `${event.clientY - button.offsetTop - radius}px`;
        circle.classList.add('login-ripple');

        const ripple = button.getElementsByClassName('login-ripple')[0];
        if (ripple) {
            ripple.remove();
        }

        button.appendChild(circle);
    }

    // Apply ripple effect to buttons
    const buttons = document.querySelectorAll('.login-btn, .register-btn');
    buttons.forEach(btn => {
        btn.addEventListener('click', createRipple);
    });

    // Password toggle function
    window.toggleRegisterPassword = function (element) {
        const input = element.previousElementSibling.previousElementSibling;
        if (input && input.tagName === 'INPUT') {
            if (input.type === "password") {
                input.type = "text";
                element.classList.replace("fa-eye", "fa-eye-slash");
            } else {
                input.type = "password";
                element.classList.replace("fa-eye-slash", "fa-eye");
            }
        }
    };

    // Switch between modals
    window.switchToLogin = function () {
        console.log('Switching to login modal');
        closeAllModals();
        setTimeout(() => openLoginModal(), 100);
    };

    window.switchToRegister = function () {
        console.log('Switching to register modal');
        closeAllModals();
        setTimeout(() => openRegisterModal(), 100);
    };

    // Make functions available globally
    window.LoginModal = {
        open: openLoginModal,
        close: closeAllModals,
        showError: showLoginError
    };

    window.RegisterModal = {
        open: openRegisterModal,
        close: closeAllModals,
        showError: showRegisterError
    };

    // Prevent form submission on disabled buttons
    document.addEventListener('click', function (e) {
        if (e.target.matches('.login-btn[disabled], .register-btn[disabled]')) {
            e.preventDefault();
            return false;
        }
    });

    // Debug: Log when DOM is ready
    console.log('Modal script loaded. Elements found:');
    console.log('Login modal:', !!loginModal);
    console.log('Register modal:', !!registerModal);
    console.log('Login form:', !!loginForm);
    console.log('Register form:', !!registerForm);

})();




//Alert for creating an account

class SimpleTopAlert {
    constructor() {
        this.alerts = [];
        this.alertCount = 0;
    }

    show(options = {}) {
        const {
            type = 'success',
            title = '',
            message = '',
            duration = 5000,
            autoClose = true
        } = options;

        // Create alert element
        const alertId = `top-alert-${++this.alertCount}`;
        const alertElement = document.createElement('div');
        alertElement.id = alertId;
        alertElement.className = `top-alert ${type}`;

        // Create alert content
        let alertContent = '';

        if (title) {
            alertContent += `<h3 class="top-alert-title">${title}</h3>`;
        }

        if (message) {
            alertContent += `<p class="top-alert-message">${message}</p>`;
        }

        if (autoClose) {
            alertContent += `<div class="top-alert-progress"></div>`;
        }

        alertElement.innerHTML = alertContent;

        // Add to page
        document.body.appendChild(alertElement);
        this.alerts.push({ id: alertId, element: alertElement });

        // Trigger animation
        setTimeout(() => {
            alertElement.classList.add('active');

            // Start progress bar if auto-close
            if (autoClose) {
                const progressBar = alertElement.querySelector('.top-alert-progress');
                if (progressBar) {
                    setTimeout(() => {
                        progressBar.classList.add('auto-close');
                    }, 100);
                }
            }
        }, 50);

        // Auto-close functionality
        if (autoClose && duration > 0) {
            setTimeout(() => {
                this.close(alertId);
            }, duration);
        }

        return alertId;
    }

    close(alertId) {
        const alertIndex = this.alerts.findIndex(alert => alert.id === alertId);
        if (alertIndex === -1) return;

        const alert = this.alerts[alertIndex];
        const element = alert.element;

        // Remove active class to trigger exit animation
        element.classList.remove('active');

        // Remove from DOM after animation
        setTimeout(() => {
            if (element && element.parentNode) {
                element.parentNode.removeChild(element);
            }
            this.alerts.splice(alertIndex, 1);
        }, 500);
    }

    // Convenience methods
    success(title, message = '', options = {}) {
        return this.show({
            type: 'success',
            title,
            message,
            ...options
        });
    }

    warning(title, message = '', options = {}) {
        return this.show({
            type: 'warning',
            title,
            message,
            ...options
        });
    }

    error(title, message = '', options = {}) {
        return this.show({
            type: 'error',
            title,
            message,
            ...options
        });
    }

    // Quick message without title
    message(message, type = 'success', options = {}) {
        return this.show({
            type,
            title: '',
            message,
            ...options
        });
    }
}

// Initialize and make globally available
const topAlert = new SimpleTopAlert();
window.topAlert = topAlert;

// Demo functions
function showSuccessDemo() {
    topAlert.success(
        'Account Created Successfully!',
        'Please check your email to verify your account before signing in.'
    );
}

function showWarningDemo() {
    topAlert.warning(
        'Verification Required',
        'Your email address needs to be verified before you can continue.'
    );
}

function showErrorDemo() {
    topAlert.error(
        'Registration Failed',
        'There was an error creating your account. Please try again.'
    );
}

// Example of how to use in your registration success handler
function showSuccess(message) {
    console.log('Success:', message);

    // Replace the old alert() with this:
    topAlert.success(
        'Account Created Successfully!',
        'Please check your email to verify your account before signing in.'
    );
}