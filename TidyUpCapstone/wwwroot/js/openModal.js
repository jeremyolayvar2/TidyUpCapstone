// Fixed openModal.js - Completely Silent AI Integration with PROPER FIXES
// ============================================================================
// AI SUGGESTION SYSTEM - Fully Silent Version (No UI Elements)
// ============================================================================
const aiSuggestion = {
    isAnalyzing: false,
    detectedCategory: null,
    detectedCondition: null,

    // Initialize AI system (no UI elements needed)
    initialize() {
        console.log("🤖 Silent AI detection system initialized");
    },

    // Analyze image for both category and condition (COMPLETELY SILENT)
    async analyzeImageForCategory(imageFile) {
        if (this.isAnalyzing) return;

        console.log("🤖 Silent AI analysis starting for:", imageFile.name);
        this.isAnalyzing = true;

        try {
            // Run both detections in parallel
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

    // Google Vision API category detection
    async detectCategory(file) {
        const formData = new FormData();
        formData.append('imageFile', file);

        // Add anti-forgery token
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

    // FIXED: Use the actual VertexAI backend results instead of hardcoded values
    async detectCondition(file) {
        console.log("Using backend VertexAI integration (no frontend endpoint needed)");

        try {
            // Since VertexAI runs in your ItemService during creation, 
            // we'll use a direct API call to get the condition prediction
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
                        conditionId: result.conditionId, // Use the actual result from VertexAI
                        conditionName: result.conditionName,
                        confidence: result.confidence
                    };
                }
            }
        } catch (error) {
            console.warn('Condition detection failed:', error);
        }

        // Fallback: return null to avoid setting incorrect values
        return { success: false };
    },

    // Apply AI results COMPLETELY SILENTLY - just set dropdown values
    applySilentAIResults(categoryResult, conditionResult) {
        console.log('AI Results received:', { categoryResult, conditionResult });

        let aiApplied = false;

        // Apply category if detected successfully
        if (categoryResult.success && categoryResult.categoryId) {
            this.detectedCategory = categoryResult;
            this.setSilentCategory(categoryResult);
            aiApplied = true;
            console.log(`✅ Category automatically set: ${categoryResult.categoryName} (${(categoryResult.confidence * 100).toFixed(0)}% confidence)`);
        }

        // Apply condition if detected successfully
        if (conditionResult.success && conditionResult.conditionId) {
            this.detectedCondition = conditionResult;
            this.setSilentCondition(conditionResult);
            aiApplied = true;
            console.log(`✅ Condition automatically set: ${conditionResult.conditionName} (${(conditionResult.confidence * 100).toFixed(0)}% confidence)`);
        }

        // HIDE any AI suggestion UI elements that might exist
        this.hideAllAISuggestionElements();

        // LOCK the dropdowns after AI detection
        if (aiApplied) {
            this.lockDropdowns();

            // Trigger form validation and price update after setting values
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

    // FIXED: Properly handle NodeList by converting to Array
    hideAllAISuggestionElements() {
        // Hide any elements with "AI Suggestion" or similar text
        const suggestionSelectors = [
            '.ai-suggestion',
            '.ai-suggestion-container',
            '.suggestion-container',
            '[data-ai-suggestion]'
        ];

        // Hide elements by selectors
        suggestionSelectors.forEach(selector => {
            const elements = document.querySelectorAll(selector);
            elements.forEach(el => {
                if (el && el.style) {
                    el.style.display = 'none';
                }
            });
        });

        // FIXED: Convert NodeList to Array before using filter
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

    // NEW: Lock dropdowns after AI detection
    lockDropdowns() {
        const categorySelect = document.getElementById('categoryInput');
        const conditionSelect = document.getElementById('conditionInput');

        if (categorySelect && this.detectedCategory) {
            categorySelect.disabled = true;
            categorySelect.style.background = '#f0f8ff';
            categorySelect.style.cursor = 'not-allowed';

            // Add visual indicator
            this.addAIDetectedLabel(categorySelect, 'Category detected by AI');
        }

        if (conditionSelect && this.detectedCondition) {
            conditionSelect.disabled = true;
            conditionSelect.style.background = '#f0f8ff';
            conditionSelect.style.cursor = 'not-allowed';

            // Add visual indicator  
            this.addAIDetectedLabel(conditionSelect, 'Condition detected by AI');
        }

        console.log("🔒 Dropdowns locked after AI detection");
    },

    // NEW: Add visual indicator that AI has set the value
    addAIDetectedLabel(element, text) {
        // Check if label already exists
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

        // Add AI icon
        const icon = document.createElement('span');
        icon.innerHTML = '🤖';
        icon.style.fontSize = '14px';
        label.insertBefore(icon, label.firstChild);

        element.parentNode.insertBefore(label, element.nextSibling);
    },

    // NEW: Unlock dropdowns (for when image is removed)
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

        // Remove AI labels
        document.querySelectorAll('.ai-detected-label').forEach(label => {
            label.remove();
        });

        console.log("🔓 Dropdowns unlocked");
    },

    // Set category dropdown value silently (no visual indicators)
    setSilentCategory(result) {
        const categorySelect = document.getElementById('categoryInput');
        if (categorySelect && result.categoryId) {
            // Set the dropdown value
            categorySelect.value = result.categoryId;

            // Trigger change event to update dependent systems
            const changeEvent = new Event('change', { bubbles: true });
            categorySelect.dispatchEvent(changeEvent);

            // Store AI values in hidden fields for backend
            const aiCategoryId = document.getElementById('aiCategoryId');
            if (aiCategoryId) {
                aiCategoryId.value = result.categoryId;
            }

            console.log(`📝 Category dropdown set to: ${result.categoryName} (ID: ${result.categoryId})`);
        }
    },

    // Set condition dropdown value silently (no visual indicators)
    setSilentCondition(result) {
        const conditionSelect = document.getElementById('conditionInput');
        if (conditionSelect && result.conditionId) {
            // Set the dropdown value
            conditionSelect.value = result.conditionId;

            // Trigger change event to update dependent systems
            const changeEvent = new Event('change', { bubbles: true });
            conditionSelect.dispatchEvent(changeEvent);

            // Store AI values in hidden fields for backend
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

    // Utility functions
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

    // FIXED: Updated to match your actual database condition IDs
    getConditionName(conditionId) {
        const conditions = {
            1: 'Excellent',
            3: 'Good',        // Your Vertex AI maps "good" to condition ID 3
            4: 'Fair'         // Your Vertex AI maps "fair" to condition ID 4
        };
        return conditions[conditionId] || 'Unknown';
    },

    // Reset function (clear stored values only)
    reset() {
        this.isAnalyzing = false;
        this.detectedCategory = null;
        this.detectedCondition = null;

        // Unlock dropdowns when resetting
        this.unlockDropdowns();

        // Clear AI hidden fields
        const aiFields = ['aiCategoryId', 'aiConditionId', 'aiConfidenceScore'];
        aiFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) field.value = '';
        });

        console.log("🤖 AI detection reset");
    }
};

// ============================================================================
// MODAL INITIALIZATION - Enhanced Version (Rest of your code remains the same)
// ============================================================================
document.addEventListener("DOMContentLoaded", function () {
    const modal = document.getElementById("createPostModal");
    if (!modal) return;

    // Initialize AI system
    aiSuggestion.initialize();

    // [Rest of your existing modal initialization code stays exactly the same...]
    // Open modal functionality - FIXED: Handle all trigger selectors properly
    const openModalSelectors = [
        "#openCreateModal",
        ".floating-add-btn",
        ".dock-item[data-label='Create']",
        ".open-create-modal",
        // Handle sidebar navigation
        "a#openCreateModal",
        // Handle dock items
        ".dock-item:nth-child(5)" // Create is usually 5th item in dock
    ];

    openModalSelectors.forEach(selector => {
        const elements = document.querySelectorAll(selector);
        elements.forEach(element => {
            if (element) {
                element.addEventListener("click", (e) => {
                    e.preventDefault();
                    modal.style.display = "flex"; // Use flex to center modal
                    console.log("📝 Modal opened via:", selector);
                });
            }
        });
    });

    // Close modal when clicking outside
    window.addEventListener("click", (e) => {
        if (e.target === modal) {
            modal.style.display = "none";
            resetForm();
        }
    });

    // Add close button functionality
    const closeBtn = document.querySelector('#createPostModal .close-modal');
    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            modal.style.display = "none";
            resetForm();
        });
    }

    // Get form elements
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const submitBtn = document.getElementById("submitPost");
    const titleInput = document.getElementById("titleInput");
    const locationInput = document.getElementById("locationInput");
    const imageInput = document.getElementById("imageInput");
    const descriptionInput = document.getElementById("descriptionInput");

    // Add event listeners for price calculation
    if (categoryInput && conditionInput) {
        categoryInput.addEventListener("change", updateCreateFinalPrice);
        conditionInput.addEventListener("change", updateCreateFinalPrice);
    }

    // Enhanced image input handling with SILENT AI detection
    if (imageInput) {
        imageInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                // Validate file
                if (!validateImageFile(file)) {
                    imageInput.value = '';
                    return;
                }

                // Show preview
                showImagePreview(file);

                // Trigger SILENT AI analysis (no UI changes, just dropdown updates)
                if (aiSuggestion) {
                    console.log("🚀 Starting AI analysis for uploaded image...");
                    aiSuggestion.analyzeImageForCategory(file);
                }

                // Update form validation
                checkFormValidity();
            }
        });
    }

    // Add event listeners to all required fields for validation
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

    // Initial form validation check
    checkFormValidity();
});

