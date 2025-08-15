function initializeSettings() {
    initTabSwitching();
    initToggleButtons();
    initFormValidation();
    initConnectedAccounts();
    initProfilePictureUpload();
    initAccountDeletion();
    initKeyboardShortcuts();
    initPasswordStrength();
    initNotificationSwitches();
    initSecurityFeatures();
    initLogoutFunctionality();
    initExportData();
    initDeactivateAccount();
    initAccessibilityFeatures();
    initProfileFeatures();
    initSidebarIntegration();
    initResponsiveLayout();

    console.log('Settings page initialized successfully');
}

/**
 * Initialize sidebar integration for responsive behavior
 */
function initSidebarIntegration() {
    const sidebar = document.getElementById('sidebar');
    const settingsContainer = document.getElementById('settings-container');

    if (!sidebar) {
        console.warn('Sidebar not found - settings will use default layout');
        return;
    }

    // Watch for sidebar toggle changes using MutationObserver
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.attributeName === 'class') {
                updateSettingsLayoutForSidebar();
            }
        });
    });

    observer.observe(sidebar, {
        attributes: true,
        attributeFilter: ['class']
    });

    // Also listen for the toggle function if it exists
    if (window.toggleSidebar) {
        const originalToggle = window.toggleSidebar;
        window.toggleSidebar = function () {
            originalToggle.apply(this, arguments);
            // Small delay to ensure DOM is updated
            setTimeout(updateSettingsLayoutForSidebar, 100);
        };
    }

    // Listen for custom sidebar events if they exist
    document.addEventListener('sidebarToggled', updateSettingsLayoutForSidebar);

    // Initial layout update
    setTimeout(updateSettingsLayoutForSidebar, 100);
}

/**
 * Update settings layout based on sidebar state
 */
function updateSettingsLayoutForSidebar() {
    const sidebar = document.getElementById('sidebar');
    const settingsContainer = document.querySelector('.settings-container');

    if (!sidebar || !settingsContainer) return;

    const isSidebarClosed = sidebar.classList.contains('close');
    const isSmallScreen = window.innerWidth <= 768;

    // Add classes to body for CSS targeting
    document.body.classList.toggle('sidebar-closed', isSidebarClosed);
    document.body.classList.toggle('sidebar-open', !isSidebarClosed);

    if (isSmallScreen) {
        // On small screens, use full width regardless of sidebar state
        settingsContainer.style.marginLeft = '0';
        settingsContainer.style.width = '100%';
        settingsContainer.style.paddingLeft = '15px';
        settingsContainer.style.paddingRight = '15px';
    } else {
        // On larger screens, adjust based on sidebar state
        if (isSidebarClosed) {
            // Sidebar is collapsed (82px) - more space available
            settingsContainer.style.marginLeft = '82px';
            settingsContainer.style.width = 'calc(100% - 82px)';
            settingsContainer.style.paddingLeft = '20px';
            settingsContainer.style.paddingRight = '20px';
        } else {
            // Sidebar is expanded (250px) - less space available
            settingsContainer.style.marginLeft = '250px';
            settingsContainer.style.width = 'calc(100% - 250px)';
            settingsContainer.style.paddingLeft = '20px';
            settingsContainer.style.paddingRight = '20px';
        }
    }

    // Trigger a custom event for other components that might need to respond
    const event = new CustomEvent('settingsLayoutUpdated', {
        detail: {
            sidebarClosed: isSidebarClosed,
            isSmallScreen: isSmallScreen
        }
    });
    document.dispatchEvent(event);
}

/**
 * Initialize responsive layout handling
 */
function initResponsiveLayout() {
    let resizeTimeout;

    function handleResize() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            updateSettingsLayoutForSidebar();
            handleResponsiveChanges();
        }, 150);
    }

    window.addEventListener('resize', handleResize);

    // Initial call
    handleResponsiveChanges();
}

/**
 * Handle responsive design changes
 */
function handleResponsiveChanges() {
    const settingsLayout = document.querySelector('.settings-layout');
    const settingsContainer = document.querySelector('.settings-container');
    const isSmallScreen = window.innerWidth <= 768;
    const isMediumScreen = window.innerWidth <= 1024;

    // Update body classes for responsive styling
    document.body.classList.toggle('small-screen', isSmallScreen);
    document.body.classList.toggle('medium-screen', isMediumScreen && !isSmallScreen);
    document.body.classList.toggle('large-screen', !isMediumScreen);

    if (isSmallScreen) {
        // Mobile layout adjustments
        if (settingsLayout) {
            settingsLayout.style.gridTemplateColumns = '1fr';
            settingsLayout.style.gap = '15px';
        }

        // Collapse toggle groups on mobile
        const toggleGroups = document.querySelectorAll('.toggle-group');
        toggleGroups.forEach(group => {
            group.style.flexDirection = 'column';
            group.style.alignItems = 'flex-start';
            group.style.gap = '15px';
        });

        // Make buttons full width on mobile
        const actionButtons = document.querySelector('.action-buttons');
        if (actionButtons) {
            actionButtons.style.flexDirection = 'column';
            const buttons = actionButtons.querySelectorAll('button, a');
            buttons.forEach(btn => {
                btn.style.width = '100%';
                btn.style.justifyContent = 'center';
            });
        }

    } else if (isMediumScreen) {
        // Tablet layout adjustments
        if (settingsLayout) {
            settingsLayout.style.gridTemplateColumns = '250px 1fr';
            settingsLayout.style.gap = '15px';
        }

        // Reset toggle groups for tablet
        const toggleGroups = document.querySelectorAll('.toggle-group');
        toggleGroups.forEach(group => {
            group.style.flexDirection = 'row';
            group.style.alignItems = 'center';
            group.style.gap = '20px';
        });

    } else {
        // Desktop layout
        if (settingsLayout) {
            settingsLayout.style.gridTemplateColumns = '300px 1fr';
            settingsLayout.style.gap = '20px';
        }

        // Reset all responsive overrides for desktop
        const toggleGroups = document.querySelectorAll('.toggle-group');
        toggleGroups.forEach(group => {
            group.style.flexDirection = 'row';
            group.style.alignItems = 'center';
            group.style.gap = '20px';
        });

        const actionButtons = document.querySelector('.action-buttons');
        if (actionButtons) {
            actionButtons.style.flexDirection = 'row';
            const buttons = actionButtons.querySelectorAll('button, a');
            buttons.forEach(btn => {
                btn.style.width = 'auto';
                btn.style.justifyContent = 'flex-start';
            });
        }
    }
}

