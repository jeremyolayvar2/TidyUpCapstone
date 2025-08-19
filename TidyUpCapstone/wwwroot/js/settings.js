class ResponsiveSettingsManager {
    constructor() {
        this.currentTab = 'profile';
        this.initialized = false;
        this.isMobile = window.innerWidth <= 768;
        this.isTablet = window.innerWidth > 768 && window.innerWidth <= 1024;
        this.isDesktop = window.innerWidth > 768;
        this.sidebarOpen = false;
        this.tabNames = {
            'profile': 'Profile & Account',
            'privacy': 'Privacy & Visibility',
            'notifications': 'Notifications',
            'security': 'Security',
            'activity': 'Token & Activity',
            'language': 'Language & Accessibility',
            'support': 'Support & Legal'
        };

        this.init();
    }

    init() {
        console.log('🔄 Initializing Responsive Settings Manager...');

        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initializeComponents());
        } else {
            this.initializeComponents();
        }
    }

    initializeComponents() {
        // Force immediate fix for tab display
        this.forceTabDisplay();

        this.initResponsiveLayout();
        this.initMobileSidebar();
        this.initTabSwitching();
        this.initProfileFeatures();
        this.initFormHandling();
        this.initNotifications();
        this.initModalHandling();
        this.initAccessibility();
        this.initResizeHandler();

        this.initialized = true;
        console.log('✅ Responsive Settings Manager initialized successfully');
    }

    // ==========================================
    // RESPONSIVE LAYOUT MANAGEMENT
    // ==========================================

    initResponsiveLayout() {
        console.log('🔄 Initializing responsive layout...');

        this.updateLayoutState();
        this.adjustContainerLayout();
    }

    updateLayoutState() {
        const wasMobile = this.isMobile;
        // Changed breakpoint to 768px
        this.isMobile = window.innerWidth <= 768;
        this.isTablet = window.innerWidth > 768 && window.innerWidth <= 1024;
        this.isDesktop = window.innerWidth > 768;

        if (wasMobile !== this.isMobile) {
            console.log(`📱 Layout changed: ${this.isMobile ? 'Mobile' : 'Desktop'}`);
            this.handleLayoutChange();
        }
    }

    handleLayoutChange() {
        if (this.isMobile) {
            this.setupMobileLayout();
        } else {
            this.setupDesktopLayout();
        }
    }

    setupMobileLayout() {
        const sidebar = document.getElementById('settings-sidebar');
        const toggleBtn = document.getElementById('mobile-menu-toggle');

        if (sidebar) {
            sidebar.classList.add('mobile-responsive');
        }
        if (toggleBtn) {
            toggleBtn.style.display = 'flex';
        }
        this.closeMobileSidebar();
    }

    setupDesktopLayout() {
        const sidebar = document.getElementById('settings-sidebar');
        const toggleBtn = document.getElementById('mobile-menu-toggle');

        if (sidebar) {
            sidebar.classList.remove('mobile-responsive', 'mobile-open');
        }
        if (toggleBtn) {
            toggleBtn.style.display = 'none';
        }
        this.sidebarOpen = false;
    }

    adjustContainerLayout() {
        const container = document.querySelector('.settings-container');
        const sidebar = document.getElementById('sidebar');

        if (!container) return;

        if (this.isMobile) {
            // Mobile layout - full width with top padding
            container.style.marginLeft = '0';
            container.style.width = '100%';
        } else {
            // Desktop layout - adjust for main sidebar
            if (sidebar && sidebar.classList.contains('close')) {
                container.style.marginLeft = '82px';
                container.style.width = 'calc(100% - 82px)';
            } else {
                container.style.marginLeft = '250px';
                container.style.width = 'calc(100% - 250px)';
            }
        }
    }

    // Add this method to handle form layout
    adjustFormLayout() {
        const formRows = document.querySelectorAll('.form-row-top, .form-row-bottom');

        formRows.forEach(row => {
            if (window.innerWidth <= 480) {
                // Stack all fields on very small screens
                row.style.display = 'block';
            } else if (window.innerWidth <= 768) {
                // 1 column on mobile
                row.style.display = 'grid';
                row.style.gridTemplateColumns = '1fr';
            } else {
                // Original grid layout for desktop (769px+)
                row.style.display = 'grid';
                row.style.gridTemplateColumns = 'repeat(auto-fit, minmax(250px, 1fr))';
            }
        });
    }

    // Add this method:
    adjustProfileLayout() {
        const profileSection = document.querySelector('.profile-avatar-section');
        const accountDetails = document.querySelector('.account-details-section');

        if (window.innerWidth <= 768) {
            if (profileSection) {
                profileSection.style.flexDirection = 'column';
                profileSection.style.textAlign = 'center';
            }
            if (accountDetails) {
                accountDetails.style.display = 'block';
            }
        } else {
            // Reset to original layout for desktop
            if (profileSection) {
                profileSection.style.flexDirection = 'row';
                profileSection.style.textAlign = 'left';
            }
            if (accountDetails) {
                accountDetails.style.display = 'grid';
            }
        }
    }

    // ==========================================
    // MOBILE SIDEBAR FUNCTIONALITY
    // ==========================================

    initMobileSidebar() {
        console.log('🔄 Initializing mobile sidebar...');

        const toggleBtn = document.getElementById('mobile-menu-toggle');
        const closeBtn = document.getElementById('sidebar-close-btn');
        const overlay = document.getElementById('sidebar-overlay');
        const sidebar = document.getElementById('settings-sidebar');

        if (!toggleBtn || !sidebar) {
            console.warn('⚠️ Mobile sidebar elements not found');
            return;
        }

        // Toggle button click
        toggleBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.toggleMobileSidebar();
        });

        // Close button click
        if (closeBtn) {
            closeBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.closeMobileSidebar();
            });
        }

        // Overlay click
        if (overlay) {
            overlay.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.closeMobileSidebar();
            });
        }

        // Close sidebar when tab is selected on mobile
        const tabLinks = document.querySelectorAll('.tab-link');
        tabLinks.forEach(link => {
            link.addEventListener('click', () => {
                if (this.isMobile && this.sidebarOpen) {
                    setTimeout(() => this.closeMobileSidebar(), 100);
                }
            });
        });

        // Escape key to close sidebar
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMobile && this.sidebarOpen) {
                this.closeMobileSidebar();
            }
        });

        console.log('✅ Mobile sidebar initialized');
    }

    toggleMobileSidebar() {
        if (this.sidebarOpen) {
            this.closeMobileSidebar();
        } else {
            this.openMobileSidebar();
        }
    }

    openMobileSidebar() {
        console.log('📱 Opening mobile sidebar...');

        const sidebar = document.getElementById('settings-sidebar');
        const overlay = document.getElementById('sidebar-overlay');
        const toggleBtn = document.getElementById('mobile-menu-toggle');

        if (sidebar) {
            sidebar.classList.add('mobile-open');
        }

        if (overlay) {
            overlay.classList.add('active');
        }

        if (toggleBtn) {
            toggleBtn.innerHTML = '<i class="bx bx-x"></i>';
            toggleBtn.setAttribute('aria-label', 'Close settings menu');
        }

        document.body.classList.add('sidebar-mobile-open');
        this.sidebarOpen = true;

        // Focus first tab link for accessibility
        const firstTabLink = sidebar?.querySelector('.tab-link');
        if (firstTabLink) {
            setTimeout(() => firstTabLink.focus(), 100);
        }
    }

    closeMobileSidebar() {
        console.log('📱 Closing mobile sidebar...');

        const sidebar = document.getElementById('settings-sidebar');
        const overlay = document.getElementById('sidebar-overlay');
        const toggleBtn = document.getElementById('mobile-menu-toggle');

        if (sidebar) {
            sidebar.classList.remove('mobile-open');
        }

        if (overlay) {
            overlay.classList.remove('active');
        }

        if (toggleBtn) {
            toggleBtn.innerHTML = '<i class="bx bx-menu"></i>';
            toggleBtn.setAttribute('aria-label', 'Open settings menu');
        }

        document.body.classList.remove('sidebar-mobile-open');
        this.sidebarOpen = false;
    }

    // ==========================================
    // RESIZE HANDLER
    // ==========================================

    initResizeHandler() {
        let resizeTimer;

        window.addEventListener('resize', () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {
                this.handleResize();
            }, 100);
        });
    }

    handleResize() {
        console.log('📏 Handling window resize...');

        this.updateLayoutState();
        this.adjustContainerLayout();
        this.adjustFormLayout();
        this.adjustProfileLayout();

        // Close mobile sidebar if switching to desktop
        if (!this.isMobile && this.sidebarOpen) {
            this.closeMobileSidebar();
        }
    }

    // Force correct tab display immediately
    forceTabDisplay() {
        console.log('🔧 Forcing correct tab display...');

        // Hide all tabs first
        const allTabs = document.querySelectorAll('.tab-content');
        allTabs.forEach(tab => {
            tab.style.display = 'none';
            tab.style.visibility = 'hidden';
            tab.style.opacity = '0';
            tab.style.position = 'absolute';
            tab.style.left = '-9999px';
            tab.classList.remove('active');
        });

        // Show profile tab
        const profileTab = document.getElementById('profile');
        if (profileTab) {
            profileTab.style.display = 'block';
            profileTab.style.visibility = 'visible';
            profileTab.style.opacity = '1';
            profileTab.style.position = 'relative';
            profileTab.style.left = '0';
            profileTab.classList.add('active');
        }

        // Set profile link as active
        const profileLink = document.querySelector('[data-tab="profile"]');
        if (profileLink) {
            document.querySelectorAll('.tab-link').forEach(link => link.classList.remove('active'));
            profileLink.classList.add('active');
        }

        console.log('✅ Tab display forced successfully');
    }

    // ==========================================
    // TAB SWITCHING
    // ==========================================

    initTabSwitching() {
        console.log('🔄 Initializing tab switching...');

        const tabLinks = document.querySelectorAll('.tab-link');
        const currentTabName = document.getElementById('current-tab-name');

        if (!tabLinks.length) {
            console.warn('⚠️ Tab elements not found');
            return;
        }

        // Add click handlers
        tabLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetTab = link.getAttribute('data-tab');
                if (targetTab) {
                    this.showTab(targetTab);
                }
            });
        });

        // Show initial tab
        this.showTab('profile');

        // Export for global access
        window.showSettingsTab = (tab) => this.showTab(tab);
    }

    showTab(targetTab) {
        console.log(`🔄 Switching to tab: ${targetTab}`);

        // Hide all tab contents with force
        document.querySelectorAll('.tab-content').forEach(content => {
            content.classList.remove('active');
            content.style.display = 'none';
            content.style.visibility = 'hidden';
            content.style.opacity = '0';
            content.style.position = 'absolute';
            content.style.left = '-9999px';
        });

        // Remove active class from all links
        document.querySelectorAll('.tab-link').forEach(link => {
            link.classList.remove('active');
        });

        // Show target tab content with force
        const targetContent = document.getElementById(targetTab);
        if (targetContent) {
            targetContent.style.display = 'block';
            targetContent.style.visibility = 'visible';
            targetContent.style.opacity = '1';
            targetContent.style.position = 'relative';
            targetContent.style.left = '0';
            targetContent.classList.add('active');
        } else {
            console.error(`❌ Tab content not found: ${targetTab}`);
            return;
        }

        // Set active link
        const activeLink = document.querySelector(`[data-tab="${targetTab}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }

        // Update header
        const currentTabName = document.getElementById('current-tab-name');
        if (currentTabName && this.tabNames[targetTab]) {
            currentTabName.textContent = this.tabNames[targetTab];
        }

        // Update state
        this.currentTab = targetTab;

        console.log(`✅ Successfully switched to ${targetTab} tab`);
    }

    // ==========================================
    // PROFILE FEATURES
    // ==========================================

    initProfileFeatures() {
        console.log('🔄 Initializing profile features...');

        this.initProfilePictureUpload();
        this.initPasswordToggle();
        this.initConnectedAccounts();
        this.initPhoneVerification();
        this.initPhoneFormatting();
        this.initFormEditing();
    }

    initProfilePictureUpload() {
        const profileAvatar = document.getElementById('profile-avatar-upload');

        if (!profileAvatar) return;

        const handleUpload = () => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = 'image/*';

            input.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    if (file.size > 5 * 1024 * 1024) {
                        this.showNotification('File size must be less than 5MB', 'error');
                        return;
                    }

                    if (!file.type.startsWith('image/')) {
                        this.showNotification('Please select a valid image file', 'error');
                        return;
                    }

                    const reader = new FileReader();
                    reader.onload = (e) => {
                        profileAvatar.style.backgroundImage = `url(${e.target.result})`;
                        profileAvatar.style.backgroundSize = 'cover';
                        profileAvatar.style.backgroundPosition = 'center';
                        profileAvatar.innerHTML = '';
                        this.showNotification('Profile picture updated successfully!', 'success');
                    };
                    reader.readAsDataURL(file);
                }
            });

            input.click();
        };

        profileAvatar.addEventListener('click', handleUpload);

        // Touch support for mobile
        profileAvatar.addEventListener('touchend', (e) => {
            e.preventDefault();
            handleUpload();
        });
    }

    initPasswordToggle() {
        const toggleBtn = document.getElementById('toggle-password');

        if (toggleBtn) {
            toggleBtn.addEventListener('click', () => {
                const passwordInput = document.getElementById('password');
                if (passwordInput) {
                    const isPassword = passwordInput.type === 'password';
                    passwordInput.type = isPassword ? 'text' : 'password';
                    toggleBtn.innerHTML = isPassword ? '<i class="bx bx-show"></i>' : '<i class="bx bx-hide"></i>';
                }
            });
        }
    }

    initConnectedAccounts() {
        const connectButtons = document.querySelectorAll('.btn-connect');

        connectButtons.forEach(button => {
            button.addEventListener('click', () => {
                const accountItem = button.closest('.account-connection-item');
                const accountName = accountItem?.querySelector('.account-info span')?.textContent;

                if (button.classList.contains('connected')) {
                    const confirmDisconnect = confirm(`Are you sure you want to disconnect ${accountName}?`);
                    if (confirmDisconnect) {
                        button.innerHTML = '<i class="bx bx-plus"></i>';
                        button.classList.remove('connected');
                        this.showNotification(`${accountName} disconnected`, 'info');
                    }
                    return;
                }

                button.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i>';
                button.disabled = true;

                setTimeout(() => {
                    button.innerHTML = '<i class="bx bx-check"></i>';
                    button.classList.add('connected');
                    button.disabled = false;
                    this.showNotification(`${accountName} connected successfully!`, 'success');
                }, 1500);
            });
        });
    }

    initFormEditing() {
        // Make all form fields editable and add change detection
        const formInputs = document.querySelectorAll('.profile-form input, .profile-form select');
        const saveButton = document.querySelector('.btn-save-profile');

        let hasChanges = false;

        formInputs.forEach(input => {
            const originalValue = input.value;

            input.addEventListener('input', () => {
                hasChanges = input.value !== originalValue;
                this.updateSaveButtonState(saveButton, hasChanges);
            });

            input.addEventListener('change', () => {
                hasChanges = input.value !== originalValue;
                this.updateSaveButtonState(saveButton, hasChanges);
            });

            // Add focus styles for better UX
            input.addEventListener('focus', () => {
                input.parentElement.classList.add('focused');
            });

            input.addEventListener('blur', () => {
                input.parentElement.classList.remove('focused');
            });
        });

        // Handle form submission
        const profileForm = document.querySelector('.profile-form');
        if (profileForm) {
            profileForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.handleProfileSave(saveButton);
            });
        }
    }

    updateSaveButtonState(button, hasChanges) {
        if (!button) return;

        if (hasChanges) {
            button.style.opacity = '1';
            button.style.transform = 'scale(1.02)';
            button.disabled = false;
        } else {
            button.style.opacity = '0.8';
            button.style.transform = 'scale(1)';
        }
    }

    handleProfileSave(button) {
        if (!button) return;

        const originalText = button.innerHTML;
        button.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving...';
        button.disabled = true;

        // Simulate API call
        setTimeout(() => {
            button.innerHTML = originalText;
            button.disabled = false;
            button.style.opacity = '0.8';
            button.style.transform = 'scale(1)';
            this.showNotification('Profile updated successfully!', 'success');
        }, 2000);
    }

    initPhoneFormatting() {
        const phoneInput = document.getElementById('phone');

        if (phoneInput) {
            phoneInput.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\D/g, '');

                // Auto-add Philippines country code if needed
                if (value.length > 0 && !value.startsWith('63')) {
                    if (value.startsWith('9')) {
                        value = '63' + value;
                    }
                }

                // Format as +63 XXX XXX XXXX
                let formattedValue = '';
                if (value.length > 0) {
                    if (value.startsWith('63')) {
                        formattedValue = '+63';
                        if (value.length > 2) {
                            formattedValue += ' ' + value.slice(2, 5);
                        }
                        if (value.length > 5) {
                            formattedValue += ' ' + value.slice(5, 8);
                        }
                        if (value.length > 8) {
                            formattedValue += ' ' + value.slice(8, 12);
                        }
                    } else {
                        formattedValue = value;
                    }
                }

                e.target.value = formattedValue;
            });
        }
    }

    // ==========================================
    // PHONE VERIFICATION
    // ==========================================

    initPhoneVerification() {
        const verifyPhoneBtn = document.getElementById('verify-phone-btn');

        if (verifyPhoneBtn) {
            verifyPhoneBtn.addEventListener('click', () => {
                this.showNotification('Phone verification feature coming soon!', 'info');
            });
        }
    }

    // ==========================================
    // FORM HANDLING
    // ==========================================

    initFormHandling() {
        console.log('🔄 Initializing form handling...');

        this.initToggleButtons();
        this.initFormValidation();
        this.initNotificationSwitches();
        this.initSecurityFeatures();
    }

    initToggleButtons() {
        const toggleGroups = document.querySelectorAll('.toggle-buttons');

        toggleGroups.forEach(group => {
            const buttons = group.querySelectorAll('.toggle-btn');
            const hiddenInput = group.querySelector('input[type="hidden"]');

            buttons.forEach(btn => {
                btn.addEventListener('click', () => {
                    buttons.forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');

                    const value = btn.getAttribute('data-value');
                    if (hiddenInput && value) {
                        hiddenInput.value = value;
                    }
                });
            });
        });
    }

    initFormValidation() {
        const forms = document.querySelectorAll('form');

        forms.forEach(form => {
            const inputs = form.querySelectorAll('input[required], input[type="email"]');

            inputs.forEach(input => {
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', () => this.clearFieldError(input));
            });

            // Form submit handler
            form.addEventListener('submit', (e) => {
                let isValid = true;
                inputs.forEach(input => {
                    if (!this.validateField(input)) {
                        isValid = false;
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    this.showNotification('Please fix the errors before submitting', 'error');
                    return;
                }

                // Show loading state
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn) {
                    const originalText = submitBtn.innerHTML;
                    submitBtn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving...';
                    submitBtn.disabled = true;

                    // Simulate save process
                    setTimeout(() => {
                        submitBtn.innerHTML = originalText;
                        submitBtn.disabled = false;
                        this.showNotification('Settings saved successfully!', 'success');
                    }, 2000);
                }
            });
        });
    }

    validateField(field) {
        const value = field.value.trim();
        const fieldType = field.type;
        const isRequired = field.hasAttribute('required');

        this.clearFieldError(field);

        if (isRequired && !value) {
            this.showFieldError(field, 'This field is required');
            return false;
        }

        if (fieldType === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                this.showFieldError(field, 'Please enter a valid email address');
                return false;
            }
        }

        if (fieldType === 'tel' && value) {
            const phoneRegex = /^[\+]?[\d\s\-\(\)]{10,}$/;
            if (!phoneRegex.test(value)) {
                this.showFieldError(field, 'Please enter a valid phone number');
                return false;
            }
        }

        return true;
    }

    showFieldError(field, message) {
        field.classList.add('error');

        const existingError = field.parentNode.querySelector('.error-message');
        if (existingError) {
            existingError.remove();
        }

        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.textContent = message;
        errorDiv.style.color = 'var(--error)';
        errorDiv.style.fontSize = '12px';
        errorDiv.style.marginTop = '4px';
        errorDiv.style.fontWeight = '500';
        field.parentNode.appendChild(errorDiv);
    }

    clearFieldError(field) {
        field.classList.remove('error');
        const errorMessage = field.parentNode.querySelector('.error-message');
        if (errorMessage) {
            errorMessage.remove();
        }
    }

    initNotificationSwitches() {
        const switches = document.querySelectorAll('.switch input[type="checkbox"]');

        switches.forEach(switchEl => {
            switchEl.addEventListener('change', () => {
                const setting = switchEl.name;
                const enabled = switchEl.checked;
                console.log(`Notification setting ${setting} changed to ${enabled}`);

                // Show feedback
                this.showNotification(`${setting} ${enabled ? 'enabled' : 'disabled'}`, 'info');
            });
        });
    }

    initSecurityFeatures() {
        // Enable 2FA
        const enable2faBtn = document.getElementById('enable-2fa');
        if (enable2faBtn) {
            enable2faBtn.addEventListener('click', () => {
                const originalText = enable2faBtn.textContent;
                enable2faBtn.textContent = 'Setting up...';
                enable2faBtn.disabled = true;

                setTimeout(() => {
                    enable2faBtn.textContent = 'Enabled ✓';
                    enable2faBtn.classList.remove('btn-secondary');
                    enable2faBtn.classList.add('btn-save');
                    this.showNotification('Two-factor authentication enabled successfully!', 'success');
                }, 2000);
            });
        }

        // Logout button
        const logoutBtn = document.getElementById('logout-btn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();

                if (confirm('Are you sure you want to logout?')) {
                    logoutBtn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Logging out...';
                    logoutBtn.disabled = true;

                    setTimeout(() => {
                        // Redirect to logout endpoint
                        window.location.href = '/Account/Logout';
                    }, 1500);
                }
            });
        }

        // Delete account button
        const deleteBtn = document.getElementById('delete-account-btn');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', (e) => {
                e.preventDefault();

                if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
                    const deleteConfirmation = prompt('Please type "DELETE" to confirm:');
                    if (deleteConfirmation === 'DELETE') {
                        deleteBtn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Deleting...';
                        deleteBtn.disabled = true;

                        setTimeout(() => {
                            this.showNotification('Account deletion initiated. You will receive an email confirmation.', 'info');
                            // Redirect to delete endpoint
                            setTimeout(() => {
                                window.location.href = '/Account/DeleteAccount';
                            }, 2000);
                        }, 1500);
                    } else {
                        this.showNotification('Account deletion cancelled.', 'warning');
                    }
                }
            });
        }

        // End session buttons
        const endSessionBtns = document.querySelectorAll('.btn-danger-small');
        endSessionBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const sessionInfo = btn.closest('.session-item').querySelector('.session-info span').textContent;
                if (confirm(`End session for ${sessionInfo}?`)) {
                    btn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i>';
                    btn.disabled = true;

                    setTimeout(() => {
                        btn.closest('.session-item').remove();
                        this.showNotification('Session ended successfully', 'success');
                    }, 1000);
                }
            });
        });
    }

    // ==========================================
    // NOTIFICATIONS
    // ==========================================

    initNotifications() {
        // Auto-hide existing notifications after page load
        setTimeout(() => {
            const existingNotifications = document.querySelectorAll('.notification');
            existingNotifications.forEach(notification => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            });
        }, 5000);
    }

    showNotification(message, type = 'info') {
        // Remove existing notification
        const existingNotification = document.querySelector('.notification');
        if (existingNotification) {
            existingNotification.remove();
        }

        // Create new notification
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;

        // Add click to dismiss
        notification.addEventListener('click', () => {
            notification.remove();
        });

        // Position notification appropriately for mobile/desktop
        if (this.isMobile) {
            notification.style.top = '90px';
            notification.style.left = '16px';
            notification.style.right = '16px';
            notification.style.maxWidth = 'none';
        }

        document.body.appendChild(notification);

        // Auto-remove after 4 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.style.opacity = '0';
                notification.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.parentNode.removeChild(notification);
                    }
                }, 300);
            }
        }, 4000);
    }

    showMobileToast(message, type = 'info', duration = 3000) {
        if (!this.isMobile) {
            this.showNotification(message, type);
            return;
        }

        const toast = document.createElement('div');
        toast.className = `mobile-toast mobile-toast-${type}`;
        toast.style.cssText = `
            background: var(--${type === 'info' ? 'primary-color' : type});
            color: var(--white);
            padding: 16px 20px;
            border-radius: 12px;
            margin-bottom: 12px;
            font-weight: 600;
            font-size: 14px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            transform: translateY(100px);
            opacity: 0;
            transition: all 0.3s ease;
            pointer-events: auto;
            position: fixed;
            bottom: 20px;
            left: 20px;
            right: 20px;
            z-index: 1001;
        `;
        toast.textContent = message;

        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateY(0)';
            toast.style.opacity = '1';
        }, 10);

        // Auto remove
        setTimeout(() => {
            toast.style.transform = 'translateY(100px)';
            toast.style.opacity = '0';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, duration);

        // Click to dismiss
        toast.addEventListener('click', () => {
            toast.style.transform = 'translateY(100px)';
            toast.style.opacity = '0';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        });
    }

    // ==========================================
    // MODAL HANDLING
    // ==========================================

    initModalHandling() {
        // Close modal on outside click
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal-overlay')) {
                // Close any open modals
            }
        });
    }

    // ==========================================
    // ACCESSIBILITY
    // ==========================================

    initAccessibility() {
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + S to save
            if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                e.preventDefault();
                const activeForm = document.querySelector('.tab-content.active form');
                if (activeForm) {
                    const saveButton = activeForm.querySelector('.btn-save, .btn-save-profile');
                    if (saveButton) {
                        saveButton.click();
                        this.showNotification('Settings saved!', 'success');
                    }
                }
            }

            // Escape to close notifications, modals, and mobile sidebar
            if (e.key === 'Escape') {
                const notification = document.querySelector('.notification');
                if (notification) {
                    notification.remove();
                }

                if (this.isMobile && this.sidebarOpen) {
                    this.closeMobileSidebar();
                }
            }

            // Arrow keys for tab navigation (desktop only)
            if (!this.isMobile && (e.key === 'ArrowRight' || e.key === 'ArrowLeft')) {
                const activeTab = document.querySelector('.tab-link.active');
                if (activeTab && document.activeElement === activeTab) {
                    e.preventDefault();
                    const tabLinks = Array.from(document.querySelectorAll('.tab-link'));
                    const currentIndex = tabLinks.indexOf(activeTab);
                    let nextIndex;

                    if (e.key === 'ArrowRight') {
                        nextIndex = (currentIndex + 1) % tabLinks.length;
                    } else {
                        nextIndex = (currentIndex - 1 + tabLinks.length) % tabLinks.length;
                    }

                    const nextTab = tabLinks[nextIndex];
                    if (nextTab) {
                        nextTab.click();
                        nextTab.focus();
                    }
                }
            }

            // Mobile menu toggle with keyboard (M key)
            if (this.isMobile && e.key === 'm' && !e.ctrlKey && !e.metaKey && !e.altKey) {
                const target = e.target;
                // Only trigger if not in an input field
                if (target.tagName !== 'INPUT' && target.tagName !== 'TEXTAREA' && target.tagName !== 'SELECT') {
                    e.preventDefault();
                    this.toggleMobileSidebar();
                }
            }
        });

        // High contrast mode toggle
        const highContrastToggle = document.querySelector('input[name="HighContrast"]');
        if (highContrastToggle) {
            highContrastToggle.addEventListener('change', () => {
                document.body.classList.toggle('high-contrast', highContrastToggle.checked);
                this.showNotification(`High contrast mode ${highContrastToggle.checked ? 'enabled' : 'disabled'}`, 'info');
            });
        }

        // Reduce motion toggle
        const reduceMotionToggle = document.querySelector('input[name="ReduceMotion"]');
        if (reduceMotionToggle) {
            reduceMotionToggle.addEventListener('change', () => {
                document.body.classList.toggle('reduce-motion', reduceMotionToggle.checked);
                this.showNotification(`Motion reduction ${reduceMotionToggle.checked ? 'enabled' : 'disabled'}`, 'info');
            });
        }

        // Large text toggle
        const largeTextToggle = document.querySelector('input[name="LargeText"]');
        if (largeTextToggle) {
            largeTextToggle.addEventListener('change', () => {
                document.body.classList.toggle('large-text', largeTextToggle.checked);
                this.showNotification(`Large text ${largeTextToggle.checked ? 'enabled' : 'disabled'}`, 'info');
            });
        }

        // Touch gesture support for mobile sidebar
        this.initTouchGestures();
    }

    initTouchGestures() {
        // Only enable touch gestures on mobile
        if (window.innerWidth > 768) return;

        let startX = 0;
        let startY = 0;
        let currentX = 0;
        let currentY = 0;
        let isScrolling = false;

        const container = document.querySelector('.settings-container');
        if (!container) return;

        container.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
            isScrolling = false;
        }, { passive: true });

        container.addEventListener('touchmove', (e) => {
            if (!startX || !startY) return;

            currentX = e.touches[0].clientX;
            currentY = e.touches[0].clientY;

            const diffX = startX - currentX;
            const diffY = startY - currentY;

            // Determine if user is scrolling vertically
            if (Math.abs(diffY) > Math.abs(diffX)) {
                isScrolling = true;
                return;
            }

            // Prevent default only for horizontal swipes
            if (Math.abs(diffX) > 10 && !isScrolling) {
                e.preventDefault();
            }
        }, { passive: false });

        container.addEventListener('touchend', (e) => {
            if (!startX || !startY || isScrolling) return;

            const diffX = startX - currentX;
            const threshold = Math.min(50, window.innerWidth * 0.15);

            // Swipe right to open sidebar (from left edge)
            if (diffX < -threshold && startX < 50 && !this.sidebarOpen) {
                this.openMobileSidebar();
            }

            // Swipe left to close sidebar
            if (diffX > threshold && this.sidebarOpen) {
                this.closeMobileSidebar();
            }

            // Reset values
            startX = 0;
            startY = 0;
            currentX = 0;
            currentY = 0;
            isScrolling = false;
        }, { passive: true });
    }

    // ==========================================
    // UTILITY METHODS
    // ==========================================

    getCurrentTab() {
        return this.currentTab;
    }

    isInitialized() {
        return this.initialized;
    }

    isMobileLayout() {
        return this.isMobile;
    }

    isSidebarOpen() {
        return this.sidebarOpen;
    }

    // Debug method
    debug() {
        console.log('=== Responsive Settings Manager Debug Info ===');
        console.log('Current tab:', this.currentTab);
        console.log('Initialized:', this.initialized);
        console.log('Is mobile:', this.isMobile);
        console.log('Sidebar open:', this.sidebarOpen);
        console.log('Window width:', window.innerWidth);
        console.log('Active tab link:', document.querySelector('.tab-link.active'));
        console.log('Active tab content:', document.querySelector('.tab-content.active'));

        const profileTab = document.getElementById('profile');
        if (profileTab) {
            const styles = getComputedStyle(profileTab);
            console.log('Profile tab styles:', {
                display: styles.display,
                visibility: styles.visibility,
                opacity: styles.opacity
            });
        }
    }

    // Force fix method for troubleshooting
    forceFix() {
        console.log('🔧 Forcing settings display fix...');

        // Force container width
        this.adjustContainerLayout();

        // Force profile tab visibility
        this.showTab('profile');

        // Close mobile sidebar if open
        if (this.sidebarOpen) {
            this.closeMobileSidebar();
        }

        console.log('✅ Display fix applied');
    }

    // Public methods for external access
    openSidebar() {
        if (this.isMobile) {
            this.openMobileSidebar();
        }
    }

    closeSidebar() {
        if (this.isMobile) {
            this.closeMobileSidebar();
        }
    }

    toggleSidebar() {
        if (this.isMobile) {
            this.toggleMobileSidebar();
        }
    }

    // Cleanup method
    destroy() {
        console.log('🧹 Cleaning up ResponsiveSettingsManager...');

        // Remove event listeners
        window.removeEventListener('resize', this.handleResize);
        window.removeEventListener('orientationchange', this.handleResize);

        // Clear any timers
        if (this.resizeTimer) {
            clearTimeout(this.resizeTimer);
        }

        this.initialized = false;
        console.log('✅ ResponsiveSettingsManager cleaned up');
    }
}

// Initialize settings manager when DOM is ready
let settingsManager;

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        settingsManager = new ResponsiveSettingsManager();

        // Handle main sidebar state changes if present
        const mainSidebar = document.getElementById('sidebar');
        if (mainSidebar) {
            const observer = new MutationObserver(() => {
                settingsManager.adjustContainerLayout();
            });
            observer.observe(mainSidebar, { attributes: true, attributeFilter: ['class'] });
        }

        // Initial layout adjustment
        setTimeout(() => {
            settingsManager.adjustContainerLayout();
        }, 100);
    });
} else {
    settingsManager = new ResponsiveSettingsManager();
}

// Export for global access
window.ResponsiveSettingsManager = ResponsiveSettingsManager;
window.showSettingsTab = (tab) => settingsManager?.showTab(tab);
window.debugSettings = () => settingsManager?.debug();
window.fixSettingsDisplay = () => settingsManager?.forceFix();
window.toggleSettingsSidebar = () => settingsManager?.toggleSidebar();
window.openSettingsSidebar = () => settingsManager?.openSidebar();
window.closeSettingsSidebar = () => settingsManager?.closeSidebar();

// Emergency layout fix for immediate issues
function emergencyLayoutFix() {
    console.log('🚨 Applying emergency layout fix...');

    // Hide all tabs
    const allTabs = document.querySelectorAll('.tab-content');
    allTabs.forEach(tab => {
        tab.style.display = 'none';
        tab.style.visibility = 'hidden';
        tab.style.opacity = '0';
        tab.style.position = 'absolute';
        tab.style.left = '-9999px';
        tab.classList.remove('active');
    });

    // Show profile tab with force
    const profileTab = document.getElementById('profile');
    if (profileTab) {
        profileTab.style.setProperty('display', 'block', 'important');
        profileTab.style.setProperty('visibility', 'visible', 'important');
        profileTab.style.setProperty('opacity', '1', 'important');
        profileTab.style.setProperty('position', 'relative', 'important');
        profileTab.style.setProperty('left', '0', 'important');
        profileTab.classList.add('active');
    }

    // Set profile link as active
    const profileLink = document.querySelector('[data-tab="profile"]');
    if (profileLink) {
        document.querySelectorAll('.tab-link').forEach(link => link.classList.remove('active'));
        profileLink.classList.add('active');
    }

    // Close mobile sidebar if open
    const sidebar = document.getElementById('settings-sidebar');
    const overlay = document.getElementById('sidebar-overlay');

    if (sidebar) {
        sidebar.classList.remove('mobile-open');
    }

    if (overlay) {
        overlay.classList.remove('active');
    }

    document.body.classList.remove('sidebar-mobile-open');

    console.log('✅ Emergency layout fix applied');
}

// Apply emergency fix if needed
window.emergencyLayoutFix = emergencyLayoutFix;

// Auto-fix on page load if there are issues
window.addEventListener('load', () => {
    setTimeout(() => {
        const profileTab = document.getElementById('profile');
        if (profileTab && getComputedStyle(profileTab).display === 'none') {
            console.warn('⚠️ Profile tab not visible, applying emergency fix...');
            emergencyLayoutFix();
        }
    }, 1000);
});

// Handle orientation change on mobile devices
window.addEventListener('orientationchange', () => {
    setTimeout(() => {
        if (settingsManager) {
            settingsManager.handleResize();
            settingsManager.adjustFormLayout();
        }
    }, 200);
});

// Handle visibility change (when app comes back into focus)
document.addEventListener('visibilitychange', () => {
    if (!document.hidden && settingsManager) {
        setTimeout(() => {
            settingsManager.adjustContainerLayout();
        }, 100);
    }
});

console.log('✅ Settings JavaScript loaded successfully');