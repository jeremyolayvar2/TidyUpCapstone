// FIXED openModal.js - Resolves Modal Display Issue & Improves UI Consistency
// ============================================================================
// AI SUGGESTION SYSTEM - Fully Silent Version
// ============================================================================
const aiSuggestion = {
    isAnalyzing: false,
    detectedCategory: null,
    detectedCondition: null,

    initialize() {
        console.log("🤖 Silent AI detection system initialized");
    },

    async analyzeImageForCategory(imageFile) {
        if (this.isAnalyzing) return;

        console.log("🤖 Silent AI analysis starting for:", imageFile.name);
        this.isAnalyzing = true;

        try {
            const [categoryResult, conditionResult] = await Promise.all([
                this.detectCategory(imageFile),
                this.detectCondition(imageFile)
            ]);

            this.applySilentAIResults(categoryResult, conditionResult);

        } catch (error) {
            console.error('AI processing failed:', error);
            console.log('Continuing with manual selection...');
        } finally {
            this.isAnalyzing = false;
        }
    },

    async detectCategory(file) {
        const formData = new FormData();
        formData.append('imageFile', file);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        try {
            const response = await fetch('/api/Vision/AnalyzeImage', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) throw new Error('Vision API failed');

            const result = await response.json();
            return {
                success: result.success,
                categoryId: result.suggestedCategoryId,
                categoryName: this.getCategoryName(result.suggestedCategoryId),
                confidence: result.confidenceScore
            };
        } catch (error) {
            console.warn('Vision API failed:', error);
            return { success: false };
        }
    },

    async detectCondition(file) {
        console.log("Using backend VertexAI integration");

        try {
            const formData = new FormData();
            formData.append('imageFile', file);

            const response = await fetch('/api/AI/DetectCondition', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    return {
                        success: true,
                        conditionId: result.conditionId,
                        conditionName: result.conditionName,
                        confidence: result.confidence
                    };
                }
            }
        } catch (error) {
            console.warn('Condition detection failed:', error);
        }

        return { success: false };
    },

    applySilentAIResults(categoryResult, conditionResult) {
        console.log('AI Results received:', { categoryResult, conditionResult });

        let aiApplied = false;

        if (categoryResult.success && categoryResult.categoryId) {
            this.detectedCategory = categoryResult;
            this.setSilentCategory(categoryResult);
            aiApplied = true;
            console.log(`✅ Category automatically set: ${categoryResult.categoryName} (${(categoryResult.confidence * 100).toFixed(0)}% confidence)`);
        }

        if (conditionResult.success && conditionResult.conditionId) {
            this.detectedCondition = conditionResult;
            this.setSilentCondition(conditionResult);
            aiApplied = true;
            console.log(`✅ Condition automatically set: ${conditionResult.conditionName} (${(conditionResult.confidence * 100).toFixed(0)}% confidence)`);
        }

        this.hideAllAISuggestionElements();

        if (aiApplied) {
            this.lockDropdowns();

            setTimeout(() => {
                if (typeof checkFormValidity === 'function') {
                    checkFormValidity();
                }
                if (typeof updateCreateFinalPrice === 'function') {
                    updateCreateFinalPrice();
                }
            }, 100);
            console.log("✅ AI detection completed - dropdown values set automatically and locked");
        } else {
            console.log("⚠️ AI detection failed - manual selection required");
        }
    },

    hideAllAISuggestionElements() {
        const suggestionSelectors = [
            '.ai-suggestion',
            '.ai-suggestion-container',
            '.suggestion-container',
            '[data-ai-suggestion]'
        ];

        suggestionSelectors.forEach(selector => {
            const elements = document.querySelectorAll(selector);
            elements.forEach(el => {
                if (el && el.style) {
                    el.style.display = 'none';
                }
            });
        });

        const allElements = Array.from(document.querySelectorAll('*'));
        const textBasedElements = allElements.filter(el =>
            el.textContent &&
            (el.textContent.includes('AI Suggestion') ||
                el.textContent.includes('Click to apply this suggestion') ||
                el.textContent.includes('Based on:'))
        );

        textBasedElements.forEach(el => {
            if (el && el.style) {
                el.style.display = 'none';
            }
        });

        console.log("🔇 AI suggestion UI elements hidden");
    },

    lockDropdowns() {
        const categorySelect = document.getElementById('categoryInput');
        const conditionSelect = document.getElementById('conditionInput');

        if (categorySelect && this.detectedCategory) {
            categorySelect.disabled = true;
            categorySelect.style.background = '#f0f8ff';
            categorySelect.style.cursor = 'not-allowed';
            this.addAIDetectedLabel(categorySelect, 'Category detected by AI');
        }

        if (conditionSelect && this.detectedCondition) {
            conditionSelect.disabled = true;
            conditionSelect.style.background = '#f0f8ff';
            conditionSelect.style.cursor = 'not-allowed';
            this.addAIDetectedLabel(conditionSelect, 'Condition detected by AI');
        }

        console.log("🔒 Dropdowns locked after AI detection");
    },

    addAIDetectedLabel(element, text) {
        const existingLabel = element.parentNode.querySelector('.ai-detected-label');
        if (existingLabel) return;

        const label = document.createElement('div');
        label.className = 'ai-detected-label';
        label.textContent = text;
        label.style.cssText = `
            font-size: 12px;
            color: #1976d2;
            margin-top: 4px;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 4px;
        `;

        const icon = document.createElement('span');
        icon.innerHTML = '🤖';
        icon.style.fontSize = '14px';
        label.insertBefore(icon, label.firstChild);

        element.parentNode.insertBefore(label, element.nextSibling);
    },

    unlockDropdowns() {
        const categorySelect = document.getElementById('categoryInput');
        const conditionSelect = document.getElementById('conditionInput');

        [categorySelect, conditionSelect].forEach(select => {
            if (select) {
                select.disabled = false;
                select.style.background = '';
                select.style.cursor = '';
            }
        });

        document.querySelectorAll('.ai-detected-label').forEach(label => {
            label.remove();
        });

        console.log("🔓 Dropdowns unlocked");
    },

    setSilentCategory(result) {
        const categorySelect = document.getElementById('categoryInput');
        if (categorySelect && result.categoryId) {
            categorySelect.value = result.categoryId;

            const changeEvent = new Event('change', { bubbles: true });
            categorySelect.dispatchEvent(changeEvent);

            const aiCategoryId = document.getElementById('aiCategoryId');
            if (aiCategoryId) {
                aiCategoryId.value = result.categoryId;
            }

            console.log(`📝 Category dropdown set to: ${result.categoryName} (ID: ${result.categoryId})`);
        }
    },

    setSilentCondition(result) {
        const conditionSelect = document.getElementById('conditionInput');
        if (conditionSelect && result.conditionId) {
            conditionSelect.value = result.conditionId;

            const changeEvent = new Event('change', { bubbles: true });
            conditionSelect.dispatchEvent(changeEvent);

            const aiConditionId = document.getElementById('aiConditionId');
            const aiConfidenceScore = document.getElementById('aiConfidenceScore');

            if (aiConditionId) {
                aiConditionId.value = result.conditionId;
            }
            if (aiConfidenceScore) {
                aiConfidenceScore.value = result.confidence;
            }

            console.log(`📝 Condition dropdown set to: ${result.conditionName} (ID: ${result.conditionId})`);
        }
    },

    getCategoryName(categoryId) {
        const categories = {
            1: 'Books & Stationery',
            2: 'Electronics & Gadgets',
            3: 'Toys & Games',
            4: 'Home & Kitchen',
            5: 'Furniture',
            6: 'Appliances',
            7: 'Health & Beauty',
            8: 'Crafts & DIY',
            9: 'School & Office',
            10: 'Sentimental Items',
            11: 'Miscellaneous',
            12: 'Clothing'
        };
        return categories[categoryId] || 'Unknown';
    },

    getConditionName(conditionId) {
        const conditions = {
            1: 'Excellent',
            3: 'Good',
            4: 'Fair'
        };
        return conditions[conditionId] || 'Unknown';
    },

    reset() {
        this.isAnalyzing = false;
        this.detectedCategory = null;
        this.detectedCondition = null;

        this.unlockDropdowns();

        const aiFields = ['aiCategoryId', 'aiConditionId', 'aiConfidenceScore'];
        aiFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) field.value = '';
        });

        console.log("🤖 AI detection reset");
    }
};