// [Rest of your existing functions remain exactly the same...]
// Just updating the removeImage function to unlock dropdowns:

function removeImage() {
    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    // FIXED: Look for the correct upload label structure
    const uploadLabel = document.querySelector('.upload-label') || document.querySelector('label[for="imageInput"]');

    if (imageInput) imageInput.value = '';
    if (imagePreview) imagePreview.style.display = 'none';

    // FIXED: Show the upload label correctly
    if (uploadLabel) {
        uploadLabel.style.display = 'block';
    }

    // Reset dropdowns to default when image is removed
    const categoryInput = document.getElementById('categoryInput');
    const conditionInput = document.getElementById('conditionInput');
    if (categoryInput) categoryInput.selectedIndex = 0;
    if (conditionInput) conditionInput.selectedIndex = 0;

    // Reset AI suggestions and UNLOCK dropdowns
    if (aiSuggestion) {
        aiSuggestion.reset(); // This now includes unlocking dropdowns
    }

    checkFormValidity();
}

// ============================================================================
// IMAGE PREVIEW FUNCTIONS (Existing functionality preserved)
// ============================================================================
function showImagePreview(file) {
    const reader = new FileReader();
    reader.onload = function (e) {
        // Use the existing HTML elements
        const previewImg = document.getElementById('previewImg');
        const imagePreview = document.getElementById('imagePreview');
        const uploadLabel = document.querySelector('.upload-label');

        if (previewImg && imagePreview && uploadLabel) {
            // Set the image source
            previewImg.src = e.target.result;

            // Show preview, hide upload button
            imagePreview.style.display = 'block';
            uploadLabel.style.display = 'none';

            console.log('✅ Image preview displayed successfully');
        }
    };
    reader.readAsDataURL(file);
}

function validateImageFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
        alert('Image size must be less than 10MB');
        return false;
    }

    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
        alert('Please select a valid image file (JPEG, PNG, or WebP)');
        return false;
    }

    return true;
}

// ============================================================================
// FORM VALIDATION (Enhanced to work with AI)
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

    if (!categoryInput || !conditionInput || !titleInput || !descriptionInput ||
        !locationInput || !imageInput || !submitBtn) {
        return;
    }

    const isValid =
        titleInput.value.trim() !== "" &&
        descriptionInput.value.trim() !== "" &&
        locationInput.value.trim() !== "" &&
        categoryInput.value !== "" &&
        conditionInput.value !== "" &&
        imageInput.files.length > 0 &&
        latitudeInput && latitudeInput.value !== "" &&
        longitudeInput && longitudeInput.value !== "";

    submitBtn.disabled = !isValid;

    // Update final price when form is valid
    if (isValid && typeof updateCreateFinalPrice === 'function') {
        updateCreateFinalPrice();
    }

    return isValid;
}

// ============================================================================
// FORM SUBMISSION (Enhanced to include AI data)
// ============================================================================
async function handleFormSubmission() {
    const submitBtn = document.getElementById("submitPost");
    if (!submitBtn || submitBtn.disabled) return;

    // Get form data
    const formData = new FormData();

    // Add form fields with correct names that match your ItemController
    const titleInput = document.getElementById("titleInput");
    const descriptionInput = document.getElementById("descriptionInput");
    const locationInput = document.getElementById("locationInput");
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const imageInput = document.getElementById("imageInput");

    if (titleInput) formData.append('ItemTitle', titleInput.value);
    if (descriptionInput) formData.append('Description', descriptionInput.value);
    if (locationInput) formData.append('LocationName', locationInput.value);
    if (categoryInput) formData.append('CategoryId', categoryInput.value);
    if (conditionInput) formData.append('ConditionId', conditionInput.value);
    if (imageInput && imageInput.files[0]) {
        formData.append('ImageFile', imageInput.files[0]);
    }

    // Add hidden location data
    const latitudeInput = document.getElementById("Latitude");
    const longitudeInput = document.getElementById("Longitude");
    const locationNameInput = document.getElementById("LocationName");

    if (latitudeInput && latitudeInput.value) formData.append('Latitude', latitudeInput.value);
    if (longitudeInput && longitudeInput.value) formData.append('Longitude', longitudeInput.value);
    if (locationNameInput && locationNameInput.value) formData.append('LocationName', locationNameInput.value);

    // Add AI detection data if available
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
        // FIXED: Use correct endpoint that matches your working controller
        const response = await fetch('/Item/Create', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (response.ok && result.success) {
            // Success - close modal and refresh
            document.getElementById("createPostModal").style.display = "none";
            resetForm();

            // Show success message
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Success!',
                    text: 'Your item has been posted successfully.',
                    icon: 'success',
                    confirmButtonText: 'OK'
                });
            } else {
                alert('Item posted successfully!');
            }

            // Refresh the page to show new item
            setTimeout(() => window.location.reload(), 1500);
        } else {
            throw new Error(result.message || 'Failed to post item');
        }
    } catch (err) {
        console.error('Error submitting form:', err);
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Error',
                text: err.message || 'Failed to post item. Please try again.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        } else {
            alert('Failed to post: ' + err.message);
        }
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================
function resetForm() {
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
}