/**
 * FIXED: Initialize tab switching functionality
 */
function initTabSwitching() {
    const tabLinks = document.querySelectorAll('.tab-link');
    const tabContents = document.querySelectorAll('.tab-content');
    const currentTabName = document.getElementById('current-tab-name');

    console.log('Tab links found:', tabLinks.length);
    console.log('Tab contents found:', tabContents.length);

    if (!tabLinks.length || !tabContents.length) {
        console.warn('Tab elements not found');
        return;
    }

    const tabNames = {
        'profile': 'Profile & Account',
        'privacy': 'Privacy & Visibility',
        'notifications': 'Notifications',
        'security': 'Security',
        'activity': 'Token & Activity',
        'language': 'Language & Accessibility',
        'support': 'Support & Legal'
    };

    // Function to show specific tab
    function showTab(targetTab) {
        console.log('Showing tab:', targetTab);

        // Hide all tab contents first
        tabContents.forEach(content => {
            content.classList.remove('active');
            content.style.display = 'none';
            content.style.visibility = 'hidden';
            content.style.opacity = '0';
        });

        // Remove active class from all links
        tabLinks.forEach(link => {
            link.classList.remove('active');
        });

        // Show the target tab content
        const targetContent = document.getElementById(targetTab);
        if (targetContent) {
            // Force display the target content
            targetContent.style.display = 'block';
            targetContent.style.visibility = 'visible';
            targetContent.style.opacity = '1';
            targetContent.classList.add('active');

            console.log('Tab content displayed:', targetTab);
            console.log('Target content styles:', {
                display: targetContent.style.display,
                visibility: targetContent.style.visibility,
                opacity: targetContent.style.opacity
            });
        } else {
            console.error('Target content not found:', targetTab);
        }

        // Add active class to clicked link
        const activeLink = document.querySelector(`[data-tab="${targetTab}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }

        // Update header
        if (currentTabName && tabNames[targetTab]) {
            currentTabName.textContent = tabNames[targetTab];
        }

        // Store current tab
        try {
            sessionStorage.setItem('activeSettingsTab', targetTab);
        } catch (e) {
            console.warn('SessionStorage not available:', e);
        }
    }

    // Add click handlers to tab links
    tabLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const targetTab = this.getAttribute('data-tab');
            if (targetTab) {
                showTab(targetTab);
            }
        });
    });

    // Initialize with profile tab
    let initialTab = 'profile';

    try {
        const savedTab = sessionStorage.getItem('activeSettingsTab');
        if (savedTab && document.getElementById(savedTab)) {
            initialTab = savedTab;
        }
    } catch (e) {
        console.warn('SessionStorage not available:', e);
    }

    // Force show the initial tab immediately
    setTimeout(() => {
        showTab(initialTab);

        // Double-check that profile tab is visible
        const profileTab = document.getElementById('profile');
        if (profileTab && initialTab === 'profile') {
            profileTab.style.display = 'block !important';
            profileTab.style.visibility = 'visible !important';
            profileTab.style.opacity = '1 !important';
        }
    }, 100);
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing settings...');

    // Initialize settings with a slight delay to ensure all elements are rendered
    setTimeout(() => {
        initializeSettings();

        // Force profile tab to be visible as a fallback
        const profileTab = document.getElementById('profile');
        const profileLink = document.querySelector('[data-tab="profile"]');

        if (profileTab) {
            profileTab.classList.add('active');
            profileTab.style.display = 'block';
            profileTab.style.visibility = 'visible';
            profileTab.style.opacity = '1';
            console.log('Profile tab forced to show');
        }

        if (profileLink) {
            profileLink.classList.add('active');
        }

        // Debug information
        console.log('Profile tab element:', profileTab);
        console.log('Profile tab classes:', profileTab ? profileTab.className : 'not found');
        console.log('Profile tab computed styles:', profileTab ? {
            display: getComputedStyle(profileTab).display,
            visibility: getComputedStyle(profileTab).visibility,
            opacity: getComputedStyle(profileTab).opacity
        } : 'not found');
    }, 200);
});

// Additional debugging function - call this from browser console if needed
window.debugTabs = function () {
    console.log('=== Tab Debug Info ===');
    console.log('Tab links:', document.querySelectorAll('.tab-link'));
    console.log('Tab contents:', document.querySelectorAll('.tab-content'));
    console.log('Active tab link:', document.querySelector('.tab-link.active'));
    console.log('Active tab content:', document.querySelector('.tab-content.active'));
    console.log('Profile tab:', document.getElementById('profile'));

    const profileTab = document.getElementById('profile');
    if (profileTab) {
        console.log('Profile tab styles:', {
            display: getComputedStyle(profileTab).display,
            visibility: getComputedStyle(profileTab).visibility,
            opacity: getComputedStyle(profileTab).opacity,
            height: getComputedStyle(profileTab).height
        });
    }
};

/**
 * Initialize toggle button functionality
 */
function initToggleButtons() {
    const toggleGroups = document.querySelectorAll('.toggle-buttons');

    if (!toggleGroups.length) {
        console.warn('Toggle groups not found');
        return;
    }

    toggleGroups.forEach(group => {
        const buttons = group.querySelectorAll('.toggle-btn');
        const hiddenInput = group.querySelector('input[type="hidden"]');

        buttons.forEach(btn => {
            btn.addEventListener('click', function () {
                // Remove active class from siblings
                buttons.forEach(b => b.classList.remove('active'));

                // Add active class to clicked button
                this.classList.add('active');

                // Store the value for form submission
                const value = this.getAttribute('data-value');
                if (hiddenInput && value) {
                    hiddenInput.value = value;
                }

                // Trigger custom event for other components to listen to
                const event = new CustomEvent('toggleChanged', {
                    detail: {
                        group: group,
                        value: value,
                        button: this
                    }
                });
                document.dispatchEvent(event);
            });
        });
    });
}

/**
 * Initialize notification switches
 */
function initNotificationSwitches() {
    const switches = document.querySelectorAll('.switch input[type="checkbox"]');

    switches.forEach(switchEl => {
        switchEl.addEventListener('change', function () {
            const setting = this.name;
            const enabled = this.checked;

            console.log(`Notification setting ${setting} changed to ${enabled}`);

            // Save setting (you can integrate with your backend here)
            try {
                localStorage.setItem(`notification_${setting}`, enabled.toString());
            } catch (e) {
                console.warn('Failed to save notification setting:', e);
            }
        });
    });
}

/**
 * Initialize security features
 */
function initSecurityFeatures() {
    // Enable 2FA button
    const enable2faBtn = document.getElementById('enable-2fa');
    if (enable2faBtn) {
        enable2faBtn.addEventListener('click', function () {
            showNotification('2FA setup process started. Check your email for instructions.', 'info');
            this.textContent = 'Setting up...';
            this.disabled = true;

            setTimeout(() => {
                this.textContent = 'Enabled';
                this.classList.remove('btn-secondary');
                this.classList.add('btn-save');
                showNotification('Two-factor authentication enabled successfully!', 'success');
            }, 2000);
        });
    }

    // End session buttons
    const endSessionBtns = document.querySelectorAll('.btn-danger-small');
    endSessionBtns.forEach(btn => {
        if (btn.textContent.includes('End Session')) {
            btn.addEventListener('click', function () {
                const confirmed = confirm('Are you sure you want to end this session?');
                if (confirmed) {
                    this.textContent = 'Ending...';
                    this.disabled = true;

                    setTimeout(() => {
                        this.closest('.session-item').remove();
                        showNotification('Session ended successfully', 'success');
                    }, 1000);
                }
            });
        }
    });

    // Password strength validation
    const newPasswordInput = document.getElementById('newPassword');
    const confirmPasswordInput = document.getElementById('confirmNewPassword');

    if (newPasswordInput && confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', function () {
            if (this.value && this.value !== newPasswordInput.value) {
                showFieldError(this, 'Passwords do not match');
            } else {
                clearFieldError(this);
            }
        });
    }
}

/**
 * Initialize logout functionality
 */
function initLogoutFunctionality() {
    const logoutBtn = document.getElementById('logout-btn');

    if (!logoutBtn) {
        console.warn('Logout button not found');
        return;
    }

    logoutBtn.addEventListener('click', function (e) {
        e.preventDefault();

        const confirmed = confirm('Are you sure you want to logout?');

        if (confirmed) {
            // Show loading state
            this.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Logging out...';
            this.disabled = true;

            // Clear any stored data
            try {
                sessionStorage.clear();
                localStorage.removeItem('activeSettingsTab');
            } catch (e) {
                console.warn('Failed to clear storage:', e);
            }

            // Simulate logout process
            setTimeout(() => {
                // Redirect to logout endpoint
                window.location.href = '/Account/Logout';
            }, 1500);
        }
    });
}

/**
 * Initialize export data functionality
 */
function initExportData() {
    const exportBtn = document.getElementById('export-data');

    if (!exportBtn) {
        console.warn('Export data button not found');
        return;
    }

    exportBtn.addEventListener('click', function () {
        this.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Preparing export...';
        this.disabled = true;

        // Simulate data export process
        setTimeout(() => {
            this.innerHTML = '<i class="bx bx-download"></i> Export My Data';
            this.disabled = false;

            // Create a mock download
            const data = {
                profile: { name: 'User Data', exported: new Date().toISOString() },
                settings: { privacy: 'public', notifications: true }
            };

            const blob = new Blob([JSON.stringify(data, null, 2)],
                { type: 'application/json' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'my-data-export.json';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            showNotification('Data exported successfully!', 'success');
        }, 2000);
    });
}

/**
 * Initialize deactivate account functionality
 */
function initDeactivateAccount() {
    const deactivateBtn = document.getElementById('deactivate-account');

    if (!deactivateBtn) {
        console.warn('Deactivate account button not found');
        return;
    }

    deactivateBtn.addEventListener('click', function () {
        const confirmed = confirm(
            'Are you sure you want to deactivate your account? ' +
            'You can reactivate it anytime by logging in again.'
        );

        if (confirmed) {
            this.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Deactivating...';
            this.disabled = true;

            setTimeout(() => {
                showNotification('Account deactivated. Redirecting to login...', 'info');
                setTimeout(() => {
                    window.location.href = '/Account/Login';
                }, 2000);
            }, 1500);
        }
    });
}

/**
 * Initialize accessibility features
 */
function initAccessibilityFeatures() {
    // High contrast toggle
    const highContrastToggle = document.querySelector('input[name="HighContrast"]');
    if (highContrastToggle) {
        highContrastToggle.addEventListener('change', function () {
            document.body.classList.toggle('high-contrast', this.checked);
            showNotification(`High contrast mode ${this.checked ? 'enabled' : 'disabled'}`, 'info');
        });
    }

    // Reduce motion toggle
    const reduceMotionToggle = document.querySelector('input[name="ReduceMotion"]');
    if (reduceMotionToggle) {
        reduceMotionToggle.addEventListener('change', function () {
            document.body.classList.toggle('reduce-motion', this.checked);
            showNotification(`Motion reduction ${this.checked ? 'enabled' : 'disabled'}`, 'info');
        });
    }

    // Font size selector
    const fontSizeSelect = document.getElementById('fontSize');
    if (fontSizeSelect) {
        fontSizeSelect.addEventListener('change', function () {
            document.body.className = document.body.className.replace(/font-size-\w+/g, '');
            document.body.classList.add(`font-size-${this.value}`);
            showNotification(`Font size changed to ${this.value}`, 'info');
        });
    }
}

/**
 * Initialize form validation
 */
function initFormValidation() {
    const forms = document.querySelectorAll('form');

    if (!forms.length) {
        console.warn('Forms not found');
        return;
    }

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!validateForm(this)) {
                e.preventDefault();
                return false;
            }
        });

        // Real-time validation
        const inputs = form.querySelectorAll('input[required], input[type="email"]');
        inputs.forEach(input => {
            input.addEventListener('blur', function () {
                validateField(this);
            });

            input.addEventListener('input', function () {
                clearFieldError(this);
            });
        });
    });
}

/**
 * Validate entire form
 */
function validateForm(form) {
    const inputs = form.querySelectorAll('input[required], input[type="email"]');
    let isValid = true;

    inputs.forEach(input => {
        if (!validateField(input)) {
            isValid = false;
        }
    });

    return isValid;
}

/**
 * Validate individual field
 */
function validateField(field) {
    const value = field.value.trim();
    const fieldType = field.type;
    const isRequired = field.hasAttribute('required');

    // Clear previous errors
    clearFieldError(field);

    // Required field validation
    if (isRequired && !value) {
        showFieldError(field, 'This field is required');
        return false;
    }

    // Email validation
    if (fieldType === 'email' && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
            showFieldError(field, 'Please enter a valid email address');
            return false;
        }
    }

    // Phone validation
    if (field.type === 'tel' && value) {
        const phoneRegex = /^[\d\-\+\(\)\s]+$/;
        if (!phoneRegex.test(value)) {
            showFieldError(field, 'Please enter a valid phone number');
            return false;
        }
    }

    // Password strength validation
    if (field.id === 'newPassword' && value) {
        const strength = calculatePasswordStrength(value);
        if (strength.score < 3) {
            showFieldError(field, 'Password is too weak. Please use a stronger password.');
            return false;
        }
    }

    return true;
}

/**
 * Show field error
 */
function showFieldError(field, message) {
    field.classList.add('error');

    // Remove existing error message
    const existingError = field.parentNode.querySelector('.error-message');
    if (existingError) {
        existingError.remove();
    }

    // Add new error message
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.textContent = message;
    field.parentNode.appendChild(errorDiv);
}

/**
 * Clear field error
 */
function clearFieldError(field) {
    field.classList.remove('error');

    const errorMessage = field.parentNode.querySelector('.error-message');
    if (errorMessage) {
        errorMessage.remove();
    }
}

/**
 * Initialize account deletion
 */
function initAccountDeletion() {
    const deleteBtn = document.getElementById('delete-account-btn');

    if (!deleteBtn) {
        console.warn('Delete account button not found');
        return;
    }

    deleteBtn.addEventListener('click', function (e) {
        e.preventDefault();

        const confirmed = confirm(
            'Are you sure you want to delete your account? ' +
            'This action cannot be undone and all your data will be permanently removed.'
        );

        if (confirmed) {
            const doubleConfirm = confirm(
                'This is your final warning. Are you absolutely sure you want to delete your account? ' +
                'Type "DELETE" in the next prompt to confirm.'
            );

            if (doubleConfirm) {
                const deleteConfirmation = prompt('Please type "DELETE" to confirm account deletion:');

                if (deleteConfirmation === 'DELETE') {
                    // Show loading state
                    this.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Deleting...';
                    this.disabled = true;

                    // Submit delete request (replace with actual endpoint)
                    setTimeout(() => {
                        showNotification('Account deletion initiated. You will be logged out shortly.', 'info');
                        setTimeout(() => {
                            window.location.href = '/Account/DeleteAccount';
                        }, 2000);
                    }, 1500);
                } else {
                    showNotification('Account deletion cancelled - confirmation text did not match.', 'warning');
                }
            }
        }
    });
}

/**
 * Initialize connected accounts
 */
function initConnectedAccounts() {
    const connectButtons = document.querySelectorAll('.btn-connect');

    connectButtons.forEach(button => {
        button.addEventListener('click', function () {
            const accountType = this.getAttribute('data-account');
            const accountItem = this.closest('.account-item');
            const accountName = accountItem.querySelector('.account-info span').textContent;

            if (this.classList.contains('connected')) {
                // Disconnect account
                const confirmDisconnect = confirm(`Are you sure you want to disconnect ${accountName}?`);
                if (confirmDisconnect) {
                    this.innerHTML = '<i class="bx bx-plus"></i>';
                    this.classList.remove('connected');
                    showNotification(`${accountName} disconnected`, 'info');
                }
                return;
            }

            // Show loading state
            this.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i>';
            this.disabled = true;

            // Simulate connection process
            setTimeout(() => {
                this.innerHTML = '<i class="bx bx-check"></i>';
                this.classList.add('connected');
                this.disabled = false;
                showNotification(`${accountName} connected successfully!`, 'success');
            }, 1500);
        });
    });
}

/**
 * Show notification message
 */
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotification = document.querySelector('.notification');
    if (existingNotification) {
        existingNotification.remove();
    }

    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    notification.style.animation = 'slideIn 0.3s ease-out';

    // Add to document
    document.body.appendChild(notification);

    // Auto remove after 4 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease-in';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

/**
 * Initialize keyboard shortcuts
 */
function initKeyboardShortcuts() {
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + S to save
        if ((e.ctrlKey || e.metaKey) && e.key === 's') {
            e.preventDefault();

            const activeForm = document.querySelector('.tab-content.active form');
            if (activeForm) {
                const saveButton = activeForm.querySelector('.btn-save');
                if (saveButton) {
                    saveButton.click();
                    showNotification('Settings saved!', 'success');
                }
            }
        }

        // Escape to close notifications
        if (e.key === 'Escape') {
            const notification = document.querySelector('.notification');
            if (notification) {
                notification.remove();
            }
        }

        // Arrow keys for tab navigation (Alt + arrows)
        if (e.altKey) {
            const tabLinks = Array.from(document.querySelectorAll('.tab-link'));
            const activeIndex = tabLinks.findIndex(link => link.classList.contains('active'));

            if (e.key === 'ArrowDown' && activeIndex < tabLinks.length - 1) {
                e.preventDefault();
                tabLinks[activeIndex + 1].click();
            } else if (e.key === 'ArrowUp' && activeIndex > 0) {
                e.preventDefault();
                tabLinks[activeIndex - 1].click();
            }
        }
    });
}

/**
 * Initialize password strength indicator
 */
function initPasswordStrength() {
    const passwordInput = document.getElementById('newPassword');
    if (!passwordInput) return;

    // Create strength indicator
    const strengthIndicator = document.createElement('div');
    strengthIndicator.className = 'password-strength';
    strengthIndicator.innerHTML = `
        <div class="strength-bar">
            <div class="strength-fill"></div>
        </div>
        <div class="strength-text">Password strength: <span></span></div>
    `;

    passwordInput.parentNode.appendChild(strengthIndicator);

    passwordInput.addEventListener('input', function () {
        const strength = calculatePasswordStrength(this.value);
        updatePasswordStrengthIndicator(strengthIndicator, strength);
    });
}

/**
 * Calculate password strength
 */
function calculatePasswordStrength(password) {
    let score = 0;
    let feedback = [];

    if (password.length >= 8) score += 1;
    else feedback.push('at least 8 characters');

    if (/[a-z]/.test(password)) score += 1;
    else feedback.push('lowercase letters');

    if (/[A-Z]/.test(password)) score += 1;
    else feedback.push('uppercase letters');

    if (/\d/.test(password)) score += 1;
    else feedback.push('numbers');

    if (/[^a-zA-Z\d]/.test(password)) score += 1;
    else feedback.push('special characters');

    const levels = ['Very Weak', 'Weak', 'Fair', 'Good', 'Strong'];
    const colors = ['#ef4444', '#f59e0b', '#eab308', '#22c55e', '#10b981'];

    return {
        score: score,
        level: levels[score] || 'Very Weak',
        color: colors[score] || '#ef4444',
        feedback: feedback
    };
}

/**
 * Update password strength indicator
 */
function updatePasswordStrengthIndicator(indicator, strength) {
    const fill = indicator.querySelector('.strength-fill');
    const text = indicator.querySelector('.strength-text span');

    if (fill && text) {
        fill.style.width = `${(strength.score / 5) * 100}%`;
        fill.style.backgroundColor = strength.color;
        text.textContent = strength.level;
        text.style.color = strength.color;
    }
}

/**
 * Handle form auto-save functionality
 */
function initAutoSave() {
    const forms = document.querySelectorAll('form');

    forms.forEach(form => {
        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            input.addEventListener('change', function () {
                // Only auto-save if localStorage is available
                if (typeof Storage !== "undefined") {
                    const formData = new FormData(form);
                    const data = Object.fromEntries(formData.entries());

                    try {
                        localStorage.setItem(`settings_draft_${form.id || 'default'}`, JSON.stringify(data));
                        showAutoSaveIndicator();
                    } catch (e) {
                        console.warn('Auto-save failed:', e);
                    }
                }
            });
        });
    });
}

/**
 * Show auto-save indicator
 */
function showAutoSaveIndicator() {
    let indicator = document.querySelector('.auto-save-indicator');

    if (!indicator) {
        indicator = document.createElement('div');
        indicator.className = 'auto-save-indicator';
        indicator.textContent = 'Draft saved';
        indicator.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            background-color: #374151;
            color: white;
            padding: 8px 16px;
            border-radius: 4px;
            font-size: 0.875rem;
            z-index: 999;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        document.body.appendChild(indicator);
    }

    // Show indicator
    indicator.style.opacity = '1';

    // Hide after 2 seconds
    setTimeout(() => {
        indicator.style.opacity = '0';
    }, 2000);
}

/**
 * Monitor sidebar changes and update layout accordingly
 */
function monitorSidebarChanges() {
    // Check if toggleSidebar function exists and override it
    if (typeof window.toggleSidebar === 'function') {
        const originalToggleSidebar = window.toggleSidebar;

        window.toggleSidebar = function () {
            // Call the original function
            originalToggleSidebar.apply(this, arguments);

            // Update our layout after a short delay
            setTimeout(() => {
                updateSettingsLayoutForSidebar();
            }, 100);
        };
    }

    // Also listen for any sidebar toggle button clicks
    const toggleButton = document.getElementById('icon-toggle');
    if (toggleButton) {
        toggleButton.addEventListener('click', function () {
            setTimeout(() => {
                updateSettingsLayoutForSidebar();
            }, 100);
        });
    }
}

/**
 * Initialize auto-save if Storage is available
 */
if (typeof Storage !== "undefined") {
    document.addEventListener('DOMContentLoaded', function () {
        setTimeout(initAutoSave, 1000); // Delay to ensure DOM is ready
    });
}

/**
 * Initialize sidebar monitoring
 */
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(monitorSidebarChanges, 500); // Delay to ensure sidebar functions are loaded
});

/**
 * Utility function to safely get elements
 */
function safeQuerySelector(selector) {
    try {
        return document.querySelector(selector);
    } catch (e) {
        console.warn('Invalid selector:', selector);
        return null;
    }
}

/**
 * Debounce function for performance optimization
 */
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

/**
 * Optimized resize handler
 */
const optimizedResize = debounce(function () {
    updateSettingsLayoutForSidebar();
    handleResponsiveChanges();
}, 250);

// Listen for window resize with debounced handler
window.addEventListener('resize', optimizedResize);

/**
 * Handle orientation change on mobile devices
 */
window.addEventListener('orientationchange', function () {
    setTimeout(() => {
        updateSettingsLayoutForSidebar();
        handleResponsiveChanges();
    }, 300);
});

/**
 * Export functions for potential external use
 */
window.SettingsPage = {
    showNotification: showNotification,
    validateForm: validateForm,
    initializeSettings: initializeSettings,
    updateSettingsLayoutForSidebar: updateSettingsLayoutForSidebar,
    handleResponsiveChanges: handleResponsiveChanges
};

/**
 * Initialize everything when DOM is ready
 */
document.addEventListener('DOMContentLoaded', function () {
    // Add a small delay to ensure all external scripts are loaded
    setTimeout(() => {
        console.log('Settings page fully initialized with sidebar responsiveness');
    }, 1000);
});

/**
 * Handle page visibility changes
 */
document.addEventListener('visibilitychange', function () {
    if (!document.hidden) {
        // Page became visible again, refresh layout
        setTimeout(updateSettingsLayoutForSidebar, 100);
    }
});

/**
 * Handle browser back/forward navigation
 */
window.addEventListener('popstate', function () {
    setTimeout(updateSettingsLayoutForSidebar, 100);
});

/**
 * CSS styles for password strength indicator (if not in CSS file)
 */
const addPasswordStrengthStyles = () => {
    if (!document.querySelector('#password-strength-styles')) {
        const style = document.createElement('style');
        style.id = 'password-strength-styles';
        style.textContent = `
            .password-strength {
                margin-top: 8px;
            }
            .strength-bar {
                height: 4px;
                background-color: #e5e7eb;
                border-radius: 2px;
                overflow: hidden;
                margin-bottom: 4px;
            }
            .strength-fill {
                height: 100%;
                transition: width 0.3s ease, background-color 0.3s ease;
                width: 0%;
            }
            .strength-text {
                font-size: 0.75rem;
                color: #6b7280;
            }
        `;
        document.head.appendChild(style);
    }
};

// Add password strength styles when DOM is ready
document.addEventListener('DOMContentLoaded', addPasswordStrengthStyles);

//Profile Tab

// Profile Tab JavaScript with Phone Verification
// Add this to your existing settings.js file

/**
 * Initialize profile-specific functionality
 */
function initProfileFeatures() {
    initPhoneVerification();
    initProfilePictureUpload();
    initPasswordToggle();
    initConnectedAccounts();

    console.log('Profile features initialized');
}

/**
 * Initialize phone verification functionality
 */
function initPhoneVerification() {
    const verifyPhoneBtn = document.getElementById('verify-phone-btn');
    const modal = document.getElementById('phone-verification-modal');
    const overlay = document.getElementById('modal-overlay');
    const closeModalBtn = document.getElementById('close-verification-modal');
    const sendCodeBtn = document.getElementById('send-verification-code');
    const verifyCodeBtn = document.getElementById('verify-code-btn');
    const resendCodeBtn = document.getElementById('resend-code-btn');
    const completeBtn = document.getElementById('complete-verification');

    if (!verifyPhoneBtn) return;

    // Open verification modal
    verifyPhoneBtn.addEventListener('click', function () {
        const phoneInput = document.getElementById('phone');
        const verificationPhoneInput = document.getElementById('verification-phone');

        if (phoneInput.value) {
            verificationPhoneInput.value = phoneInput.value;
        }

        showVerificationModal();
    });

    // Close modal
    const closeModal = () => {
        hideVerificationModal();
        resetVerificationSteps();
    };

    closeModalBtn?.addEventListener('click', closeModal);
    overlay?.addEventListener('click', closeModal);

    // Send verification code
    sendCodeBtn?.addEventListener('click', function () {
        const countryCode = document.getElementById('country-code').value;
        const phoneNumber = document.getElementById('verification-phone').value.trim();

        if (!phoneNumber) {
            showNotification('Please enter a valid phone number', 'error');
            return;
        }

        const fullPhoneNumber = countryCode + phoneNumber;

        // Show loading state
        this.textContent = 'Sending...';
        this.disabled = true;

        // Simulate API call to send SMS
        setTimeout(() => {
            document.getElementById('phone-display').textContent = fullPhoneNumber;
            showVerificationStep('step-code');
            showNotification('Verification code sent successfully!', 'success');
            startResendTimer();

            this.textContent = 'Send Verification Code';
            this.disabled = false;
        }, 2000);
    });

    // Initialize code inputs
    initCodeInputs();

    // Verify code
    verifyCodeBtn?.addEventListener('click', function () {
        const code = getEnteredCode();

        if (code.length !== 6) {
            showNotification('Please enter the complete 6-digit code', 'error');
            return;
        }

        // Show loading state
        this.textContent = 'Verifying...';
        this.disabled = true;

        // Simulate API call to verify code
        setTimeout(() => {
            if (code === '123456') { // Mock verification
                showVerificationStep('step-success');
                showNotification('Phone number verified successfully!', 'success');
                updateVerificationStatus();
            } else {
                showNotification('Invalid verification code. Please try again.', 'error');
                clearCodeInputs();
            }

            this.textContent = 'Verify Code';
            this.disabled = false;
        }, 1500);
    });

    // Resend code
    resendCodeBtn?.addEventListener('click', function () {
        if (this.disabled) return;

        this.textContent = 'Resending...';
        this.disabled = true;

        setTimeout(() => {
            showNotification('Verification code resent!', 'info');
            startResendTimer();
        }, 1000);
    });

    // Complete verification
    completeBtn?.addEventListener('click', function () {
        hideVerificationModal();
        resetVerificationSteps();
        showNotification('Your account is now verified!', 'success');
    });
}

/**
 * Show verification modal
 */
function showVerificationModal() {
    const modal = document.getElementById('phone-verification-modal');
    const overlay = document.getElementById('modal-overlay');

    if (modal && overlay) {
        overlay.classList.add('active');
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
}

/**
 * Hide verification modal
 */
function hideVerificationModal() {
    const modal = document.getElementById('phone-verification-modal');
    const overlay = document.getElementById('modal-overlay');

    if (modal && overlay) {
        overlay.classList.remove('active');
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

/**
 * Show specific verification step
 */
function showVerificationStep(stepId) {
    const steps = document.querySelectorAll('.verification-step');
    steps.forEach(step => step.classList.remove('active'));

    const targetStep = document.getElementById(stepId);
    if (targetStep) {
        targetStep.classList.add('active');
    }
}

/**
 * Reset verification steps to initial state
 */
function resetVerificationSteps() {
    showVerificationStep('step-phone');
    clearCodeInputs();

    const resendBtn = document.getElementById('resend-code-btn');
    if (resendBtn) {
        resendBtn.disabled = false;
        resendBtn.textContent = 'Resend Code';
    }
}

/**
 * Initialize code input functionality
 */
function initCodeInputs() {
    const codeInputs = document.querySelectorAll('.code-input');

    codeInputs.forEach((input, index) => {
        input.addEventListener('input', function (e) {
            const value = e.target.value;

            // Only allow digits
            if (!/^\d$/.test(value)) {
                e.target.value = '';
                return;
            }

            // Add filled class
            e.target.classList.add('filled');

            // Move to next input
            if (value && index < codeInputs.length - 1) {
                codeInputs[index + 1].focus();
            }

            // Auto-verify when all digits are entered
            if (index === codeInputs.length - 1 && value) {
                setTimeout(() => {
                    const verifyBtn = document.getElementById('verify-code-btn');
                    if (verifyBtn && !verifyBtn.disabled) {
                        verifyBtn.click();
                    }
                }, 500);
            }
        });

        input.addEventListener('keydown', function (e) {
            // Handle backspace
            if (e.key === 'Backspace' && !e.target.value && index > 0) {
                codeInputs[index - 1].focus();
                codeInputs[index - 1].value = '';
                codeInputs[index - 1].classList.remove('filled');
            }

            // Handle paste
            if (e.key === 'v' && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                navigator.clipboard.readText().then(text => {
                    const digits = text.replace(/\D/g, '').slice(0, 6);
                    fillCodeInputs(digits);
                });
            }
        });

        input.addEventListener('focus', function () {
            this.select();
        });
    });
}

/**
 * Fill code inputs with digits
 */
function fillCodeInputs(digits) {
    const codeInputs = document.querySelectorAll('.code-input');

    codeInputs.forEach((input, index) => {
        if (digits[index]) {
            input.value = digits[index];
            input.classList.add('filled');
        } else {
            input.value = '';
            input.classList.remove('filled');
        }
    });

    // Focus on next empty input or last input
    const nextEmptyIndex = digits.length < 6 ? digits.length : 5;
    codeInputs[nextEmptyIndex]?.focus();
}

/**
 * Get entered verification code
 */
function getEnteredCode() {
    const codeInputs = document.querySelectorAll('.code-input');
    return Array.from(codeInputs).map(input => input.value).join('');
}

/**
 * Clear all code inputs
 */
function clearCodeInputs() {
    const codeInputs = document.querySelectorAll('.code-input');
    codeInputs.forEach(input => {
        input.value = '';
        input.classList.remove('filled');
    });
    codeInputs[0]?.focus();
}

/**
 * Start resend timer
 */
function startResendTimer() {
    const resendBtn = document.getElementById('resend-code-btn');
    const timerSpan = document.getElementById('resend-timer');

    if (!resendBtn || !timerSpan) return;

    let timeLeft = 60;
    resendBtn.disabled = true;

    const timer = setInterval(() => {
        timerSpan.textContent = `(${timeLeft}s)`;
        timeLeft--;

        if (timeLeft < 0) {
            clearInterval(timer);
            timerSpan.textContent = '';
            resendBtn.disabled = false;
            resendBtn.textContent = 'Resend Code';
        }
    }, 1000);
}

/**
 * Update verification status in UI
 */
function updateVerificationStatus() {
    const verifyBtn = document.getElementById('verify-phone-btn');
    const verificationBadge = document.getElementById('verification-badge');

    if (verifyBtn) {
        verifyBtn.textContent = 'Verified ?';
        verifyBtn.style.background = 'linear-gradient(135deg, #10b981, #059669)';
        verifyBtn.disabled = true;
    }

    if (verificationBadge) {
        verificationBadge.style.display = 'flex';
    }

    // Update phone input to show verified status
    const phoneInput = document.getElementById('phone');
    if (phoneInput) {
        phoneInput.style.borderColor = '#10b981';
        phoneInput.style.backgroundColor = '#f0fdf4';
    }
}

/**
 * Initialize password toggle functionality
 */
function initPasswordToggle() {
    const toggleBtn = document.getElementById('toggle-password');
    const passwordInput = document.getElementById('password');

    if (!toggleBtn || !passwordInput) return;

    toggleBtn.addEventListener('click', function () {
        const isPassword = passwordInput.type === 'password';

        passwordInput.type = isPassword ? 'text' : 'password';
        this.innerHTML = isPassword ? '<i class="bx bx-show"></i>' : '<i class="bx bx-hide"></i>';
    });
}

/**
 * Initialize enhanced profile picture upload
 */
function initProfilePictureUpload() {
    const profileAvatar = document.getElementById('profile-avatar-upload');
    const editBtn = document.getElementById('edit-profile-btn');

    if (!profileAvatar) return;

    const handleUpload = () => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';

        input.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                // Validate file size (max 5MB)
                if (file.size > 5 * 1024 * 1024) {
                    showNotification('File size must be less than 5MB', 'error');
                    return;
                }

                // Validate file type
                if (!file.type.startsWith('image/')) {
                    showNotification('Please select a valid image file', 'error');
                    return;
                }

                const reader = new FileReader();
                reader.onload = function (e) {
                    profileAvatar.style.backgroundImage = `url(${e.target.result})`;
                    profileAvatar.style.backgroundSize = 'cover';
                    profileAvatar.style.backgroundPosition = 'center';
                    profileAvatar.innerHTML = '';
                    showNotification('Profile picture updated successfully!', 'success');
                };
                reader.readAsDataURL(file);
            }
        });

        input.click();
    };

    profileAvatar.addEventListener('click', handleUpload);
    editBtn?.addEventListener('click', handleUpload);

    profileAvatar.style.cursor = 'pointer';
    profileAvatar.title = 'Click to upload profile picture';
}

/**
 * Handle escape key to close modal
 */
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const modal = document.getElementById('phone-verification-modal');
        if (modal && modal.classList.contains('active')) {
            hideVerificationModal();
            resetVerificationSteps();
        }
    }
});

/**
 * Prevent modal content click from closing modal
 */
document.addEventListener('DOMContentLoaded', function () {
    const modalContent = document.querySelector('.verification-modal-content');
    if (modalContent) {
        modalContent.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }
});

/**
 * Format phone number as user types
 */
function formatPhoneNumber(input) {
    let value = input.value.replace(/\D/g, '');
    let formattedValue = '';

    if (value.length > 0) {
        if (value.length <= 3) {
            formattedValue = value;
        } else if (value.length <= 6) {
            formattedValue = `${value.slice(0, 3)}-${value.slice(3)}`;
        } else {
            formattedValue = `${value.slice(0, 3)}-${value.slice(3, 6)}-${value.slice(6, 10)}`;
        }
    }

    input.value = formattedValue;
}

// Add phone number formatting to phone inputs
document.addEventListener('DOMContentLoaded', function () {
    const phoneInputs = document.querySelectorAll('input[type="tel"]');
    phoneInputs.forEach(input => {
        input.addEventListener('input', function () {
            formatPhoneNumber(this);
        });
    });
});

/**
 * Export profile functions for external use
 */
window.ProfileSettings = {
    showVerificationModal: showVerificationModal,
    hideVerificationModal: hideVerificationModal,
    updateVerificationStatus: updateVerificationStatus,
    initProfileFeatures: initProfileFeatures
};