// ============================================================================
// MODAL INITIALIZATION - FIXED VERSION
// ============================================================================
document.addEventListener("DOMContentLoaded", function () {
    console.log("📝 Modal system initializing...");

    const modal = document.getElementById("createPostModal");
    if (!modal) {
        console.warn("Create post modal not found");
        return;
    }

    // Initialize AI system
    aiSuggestion.initialize();

    // FIXED: Enhanced modal opening functionality
    function openCreateModal(event) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        console.log("📝 Opening create modal...");

        // CRITICAL FIX: Use display: flex instead of block for proper centering
        modal.style.display = "flex";

        // Add show class with slight delay for animation
        setTimeout(() => {
            modal.classList.add("show");
        }, 10);

        // Prevent body scroll on mobile
        document.body.style.overflow = 'hidden';

        // Focus management for accessibility
        const firstInput = modal.querySelector('input, textarea, select');
        if (firstInput) {
            setTimeout(() => firstInput.focus(), 300);
        }

        console.log("✅ Modal opened successfully");
    }

    function closeCreateModal() {
        console.log("📝 Closing create modal...");

        modal.classList.remove("show");

        setTimeout(() => {
            modal.style.display = "none";
            document.body.style.overflow = '';
            resetCreateForm();
        }, 300);

        console.log("✅ Modal closed");
    }

    // FIXED: Comprehensive modal trigger handling
    const modalTriggers = [
        "#openCreateModal",
        ".floating-add-btn",
        "a#openCreateModal",
        ".open-create-modal",
        ".dock-item[data-label='Create']",
        ".dock-item:nth-child(5)"
    ];

    modalTriggers.forEach(selector => {
        const elements = document.querySelectorAll(selector);
        console.log(`Found ${elements.length} elements for selector: ${selector}`);

        elements.forEach((element, index) => {
            if (element) {
                // Remove existing listeners to prevent duplicates
                element.removeEventListener("click", openCreateModal);
                element.addEventListener("click", openCreateModal);
                console.log(`✅ Event listener added to trigger ${index + 1}: ${selector}`);
            }
        });
    });

    // FIXED: Enhanced close functionality
    const closeBtn = modal.querySelector('.close-modal');
    if (closeBtn) {
        closeBtn.addEventListener("click", closeCreateModal);
        console.log("✅ Close button event listener added");
    }

    // Close modal on outside click
    modal.addEventListener("click", (e) => {
        if (e.target === modal) {
            closeCreateModal();
        }
    });

    // Close modal on Escape key
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && modal.style.display === "flex") {
            closeCreateModal();
        }
    });

    // Get form elements
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const submitBtn = document.getElementById("submitPost");
    const titleInput = document.getElementById("titleInput");
    const locationInput = document.getElementById("locationInput");
    const imageInput = document.getElementById("imageInput");
    const descriptionInput = document.getElementById("descriptionInput");

    // Add price calculation listeners
    if (categoryInput && conditionInput) {
        categoryInput.addEventListener("change", updateCreateFinalPrice);
        conditionInput.addEventListener("change", updateCreateFinalPrice);
    }

    // ENHANCED: Image input handling with better error handling
    if (imageInput) {
        imageInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (!file) return;

            console.log("🖼️ Image selected:", file.name);

            // Validate file
            if (!validateImageFile(file)) {
                imageInput.value = '';
                showNotification('Please select a valid image file', 'error');
                return;
            }

            // Show preview
            showImagePreview(file);

            // Trigger AI analysis
            if (aiSuggestion) {
                console.log("🚀 Starting AI analysis...");
                aiSuggestion.analyzeImageForCategory(file);
            }

            // Update form validation
            checkFormValidity();
        });
    }

    // Form validation listeners
    const requiredFields = [titleInput, descriptionInput, locationInput, imageInput, categoryInput, conditionInput];
    requiredFields.forEach(field => {
        if (field) {
            field.addEventListener("change", checkFormValidity);
            field.addEventListener("input", checkFormValidity);
        }
    });

    // Form submission
    if (submitBtn) {
        submitBtn.addEventListener("click", handleFormSubmission);
    }

    // Initial form validation
    checkFormValidity();

    // Make functions globally available
    window.openCreateModal = openCreateModal;
    window.closeCreateModal = closeCreateModal;

    console.log("✅ Modal system initialized successfully");
});

