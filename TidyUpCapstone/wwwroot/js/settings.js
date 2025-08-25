console.log('Settings JavaScript loading...');

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
        console.log('Initializing Settings Manager...');

        // Wait for DOM and jQuery to be ready
        $(document).ready(() => {
            this.initializeComponents();
        });
    }

    initializeComponents() {
        // Initialize components in correct order
        this.initTabSwitching();           // Fix tabs first
        this.initResponsiveLayout();
        this.initMobileSidebar();
        this.initProfileFeatures();
        this.initFormHandling();
        this.initNotifications();
        this.initAccessibility();
        this.initResizeHandler();

        this.initialized = true;
        console.log('Settings Manager initialized successfully');
    }

    // ==========================================
    // FIXED TAB SWITCHING - Core functionality
    // ==========================================

    initTabSwitching() {
        console.log('Initializing tab switching...');

        // Simple, working tab switching
        $('.tab-link').on('click', (e) => {
            e.preventDefault();
            const targetTab = $(e.currentTarget).data('tab');
            if (targetTab) {
                this.showTab(targetTab);
            }
        });

        // Show initial tab
        this.showTab('profile');

        console.log('Tab switching initialized');
    }

    showTab(targetTab) {
        console.log(`Switching to tab: ${targetTab}`);

        // Hide all tabs - simple and effective
        $('.tab-content').removeClass('active').hide();
        $('.tab-link').removeClass('active').attr('aria-selected', 'false');

        // Show target tab
        $(`#${targetTab}`).addClass('active').show();
        $(`.tab-link[data-tab="${targetTab}"]`).addClass('active').attr('aria-selected', 'true');

        // Update header
        const tabName = this.tabNames[targetTab] || targetTab;
        $('#current-tab-name').text(tabName);

        // Update state
        this.currentTab = targetTab;

        // Close mobile sidebar if open
        if (this.isMobile && this.sidebarOpen) {
            this.closeMobileSidebar();
        }

        console.log(`Successfully switched to ${targetTab} tab`);
    }

    // ==========================================
    // RESPONSIVE LAYOUT MANAGEMENT
    // ==========================================

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
        const sidebar = $('#settings-sidebar');
        const toggleBtn = $('#mobile-menu-toggle');

        sidebar.addClass('mobile-responsive');
        toggleBtn.show();
        this.closeMobileSidebar();
    }

    setupDesktopLayout() {
        const sidebar = $('#settings-sidebar');
        const toggleBtn = $('#mobile-menu-toggle');

        sidebar.removeClass('mobile-responsive mobile-open');
        toggleBtn.hide();
        this.sidebarOpen = false;
    }

    adjustContainerLayout() {
        const container = $('.settings-container');
        const sidebar = $('#sidebar');

        if (!container.length) return;

        if (this.isMobile) {
            container.css({
                'margin-left': '0',
                'width': '100%'
            });
        } else {
            if (sidebar.length && sidebar.hasClass('close')) {
                container.css({
                    'margin-left': '82px',
                    'width': 'calc(100% - 82px)'
                });
            } else {
                container.css({
                    'margin-left': '250px',
                    'width': 'calc(100% - 250px)'
                });
            }
        }
    }

    adjustFormLayout() {
        const formRows = $('.form-row-top, .form-row-bottom');

        formRows.each(function () {
            const row = $(this);
            if (window.innerWidth <= 480) {
                row.css('display', 'block');
            } else if (window.innerWidth <= 768) {
                row.css({
                    'display': 'grid',
                    'grid-template-columns': '1fr'
                });
            } else {
                row.css({
                    'display': 'grid',
                    'grid-template-columns': 'repeat(auto-fit, minmax(250px, 1fr))'
                });
            }
        });
    }

    adjustProfileLayout() {
        const profileSection = $('.profile-avatar-section');
        const accountDetails = $('.account-details-section');

        if (window.innerWidth <= 768) {
            profileSection.css({
                'flex-direction': 'column',
                'text-align': 'center'
            });
            accountDetails.css('display', 'block');
        } else {
            profileSection.css({
                'flex-direction': 'row',
                'text-align': 'left'
            });
            accountDetails.css('display', 'grid');
        }
    }

    // ==========================================
    // MOBILE SIDEBAR FUNCTIONALITY
    // ==========================================

    initMobileSidebar() {
        console.log('Initializing mobile sidebar...');

        // Toggle button click
        $('#mobile-menu-toggle').on('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.toggleMobileSidebar();
        });

        // Close button click
        $('#sidebar-close-btn').on('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.closeMobileSidebar();
        });

        // Overlay click
        $('#sidebar-overlay').on('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.closeMobileSidebar();
        });

        // Escape key to close sidebar
        $(document).on('keydown', (e) => {
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

        $('#settings-sidebar').addClass('mobile-open');
        $('#sidebar-overlay').addClass('active');
        $('#mobile-menu-toggle').html('<i class="bx bx-x"></i>').attr('aria-label', 'Close settings menu');
        $('body').addClass('sidebar-mobile-open');

        this.sidebarOpen = true;

        // Focus first tab link for accessibility
        setTimeout(() => {
            $('#settings-sidebar .tab-link').first().focus();
        }, 100);
    }

    closeMobileSidebar() {
        console.log('Closing mobile sidebar...');

        $('#settings-sidebar').removeClass('mobile-open');
        $('#sidebar-overlay').removeClass('active');
        $('#mobile-menu-toggle').html('<i class="bx bx-menu"></i>').attr('aria-label', 'Open settings menu');
        $('body').removeClass('sidebar-mobile-open');

        this.sidebarOpen = false;
    }

    // ==========================================
    // RESIZE HANDLER
    // ==========================================

    initResizeHandler() {
        let resizeTimer;

        $(window).on('resize', () => {
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

        // Close mobile sidebar if switching to desktop
        if (!this.isMobile && this.sidebarOpen) {
            this.closeMobileSidebar();
        }
    }

    // ==========================================
    // PROFILE FEATURES
    // ==========================================

    initProfileFeatures() {
        console.log('Initializing profile features...');

        this.initProfilePictureUpload();
        this.initPasswordToggle();
        this.initConnectedAccounts();
        this.initPhoneVerification();
        this.initPhoneFormatting();
        this.initFormEditing();
        this.initSaveButtonClick();
    }

    initProfilePictureUpload() {
        const profileAvatar = $('#profile-avatar-upload');
        const fileInput = $('#profile-picture-input');

        if (!profileAvatar.length || !fileInput.length) return;

        profileAvatar.on('click', () => {
            fileInput.click();
        });

        fileInput.on('change', (e) => {
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
                    profileAvatar.css({
                        'background-image': `url(${e.target.result})`,
                        'background-size': 'cover',
                        'background-position': 'center'
                    }).html('');
                    this.showNotification('Profile picture selected! Click Save Changes to upload.', 'success');
                };
                reader.readAsDataURL(file);
            }
        });
    }

    initPasswordToggle() {
        $('#toggle-password').on('click', () => {
            const passwordInput = $('#password');
            const isPassword = passwordInput.attr('type') === 'password';

            passwordInput.attr('type', isPassword ? 'text' : 'password');
            $('#toggle-password').html(isPassword ? '<i class="bx bx-show"></i>' : '<i class="bx bx-hide"></i>');
        });
    }

    initConnectedAccounts() {
        $('.btn-connect').on('click', function () {
            const button = $(this);
            const accountItem = button.closest('.account-connection-item');
            const accountName = accountItem.find('.account-info span').text();

            if (button.hasClass('connected')) {
                const confirmDisconnect = confirm(`Are you sure you want to disconnect ${accountName}?`);
                if (confirmDisconnect) {
                    button.html('<i class="bx bx-plus"></i>').removeClass('connected');
                    settingsManager.showNotification(`${accountName} disconnected`, 'info');
                }
                return;
            }

            button.html('<i class="bx bx-loader-alt bx-spin"></i>').prop('disabled', true);

            setTimeout(() => {
                button.html('<i class="bx bx-check"></i>').addClass('connected').prop('disabled', false);
                settingsManager.showNotification(`${accountName} connected successfully!`, 'success');
            }, 1500);
        });
    }

    initFormEditing() {
        const formInputs = $('.profile-form input, .profile-form select');
        const saveButton = $('.btn-save-profile');
        let hasChanges = false;

        formInputs.each(function () {
            const input = $(this);
            const originalValue = input.val();

            input.on('input change', () => {
                hasChanges = input.val() !== originalValue;
                settingsManager.updateSaveButtonState(saveButton, hasChanges);
            });

            input.on('focus', () => {
                input.parent().addClass('focused');
            });

            input.on('blur', () => {
                input.parent().removeClass('focused');
            });
        });
    }

    updateSaveButtonState(button, hasChanges) {
        if (!button.length) return;

        if (hasChanges) {
            button.css({
                'opacity': '1',
                'transform': 'scale(1.02)'
            }).prop('disabled', false);
        } else {
            button.css({
                'opacity': '0.8',
                'transform': 'scale(1)'
            });
        }
    }

    initSaveButtonClick() {
        $('.btn-save-profile').on('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.handleProfileSave($(e.currentTarget));
        });
    }

    handleProfileSave(button) {
        if (!button.length) return;

        const originalText = button.html();
        button.html('<i class="bx bx-loader-alt bx-spin"></i> Saving...').prop('disabled', true);

        // Create FormData to handle both regular fields and file upload
        const formData = new FormData();

        // Add regular form fields
        formData.append('FirstName', $('#firstName').val() || '');
        formData.append('LastName', $('#lastName').val() || '');
        formData.append('Phone', $('#phone').val() || '');
        formData.append('Email', $('#email').val() || '');
        formData.append('Location', $('#location').val() || '');
        formData.append('Username', $('#username').val() || '');
        formData.append('Gender', $('#gender').val() || '');
        formData.append('Birthday', $('#birthday').val() || '');

        // Add profile picture file if selected
        const fileInput = $('#profile-picture-input')[0];
        if (fileInput && fileInput.files[0]) {
            formData.append('ProfilePicture', fileInput.files[0]);
        }

        // Add anti-forgery token
        const token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Submit to controller
        fetch('/Settings/UpdateProfile', {
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
                button.html(originalText).prop('disabled', false);
                this.showNotification('Profile updated successfully!', 'success');

                // Reload the page to show updated data
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            })
            .catch(error => {
                button.html(originalText).prop('disabled', false);
                console.error('Error:', error);
                this.showNotification('Error updating profile. Please try again.', 'error');
            });
    }

    initPhoneFormatting() {
        $('#phone').on('input', (e) => {
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

    initPhoneVerification() {
        $('#verify-phone-btn').on('click', () => {
            this.showNotification('Phone verification feature coming soon!', 'info');
        });
    }

    // ==========================================
    // FORM HANDLING
    // ==========================================

    initFormHandling() {
        console.log('Initializing form handling...');

        this.initToggleButtons();
        this.initFormValidation();
        this.initNotificationSwitches();
        this.initSecurityFeatures();
        this.initPrivacySettings();
    }

    initToggleButtons() {
        $('.toggle-buttons').each(function () {
            const group = $(this);
            const buttons = group.find('.toggle-btn');
            const hiddenInput = group.find('input[type="hidden"]');

            buttons.on('click', function () {
                const btn = $(this);

                buttons.removeClass('active').attr('aria-checked', 'false');
                btn.addClass('active').attr('aria-checked', 'true');

                const value = btn.data('value');
                if (hiddenInput.length && value) {
                    hiddenInput.val(value);
                }

                if (group.closest('#privacy').length) {
                    const settingName = settingsManager.getSettingDisplayName(hiddenInput.attr('name') || '');
                    settingsManager.showNotification(`${settingName} updated to ${value}`, 'info');
                }
            });
        });
    }

    initFormValidation() {
        $('form').each(function () {
            const form = $(this);
            const inputs = form.find('input[required], input[type="email"]');

            inputs.on('blur', function () {
                settingsManager.validateField($(this));
            });

            inputs.on('input', function () {
                settingsManager.clearFieldError($(this));
            });

            form.on('submit', function (e) {
                let isValid = true;
                inputs.each(function () {
                    if (!settingsManager.validateField($(this))) {
                        isValid = false;
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    settingsManager.showNotification('Please fix the errors before submitting', 'error');
                    return;
                }

                const submitBtn = form.find('button[type="submit"]');
                if (submitBtn.length) {
                    const originalText = submitBtn.html();
                    submitBtn.html('<i class="bx bx-loader-alt bx-spin"></i> Saving...').prop('disabled', true);

                    setTimeout(() => {
                        submitBtn.html(originalText).prop('disabled', false);
                        settingsManager.showNotification('Settings saved successfully!', 'success');
                    }, 2000);
                }
            });
        });
    }

    validateField(field) {
        const value = field.val().trim();
        const fieldType = field.attr('type');
        const isRequired = field.is('[required]');

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
        field.addClass('error');

        const existingError = field.parent().find('.error-message');
        existingError.remove();

        const errorDiv = $('<div class="error-message">' + message + '</div>');
        errorDiv.css({
            'color': 'var(--error)',
            'font-size': '12px',
            'margin-top': '4px',
            'font-weight': '500'
        });
        field.parent().append(errorDiv);
    }

    clearFieldError(field) {
        field.removeClass('error');
        field.parent().find('.error-message').remove();
    }

    initNotificationSwitches() {
        $('.switch input[type="checkbox"]').on('change', function () {
            const setting = $(this).attr('name');
            const enabled = $(this).is(':checked');
            console.log(`Notification setting ${setting} changed to ${enabled}`);

            settingsManager.showNotification(`${setting} ${enabled ? 'enabled' : 'disabled'}`, 'info');
        });
    }

    initSecurityFeatures() {
        // Enable 2FA
        $('#enable-2fa').on('click', function () {
            const btn = $(this);
            const originalText = btn.text();
            btn.text('Setting up...').prop('disabled', true);

            setTimeout(() => {
                btn.text('Enabled ✓').removeClass('btn-secondary').addClass('btn-save');
                settingsManager.showNotification('Two-factor authentication enabled successfully!', 'success');
            }, 2000);
        });

        // Logout button
        $('#logout-btn').on('click', function (e) {
            e.preventDefault();

            if (confirm('Are you sure you want to logout?')) {
                const form = $('<form method="POST" action="/Account/Logout"></form>');

                const tokenElement = $('input[name="__RequestVerificationToken"]');
                if (tokenElement.length) {
                    const tokenInput = $('<input type="hidden" name="__RequestVerificationToken">');
                    tokenInput.val(tokenElement.val());
                    form.append(tokenInput);
                }

                const returnInput = $('<input type="hidden" name="returnUrl" value="/">');
                form.append(returnInput);

                $('body').append(form);
                form.submit();
            }
        });

        // Delete account button
        $('#delete-account-btn').on('click', function (e) {
            e.preventDefault();

            if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
                window.location.href = '/Auth/DeleteAccount';
            }
        });

        // End session buttons
        $('.btn-danger-small').on('click', function () {
            const btn = $(this);
            const sessionInfo = btn.closest('.session-item').find('.session-info span').text();

            if (confirm(`End session for ${sessionInfo}?`)) {
                btn.html('<i class="bx bx-loader-alt bx-spin"></i>').prop('disabled', true);

                setTimeout(() => {
                    btn.closest('.session-item').remove();
                    settingsManager.showNotification('Session ended successfully', 'success');
                }, 1000);
            }
        });
    }

    initPrivacySettings() {
        console.log('Initializing privacy settings...');

        // Initialize privacy form submission
        $('#privacy .btn-save').on('click', (e) => {
            e.preventDefault();
            this.handlePrivacySave();
        });

        // Load settings when privacy tab is clicked
        $('.tab-link[data-tab="privacy"]').on('click', () => {
            setTimeout(() => this.loadPrivacySettings(), 100);
        });

        console.log('Privacy settings initialized');
    }

    handlePrivacySave() {
        const saveButton = $('#privacy .btn-save');
        if (!saveButton.length) return;

        const originalText = saveButton.html();
        saveButton.html('<i class="bx bx-loader-alt bx-spin"></i> Saving Privacy Settings...').prop('disabled', true);

        // Collect all privacy form data
        const formData = new FormData();
        const hiddenInputs = $('#privacy input[type="hidden"]');

        hiddenInputs.each(function () {
            const input = $(this);
            if (input.attr('name') && input.attr('name') !== '__RequestVerificationToken') {
                formData.append(input.attr('name'), input.val());
                console.log(`Form data: ${input.attr('name')} = ${input.val()}`);
            }
        });

        // Add anti-forgery token
        const antiForgeryToken = $('#privacy input[name="__RequestVerificationToken"]');
        if (antiForgeryToken.length) {
            formData.append('__RequestVerificationToken', antiForgeryToken.val());
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
                saveButton.html(originalText).prop('disabled', false);
                this.showNotification('Privacy settings saved successfully!', 'success');
                console.log('Privacy settings saved successfully');
            })
            .catch(error => {
                saveButton.html(originalText).prop('disabled', false);
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
                    const input = $(`#privacy input[name="${inputName}"]`);

                    if (input.length) {
                        input.val(data[key]);

                        const group = input.closest('.toggle-buttons');
                        if (group.length) {
                            const buttons = group.find('.toggle-btn');

                            buttons.removeClass('active').attr('aria-checked', 'false');
                            buttons.filter(`[data-value="${data[key]}"]`).addClass('active').attr('aria-checked', 'true');
                        }
                    }
                });
            })
            .catch(error => {
                console.error('Error loading privacy settings:', error);
                this.showNotification('Failed to load privacy settings', 'error');
            });
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

    // ==========================================
    // NOTIFICATIONS
    // ==========================================

    initNotifications() {
        // Auto-hide existing notifications after page load
        setTimeout(() => {
            $('.notification').remove();
        }, 5000);
    }

    showNotification(message, type = 'info') {
        $('.notification').remove();

        const notification = $('<div class="notification notification-' + type + '">' + message + '</div>');

        notification.css({
            'position': 'fixed',
            'top': this.isMobile ? '90px' : '20px',
            'right': '20px',
            'left': this.isMobile ? '20px' : 'auto',
            'background': type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6',
            'color': 'white',
            'padding': '12px 20px',
            'border-radius': '8px',
            'z-index': '9999',
            'box-shadow': '0 4px 12px rgba(0,0,0,0.15)',
            'cursor': 'pointer',
            'font-weight': '500',
            'transition': 'all 0.3s ease'
        });

        $('body').append(notification);

        notification.on('click', () => {
            notification.remove();
        });

        setTimeout(() => {
            notification.fadeOut(() => {
                notification.remove();
            });
        }, 4000);
    }

    // ==========================================
    // ACCESSIBILITY
    // ==========================================

    initAccessibility() {
        // Keyboard shortcuts
        $(document).on('keydown', (e) => {
            // Ctrl/Cmd + S to save
            if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                e.preventDefault();
                const activeForm = $('.tab-content.active form');
                if (activeForm.length) {
                    const saveButton = activeForm.find('.btn-save, .btn-save-profile');
                    if (saveButton.length) {
                        saveButton.click();
                        this.showNotification('Settings saved!', 'success');
                    }
                }
            }

            // Escape to close notifications and mobile sidebar
            if (e.key === 'Escape') {
                $('.notification').remove();

                if (this.isMobile && this.sidebarOpen) {
                    this.closeMobileSidebar();
                }
            }

            // Arrow keys for tab navigation (desktop only)
            if (!this.isMobile && (e.key === 'ArrowRight' || e.key === 'ArrowLeft')) {
                const activeTab = $('.tab-link.active');
                if (activeTab.length && $(document.activeElement).is('.tab-link')) {
                    e.preventDefault();
                    const tabLinks = $('.tab-link');
                    const currentIndex = tabLinks.index(activeTab);
                    let nextIndex;

                    if (e.key === 'ArrowRight') {
                        nextIndex = (currentIndex + 1) % tabLinks.length;
                    } else {
                        nextIndex = (currentIndex - 1 + tabLinks.length) % tabLinks.length;
                    }

                    const nextTab = tabLinks.eq(nextIndex);
                    if (nextTab.length) {
                        nextTab.click().focus();
                    }
                }
            }

            // Mobile menu toggle with keyboard (M key)
            if (this.isMobile && e.key === 'm' && !e.ctrlKey && !e.metaKey && !e.altKey) {
                const target = $(e.target);
                if (!target.is('input, textarea, select')) {
                    e.preventDefault();
                    this.toggleMobileSidebar();
                }
            }
        });

        // High contrast mode toggle
        $('input[name="HighContrast"]').on('change', function () {
            const enabled = $(this).is(':checked');
            $('body').toggleClass('high-contrast', enabled);
            settingsManager.showNotification(`High contrast mode ${enabled ? 'enabled' : 'disabled'}`, 'info');
        });

        // Reduce motion toggle
        $('input[name="ReduceMotion"]').on('change', function () {
            const enabled = $(this).is(':checked');
            $('body').toggleClass('reduce-motion', enabled);
            settingsManager.showNotification(`Motion reduction ${enabled ? 'enabled' : 'disabled'}`, 'info');
        });

        // Large text toggle
        $('input[name="LargeText"]').on('change', function () {
            const enabled = $(this).is(':checked');
            $('body').toggleClass('large-text', enabled);
            settingsManager.showNotification(`Large text ${enabled ? 'enabled' : 'disabled'}`, 'info');
        });

        // Touch gesture support for mobile sidebar
        this.initTouchGestures();
    }

    initTouchGestures() {
        if (window.innerWidth > 768) return;

        let startX = 0;
        let startY = 0;
        let currentX = 0;
        let currentY = 0;
        let isScrolling = false;

        const container = $('.settings-container');
        if (!container.length) return;

        container[0].addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
            isScrolling = false;
        }, { passive: true });

        container[0].addEventListener('touchmove', (e) => {
            if (!startX || !startY) return;

            currentX = e.touches[0].clientX;
            currentY = e.touches[0].clientY;

            const diffX = startX - currentX;
            const diffY = startY - currentY;

            if (Math.abs(diffY) > Math.abs(diffX)) {
                isScrolling = true;
                return;
            }

            if (Math.abs(diffX) > 10 && !isScrolling) {
                e.preventDefault();
            }
        }, { passive: false });

        container[0].addEventListener('touchend', (e) => {
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

    debug() {
        console.log('=== Settings Manager Debug Info ===');
        console.log('Current tab:', this.currentTab);
        console.log('Initialized:', this.initialized);
        console.log('Is mobile:', this.isMobile);
        console.log('Sidebar open:', this.sidebarOpen);
        console.log('Window width:', window.innerWidth);
        console.log('Active tab link:', $('.tab-link.active'));
        console.log('Active tab content:', $('.tab-content.active'));
    }

    forceFix() {
        console.log('Forcing settings display fix...');
        this.adjustContainerLayout();
        this.showTab('profile');
        if (this.sidebarOpen) {
            this.closeMobileSidebar();
        }
        console.log('Display fix applied');
    }

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

    destroy() {
        console.log('Cleaning up Settings Manager...');
        $(window).off('resize');
        $(document).off('keydown');
        this.initialized = false;
        console.log('Settings Manager cleaned up');
    }
}

// Initialize settings manager
let settingsManager;

$(document).ready(() => {
    settingsManager = new ResponsiveSettingsManager();

    // Handle main sidebar state changes if present
    const mainSidebar = $('#sidebar');
    if (mainSidebar.length) {
        const observer = new MutationObserver(() => {
            settingsManager.adjustContainerLayout();
        });
        observer.observe(mainSidebar[0], { attributes: true, attributeFilter: ['class'] });
    }

    // Initial layout adjustment
    setTimeout(() => {
        settingsManager.adjustContainerLayout();
    }, 100);
});

// Export for global access
window.ResponsiveSettingsManager = ResponsiveSettingsManager;
window.showSettingsTab = (tab) => settingsManager?.showTab(tab);
window.debugSettings = () => settingsManager?.debug();
window.fixSettingsDisplay = () => settingsManager?.forceFix();
window.toggleSettingsSidebar = () => settingsManager?.toggleSidebar();
window.openSettingsSidebar = () => settingsManager?.openSidebar();
window.closeSettingsSidebar = () => settingsManager?.closeSidebar();

// Emergency layout fix function
window.emergencyLayoutFix = function () {
    console.log('Applying emergency layout fix...');

    $('.tab-content').removeClass('active').hide();
    $('.tab-link').removeClass('active');

    $('#profile').addClass('active').show();
    $('.tab-link[data-tab="profile"]').addClass('active');

    $('#settings-sidebar').removeClass('mobile-open');
    $('#sidebar-overlay').removeClass('active');
    $('body').removeClass('sidebar-mobile-open');

    console.log('Emergency layout fix applied');
};

// Handle orientation change on mobile devices
$(window).on('orientationchange', () => {
    setTimeout(() => {
        if (settingsManager) {
            settingsManager.handleResize();
            settingsManager.adjustFormLayout();
        }
    }, 200);
});

// Handle visibility change (when app comes back into focus)
$(document).on('visibilitychange', () => {
    if (!document.hidden && settingsManager) {
        setTimeout(() => {
            settingsManager.adjustContainerLayout();
        }, 100);
    }
});

// Auto-fix on page load if there are issues
$(window).on('load', () => {
    setTimeout(() => {
        const profileTab = $('#profile');
        if (profileTab.length && !profileTab.is(':visible')) {
            console.warn('Profile tab not visible, applying emergency fix...');
            window.emergencyLayoutFix();
        }
    }, 1000);
});

console.log('Settings JavaScript loaded successfully');