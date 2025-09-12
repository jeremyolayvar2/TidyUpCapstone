// Fixed openModal.js - Completely Silent AI Integration
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

    // Vertex AI condition detection
    async detectCondition(file) {
        const formData = new FormData();
        formData.append('imageFile', file);

        // Add anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        try {
            const response = await fetch('/api/AI/DetectCondition', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) throw new Error('Vertex AI failed');

            const result = await response.json();
            return {
                success: result.success,
                conditionId: result.conditionId,
                conditionName: this.getConditionName(result.conditionId),
                confidence: result.confidence
            };
        } catch (error) {
            console.warn('Vertex AI failed:', error);
            return { success: false };
        }
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
            console.log(`Category automatically set: ${categoryResult.categoryName} (${(categoryResult.confidence * 100).toFixed(0)}% confidence)`);
        }

        // Apply condition if detected successfully
        if (conditionResult.success && conditionResult.conditionId) {
            this.detectedCondition = conditionResult;
            this.setSilentCondition(conditionResult);
            aiApplied = true;
            console.log(`Condition automatically set: ${conditionResult.conditionName} (${(conditionResult.confidence * 100).toFixed(0)}% confidence)`);
        }

        if (aiApplied) {
            // Trigger form validation and price update after setting values
            setTimeout(() => {
                if (typeof checkFormValidity === 'function') {
                    checkFormValidity();
                }
                if (typeof updateCreateFinalPrice === 'function') {
                    updateCreateFinalPrice();
                }
            }, 100);
            console.log("AI detection completed - dropdown values set automatically");
        } else {
            console.log("AI detection failed - manual selection required");
        }
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

    getConditionName(conditionId) {
        const conditions = {
            1: 'Excellent',
            2: 'Good',
            3: 'Good',  // Your system uses 3 for Good
            4: 'Fair'   // Your system uses 4 for Fair
        };
        return conditions[conditionId] || 'Unknown';
    },

    // Reset function (clear stored values only)
    reset() {
        this.isAnalyzing = false;
        this.detectedCategory = null;
        this.detectedCondition = null;

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
// MODAL INITIALIZATION - Enhanced Version
// ============================================================================
document.addEventListener("DOMContentLoaded", function () {
    const modal = document.getElementById("createPostModal");
    if (!modal) return;

    // Initialize AI system
    aiSuggestion.initialize();

    // Open modal functionality
    const openModalBtns = document.querySelectorAll("#openCreateModal, .open-create-modal");
    openModalBtns.forEach(btn => {
        if (btn) {
            btn.addEventListener("click", () => {
                modal.style.display = "block";
            });
        }
    });

    // Close modal when clicking outside
    window.addEventListener("click", (e) => {
        if (e.target === modal) {
            modal.style.display = "none";
            resetForm();
        }
    });

    // Add close button functionality
    const closeBtn = document.querySelector('.modal .close-modal');
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

// ============================================================================
// IMAGE PREVIEW FUNCTIONS (Existing functionality preserved)
// ============================================================================
function showImagePreview(file) {
    const reader = new FileReader();
    reader.onload = function (e) {
        // Try to find existing preview elements first
        let previewImg = document.getElementById('previewImg');
        let imagePreview = document.getElementById('imagePreview');
        const uploadSection = document.querySelector('.upload-section label');

        // If preview elements don't exist, create them
        if (!previewImg || !imagePreview) {
            const uploadSectionContainer = document.querySelector('.upload-section');
            if (uploadSectionContainer) {
                // Create preview container
                imagePreview = document.createElement('div');
                imagePreview.id = 'imagePreview';
                imagePreview.className = 'image-preview';
                imagePreview.style.cssText = `
                    position: relative;
                    display: block;
                    margin-top: 10px;
                    border-radius: 8px;
                    overflow: hidden;
                    max-width: 100%;
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
                `;

                // Create preview image
                previewImg = document.createElement('img');
                previewImg.id = 'previewImg';
                previewImg.style.cssText = `
                    width: 100%;
                    height: auto;
                    max-height: 300px;
                    object-fit: cover;
                    display: block;
                `;

                // Create remove button
                const removeBtn = document.createElement('button');
                removeBtn.type = 'button';
                removeBtn.className = 'remove-image';
                removeBtn.onclick = removeImage;
                removeBtn.style.cssText = `
                    position: absolute;
                    top: 8px;
                    right: 8px;
                    background: rgba(244, 67, 54, 0.9);
                    color: white;
                    border: none;
                    border-radius: 50%;
                    width: 32px;
                    height: 32px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    cursor: pointer;
                    transition: all 0.2s ease;
                `;
                removeBtn.innerHTML = '<span class="material-symbols-outlined">close</span>';

                imagePreview.appendChild(previewImg);
                imagePreview.appendChild(removeBtn);
                uploadSectionContainer.appendChild(imagePreview);
            }
        }

        if (previewImg && imagePreview && uploadSection) {
            previewImg.src = e.target.result;
            imagePreview.style.display = 'block';
            uploadSection.style.display = 'none';
        }
    };
    reader.readAsDataURL(file);
}

function removeImage() {
    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const uploadSection = document.querySelector('.upload-section label');

    if (imageInput) imageInput.value = '';
    if (imagePreview) imagePreview.style.display = 'none';
    if (uploadSection) uploadSection.style.display = 'block';

    // Reset dropdowns to default when image is removed
    const categoryInput = document.getElementById('categoryInput');
    const conditionInput = document.getElementById('conditionInput');
    if (categoryInput) categoryInput.selectedIndex = 0;
    if (conditionInput) conditionInput.selectedIndex = 0;

    // Reset AI suggestions
    if (aiSuggestion) {
        aiSuggestion.reset();
    }

    checkFormValidity();
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

    // Add all form fields
    const fields = [
        'titleInput', 'descriptionInput', 'locationInput',
        'categoryInput', 'conditionInput', 'imageInput'
    ];

    fields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            if (field.type === 'file') {
                if (field.files[0]) {
                    formData.append('ImageFile', field.files[0]);
                }
            } else {
                formData.append(field.name || fieldId.replace('Input', ''), field.value);
            }
        }
    });

    // Add hidden location data
    const hiddenFields = ['Latitude', 'Longitude', 'LocationName'];
    hiddenFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field && field.value) {
            formData.append(fieldId, field.value);
        }
    });

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
    submitBtn.innerHTML = '<span class="loading-spinner"></span><span class="btn-text">Posting...</span>';

    try {
        const response = await fetch('/api/Items/Create', {
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
        submitBtn.innerHTML = '<span class="btn-text">Post</span>';
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
            } else if (input.type !== 'hidden') {
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
document.addEventListener("DOMContentLoaded", function () {
    if (typeof google !== 'undefined' && google.maps && google.maps.places) {
        initGoogleAutocomplete();
    }
});

function initGoogleAutocomplete() {
    const input = document.getElementById("locationInput");
    if (!input) return;

    const autocomplete = new google.maps.places.Autocomplete(input, {
        types: ['geocode'],
        componentRestrictions: { country: 'ph' }
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
        }
    });
}

// ============================================================================
// GLOBAL FUNCTIONS
// ============================================================================
window.removeImage = removeImage;
window.aiSuggestion = aiSuggestion;
window.checkFormValidity = checkFormValidity;