// ============================================================================
// IMAGE PREVIEW FUNCTIONS
// ============================================================================
function showImagePreview(file) {
    const reader = new FileReader();
    reader.onload = function (e) {
        const previewImg = document.getElementById('previewImg');
        const imagePreview = document.getElementById('imagePreview');
        const uploadLabel = document.querySelector('.upload-label');

        if (previewImg && imagePreview && uploadLabel) {
            previewImg.src = e.target.result;
            imagePreview.style.display = 'flex';
            uploadLabel.style.display = 'none';

            console.log('✅ Image preview displayed');
        } else {
            console.error('Image preview elements not found');
        }
    };
    reader.readAsDataURL(file);
}

function removeImage() {
    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const uploadLabel = document.querySelector('.upload-label');

    if (imageInput) imageInput.value = '';
    if (imagePreview) imagePreview.style.display = 'none';
    if (uploadLabel) uploadLabel.style.display = 'flex';

    // Reset dropdowns
    const categoryInput = document.getElementById('categoryInput');
    const conditionInput = document.getElementById('conditionInput');
    if (categoryInput) categoryInput.selectedIndex = 0;
    if (conditionInput) conditionInput.selectedIndex = 0;

    // Reset AI suggestions
    if (aiSuggestion) {
        aiSuggestion.reset();
    }

    checkFormValidity();
    console.log('🗑️ Image removed and form reset');
}

function validateImageFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
        return false;
    }

    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
        return false;
    }

    return true;
}

// ============================================================================
// FORM VALIDATION - Enhanced Version
// ============================================================================
function checkFormValidity() {
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const titleInput = document.getElementById("titleInput");
    const descriptionInput = document.getElementById("descriptionInput");
    const locationInput = document.getElementById("locationInput");
    const imageInput = document.getElementById("imageInput");
    const submitBtn = document.getElementById("submitPost");
    const latitudeInput = document.getElementById("Latitude");
    const longitudeInput = document.getElementById("Longitude");

    if (!submitBtn) return false;

    const isValid =
        titleInput?.value.trim() !== "" &&
        descriptionInput?.value.trim() !== "" &&
        locationInput?.value.trim() !== "" &&
        categoryInput?.value !== "" &&
        conditionInput?.value !== "" &&
        imageInput?.files?.length > 0 &&
        latitudeInput?.value !== "" &&
        longitudeInput?.value !== "";

    // Enhanced button state management
    submitBtn.disabled = !isValid;

    if (isValid) {
        submitBtn.classList.add('valid');
        submitBtn.classList.remove('invalid');
    } else {
        submitBtn.classList.remove('valid');
        submitBtn.classList.add('invalid');
    }

    // Update price calculation
    if (isValid && typeof updateCreateFinalPrice === 'function') {
        updateCreateFinalPrice();
    }

    return isValid;
}

// ============================================================================
// FORM SUBMISSION - Enhanced Error Handling
// ============================================================================
async function handleFormSubmission() {
    const submitBtn = document.getElementById("submitPost");
    if (!submitBtn || submitBtn.disabled) return;

    // Validate form before submission
    if (!checkFormValidity()) {
        showNotification('Please fill in all required fields', 'error');
        return;
    }

    const formData = new FormData();

    // Collect form data
    const formFields = {
        'ItemTitle': document.getElementById("titleInput"),
        'Description': document.getElementById("descriptionInput"),
        'LocationName': document.getElementById("locationInput"),
        'CategoryId': document.getElementById("categoryInput"),
        'ConditionId': document.getElementById("conditionInput"),
        'Latitude': document.getElementById("Latitude"),
        'Longitude': document.getElementById("Longitude")
    };

    // Add form field data
    Object.entries(formFields).forEach(([name, element]) => {
        if (element && element.value) {
            formData.append(name, element.value);
        }
    });

    // Add image file
    const imageInput = document.getElementById("imageInput");
    if (imageInput && imageInput.files[0]) {
        formData.append('ImageFile', imageInput.files[0]);
    }

    // Add location name to hidden field
    const locationNameInput = document.getElementById("LocationName");
    if (locationNameInput && locationNameInput.value) {
        formData.append('LocationName', locationNameInput.value);
    }

    // Add AI detection data
    const aiFields = ['aiCategoryId', 'aiConditionId', 'aiConfidenceScore'];
    aiFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field && field.value) {
            formData.append(fieldId, field.value);
        }
    });

    // Add anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    // Show loading state
    submitBtn.disabled = true;
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<span class="loading-spinner"></span><span class="btn-text">Posting...</span>';

    try {
        console.log("📤 Submitting form data...");

        const response = await fetch('/Item/Create', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (response.ok && result.success) {
            console.log("✅ Item posted successfully");

            // Close modal and reset form
            const modal = document.getElementById("createPostModal");
            if (modal) {
                modal.classList.remove("show");
                setTimeout(() => {
                    modal.style.display = "none";
                    document.body.style.overflow = '';
                }, 300);
            }

            resetCreateForm();

            // Show success message
            showNotification('Item posted successfully! 🎉', 'success');

            // Refresh page to show new item
            setTimeout(() => window.location.reload(), 1500);
        } else {
            throw new Error(result.message || 'Failed to post item');
        }
    } catch (err) {
        console.error('❌ Error submitting form:', err);
        showNotification(err.message || 'Failed to post item. Please try again.', 'error');
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// ============================================================================
// UTILITY FUNCTIONS - Enhanced
// ============================================================================
function resetCreateForm() {
    console.log("🔄 Resetting create form...");

    const form = document.getElementById('createPostModal');
    if (form) {
        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            if (input.type === 'file') {
                input.value = '';
            } else if (input.tagName === 'SELECT') {
                input.selectedIndex = 0;
            } else if (input.type !== 'hidden' && input.name !== '__RequestVerificationToken') {
                input.value = '';
            }
        });
    }

    // Reset image preview
    removeImage();

    // Reset AI suggestions
    if (aiSuggestion) {
        aiSuggestion.reset();
    }

    // Reset price display
    const finalPriceElement = document.getElementById("finalPrice");
    if (finalPriceElement) {
        finalPriceElement.textContent = "0.00";
    }

    // Reset form validation
    checkFormValidity();

    console.log("✅ Form reset completed");
}

