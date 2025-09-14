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
        console.log('Initializing Responsive Settings Manager...');

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initializeComponents());
        } else {
            this.initializeComponents();
        }
    }

    initializeComponents() {
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

    // RESPONSIVE LAYOUT MANAGEMENT
    initResponsiveLayout() {
        console.log('Initializing responsive layout...');
        this.updateLayoutState();
        this.adjustContainerLayout();
    }

    updateLayoutState() {
        const wasMobile = this.isMobile;
        this.isMobile = window.innerWidth <= 768;
        this.isTablet = window.innerWidth > 768 && window.innerWidth <= 1024;
        this.isDesktop = window.innerWidth > 768;

        if (wasMobile !== this.isMobile) {
            console.log(`Layout changed: ${this.isMobile ? 'Mobile' : 'Desktop'}`);
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
            container.style.marginLeft = '0';
            container.style.width = '100%';
        } else {
            if (sidebar && sidebar.classList.contains('close')) {
                container.style.marginLeft = '82px';
                container.style.width = 'calc(100% - 82px)';
            } else {
                container.style.marginLeft = '250px';
                container.style.width = 'calc(100% - 250px)';
            }
        }
    }

    adjustFormLayout() {
        const formRows = document.querySelectorAll('.form-row-top, .form-row-bottom');

        formRows.forEach(row => {
            if (window.innerWidth <= 480) {
                row.style.display = 'block';
            } else if (window.innerWidth <= 768) {
                row.style.display = 'grid';
                row.style.gridTemplateColumns = '1fr';
            } else {
                row.style.display = 'grid';
                row.style.gridTemplateColumns = 'repeat(auto-fit, minmax(250px, 1fr))';
            }
        });
    }

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
        await this.applyLanguageImmediately(language);
        this.applyTimezoneImmediately(timezone);
        this.applyAccessibilitySettingsImmediately(accessibilitySettings);
        this.updateLanguageCookie(language);
    }

    async applyLanguageImmediately(languageCode) {
        try {
            const response = await fetch(`/Settings/GetTranslations?languageCode=${languageCode}`);
            const data = await response.json();

            if (data.success) {
                this.currentTranslations = data.translations;

                document.documentElement.setAttribute('lang', languageCode);
                document.documentElement.setAttribute('data-language', languageCode);

                const direction = ['ar', 'he', 'fa'].includes(languageCode) ? 'rtl' : 'ltr';
                document.documentElement.setAttribute('dir', direction);

                this.updateAllTranslatableElements();
                console.log(`Language applied immediately: ${languageCode}`);
            }
        } catch (error) {
            console.error('Error applying language:', error);
        }
    }

    applyTimezoneImmediately(timezone) {
        this.displayTimezoneInfo(timezone);
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
        document.cookie = `user_language=${language}; path=/; max-age=${365 * 24 * 60 * 60}`;
    }

    displayTimezoneInfo(timezone) {
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

    // MOBILE SIDEBAR FUNCTIONALITY
    initMobileSidebar() {
        console.log('Initializing mobile sidebar...');

        const toggleBtn = document.getElementById('mobile-menu-toggle');
        const closeBtn = document.getElementById('sidebar-close-btn');
        const overlay = document.getElementById('sidebar-overlay');
        const sidebar = document.getElementById('settings-sidebar');

        if (!toggleBtn || !sidebar) {
            console.warn('Mobile sidebar elements not found');
            return;
        }

        toggleBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.toggleMobileSidebar();
        });

        if (closeBtn) {
            closeBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.closeMobileSidebar();
            });
        }

        if (overlay) {
            overlay.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                this.closeMobileSidebar();
            });
        }

        const tabLinks = document.querySelectorAll('.tab-link');
        tabLinks.forEach(link => {
            link.addEventListener('click', () => {
                if (this.isMobile && this.sidebarOpen) {
                    setTimeout(() => this.closeMobileSidebar(), 100);
                }
            });
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMobile && this.sidebarOpen) {
                this.closeMobileSidebar();
            }
        });

        console.log('Mobile sidebar initialized');
    }

    toggleMobileSidebar() {
        if (this.sidebarOpen) {
            this.closeMobileSidebar();
        } else {
            this.openMobileSidebar();
        }
    }

    openMobileSidebar() {
        console.log('Opening mobile sidebar...');

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

        const firstTabLink = sidebar?.querySelector('.tab-link');
        if (firstTabLink) {
            setTimeout(() => firstTabLink.focus(), 100);
        }
    }

    closeMobileSidebar() {
        console.log('Closing mobile sidebar...');

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

    // RESIZE HANDLER
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
        console.log('Handling window resize...');

        this.updateLayoutState();
        this.adjustContainerLayout();
        this.adjustFormLayout();
        this.adjustProfileLayout();

        if (!this.isMobile && this.sidebarOpen) {
            this.closeMobileSidebar();
        }
    }

    // TAB DISPLAY
    initTabDisplay() {
        console.log('Initializing tab display...');

        document.querySelectorAll('.tab-content').forEach(tab => {
            tab.classList.remove('active');
        });

        const profileTab = document.getElementById('profile');
        if (profileTab) {
            profileTab.classList.add('active');
        }

        const profileLink = document.querySelector('[data-tab="profile"]');
        if (profileLink) {
            document.querySelectorAll('.tab-link').forEach(link => link.classList.remove('active'));
            profileLink.classList.add('active');
        }

        console.log('Tab display initialized');
    }

    // TAB SWITCHING
    initTabSwitching() {
        console.log('Initializing tab switching...');

        const tabLinks = document.querySelectorAll('.tab-link');

        if (!tabLinks.length) {
            console.warn('Tab elements not found');
            return;
        }

        tabLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetTab = link.getAttribute('data-tab');
                if (targetTab) {
                    this.showTab(targetTab);
                }
            });
        });

        this.showTab('profile');
        window.showSettingsTab = (tab) => this.showTab(tab);
    }

    showTab(targetTab) {
        console.log(`Switching to tab: ${targetTab}`);

        document.querySelectorAll('.tab-content').forEach(content => {
            content.classList.remove('active');
        });
        document.querySelectorAll('.tab-link').forEach(link => {
            link.classList.remove('active');
        });

        const targetContent = document.getElementById(targetTab);
        if (targetContent) {
            targetContent.classList.add('active');
        } else {
            console.error(`Tab content not found: ${targetTab}`);
            return;
        }

        const activeLink = document.querySelector(`[data-tab="${targetTab}"]`);
        if (activeLink) {
            activeLink.classList.add('active');
        }

        const currentTabName = document.getElementById('current-tab-name');
        if (currentTabName && this.tabNames[targetTab]) {
            currentTabName.textContent = this.tabNames[targetTab];
        }

        // ADD THIS - Load profile data when switching to profile tab
        if (targetTab === 'profile') {
            setTimeout(() => {
                this.loadProfileData();
            }, 100);
        }

        if (targetTab === 'language' && !this.languageSettingsLoaded) {
            setTimeout(() => {
                this.loadLanguageSettings();
                this.languageSettingsLoaded = true;
            }, 100);
        }

        this.currentTab = targetTab;
        console.log(`Successfully switched to ${targetTab} tab`);
    }

    // PROFILE FEATURES
    initProfileFeatures() {
        console.log('Initializing profile features...');

        // ADD THIS LINE - Load profile data when profile features initialize
        this.loadProfileData();

        this.initProfilePictureUpload();
        this.initPasswordToggle();
        this.initConnectedAccounts();
        this.initPhoneVerification();
        this.initPhoneFormatting();
        this.initFormEditing();
        this.initSaveButtonClick();
    }

    async loadProfileData() {
        try {
            console.log('Loading profile data from backend...');

            const response = await fetch('/Settings/GetProfileData');
            const data = await response.json();

            if (data.success) {
                const profile = data.profile;

                console.log('=== Detailed Profile Data ===');
                console.log('Location:', profile.Location);
                console.log('Gender:', profile.Gender);
                console.log('Birthday:', profile.Birthday);
                console.log('FirstName:', profile.FirstName);
                console.log('LastName:', profile.LastName);
                console.log('Email:', profile.Email);
                console.log('Username:', profile.Username);
                console.log('Full profile object:', profile);
                console.log('Raw phoneNumber:', data.phoneNumber);

                // Populate profile header
                const displayName = document.getElementById('profile-display-name');
                const displayUsername = document.getElementById('profile-display-username');

                if (displayName && profile.firstName && profile.lastName) {
                    displayName.textContent = `${profile.firstName} ${profile.lastName}`;
                }

                if (displayUsername && profile.username) {
                    displayUsername.textContent = `@${profile.username}`;
                }

                // ENHANCED field population with debugging
                console.log('=== Populating Form Fields ===');
                this.populateFormFieldDebug('firstName', profile.firstName);
                this.populateFormFieldDebug('lastName', profile.lastName);
                this.populateFormFieldDebug('email', profile.email);
                this.populateFormFieldDebug('username', profile.username);
                this.populateFormFieldDebug('phone', data.phoneNumber);
                this.populateFormFieldDebug('location', profile.location);
                this.populateFormFieldDebug('gender', profile.gender);

                // Handle birthday formatting for date input
                if (profile.birthday) {
                    const birthday = new Date(profile.birthday);
                    const formattedDate = birthday.toISOString().split('T')[0];
                    this.populateFormFieldDebug('birthday', formattedDate);
                }

                // Handle profile picture
                if (profile.profilePictureUrl || profile.avatarUrl) {
                    const profileAvatar = document.getElementById('profile-avatar-upload');
                    if (profileAvatar) {
                        const imageUrl = profile.profilePictureUrl || profile.avatarUrl;
                        console.log('Setting profile image URL:', imageUrl);
                        const cacheBustUrl = `${imageUrl}?t=${Date.now()}`;
                        profileAvatar.style.backgroundImage = `url(${cacheBustUrl})`;
                        profileAvatar.style.backgroundSize = 'cover';
                        profileAvatar.style.backgroundPosition = 'center';
                        profileAvatar.innerHTML = '';
                    }
                }

                console.log('Profile data loaded successfully:', profile);
                return true;
            } else {
                console.error('Failed to load profile data:', data.message);
                this.showNotification('Failed to load profile data', 'error');
                return false;
            }
        } catch (error) {
            console.error('Error loading profile data:', error);
            this.showNotification('Error loading profile data', 'error');
            return false;
        }
    }

    // Add this new debug method
    populateFormFieldDebug(fieldId, value) {
        console.log(`Trying to populate field '${fieldId}' with value: '${value}'`);

        const field = document.getElementById(fieldId);
        if (!field) {
            console.error(`❌ Field with ID '${fieldId}' NOT FOUND in DOM`);
            return;
        }

        console.log(`✅ Found field '${fieldId}':`, field);

        if (value !== null && value !== undefined && value !== '') {
            field.value = value;
            console.log(`✅ Set field '${fieldId}' to: '${field.value}'`);

            // For select elements, ensure the option exists
            if (field.tagName === 'SELECT') {
                const option = field.querySelector(`option[value="${value}"]`);
                if (option) {
                    field.value = value;
                    console.log(`✅ Select field '${fieldId}' set to: '${value}'`);
                } else {
                    console.error(`❌ Option '${value}' not found for select field '${fieldId}'`);
                    // List available options
                    const options = Array.from(field.options).map(opt => opt.value);
                    console.log(`Available options:`, options);
                }
            }
        } else {
            console.log(`⚠️ Skipping field '${fieldId}' - empty value`);
        }
    }


    handleProfileSave(button) {
        if (!button) return;

        const originalText = button.innerHTML;
        button.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving...';
        button.disabled = true;

        const formData = new FormData();

        // FIXED: Only send editable fields that the controller actually processes
        formData.append('Phone', document.getElementById('phone')?.value || '');
        formData.append('Location', document.getElementById('location')?.value || '');
        formData.append('Gender', document.getElementById('gender')?.value || '');
        formData.append('Birthday', document.getElementById('birthday')?.value || '');
        formData.append('MarketingEmailsEnabled', 'false');

        // Handle profile picture upload
        const fileInput = document.getElementById('profile-picture-input');
        if (fileInput && fileInput.files[0]) {
            formData.append('ProfilePicture', fileInput.files[0]);
            console.log('Profile picture file added to form data');
        }

        // Add antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }

        // Debug logging
        console.log('Sending form data:');
        for (let pair of formData.entries()) {
            console.log(pair[0] + ': ' + pair[1]);
        }

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

                    // Reload profile data to show updated values
                    setTimeout(() => {
                        this.loadProfileData();
                    }, 1000);
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

    initSaveButtonClick() {
        const saveButton = document.querySelector('.btn-save-profile');
        if (saveButton) {
            console.log('Setting up save button click handler');

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

    initPhoneVerification() {
        const verifyPhoneBtn = document.getElementById('verify-phone-btn');

        if (verifyPhoneBtn) {
            verifyPhoneBtn.addEventListener('click', () => {
                this.handlePhoneVerification();
            });
        }
    }

    async handlePhoneVerification() {
        const phoneInput = document.getElementById('phone');
        const phoneNumber = phoneInput?.value?.trim();

        if (!phoneNumber) {
            this.showNotification('Please enter a phone number first', 'error');
            return;
        }

        const verifyBtn = document.getElementById('verify-phone-btn');
        const originalText = verifyBtn.innerHTML;
        verifyBtn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Sending...';
        verifyBtn.disabled = true;

        try {
            const formData = new FormData();
            formData.append('phoneNumber', phoneNumber);

            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                formData.append('__RequestVerificationToken', token.value);
            }

            const response = await fetch('/Settings/SendVerificationCode', {
                method: 'POST',
                body: formData
            });

            const data = await response.json();

            if (data.success) {
                this.showNotification(data.message, 'success');
                // TODO: Show verification code input modal
                console.log('Check console for verification code (development mode)');
            } else {
                this.showNotification(data.message, 'error');
            }
        } catch (error) {
            console.error('Phone verification error:', error);
            this.showNotification('Failed to send verification code', 'error');
        } finally {
            verifyBtn.innerHTML = originalText;
            verifyBtn.disabled = false;
        }
    }

    initProfilePictureUpload() {
        const profileAvatar = document.getElementById('profile-avatar-upload');
        const fileInput = document.getElementById('profile-picture-input');

        if (!profileAvatar || !fileInput) return;

        const handleUpload = () => {
            fileInput.click();
        };

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
    initPhoneFormatting() {
        const phoneInput = document.getElementById('phone');

        if (phoneInput) {
            phoneInput.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\D/g, '');

                if (value.length > 0 && !value.startsWith('63')) {
                    if (value.startsWith('9')) {
                        value = '63' + value;
                    }
                }

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

    initPhoneVerification() {
        const verifyPhoneBtn = document.getElementById('verify-phone-btn');

        if (verifyPhoneBtn) {
            verifyPhoneBtn.addEventListener('click', () => {
                this.showNotification('Phone verification feature coming soon!', 'info');
            });
        }
    }

    // FORM HANDLING
    initFormHandling() {
        console.log('Initializing form handling...');

        this.initToggleButtons();
        this.initFormValidation();
        this.initNotificationSwitches();
        this.initSecurityFeatures();
        this.initNotificationSettings();
        this.initPrivacySettings();
        this.initLanguageSettings();

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

                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn) {
                    const originalText = submitBtn.innerHTML;
                    submitBtn.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving...';
                    submitBtn.disabled = true;

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

                this.showNotification(`${setting} ${enabled ? 'enabled' : 'disabled'}`, 'info');
            });
        });
    }

    // SECURITY FEATURES
    initSecurityFeatures() {
        console.log('Initializing security features...');

        this.initSecurityPasswordChange();
        this.initActiveSessionsManagement();
        this.initLogoutAndDeleteAccount();

        const securityTab = document.querySelector('[data-tab="security"]');
        if (securityTab) {
            securityTab.addEventListener('click', () => {
                setTimeout(() => {
                    this.loadSecurityData();
                }, 100);
            });
        }

        if (document.querySelector('#security.active')) {
            this.loadSecurityData();
        }
    }

    initSecurityPasswordChange() {
        const updateButton = document.querySelector('#security .btn-save');

        if (!updateButton) {
            console.warn('Security form elements not found');
            return;
        }

        updateButton.addEventListener('click', (e) => {
            e.preventDefault();
            this.handleSecurityUpdate();
        });

        const newPasswordInput = document.getElementById('newPassword');
        const confirmPasswordInput = document.getElementById('confirmNewPassword');

        if (newPasswordInput && confirmPasswordInput) {
            const validatePasswords = () => {
                if (newPasswordInput.value && confirmPasswordInput.value) {
                    if (newPasswordInput.value !== confirmPasswordInput.value) {
                        this.showFieldError(confirmPasswordInput, 'Passwords do not match');
                    } else {
                        this.clearFieldError(confirmPasswordInput);
                    }
                }
            };

            newPasswordInput.addEventListener('input', validatePasswords);
            confirmPasswordInput.addEventListener('input', validatePasswords);
        }
    }

    handleSecurityUpdate() {
        const updateButton = document.querySelector('#security .btn-save');
        if (!updateButton) return;

        const originalText = updateButton.innerHTML;
        updateButton.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Updating Password...';
        updateButton.disabled = true;

        const formData = new FormData();
        formData.append('CurrentPassword', document.getElementById('currentPassword')?.value || '');
        formData.append('NewPassword', document.getElementById('newPassword')?.value || '');
        formData.append('ConfirmNewPassword', document.getElementById('confirmNewPassword')?.value || '');

        const token = document.querySelector('#security input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }

        fetch('/Security/UpdateSecurity', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                updateButton.innerHTML = originalText;
                updateButton.disabled = false;

                if (data.success) {
                    this.showNotification(data.message, 'success');
                    document.getElementById('currentPassword').value = '';
                    document.getElementById('newPassword').value = '';
                    document.getElementById('confirmNewPassword').value = '';
                } else {
                    this.showNotification(data.message, 'error');
                }
            })
            .catch(error => {
                updateButton.innerHTML = originalText;
                updateButton.disabled = false;
                console.error('Error updating security settings:', error);
                this.showNotification('An error occurred while updating security settings.', 'error');
            });
    }

    initActiveSessionsManagement() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-danger-small')) {
                e.preventDefault();
                const button = e.target.closest('.btn-danger-small');
                const sessionItem = button.closest('.session-item');
                const sessionInfo = sessionItem?.querySelector('.session-info span')?.textContent;

                if (confirm(`End session for ${sessionInfo}?`)) {
                    this.handleEndSession(button, sessionItem);
                }
            }
        });
    }

    handleEndSession(button, sessionItem) {
        const originalContent = button.innerHTML;
        button.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i>';
        button.disabled = true;

        const sessionId = button.dataset.sessionId;

        if (!sessionId) {
            this.showNotification('Unable to end session - session ID missing', 'error');
            button.innerHTML = originalContent;
            button.disabled = false;
            return;
        }

        const formData = new FormData();
        formData.append('sessionId', sessionId);

        const token = document.querySelector('#security input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }

        fetch('/Security/EndSession', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    sessionItem.style.opacity = '0';
                    setTimeout(() => {
                        if (sessionItem.parentNode) {
                            sessionItem.parentNode.removeChild(sessionItem);
                        }
                    }, 300);
                    this.showNotification(data.message, 'success');
                } else {
                    button.innerHTML = originalContent;
                    button.disabled = false;
                    this.showNotification(data.message, 'error');
                }
            })
            .catch(error => {
                button.innerHTML = originalContent;
                button.disabled = false;
                console.error('Error ending session:', error);
                this.showNotification('An error occurred while ending the session.', 'error');
            });
    }

    loadSecurityData() {
        console.log('Loading security data...');

        Promise.all([
            this.loadActiveSessions(),
            this.loadSecurityHistory()
        ]).then(() => {
            console.log('Security data loaded successfully');
        }).catch(error => {
            console.error('Error loading security data:', error);
        });
    }

    loadActiveSessions() {
        return fetch('/Security/GetActiveSessions')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    this.displayActiveSessions(data.sessions);
                }
            });
    }

    displayActiveSessions(sessions) {
        let activeSessionsSection = null;
        const sections = document.querySelectorAll('#security .security-section');
        sections.forEach(section => {
            const heading = section.querySelector('h4');
            if (heading && heading.textContent.includes('Active Sessions')) {
                activeSessionsSection = section;
            }
        });

        if (!activeSessionsSection) return;

        const existingItems = activeSessionsSection.querySelectorAll('.session-item');
        existingItems.forEach(item => item.remove());

        sessions.forEach(session => {
            const sessionItem = document.createElement('div');
            sessionItem.className = 'session-item';

            const lastActivity = session.isCurrentSession ? 'Active now' : this.formatRelativeTime(session.lastActivity);

            sessionItem.innerHTML = `
                <div class="session-info">
                    <span>${session.deviceInfo}</span>
                    <small>${session.location} • ${lastActivity}</small>
                </div>
                ${session.isCurrentSession ?
                    '<span class="session-status active">Current</span>' :
                    `<button type="button" class="btn-danger-small" data-session-id="${session.sessionId}" aria-label="End session">
                        <i class="bx bx-x" aria-hidden="true"></i>
                        End Session
                    </button>`
                }
            `;

            activeSessionsSection.appendChild(sessionItem);
        });
    }

    loadSecurityHistory() {
        return fetch('/Security/GetSecurityHistory')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    this.displaySecurityHistory(data.events);
                }
            });
    }

    displaySecurityHistory(events) {
        const historyContainer = document.querySelector('#security .security-section:last-child > div');
        if (!historyContainer) return;

        historyContainer.innerHTML = '';

        events.forEach((event, index) => {
            const eventItem = document.createElement('div');
            eventItem.style.cssText = `
                display: flex; 
                align-items: center; 
                gap: 16px; 
                padding: 16px 0; 
                ${index < events.length - 1 ? 'border-bottom: 1px solid var(--gray-200);' : ''}
            `;

            const iconClass = event.success ? 'bx bx-check' : 'bx bx-x';
            const iconColor = event.success ? 'var(--success)' : 'var(--error)';
            const eventTitle = event.success ? 'Successful login' : 'Failed login attempt';

            eventItem.innerHTML = `
                <div style="width: 40px; height: 40px; background-color: ${iconColor}; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white;">
                    <i class="${iconClass}" style="font-size: 20px;"></i>
                </div>
                <div style="flex: 1;">
                    <div style="font-weight: 600; color: var(--gray-800);">${eventTitle}</div>
                    <small style="color: var(--gray-600);">${event.userAgent} • ${event.location} • ${this.formatRelativeTime(event.loginTime)}</small>
                </div>
            `;

            historyContainer.appendChild(eventItem);
        });
    }

    formatRelativeTime(timestamp) {
        const now = new Date();
        const time = new Date(timestamp);
        const diffInSeconds = Math.floor((now - time) / 1000);

        if (diffInSeconds < 60) return 'Just now';
        if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} minutes ago`;
        if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
        return `${Math.floor(diffInSeconds / 86400)} days ago`;
    }

    initLogoutAndDeleteAccount() {
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
    }

    // NOTIFICATIONS
    initNotifications() {
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
        const existingNotification = document.querySelector('.notification');
        if (existingNotification) {
            existingNotification.remove();
        }

        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;

        notification.addEventListener('click', () => {
            notification.remove();
        });

        if (this.isMobile) {
            notification.style.top = '90px';
            notification.style.left = '16px';
            notification.style.right = '16px';
            notification.style.maxWidth = 'none';
        }

        document.body.appendChild(notification);

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

    initNotificationSettings() {
        console.log('Initializing notification settings...');

        // Check browser support and permission status
        this.checkDesktopNotificationPermission();

        // Initialize desktop notification toggle handler
        this.initDesktopNotificationToggle();

        // Initialize test notification button
        this.initTestNotificationButton();

        // Initialize save button (existing functionality)
        const notificationSaveButton = document.querySelector('#notifications .btn-save');
        if (notificationSaveButton) {
            notificationSaveButton.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleNotificationSave();
            });
        }

        // Initialize switch change feedback (existing functionality)
        const notificationSwitches = document.querySelectorAll('#notifications .switch input[type="checkbox"]');
        notificationSwitches.forEach(switchEl => {
            switchEl.addEventListener('change', () => {
                const settingName = this.getNotificationSettingName(switchEl.name);
                this.showNotification(`${settingName} ${switchEl.checked ? 'enabled' : 'disabled'}`, 'info');
            });
        });
    }

    handleNotificationSave() {
        const notificationSaveButton = document.querySelector('#notifications .btn-save');
        if (!notificationSaveButton) return;

        const originalText = notificationSaveButton.innerHTML;
        notificationSaveButton.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving Notification Settings...';
        notificationSaveButton.disabled = true;

        const formData = new FormData();

        const emailNewMessages = document.querySelector('input[name="EmailNewMessages"]')?.checked || false;
        const emailItemUpdates = document.querySelector('input[name="EmailItemUpdates"]')?.checked || false;
        const emailWeeklySummary = document.querySelector('input[name="EmailWeeklySummary"]')?.checked || false;
        const desktopNotifications = document.querySelector('input[name="DesktopNotifications"]')?.checked || false;

        formData.append('EmailNewMessages', emailNewMessages);
        formData.append('EmailItemUpdates', emailItemUpdates);
        formData.append('EmailWeeklySummary', emailWeeklySummary);
        formData.append('DesktopNotifications', desktopNotifications);

        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            formData.append('__RequestVerificationToken', token.value);
        }

        fetch('/NotificationSettings/UpdateNotifications', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                notificationSaveButton.innerHTML = originalText;
                notificationSaveButton.disabled = false;

                if (response.ok) {
                    this.showNotification('Notification settings saved successfully!', 'success');
                    console.log('Notification settings saved successfully');
                } else {
                    throw new Error('Save failed');
                }
            })
            .catch(error => {
                notificationSaveButton.innerHTML = originalText;
                notificationSaveButton.disabled = false;
                console.error('Error saving notification settings:', error);
                this.showNotification('Error saving notification settings. Please try again.', 'error');
            });
    }

    getNotificationSettingName(settingName) {
        const names = {
            'EmailNewMessages': 'Email notifications for new messages',
            'EmailItemUpdates': 'Email notifications for item updates',
            'EmailWeeklySummary': 'Weekly summary emails',
            'DesktopNotifications': 'Desktop notifications'
        };
        return names[settingName] || settingName;
    }
    checkDesktopNotificationPermission() {
        if (!('Notification' in window)) {
            const statusElement = document.getElementById('desktop-notification-status');
            const desktopToggle = document.querySelector('input[name="DesktopNotifications"]');

            if (statusElement) {
                statusElement.textContent = 'Desktop notifications are not supported in this browser';
                statusElement.style.color = '#dc3545';
            }

            if (desktopToggle) {
                desktopToggle.disabled = true;
            }
            return;
        }

        this.updateNotificationStatus(Notification.permission);
    }

    updateNotificationStatus(permission) {
        const statusElement = document.getElementById('desktop-notification-status');
        if (!statusElement) return;

        const statusMessages = {
            'granted': 'Desktop notifications enabled ✓',
            'denied': 'Notifications blocked. Click the lock icon in your address bar to enable.',
            'default': 'Click to enable desktop notifications'
        };

        const statusColors = {
            'granted': '#28a745',
            'denied': '#dc3545',
            'default': '#6c757d'
        };

        statusElement.textContent = statusMessages[permission] || statusMessages.default;
        statusElement.style.color = statusColors[permission] || statusColors.default;

        this.updateTestButtonVisibility();
    }

    initDesktopNotificationToggle() {
        const desktopToggle = document.querySelector('input[name="DesktopNotifications"]');
        if (!desktopToggle) return;

        desktopToggle.addEventListener('change', () => {
            if (desktopToggle.checked) {
                this.requestNotificationPermission();
            } else {
                this.updateTestButtonVisibility();
            }
        });
    }

    requestNotificationPermission() {
        if (!('Notification' in window)) {
            this.showNotification('Desktop notifications are not supported in this browser', 'error');
            return;
        }

        if (Notification.permission === 'default') {
            Notification.requestPermission().then((permission) => {
                this.updateNotificationStatus(permission);

                if (permission === 'granted') {
                    this.showWelcomeNotification();
                } else if (permission === 'denied') {
                    const toggle = document.querySelector('input[name="DesktopNotifications"]');
                    if (toggle) toggle.checked = false;
                    this.showNotification('Please enable notifications in your browser settings', 'error');
                }
            });
        } else if (Notification.permission === 'denied') {
            const toggle = document.querySelector('input[name="DesktopNotifications"]');
            if (toggle) toggle.checked = false;
            this.showNotification('Notifications are blocked. Please enable them in your browser settings.', 'error');
        }
    }

    initTestNotificationButton() {
        const testBtn = document.getElementById('test-notification-btn');
        if (!testBtn) return;

        testBtn.addEventListener('click', (e) => {
            e.preventDefault();
            this.showTestNotification();
        });

        this.updateTestButtonVisibility();
    }

    showTestNotification() {
        if (Notification.permission === 'granted') {
            const notification = new Notification('TidyUp Test Notification', {
                body: 'Desktop notifications are working correctly!',
                icon: '/favicon.ico',
                tag: 'tidyup-test',
                requireInteraction: false
            });

            notification.onclick = () => {
                window.focus();
                notification.close();
            };

            // Auto close after 5 seconds
            setTimeout(() => {
                notification.close();
            }, 5000);

            this.showNotification('Test notification sent!', 'success');
        } else if (Notification.permission === 'denied') {
            this.showNotification('Notifications are blocked. Please enable them in your browser settings.', 'error');
        } else {
            this.requestNotificationPermission();
        }
    }

    showWelcomeNotification() {
        const notification = new Notification('TidyUp Notifications Enabled', {
            body: 'You\'ll now receive desktop notifications for important updates!',
            icon: '/favicon.ico',
            tag: 'tidyup-welcome'
        });

        notification.onclick = () => {
            window.focus();
            notification.close();
        };

        setTimeout(() => {
            notification.close();
        }, 4000);
    }

    updateTestButtonVisibility() {
        const testSection = document.getElementById('test-notification-section');
        const desktopToggle = document.querySelector('input[name="DesktopNotifications"]');

        if (testSection && desktopToggle) {
            if (desktopToggle.checked && Notification.permission === 'granted') {
                testSection.style.display = 'flex';
            } else {
                testSection.style.display = 'none';
            }
        }
    }

    // Global function for other parts of your app to use
    showAppNotification(title, body, tag = null) {
        const desktopToggle = document.querySelector('input[name="DesktopNotifications"]');

        if (Notification.permission === 'granted' && desktopToggle?.checked) {
            const notification = new Notification(title, {
                body: body,
                icon: '/favicon.ico',
                tag: tag || 'tidyup-' + Date.now()
            });

            notification.onclick = () => {
                window.focus();
                notification.close();
            };

            setTimeout(() => {
                notification.close();
            }, 5000);
        }
    }

    // PRIVACY SETTINGS
    initPrivacySettings() {
        console.log('Initializing privacy settings...');

        this.initPrivacyToggleButtons();
        this.initPrivacyFormSubmission();

        const privacyTab = document.querySelector('[data-tab="privacy"]');
        if (privacyTab) {
            privacyTab.addEventListener('click', () => {
                setTimeout(() => this.loadPrivacySettings(), 100);
            });
        }

        if (document.querySelector('#privacy.active')) {
            this.loadPrivacySettings();
        }

        console.log('Privacy settings initialized');
    }

    initPrivacyToggleButtons() {
        const privacyToggles = document.querySelectorAll('#privacy .toggle-buttons');

        privacyToggles.forEach(group => {
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
                        console.log(`Updated ${hiddenInput.name} to: ${value}`);
                    }

                    if (hiddenInput?.name) {
                        const settingName = this.getSettingDisplayName(hiddenInput.name);
                        this.showNotification(`${settingName} updated to ${value}`, 'info');
                    }
                });
            });
        });
    }

    initPrivacyFormSubmission() {
        const privacySaveButton = document.querySelector('#privacy .btn-save');

        if (privacySaveButton) {
            privacySaveButton.addEventListener('click', (e) => {
                e.preventDefault();
                this.handlePrivacySave();
            });
        }
    }

    handlePrivacySave() {
        const privacySaveButton = document.querySelector('#privacy .btn-save');
        if (!privacySaveButton) return;

        const originalText = privacySaveButton.innerHTML;
        privacySaveButton.innerHTML = '<i class="bx bx-loader-alt bx-spin"></i> Saving Privacy Settings...';
        privacySaveButton.disabled = true;

        const formData = new FormData();
        const hiddenInputs = document.querySelectorAll('#privacy input[type="hidden"]');

        hiddenInputs.forEach(input => {
            if (input.name && input.name !== '__RequestVerificationToken') {
                formData.append(input.name, input.value);
                console.log(`Form data: ${input.name} = ${input.value}`);
            }
        });

        const antiForgeryToken = document.querySelector('#privacy input[name="__RequestVerificationToken"]');
        if (antiForgeryToken) {
            formData.append('__RequestVerificationToken', antiForgeryToken.value);
        }

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
                privacySaveButton.innerHTML = originalText;
                privacySaveButton.disabled = false;
                this.showNotification('Privacy settings saved successfully!', 'success');
                console.log('Privacy settings saved successfully');
            })
            .catch(error => {
                privacySaveButton.innerHTML = originalText;
                privacySaveButton.disabled = false;
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

                Object.keys(data).forEach(key => {
                    const inputName = this.mapApiKeyToInputName(key);
                    const input = document.querySelector(`#privacy input[name="${inputName}"]`);

                    if (input) {
                        input.value = data[key];

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

    // LANGUAGE SETTINGS
    initLanguageSettings() {
        console.log('Initializing language & accessibility settings...');

        const languageSaveButton = document.querySelector('#language .btn-save');
        if (languageSaveButton) {
            languageSaveButton.addEventListener('click', (e) => {
                e.preventDefault();
                this.handleLanguageSave();
            });
        }

        this.loadLanguageSettings();
        this.languageSettingsLoaded = true;
        this.initAccessibilitySwitches();
        this.loadAndApplyCurrentLanguage();
    }

    async loadAndApplyCurrentLanguage() {
        try {
            const response = await fetch('/Settings/GetLanguageSettings');
            const data = await response.json();

            if (data.success && data.data.language) {
                await this.applyLanguageImmediately(data.data.language);
                console.log(`Applied saved language on page load: ${data.data.language}`);
            }
        } catch (error) {
            console.error('Error loading current language:', error);
        }
    }

    handleLanguageSave() {
        const languageSaveButton = document.querySelector('#language .btn-save');
        if (!languageSaveButton) return;

        const originalText = languageSaveButton.innerHTML;
        languageSaveButton.innerHTML = '<i class="bx bx-loader-alt bx-spin" aria-hidden="true"></i> <span data-translate="saving">Saving...</span>';
        languageSaveButton.disabled = true;

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

        fetch('/Settings/UpdateLanguage', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                languageSaveButton.innerHTML = originalText;
                languageSaveButton.disabled = false;

                if (data.success) {
                    this.applyLanguageChangesImmediately(language, timezone, {
                        highContrast, largeText, reduceMotion, screenReader
                    });

                    if (window.globalAccessibility) {
                        window.globalAccessibility.refresh();
                    }

                    this.showNotification(this.getTranslation('settings_saved') || data.message, 'success');
                } else {
                    this.showNotification(this.getTranslation('error_saving') || data.message, 'error');
                }
            })
            .catch(error => {
                languageSaveButton.innerHTML = originalText;
                languageSaveButton.disabled = false;
                console.error('Error:', error);
                this.showNotification(this.getTranslation('error_saving') || 'An error occurred while saving settings. Please try again.', 'error');
            });
    }

    loadLanguageSettings() {
        fetch('/Settings/GetLanguageSettings')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const languageSelect = document.querySelector('select[name="Language"]');
                    const timezoneSelect = document.querySelector('select[name="Timezone"]');
                    const highContrastCheckbox = document.querySelector('input[name="HighContrast"]');
                    const largeTextCheckbox = document.querySelector('input[name="LargeText"]');
                    const reduceMotionCheckbox = document.querySelector('input[name="ReduceMotion"]');
                    const screenReaderCheckbox = document.querySelector('input[name="ScreenReader"]');

                    if (languageSelect) languageSelect.value = data.data.language || 'en';
                    if (timezoneSelect) timezoneSelect.value = data.data.timezone || 'Asia/Manila';
                    if (highContrastCheckbox) highContrastCheckbox.checked = data.data.highContrast || false;
                    if (largeTextCheckbox) largeTextCheckbox.checked = data.data.largeText || false;
                    if (reduceMotionCheckbox) reduceMotionCheckbox.checked = data.data.reduceMotion || false;
                    if (screenReaderCheckbox) screenReaderCheckbox.checked = data.data.screenReader || false;

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

    initAccessibilitySwitches() {
        const accessibilitySwitches = document.querySelectorAll('#language .switch input[type="checkbox"]');

        accessibilitySwitches.forEach(switchEl => {
            switchEl.addEventListener('change', () => {
                const setting = switchEl.name;
                const enabled = switchEl.checked;

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

                const settingName = this.getAccessibilitySettingName(setting);
                this.showNotification(`${settingName} ${enabled ? 'enabled' : 'disabled'}`, 'info');
            });
        });
    }

    applyAccessibilitySettings(highContrast, largeText, reduceMotion) {
        this.toggleHighContrast(highContrast);
        this.toggleLargeText(largeText);
        this.toggleReduceMotion(reduceMotion);
    }

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
        const body = document.body;
        if (enabled) {
            body.classList.add('screen-reader-mode');
            this.enhanceScreenReaderSupport();
        } else {
            body.classList.remove('screen-reader-mode');
        }
    }

    enhanceScreenReaderSupport() {
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

    // MODAL HANDLING
    initModalHandling() {
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('modal-overlay')) {
                // Close any open modals
            }
        });
    }

    // ACCESSIBILITY
    initAccessibility() {
        document.addEventListener('keydown', (e) => {
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