// Fixed openModal.js - Works with div structure and includes AI integration

// ============================================================================
// AI CATEGORY SUGGESTION CLASS
// ============================================================================
class AICategorySuggestion {
    constructor() {
        this.isAnalyzing = false;
        this.currentSuggestion = null;
        this.initializeUI();
    }

    initializeUI() {
        this.createSuggestionUI();
    }

    createSuggestionUI() {
        const categoryInput = document.getElementById('categoryInput');
        if (!categoryInput) return;

        // Create AI suggestion container
        const suggestionContainer = document.createElement('div');
        suggestionContainer.id = 'aiSuggestionContainer';
        suggestionContainer.className = 'ai-suggestion-container';
        suggestionContainer.style.display = 'none';
        suggestionContainer.innerHTML = `
            <div class="ai-suggestion-content">
                <div class="ai-suggestion-header">
                    <span class="ai-icon">🤖</span>
                    <span class="ai-text">AI Suggestion</span>
                    <div class="ai-loader" id="aiLoader" style="display: none;">
                        <div class="spinner"></div>
                        <span>Analyzing...</span>
                    </div>
                </div>
                <div class="ai-suggestion-body" id="aiSuggestionBody">
                    <!-- Suggestion content will be inserted here -->
                </div>
            </div>
        `;

        // Insert after category input
        categoryInput.parentNode.insertBefore(suggestionContainer, categoryInput.nextSibling);
        this.addSuggestionStyles();
    }

    addSuggestionStyles() {
        if (document.getElementById('aiSuggestionStyles')) return;

        const style = document.createElement('style');
        style.id = 'aiSuggestionStyles';
        style.textContent = `
            .ai-suggestion-container {
                margin-top: 10px;
                border: 2px solid #e3f2fd;
                border-radius: 8px;
                background: linear-gradient(135deg, #f8f9fa 0%, #e3f2fd 100%);
                animation: slideDown 0.3s ease-out;
            }
            .ai-suggestion-content { padding: 12px; }
            .ai-suggestion-header {
                display: flex;
                align-items: center;
                gap: 8px;
                margin-bottom: 8px;
                font-weight: 600;
                color: #1976d2;
            }
            .ai-loader {
                display: flex;
                align-items: center;
                gap: 8px;
                margin-left: auto;
            }
            .spinner {
                width: 16px;
                height: 16px;
                border: 2px solid #e3f2fd;
                border-top: 2px solid #1976d2;
                border-radius: 50%;
                animation: spin 1s linear infinite;
            }
            .suggestion-option {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 8px 12px;
                margin: 4px 0;
                background: white;
                border: 1px solid #ddd;
                border-radius: 6px;
                cursor: pointer;
                transition: all 0.2s ease;
            }
            .suggestion-option:hover {
                background: #f0f8ff;
                border-color: #1976d2;
                transform: translateY(-1px);
            }
            .confidence-badge {
                background: #4caf50;
                color: white;
                padding: 2px 8px;
                border-radius: 12px;
                font-size: 12px;
                font-weight: 500;
            }
            .confidence-badge.medium { background: #ff9800; }
            .confidence-badge.low { background: #757575; }
            @keyframes slideDown {
                from { opacity: 0; transform: translateY(-10px); }
                to { opacity: 1; transform: translateY(0); }
            }
            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
        `;
        document.head.appendChild(style);
    }