function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification-toast');
    existingNotifications.forEach(n => n.remove());

    const notification = document.createElement('div');
    notification.className = `notification-toast ${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        border-radius: 12px;
        color: white;
        font-weight: 600;
        z-index: 20000;
        max-width: 400px;
        box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2);
        backdrop-filter: blur(10px);
        transform: translateX(100%);
        transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        display: flex;
        align-items: center;
        gap: 12px;
        font-family: 'Montserrat', sans-serif;
    `;

    // Set background based on type
    const backgrounds = {
        success: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
        error: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)',
        info: 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)'
    };

    const icons = {
        success: '<i class="bx bx-check-circle"></i>',
        error: '<i class="bx bx-error-circle"></i>',
        info: '<i class="bx bx-info-circle"></i>'
    };

    notification.style.background = backgrounds[type] || backgrounds.info;
    notification.innerHTML = `${icons[type] || icons.info}<span>${message}</span>`;

    document.body.appendChild(notification);

    // Animate in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);

    // Auto-remove after 4 seconds
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

// ============================================================================
// GOOGLE PLACES INTEGRATION
// ============================================================================
function triggerFormValidation() {
    if (typeof checkFormValidity === 'function') {
        checkFormValidity();
    }
}

// Initialize Google Places when ready
function initGoogleAutocomplete() {
    const input = document.getElementById("locationInput");
    if (!input) return;

    try {
        const autocomplete = new google.maps.places.Autocomplete(input, {
            types: ['geocode'],
            componentRestrictions: { country: 'ph' },
            fields: ['formatted_address', 'geometry', 'place_id']
        });

        autocomplete.addListener("place_changed", () => {
            const place = autocomplete.getPlace();
            if (place.geometry && place.geometry.location) {
                const latField = document.getElementById("Latitude");
                const lngField = document.getElementById("Longitude");
                const locationNameField = document.getElementById("LocationName");

                if (latField) latField.value = place.geometry.location.lat();
                if (lngField) lngField.value = place.geometry.location.lng();
                if (locationNameField) locationNameField.value = place.formatted_address;

                console.log("📍 Location selected:", place.formatted_address);
                triggerFormValidation();
            }
        });

        console.log("✅ Google Places autocomplete initialized");
    } catch (error) {
        console.error("❌ Google Places initialization failed:", error);
        initManualLocationFallback();
    }
}

function initManualLocationFallback() {
    const locationInput = document.getElementById('locationInput');
    if (locationInput) {
        locationInput.placeholder = 'Enter location manually (e.g., Manila, Philippines)';

        locationInput.addEventListener('blur', function () {
            const location = this.value.trim();
            if (location && location.length > 3) {
                const latField = document.getElementById("Latitude");
                const lngField = document.getElementById("Longitude");
                const locationNameField = document.getElementById("LocationName");

                if (latField) latField.value = '14.5995';
                if (lngField) lngField.value = '120.9842';
                if (locationNameField) locationNameField.value = location;

                console.log("📍 Manual location set:", location);
                triggerFormValidation();
            }
        });

        console.log("✅ Manual location fallback initialized");
    }
}

// Initialize Google Places when DOM is ready
document.addEventListener("DOMContentLoaded", function () {
    setTimeout(() => {
        if (typeof google !== 'undefined' && google.maps && google.maps.places) {
            console.log("Google Places API detected");
            initGoogleAutocomplete();
        } else {
            console.warn("Google Places API not available, using fallback");
            initManualLocationFallback();
        }
    }, 1000);
});

// ============================================================================
// GLOBAL EXPORTS
// ============================================================================
window.removeImage = removeImage;
window.aiSuggestion = aiSuggestion;
window.checkFormValidity = checkFormValidity;
window.resetCreateForm = resetCreateForm;
window.showNotification = showNotification;
window.handleFormSubmission = handleFormSubmission;

console.log("✅ Enhanced openModal.js loaded successfully");