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

    // Individual modal closing functions
    function closeLoginModalOnly() {
        console.log('Closing login modal only');
        if (loginModal) {
            loginModal.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    function closeRegisterModalOnly() {
        console.log('Closing register modal only');
        if (registerModal) {
            registerModal.classList.remove('active');
            document.body.style.overflow = '';
        }
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

    // FIXED: Success message functions (these were missing)
    function showLoginSuccess(message) {
        console.log('Login success:', message);
        topAlert.success('Login Successful!', message);

        if (loginErrorAlert) {
            loginErrorAlert.style.display = 'none';
        }
    }

    function showRegisterSuccess(message) {
        console.log('Register success:', message);
        topAlert.success('Registration Successful!', message);

        if (registerErrorAlert) {
            registerErrorAlert.style.display = 'none';
        }
    }

    function showSuccess(message) {
        console.log('Success:', message);
        topAlert.success(
            'Account Created Successfully!',
            'Please check your email to verify your account before signing in.'
        );
    }

    // Handle server-side data and URL parameters
    function handleServerData() {
        console.log('Handling server data...');

        if (typeof window.serverData !== 'undefined') {
            console.log('Server data found:', window.serverData);

            // Handle login errors from OAuth or other server operations
            if (window.serverData.loginError && window.serverData.loginError.trim() !== '') {
                console.log('Showing login error from server:', window.serverData.loginError);
                showLoginError(window.serverData.loginError);
                openLoginModal();
            }

            // Handle success messages
            if (window.serverData.successMessage && window.serverData.successMessage.trim() !== '') {
                console.log('Showing success message from server:', window.serverData.successMessage);
                topAlert.success('Success!', window.serverData.successMessage);
            }

            // Handle URL error parameter (from OAuth failures)
            if (window.serverData.error && window.serverData.error.trim() !== '') {
                console.log('Showing error from URL parameter:', window.serverData.error);
                let errorMessage = 'An error occurred. Please try again.';

                switch (window.serverData.error) {
                    case 'oauth_failed':
                        errorMessage = 'There was an error with Google sign-in. Please try again.';
                        break;
                    case 'access_denied':
                        errorMessage = 'Access was denied. Please try signing in again.';
                        break;
                    default:
                        errorMessage = window.serverData.error;
                }

                showLoginError(errorMessage);
                openLoginModal();
            }

            // Handle URL parameters to show specific modals
            if (window.serverData.showLogin === 'true') {
                console.log('URL parameter indicates login modal should be shown');
                setTimeout(() => openLoginModal(), 100);
            }

            if (window.serverData.showRegister === 'true') {
                console.log('URL parameter indicates register modal should be shown');
                setTimeout(() => openRegisterModal(), 100);
            }
        } else {
            console.log('No server data found, checking URL parameters manually');

            // Fallback: Check URL parameters directly
            const urlParams = new URLSearchParams(window.location.search);

            if (urlParams.get('showLogin') === 'true') {
                console.log('URL showLogin parameter found');
                setTimeout(() => openLoginModal(), 100);
            }

            if (urlParams.get('showRegister') === 'true') {
                console.log('URL showRegister parameter found');
                setTimeout(() => openRegisterModal(), 100);
            }

            // Check for error parameter in URL
            const errorParam = urlParams.get('error');
            if (errorParam) {
                let errorMessage = 'An error occurred. Please try again.';

                switch (errorParam) {
                    case 'oauth_failed':
                        errorMessage = 'There was an error with Google sign-in. Please try again.';
                        break;
                    case 'access_denied':
                        errorMessage = 'Access was denied. Please try signing in again.';
                        break;
                    default:
                        errorMessage = errorParam;
                }

                showLoginError(errorMessage);
                setTimeout(() => openLoginModal(), 100);
            }
        }
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
                const formData = new FormData(loginForm);

                console.log('Submitting login request...');
                const response = await fetch('/Account/ModalLogin', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const result = await response.json();
                console.log('Login response received:', result);

                if (result.success) {
                    console.log('Login successful, redirecting to:', result.redirectUrl);

                    // Show success message
                    showLoginSuccess('Sign in successful! Redirecting...');

                    // FIXED: Use the exact URL returned by the server
                    const redirectUrl = result.redirectUrl;
                    console.log('Redirecting to:', redirectUrl);

                    // Small delay to show success message, then redirect
                    setTimeout(() => {
                        window.location.href = redirectUrl;
                    }, 500);

                } else {
                    console.log('Login failed:', result.message);
                    showLoginError(result.message || 'An error occurred during login.');
                }
            } catch (error) {
                console.error('Login error:', error);
                showLoginError('An unexpected error occurred. Please try again.');
            } finally {
                // Reset button state
                if (loginSubmitBtn) {
                    loginSubmitBtn.disabled = false;
                    loginSubmitBtn.innerHTML = 'Sign In';
                }
            }
        });
    }

    // FIXED: Handle register form submission
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
                const formData = new FormData(registerForm);

                // Fix checkbox values
                const acceptTermsCheckbox = document.getElementById('acceptTerms');
                const acceptPrivacyCheckbox = document.getElementById('acceptPrivacy');
                const marketingEmailsCheckbox = document.getElementById('marketingEmails');

                formData.delete('AcceptTerms');
                formData.delete('AcceptPrivacy');
                formData.delete('MarketingEmails');

                formData.append('AcceptTerms', acceptTermsCheckbox.checked ? 'true' : 'false');
                formData.append('AcceptPrivacy', acceptPrivacyCheckbox.checked ? 'true' : 'false');
                formData.append('MarketingEmails', marketingEmailsCheckbox.checked ? 'true' : 'false');

                console.log('Form data being sent:', {
                    AcceptTerms: acceptTermsCheckbox.checked,
                    AcceptPrivacy: acceptPrivacyCheckbox.checked,
                    MarketingEmails: marketingEmailsCheckbox.checked
                });

                const response = await fetch('/Account/ModalRegister', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const result = await response.json();
                console.log('Register response:', result);

                if (result.success) {
                    console.log('Registration successful');

                    // Show success message
                    showRegisterSuccess(result.message || 'Account created successfully!');

                    // Handle different scenarios
                    if (result.showLogin) {
                        // For new registrations that need email verification
                        setTimeout(() => {
                            closeRegisterModalOnly();
                            setTimeout(() => {
                                openLoginModal();
                            }, 300);
                        }, 2000);
                    } else if (result.redirectUrl) {
                        // For OAuth or auto-verified users - FIXED: Use server URL
                        setTimeout(() => {
                            window.location.href = result.redirectUrl;
                        }, 2000);
                    } else {
                        // Default fallback
                        setTimeout(() => {
                            closeRegisterModalOnly();
                            setTimeout(() => {
                                openLoginModal();
                            }, 300);
                        }, 2000);
                    }

                } else {
                    console.log('Registration failed:', result.message);
                    showRegisterError(result.message || 'An error occurred during registration.');
                }
            } catch (error) {
                console.error('Register error:', error);
                showRegisterError('An unexpected error occurred. Please try again.');
            } finally {
                // Reset button state
                if (registerSubmitBtn) {
                    registerSubmitBtn.disabled = false;
                    registerSubmitBtn.innerHTML = 'Start Your Journey';
                }
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
        showError: showLoginError,
        showSuccess: showLoginSuccess
    };

    window.RegisterModal = {
        open: openRegisterModal,
        close: closeAllModals,
        showError: showRegisterError,
        showSuccess: showRegisterSuccess
    };

    // Prevent form submission on disabled buttons
    document.addEventListener('click', function (e) {
        if (e.target.matches('.login-btn[disabled], .register-btn[disabled]')) {
            e.preventDefault();
            return false;
        }
    });

    // Initialize modal handling when DOM is ready
    function initializeModals() {
        console.log('Initializing modals...');
        handleServerData();

        // Clean up URL parameters after processing
        if (window.history && window.history.replaceState) {
            const url = new URL(window.location);
            const params = ['error', 'showLogin', 'showRegister'];
            let hasParams = false;

            params.forEach(param => {
                if (url.searchParams.has(param)) {
                    url.searchParams.delete(param);
                    hasParams = true;
                }
            });

            if (hasParams) {
                window.history.replaceState({}, document.title, url.pathname);
            }
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeModals);
    } else {
        setTimeout(initializeModals, 100);
    }

    // Debug: Log when DOM is ready
    console.log('Modal script loaded. Elements found:');
    console.log('Login modal:', !!loginModal);
    console.log('Register modal:', !!registerModal);
    console.log('Login form:', !!loginForm);
    console.log('Register form:', !!registerForm);

})();

// Alert system for creating an account
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