// ============================================================================
// GOOGLE PLACES INTEGRATION (Preserved existing functionality)
// ============================================================================
function triggerFormValidation() {
    if (typeof checkFormValidity === 'function') {
        checkFormValidity();
    }
}

// Make sure Google Places integration works
document.addEventListener("DOMContentLoaded", function () {
    // Add delay to ensure Google Maps API is loaded
    setTimeout(() => {
        if (typeof google !== 'undefined' && google.maps && google.maps.places) {
            initGoogleAutocomplete();
            console.log("Google Places API initialized");
        } else {
            console.warn("Google Places API not available, using manual location input");
            initManualLocationFallback();
        }
    }, 1000);
});

function initGoogleAutocomplete() {
    const input = document.getElementById("locationInput");
    if (!input) return;

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

            console.log("Location selected:", place.formatted_address);

            // Trigger form validation
            triggerFormValidation();
        }
    });
}

function initManualLocationFallback() {
    const locationInput = document.getElementById('locationInput');
    if (locationInput) {
        locationInput.placeholder = 'Enter location manually (e.g., Manila, Philippines)';

        locationInput.addEventListener('blur', function () {
            const location = this.value.trim();
            if (location && location.length > 3) {
                // Set default Philippines coordinates
                const latField = document.getElementById("Latitude");
                const lngField = document.getElementById("Longitude");
                const locationNameField = document.getElementById("LocationName");

                if (latField) latField.value = '14.5995';
                if (lngField) lngField.value = '120.9842';
                if (locationNameField) locationNameField.value = location;

                console.log("Manual location set:", location);
                triggerFormValidation();
            }
        });
    }
}

// ============================================================================
// GLOBAL FUNCTIONS
// ============================================================================
window.removeImage = removeImage;
window.aiSuggestion = aiSuggestion;
window.checkFormValidity = checkFormValidity;
window.closeCreateModal = function () {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'none';
    }
    resetForm();
};