    async analyzeImageForCategory(imageFile) {
        if (this.isAnalyzing) return;

        console.log("🤖 AI analysis starting for:", imageFile.name);
        this.isAnalyzing = true;
        this.showAnalysisLoader(true);

        try {
            const formData = new FormData();
            formData.append('imageFile', imageFile);

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }

            const response = await fetch('/api/Vision/AnalyzeImage', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                this.currentSuggestion = result;
                this.displaySuggestion(result);
                this.showSuggestionContainer(true);
            } else {
                this.showFallbackMessage();
            }

        } catch (error) {
            console.error("❌ AI Analysis error:", error);
            this.showErrorMessage(error.message);
        } finally {
            this.isAnalyzing = false;
            this.showAnalysisLoader(false);
        }
    }

    displaySuggestion(result) {
        const bodyElement = document.getElementById('aiSuggestionBody');
        if (!bodyElement) return;

        const confidenceLevel = this.getConfidenceLevel(result.confidenceScore);
        const categoryName = result.categoryName || 'Unknown Category';

        bodyElement.innerHTML = `
            <div class="suggestion-option" onclick="window.aiSuggestion.applySuggestion(${result.suggestedCategoryId})">
                <div>
                    <strong>${categoryName}</strong>
                    <div style="font-size: 12px; color: #666; margin-top: 2px;">
                        Based on: ${result.topLabels?.slice(0, 2).map(l => l.description).join(', ') || 'Image analysis'}
                    </div>
                </div>
                <span class="confidence-badge ${confidenceLevel.class}">
                    ${Math.round(result.confidenceScore * 100)}%
                </span>
            </div>
            <div style="margin-top: 8px; font-size: 12px; color: #666;">
                Click to apply this suggestion
            </div>
        `;
    }

    applySuggestion(categoryId) {
        const categorySelect = document.getElementById('categoryInput');
        if (categorySelect) {
            categorySelect.value = categoryId;

            const changeEvent = new Event('change', { bubbles: true });
            categorySelect.dispatchEvent(changeEvent);

            categorySelect.style.borderColor = '#4caf50';
            setTimeout(() => {
                categorySelect.style.borderColor = '';
            }, 2000);

            // Update form validation
            checkFormValidity();
            updateCreateFinalPrice();
        }
    }

    getConfidenceLevel(score) {
        if (score >= 0.7) return { class: 'high', label: 'High' };
        if (score >= 0.4) return { class: 'medium', label: 'Medium' };
        return { class: 'low', label: 'Low' };
    }

    showAnalysisLoader(show) {
        const loader = document.getElementById('aiLoader');
        if (loader) {
            loader.style.display = show ? 'flex' : 'none';
        }
        if (show) {
            this.showSuggestionContainer(true);
            const bodyElement = document.getElementById('aiSuggestionBody');
            if (bodyElement) {
                bodyElement.innerHTML = '<div style="color: #666; font-style: italic;">Analyzing image...</div>';
            }
        }
    }

    showSuggestionContainer(show) {
        const container = document.getElementById('aiSuggestionContainer');
        if (container) {
            container.style.display = show ? 'block' : 'none';
        }
    }

    showFallbackMessage() {
        const bodyElement = document.getElementById('aiSuggestionBody');
        if (bodyElement) {
            bodyElement.innerHTML = `
                <div style="color: #ff9800; font-style: italic;">
                    AI analysis unavailable. Please select category manually.
                </div>
            `;
        }
        this.showSuggestionContainer(true);
    }

    showErrorMessage(error) {
        const bodyElement = document.getElementById('aiSuggestionBody');
        if (bodyElement) {
            bodyElement.innerHTML = `
                <div style="color: #f44336; font-style: italic;">
                    Analysis failed. Please select category manually.
                </div>
            `;
        }
        this.showSuggestionContainer(true);
    }

    reset() {
        this.currentSuggestion = null;
        this.isAnalyzing = false;
        this.showSuggestionContainer(false);
        this.showAnalysisLoader(false);
    }
}

// ============================================================================
// GLOBAL VARIABLES
// ============================================================================
let aiSuggestion = null;

