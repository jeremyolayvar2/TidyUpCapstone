class ResponsiveSettingsManager {
    constructor() {
        this.currentTab = 'profile';
        this.initialized = false;
        this.languageSettingsLoaded = false;
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
        // Initialize tab display first without forcing
        this.initTabDisplay();

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
        console.log('Responsive Settings Manager initialized successfully');
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
    async applyLanguageChangesImmediately(language, timezone, accessibilitySettings) {
        // 1. Apply language changes
        await this.applyLanguageImmediately(language);

        // 2. Apply timezone changes  
        this.applyTimezoneImmediately(timezone);

        // 3. Apply accessibility settings
        this.applyAccessibilitySettingsImmediately(accessibilitySettings);

        // 4. Update browser cookie for future requests
        this.updateLanguageCookie(language);
    }

    async applyLanguageImmediately(languageCode) {
        try {
            // Fetch translations for the new language
            const response = await fetch(`/Settings/GetTranslations?languageCode=${languageCode}`);
            const data = await response.json();

            if (data.success) {
                this.currentTranslations = data.translations;

                // Apply to document
                document.documentElement.setAttribute('lang', languageCode);
                document.documentElement.setAttribute('data-language', languageCode);

                // Apply RTL/LTR
                const direction = ['ar', 'he', 'fa'].includes(languageCode) ? 'rtl' : 'ltr';
                document.documentElement.setAttribute('dir', direction);

                // Update all translatable elements
                this.updateAllTranslatableElements();

                console.log(`Language applied immediately: ${languageCode}`);
            }
        } catch (error) {
            console.error('Error applying language:', error);
        }
    }

    applyTimezoneImmediately(timezone) {
        // Update any displayed times on the current page
        this.displayTimezoneInfo(timezone);

        // Store timezone for future use
        this.currentTimezone = timezone;

        console.log(`Timezone applied immediately: ${timezone}`);
    }

    applyAccessibilitySettingsImmediately(settings) {
        this.toggleHighContrast(settings.highContrast);
        this.toggleLargeText(settings.largeText);
        this.toggleReduceMotion(settings.reduceMotion);
        this.toggleScreenReaderMode(settings.screenReader);

        console.log('Accessibility settings applied immediately');
    }

    updateAllTranslatableElements() {
        document.querySelectorAll('[data-translate]').forEach(element => {
            const key = element.getAttribute('data-translate');
            const translation = this.getTranslation(key);
            if (translation) {
                element.textContent = translation;
            }
        });
    }

    getTranslation(key) {
        return this.currentTranslations && this.currentTranslations[key]
            ? this.currentTranslations[key]
            : key;
    }

    updateLanguageCookie(language) {
        // Set cookie so server knows about language preference on next request
        document.cookie = `user_language=${language}; path=/; max-age=${365 * 24 * 60 * 60}`; // 1 year
    }

    displayTimezoneInfo(timezone) {
        // Create or update timezone info display
        let timezoneInfo = document.getElementById('timezone-info');

        if (!timezoneInfo) {
            timezoneInfo = document.createElement('div');
            timezoneInfo.id = 'timezone-info';
            timezoneInfo.className = 'timezone-info';

            const timezoneSelect = document.querySelector('select[name="Timezone"]');
            if (timezoneSelect) {
                timezoneSelect.parentNode.appendChild(timezoneInfo);
            }
        }

        try {
            const now = new Date();
            const timeInTimezone = new Intl.DateTimeFormat('en-US', {
                timeZone: timezone,
                weekday: 'short',
                hour: '2-digit',
                minute: '2-digit',
                timeZoneName: 'short'
            }).format(now);

            timezoneInfo.innerHTML = `
            <small style="color: var(--text-secondary); margin-top: 8px; display: block;">
                Current time: ${timeInTimezone}
            </small>
        `;

        } catch (error) {
            timezoneInfo.innerHTML = `<small style="color: var(--error);">Invalid timezone</small>`;
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

    // Simple tab display initialization
    initTabDisplay() {
        console.log('Initializing tab display...');

        // Hide all tabs simply
        document.querySelectorAll('.tab-content').forEach(tab => {
            tab.classList.remove('active');
        });

        // Show profile tab
        const profileTab = document.getElementById('profile');
        if (profileTab) {
            profileTab.classList.add('active');
        }

        // Set profile link as active
        const profileLink = document.querySelector('[data-tab="profile"]');
        if (profileLink) {
            document.querySelectorAll('.tab-link').forEach(link => link.classList.remove('active'));
            profileLink.classList.add('active');
        }

        console.log('Tab display initialized');
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
        console.log(`Switching to tab: ${targetTab}`);

        // Simple hide/show approach
        document.querySelectorAll('.tab-content').forEach(content => {
            content.classList.remove('active');
        });
        document.querySelectorAll('.tab-link').forEach(link => {
            link.classList.remove('active');
        });

        // Show target tab
        const targetContent = document.getElementById(targetTab);
        if (targetContent) {
            targetContent.classList.add('active');
        } else {
            console.error(`Tab content not found: ${targetTab}`);
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

        if (targetTab === 'language' && !this.languageSettingsLoaded) {
            setTimeout(() => {
                this.loadLanguageSettings();
                this.languageSettingsLoaded = true;
            }, 100);
        }

        // Update state
        this.currentTab = targetTab;
        console.log(`Successfully switched to ${targetTab} tab`);
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
        this.initSaveButtonClick();
    }

    initSaveButtonClick() {
        const saveButton = document.querySelector('.btn-save-profile');
        if (saveButton) {
            console.log('Setting up save button click handler');

            // Add the click handler
            saveButton.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('Save button clicked - calling handleProfileSave');
                this.handleProfileSave(saveButton);
            });
        } else {
            console.warn('Save button not found');
        }
    }

    initProfilePictureUpload() {
        const profileAvatar = document.getElementById('profile-avatar-upload');
        const fileInput = document.getElementById('profile-picture-input'); // Use the existing hidden input

        if (!profileAvatar || !fileInput) return;

        const handleUpload = () => {
            fileInput.click(); // Trigger the hidden file input
        };

        // Handle file selection
        fileInput.addEventListener('change', (e) => {
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
                    this.showNotification('Profile picture selected! Click Save Changes to upload.', 'success');
                };
                reader.readAsDataURL(file);
            }
        });

        profileAvatar.addEventListener('click', handleUpload);
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

        // Create FormData to handle both regular fields and file upload
        const formData = new FormData();

        // Add fields that match UpdateUserProfileDto exactly
        formData.append('Username', document.getElementById('username')?.value || '');
        formData.append('FirstName', document.getElementById('firstName')?.value || '');
        formData.append('LastName', document.getElementById('lastName')?.value || '');
        formData.append('Phone', document.getElementById('phone')?.value || '');
        formData.append('PhoneNumber', document.getElementById('phone')?.value || ''); // DTO has both Phone and PhoneNumber
        formData.append('Email', document.getElementById('email')?.value || '');
        formData.append('Location', document.getElementById('location')?.value || '');
        formData.append('Gender', document.getElementById('gender')?.value || '');
        formData.append('Birthday', document.getElementById('birthday')?.value || '');
        formData.append('MarketingEmailsEnabled', 'false'); // Default value for required DTO field
        formData.append('AvatarUrl', ''); // Default value

        // Add profile picture file if selected
        const fileInput = document.getElementById('profile-picture-input');
        if (fileInput && fileInput.files[0]) {
            formData.append('ProfilePicture', fileInput.files[0]);
            console.log('Profile picture file added to form data');
        }

        // Add anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }

        // Log what we're sending
        console.log('Sending form data:');
        for (let pair of formData.entries()) {
            console.log(pair[0] + ': ' + pair[1]);
        }

        // Submit to controller
        fetch('/Settings/UpdateProfile', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                console.log('Response status:', response.status);
                button.innerHTML = originalText;
                button.disabled = false;

                if (response.status === 200 || response.status === 302) {
                    this.showNotification('Profile updated successfully!', 'success');
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                } else {
                    throw new Error(`Server responded with status: ${response.status}`);
                }
            })
            .catch(error => {
                button.innerHTML = originalText;
                button.disabled = false;
                console.error('Error:', error);
                this.showNotification('Error updating profile. Please try again.', 'error');
            });
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
        this.initPrivacySettings();
        this.initLanguageSettings();

        // Handle privacy form submission specifically
        const privacyForm = document.querySelector('#privacy form');
        if (privacyForm) {
            privacyForm.addEventListener('submit', (e) => {
                this.handlePrivacyFormSubmit(privacyForm);
            });
        }
    }

    initToggleButtons() {
        const toggleGroups = document.querySelectorAll('.toggle-buttons');

        toggleGroups.forEach(group => {
            const buttons = group.querySelectorAll('.toggle-btn');
            const hiddenInput = group.querySelector('input[type="hidden"]');

            buttons.forEach(btn => {
                btn.addEventListener('click', () => {
                    buttons.forEach(b => {
                        b.classList.remove('active');
                        b.setAttribute('aria-checked', 'false');
                    });

                    btn.classList.add('active');
                    btn.setAttribute('aria-checked', 'true');

                    const value = btn.getAttribute('data-value');
                    if (hiddenInput && value) {
                        hiddenInput.value = value;
                    }

                    if (group.closest('#privacy')) {
                        const settingName = this.getSettingDisplayName(hiddenInput?.name || '');
                        this.showNotification(`${settingName} updated to ${value}`, 'info');
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
                    // Create a form and submit it to Identity's logout
                    const form = document.createElement('form');
                    form.method = 'POST';
                    form.action = '/Account/Logout';

                    // Add anti-forgery token
                    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                    if (tokenElement) {
                        const tokenInput = document.createElement('input');
                        tokenInput.type = 'hidden';
                        tokenInput.name = '__RequestVerificationToken';
                        tokenInput.value = tokenElement.value;
                        form.appendChild(tokenInput);
                    }

                    // Add return URL
                    document.body.appendChild(form);
                    form.submit();
                }
            });
        }

        // Delete account button
        const deleteBtn = document.getElementById('delete-account-btn');
        if (deleteBtn) {
            deleteBtn.addEventListener('click', (e) => {
                e.preventDefault();

                if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
                    window.location.href = '/Auth/DeleteAccount';
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

    // ==========================================
    // PRIVACY SETTINGS FUNCTIONALITY
    // ==========================================

    initPrivacySettings() {
        console.log('Initializing privacy settings...');

        // Initialize privacy toggle buttons specifically
        this.initPrivacyToggleButtons();

        // Initialize privacy form submission
        this.initPrivacyFormSubmission();

        // Load settings when privacy tab is clicked
        const privacyTab = document.querySelector('[data-tab="privacy"]');
        if (privacyTab) {
            privacyTab.addEventListener('click', () => {
                setTimeout(() => this.loadPrivacySettings(), 100);
            });
        }

        // Load settings if privacy tab is already active
        if (document.querySelector('#privacy.active')) {
            this.loadPrivacySettings();
        }

        console.log('Privacy settings initialized');
    }

    initPrivacyToggleButtons() {
        // Get privacy-specific toggle groups
        const privacyToggles = document.querySelectorAll('#privacy .toggle-buttons');

        privacyToggles.forEach(group => {
            const buttons = group.querySelectorAll('.toggle-btn');
            const hiddenInput = group.querySelector('input[type="hidden"]');

            buttons.forEach(btn => {
                btn.addEventListener('click', () => {
                    // Remove active from all buttons in this group
                    buttons.forEach(b => {
                        b.classList.remove('active');
                        b.setAttribute('aria-checked', 'false');
                    });

                    // Add active to clicked button
                    btn.classList.add('active');
                    btn.setAttribute('aria-checked', 'true');

                    // Update hidden input value
                    const value = btn.getAttribute('data-value');
                    if (hiddenInput && value) {
                        hiddenInput.value = value;
                        console.log(`Updated ${hiddenInput.name} to: ${value}`);
                    }

                    // Show feedback notification
                    if (hiddenInput?.name) {
                        const settingName = this.getSettingDisplayName(hiddenInput.name);
                        this.showNotification(`${settingName} updated to ${value}`, 'info');
                    }
                });
            });
        });
    }

    initPrivacyFormSubmission() {
        // Handle the save button in privacy tab
        const saveButton = document.querySelector('#privacy .btn-save');

        if (saveButton) {
            saveButton.addEventListener('click', (e) => {
                e.preventDefault();
                this.handlePrivacySave();
            });
        }
    }

    handlePrivacySave() {
        const saveButton = document.querySelector('#privacy .btn-save');
        if (!saveButton) return;

        const originalText = saveButton.innerHTML;
        saveButton.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving Privacy Settings...';
        saveButton.disabled = true;

        // Collect all privacy form data
        const formData = new FormData();
        const hiddenInputs = document.querySelectorAll('#privacy input[type="hidden"]');

        hiddenInputs.forEach(input => {
            if (input.name && input.name !== '__RequestVerificationToken') {
                formData.append(input.name, input.value);
                console.log(`Form data: ${input.name} = ${input.value}`);
            }
        });

        // Add anti-forgery token
        const antiForgeryToken = document.querySelector('#privacy input[name="__RequestVerificationToken"]');
        if (antiForgeryToken) {
            formData.append('__RequestVerificationToken', antiForgeryToken.value);
        }

        // Submit to your controller
        fetch('/PrivacySettings/UpdatePrivacy', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (response.ok) {
                    return response.text();
                }
                throw new Error('Network response was not ok');
            })
            .then(data => {
                saveButton.innerHTML = originalText;
                saveButton.disabled = false;
                this.showNotification('Privacy settings saved successfully!', 'success');
                console.log('Privacy settings saved successfully');
            })
            .catch(error => {
                saveButton.innerHTML = originalText;
                saveButton.disabled = false;
                console.error('Error saving privacy settings:', error);
                this.showNotification('Error saving privacy settings. Please try again.', 'error');
            });
    }

    loadPrivacySettings() {
        console.log('Loading privacy settings...');

        fetch('/PrivacySettings/GetSettings')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to load privacy settings');
                }
                return response.json();
            })
            .then(data => {
                console.log('Loaded privacy settings:', data);

                // Map the response data to form fields
                Object.keys(data).forEach(key => {
                    // Convert API response keys to match your HTML input names
                    const inputName = this.mapApiKeyToInputName(key);
                    const input = document.querySelector(`#privacy input[name="${inputName}"]`);

                    if (input) {
                        input.value = data[key];

                        // Update the toggle button display
                        const group = input.closest('.toggle-buttons');
                        if (group) {
                            const buttons = group.querySelectorAll('.toggle-btn');

                            buttons.forEach(btn => {
                                btn.classList.remove('active');
                                btn.setAttribute('aria-checked', 'false');

                                if (btn.dataset.value === data[key]) {
                                    btn.classList.add('active');
                                    btn.setAttribute('aria-checked', 'true');
                                }
                            });
                        }
                    }
                });
            })
            .catch(error => {
                console.error('Error loading privacy settings:', error);
                this.showNotification('Failed to load privacy settings', 'error');
            });
    }

    initLanguageSettings() {
    console.log('Initializing language & accessibility settings...');

    const languageSaveButton = document.querySelector('#language .btn-save');
    if (languageSaveButton) {
        languageSaveButton.addEventListener('click', (e) => {
            e.preventDefault();
            this.handleLanguageSave();
        });
    }

    // Always load settings from DB
    this.loadLanguageSettings();
    this.languageSettingsLoaded = true;
    this.initAccessibilitySwitches();
    
    // Load translations immediately for current language
    this.loadAndApplyCurrentLanguage();
}

async loadAndApplyCurrentLanguage() {
    try {
        // Get the user's saved language
        const response = await fetch('/Settings/GetLanguageSettings');
        const data = await response.json();
        
        if (data.success && data.data.language) {
            // Apply the language immediately without saving
            await this.applyLanguageImmediately(data.data.language);
            console.log(`Applied saved language on page load: ${data.data.language}`);
        }
    } catch (error) {
        console.error('Error loading current language:', error);
    }
}

    // Handle language settings save
    handleLanguageSave() {
        const saveButton = document.querySelector('#language .btn-save');
        if (!saveButton) return;

        const originalText = saveButton.innerHTML;
        saveButton.innerHTML = '<i class="bx bx-loader-alt bx-spin" aria-hidden="true"></i> <span data-translate="saving">Saving...</span>';
        saveButton.disabled = true;

        // Get form data
        const language = document.querySelector('select[name="Language"]')?.value;
        const timezone = document.querySelector('select[name="Timezone"]')?.value;
        const highContrast = document.querySelector('input[name="HighContrast"]')?.checked || false;
        const largeText = document.querySelector('input[name="LargeText"]')?.checked || false;
        const reduceMotion = document.querySelector('input[name="ReduceMotion"]')?.checked || false;
        const screenReader = document.querySelector('input[name="ScreenReader"]')?.checked || false;

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const formData = new FormData();
        formData.append('Language', language || 'en');
        formData.append('Timezone', timezone || 'Asia/Manila');
        formData.append('HighContrast', highContrast);
        formData.append('LargeText', largeText);
        formData.append('ReduceMotion', reduceMotion);
        formData.append('ScreenReader', screenReader);

        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Save to server
        fetch('/Settings/UpdateLanguage', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                saveButton.innerHTML = originalText;
                saveButton.disabled = false;

                if (data.success) {
                    // Apply changes immediately in settings page
                    this.applyLanguageChangesImmediately(language, timezone, {
                        highContrast, largeText, reduceMotion, screenReader
                    });

                    // Refresh global accessibility for entire application
                    if (window.globalAccessibility) {
                        window.globalAccessibility.refresh();
                    }

                    this.showNotification(this.getTranslation('settings_saved') || data.message, 'success');
                } else {
                    this.showNotification(this.getTranslation('error_saving') || data.message, 'error');
                }
            })
            .catch(error => {
                saveButton.innerHTML = originalText;
                saveButton.disabled = false;
                console.error('Error:', error);
                this.showNotification(this.getTranslation('error_saving') || 'An error occurred while saving settings. Please try again.', 'error');
            });
    }
    // Load current language settings
    loadLanguageSettings() {
        fetch('/Settings/GetLanguageSettings')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Populate the form with current settings
                    const languageSelect = document.querySelector('select[name="Language"]');
                    const timezoneSelect = document.querySelector('select[name="Timezone"]');
                    const highContrastCheckbox = document.querySelector('input[name="HighContrast"]');
                    const largeTextCheckbox = document.querySelector('input[name="LargeText"]');
                    const reduceMotionCheckbox = document.querySelector('input[name="ReduceMotion"]');
                    const screenReaderCheckbox = document.querySelector('input[name="ScreenReader"]');

                    // CHANGE THESE LINES - use lowercase property names
                    if (languageSelect) languageSelect.value = data.data.language || 'en';
                    if (timezoneSelect) timezoneSelect.value = data.data.timezone || 'Asia/Manila';
                    if (highContrastCheckbox) highContrastCheckbox.checked = data.data.highContrast || false;
                    if (largeTextCheckbox) largeTextCheckbox.checked = data.data.largeText || false;
                    if (reduceMotionCheckbox) reduceMotionCheckbox.checked = data.data.reduceMotion || false;
                    if (screenReaderCheckbox) screenReaderCheckbox.checked = data.data.screenReader || false;

                    // Apply current accessibility settings to the page
                    this.applyAccessibilitySettings(
                        data.data.highContrast || false,
                        data.data.largeText || false,
                        data.data.reduceMotion || false
                    );
                }
            })
            .catch(error => {
                console.error('Error loading language settings:', error);
            });
    }

    // Initialize accessibility switches with immediate feedback
    initAccessibilitySwitches() {
        const accessibilitySwitches = document.querySelectorAll('#language .switch input[type="checkbox"]');

        accessibilitySwitches.forEach(switchEl => {
            switchEl.addEventListener('change', () => {
                const setting = switchEl.name;
                const enabled = switchEl.checked;

                // Apply the setting immediately for better UX
                switch (setting) {
                    case 'HighContrast':
                        this.toggleHighContrast(enabled);
                        break;
                    case 'LargeText':
                        this.toggleLargeText(enabled);
                        break;
                    case 'ReduceMotion':
                        this.toggleReduceMotion(enabled);
                        break;
                    case 'ScreenReader':
                        this.toggleScreenReaderMode(enabled);
                        break;
                }

                // Show immediate feedback
                const settingName = this.getAccessibilitySettingName(setting);
                this.showNotification(`${settingName} ${enabled ? 'enabled' : 'disabled'}`, 'info');
            });
        });
    }

    // Apply accessibility settings to the page
    applyAccessibilitySettings(highContrast, largeText, reduceMotion) {
        this.toggleHighContrast(highContrast);
        this.toggleLargeText(largeText);
        this.toggleReduceMotion(reduceMotion);
    }

    // Individual accessibility setting toggles
    toggleHighContrast(enabled) {
        const body = document.body;
        if (enabled) {
            body.classList.add('high-contrast-mode');
        } else {
            body.classList.remove('high-contrast-mode');
        }
    }

    toggleLargeText(enabled) {
        const body = document.body;
        if (enabled) {
            body.classList.add('large-text-mode');
        } else {
            body.classList.remove('large-text-mode');
        }
    }

    toggleReduceMotion(enabled) {
        const body = document.body;
        if (enabled) {
            body.classList.add('reduce-motion-mode');
        } else {
            body.classList.remove('reduce-motion-mode');
        }
    }

    toggleScreenReaderMode(enabled) {
        // Add screen reader optimizations
        const body = document.body;
        if (enabled) {
            body.classList.add('screen-reader-mode');
            // Add aria-live regions, focus management, etc.
            this.enhanceScreenReaderSupport();
        } else {
            body.classList.remove('screen-reader-mode');
        }
    }

    enhanceScreenReaderSupport() {
        // Add live region for announcements if it doesn't exist
        if (!document.getElementById('sr-live-region')) {
            const liveRegion = document.createElement('div');
            liveRegion.id = 'sr-live-region';
            liveRegion.setAttribute('aria-live', 'polite');
            liveRegion.setAttribute('aria-atomic', 'true');
            liveRegion.style.position = 'absolute';
            liveRegion.style.left = '-10000px';
            liveRegion.style.width = '1px';
            liveRegion.style.height = '1px';
            liveRegion.style.overflow = 'hidden';
            document.body.appendChild(liveRegion);
        }
    }

    // Helper method to get user-friendly setting names
    getAccessibilitySettingName(settingName) {
        const names = {
            'HighContrast': 'High Contrast Mode',
            'LargeText': 'Large Text',
            'ReduceMotion': 'Reduce Motion',
            'ScreenReader': 'Screen Reader Support'
        };
        return names[settingName] || settingName;
    }

    mapApiKeyToInputName(apiKey) {
        // Map API response keys to your HTML input names
        const keyMapping = {
            'profileVisibility': 'ProfileVisibility',
            'locationVisibility': 'LocationVisibility',
            'activityStreaksVisibility': 'ActivityStreaksVisibility',
            'onlineStatus': 'OnlineStatus',
            'searchIndexing': 'SearchIndexing',
            'contactVisibility': 'ContactVisibility',
            'activityHistory': 'ActivityHistory'
        };

        return keyMapping[apiKey] || apiKey;
    }

    getSettingDisplayName(settingName) {
        const displayNames = {
            'ProfileVisibility': 'Profile Visibility',
            'LocationVisibility': 'Location Sharing',
            'ActivityStreaksVisibility': 'Activity Streaks',
            'OnlineStatus': 'Online Status',
            'SearchIndexing': 'Search Engine Indexing',
            'ContactVisibility': 'Contact Information',
            'ActivityHistory': 'Activity History'
        };
        return displayNames[settingName] || settingName;
    }

    handlePrivacyFormSubmit(form) {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (!submitBtn) return;

        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving Privacy Settings...';
        submitBtn.disabled = true;

        setTimeout(() => {
            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;
        }, 5000);
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
        });
    }
}

// Initialize settings manager when DOM is ready
let settingsManager;

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        settingsManager = new ResponsiveSettingsManager();
    });
} else {
    settingsManager = new ResponsiveSettingsManager();
}

// Export for global access
window.ResponsiveSettingsManager = ResponsiveSettingsManager;
window.showSettingsTab = (tab) => settingsManager?.showTab(tab);
window.debugSettings = () => settingsManager?.debug();

console.log('Settings JavaScript loaded successfully');