// ============================================================================
// MAIN INITIALIZATION
// ============================================================================
document.addEventListener("DOMContentLoaded", () => {
    console.log("OpenModal.js loaded");

    const modal = document.getElementById("createPostModal");

    // Initialize AI suggestion system
    if (!aiSuggestion) {
        aiSuggestion = new AICategorySuggestion();
        window.aiSuggestion = aiSuggestion; // Make it globally accessible
    }

    // Modal triggers - Updated selectors
    const triggers = document.querySelectorAll(
        '#openCreateModal, .floating-add-btn, .dock-item[data-label="Create"]'
    );

    triggers.forEach(trigger => {
        trigger.addEventListener("click", (e) => {
            e.preventDefault();
            if (modal) {
                modal.style.display = "flex";
                console.log("Modal opened");
            } else {
                console.error("Modal not found!");
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

    // Image preview functionality
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

                // Trigger AI analysis
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
// IMAGE PREVIEW FUNCTIONS
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
// FORM VALIDATION
// ============================================================================
function checkFormValidity() {
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const titleInput = document.getElementById("titleInput");
    const descriptionInput = document.getElementById("descriptionInput");
    const locationInput = document.getElementById("locationInput");
    const imageInput = document.getElementById("imageInput");
    const submitBtn = document.getElementById("submitPost");

    const categoryId = categoryInput?.value;
    const conditionId = conditionInput?.value;
    const title = titleInput?.value?.trim();
    const description = descriptionInput?.value?.trim();
    const location = locationInput?.value?.trim();
    const image = imageInput?.files?.length > 0;

    const isValid = categoryId && conditionId && title && description && location && image;

    if (submitBtn) {
        submitBtn.disabled = !isValid;

        if (isValid) {
            submitBtn.classList.add('valid');
            submitBtn.classList.remove('invalid');
        } else {
            submitBtn.classList.remove('valid');
            submitBtn.classList.add('invalid');
        }
    }
}

// ============================================================================
// PRICE CALCULATION
// ============================================================================
async function updateCreateFinalPrice() {
    const categorySelect = document.getElementById("categoryInput");
    const conditionSelect = document.getElementById("conditionInput");
    const finalPriceElement = document.getElementById("finalPrice");

    if (!categorySelect || !conditionSelect || !finalPriceElement) {
        return;
    }

    const categoryId = parseInt(categorySelect.value);
    const conditionId = parseInt(conditionSelect.value);

    if (!categoryId || !conditionId || isNaN(categoryId) || isNaN(conditionId)) {
        finalPriceElement.textContent = "0.00";
        return;
    }

    try {
        finalPriceElement.textContent = "Calculating...";

        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result && typeof result.finalPrice === 'number') {
            finalPriceElement.textContent = result.finalPrice.toFixed(2);
        } else {
            throw new Error("Invalid response format");
        }

    } catch (error) {
        console.error("Price calculation failed:", error);
        finalPriceElement.textContent = "0.00";
    }
}

// ============================================================================
// FORM SUBMISSION
// ============================================================================
async function handleFormSubmission(e) {
    e.preventDefault();

    const submitBtn = document.getElementById("submitPost");
    const titleInput = document.getElementById("titleInput");
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const descriptionInput = document.getElementById("descriptionInput");
    const imageInput = document.getElementById("imageInput");
    const locationInput = document.getElementById("locationInput");
    const latitudeInput = document.getElementById("Latitude");
    const longitudeInput = document.getElementById("Longitude");

    // Validate required fields
    if (!titleInput?.value?.trim() || !categoryInput?.value || !conditionInput?.value ||
        !descriptionInput?.value?.trim() || !locationInput?.value?.trim() || !imageInput?.files?.length) {

        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Information',
                text: 'Please fill in all required fields.'
            });
        } else {
            alert('Please fill in all required fields.');
        }
        return;
    }

    // Check location coordinates
    const latitude = latitudeInput?.value || '14.5995';  // Default Manila
    const longitude = longitudeInput?.value || '120.9842';

    try {
        // Show loading state
        submitBtn.disabled = true;
        submitBtn.textContent = "Posting...";

        const formData = new FormData();

        // Add anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        formData.append("ItemTitle", titleInput.value.trim());
        formData.append("CategoryId", parseInt(categoryInput.value));
        formData.append("ConditionId", parseInt(conditionInput.value));
        formData.append("Description", descriptionInput.value.trim());
        formData.append("Latitude", parseFloat(latitude));
        formData.append("Longitude", parseFloat(longitude));
        formData.append("LocationName", locationInput.value.trim());
        formData.append("ImageFile", imageInput.files[0]);

        const response = await fetch("/Item/Create", {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const result = await response.json();

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: 'Posted Successfully!',
                    text: 'Your item has been posted with AI analysis!',
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                alert('Posted successfully!');
            }

            // Close modal and reset
            document.getElementById('createPostModal').style.display = 'none';
            resetForm();

            // Reload page to show new item
            setTimeout(() => window.location.reload(), 1500);

        } else {
            const errorText = await response.text();
            throw new Error(errorText || 'Failed to post item');
        }
    } catch (err) {
        console.error("Submission error:", err);

        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Failed to Post',
                text: err.message || 'Please try again.'
            });
        } else {
            alert('Failed to post: ' + err.message);
        }
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        submitBtn.textContent = "Post";
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
// GOOGLE PLACES INTEGRATION
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

            if (latField) latField.value = place.geometry.location.lat();
            if (lngField) lngField.value = place.geometry.location.lng();
        }
    });
}

// ============================================================================
// GLOBAL FUNCTIONS
// ============================================================================
window.removeImage = removeImage;
window.aiSuggestion = aiSuggestion;