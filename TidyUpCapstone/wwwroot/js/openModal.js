// FIXED openModal.js with proper debugging and image preview

document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("createPostModal");

    // Updated selector to match your actual HTML structure
    const triggers = document.querySelectorAll(
        '#openCreateModal, .dock-item[data-label="Create"]'
    );

    triggers.forEach(trigger => {
        trigger.addEventListener("click", (e) => {
            e.preventDefault();
            if (modal) {
                modal.style.display = "flex";
            } else {
                console.error("Modal not found!");
            }
        });
    });

    // Close modal when clicking outside
    window.addEventListener("click", (e) => {
        if (e.target === modal) {
            modal.style.display = "none";
            resetCreateForm();
        }
    });

    // Add close button functionality
    const closeBtn = document.querySelector('.modal .close-modal');
    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            modal.style.display = "none";
            resetCreateForm();
        });
    }

    // Get form elements
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const submitBtn = document.getElementById("submitPost");
    const titleInput = document.getElementById("titleInput");
    const locationInput = document.getElementById("locationInput");
    const imageInput = document.getElementById("imageInput");

    // Add event listeners for price calculation
    if (categoryInput && conditionInput) {
        categoryInput.addEventListener("change", updateCreateFinalPrice);
        conditionInput.addEventListener("change", updateCreateFinalPrice);
    }

    // Form validation setup
    setupFormValidation();

    // MAIN FORM SUBMISSION LOGIC
    if (submitBtn) {
        submitBtn.addEventListener("click", handleFormSubmission);
    }

    // FIXED: Initialize image preview properly
    setupImagePreview();
});

// FIXED: Proper image preview setup
function setupImagePreview() {
    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const previewImg = document.getElementById('previewImg');
    const uploadLabel = document.querySelector('.upload-label');

    console.log("Setting up image preview...");
    console.log("Image input:", !!imageInput);
    console.log("Image preview:", !!imagePreview);
    console.log("Preview img:", !!previewImg);
    console.log("Upload label:", !!uploadLabel);

    if (imageInput && imagePreview && previewImg && uploadLabel) {
        imageInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            console.log("File selected:", file?.name, file?.size, file?.type);

            if (file) {
                if (validateImageFile(file)) {
                    handleImagePreview(file, imagePreview, previewImg, uploadLabel);
                } else {
                    // Reset input if invalid
                    imageInput.value = '';
                    triggerFormValidation();
                }
            } else {
                resetUploadSection();
            }
        });

        // Setup remove image button
        setupRemoveImageButton();
    } else {
        console.error("Image preview elements not found!");
        console.log("Looking for elements with IDs: imageInput, imagePreview, previewImg");
        console.log("And class: upload-label");
    }
}

function validateImageFile(file) {
    console.log("Validating file:", file.name, file.type, file.size);

    // Check file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
        showErrorMessage('Please upload a valid image file (JPEG, PNG, or GIF).');
        return false;
    }

    // Check file size (10MB limit)
    const maxSize = 10 * 1024 * 1024; // 10MB in bytes
    if (file.size > maxSize) {
        showErrorMessage('Image file size must be less than 10MB.');
        return false;
    }

    return true;
}

function handleImagePreview(file, imagePreview, previewImg, uploadLabel) {
    console.log("Creating image preview for:", file.name);

    const reader = new FileReader();
    reader.onload = function (e) {
        console.log("Image loaded, setting preview");
        previewImg.src = e.target.result;
        imagePreview.style.display = 'block';
        uploadLabel.style.display = 'none';
        triggerFormValidation();
    };
    reader.onerror = function (e) {
        console.error("Error reading file:", e);
        showErrorMessage("Error reading image file");
    };
    reader.readAsDataURL(file);
}

function setupRemoveImageButton() {
    document.addEventListener('click', function (e) {
        if (e.target.closest('.remove-image')) {
            console.log("Remove image clicked");
            removeImage();
        }
    });
}

function removeImage() {
    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const uploadLabel = document.querySelector('.upload-label');

    if (imageInput) imageInput.value = '';
    if (imagePreview) imagePreview.style.display = 'none';
    if (uploadLabel) uploadLabel.style.display = 'block';

    triggerFormValidation();
}

function resetUploadSection() {
    const imagePreview = document.getElementById('imagePreview');
    const uploadLabel = document.querySelector('.upload-label');

    if (imagePreview) imagePreview.style.display = 'none';
    if (uploadLabel) uploadLabel.style.display = 'block';
}

// COMPREHENSIVE DEBUG VERSION - Enhanced form submission handler
async function handleFormSubmission(e) {
    e.preventDefault();

    // COMPREHENSIVE DEBUGGING
    console.log("=== FORM SUBMISSION DEBUG START ===");

    // Check all form inputs
    const titleInput = document.getElementById("titleInput");
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const descriptionInput = document.getElementById("descriptionInput");
    const imageInput = document.getElementById("imageInput");
    const locationInput = document.getElementById("locationInput");
    const latitudeInput = document.getElementById("Latitude");
    const longitudeInput = document.getElementById("Longitude");
    const locationNameInput = document.getElementById("LocationName");

    console.log("1. INPUT FIELD VALUES:");
    console.log("   Title:", titleInput?.value);
    console.log("   Category:", categoryInput?.value);
    console.log("   Condition:", conditionInput?.value);
    console.log("   Description:", descriptionInput?.value);
    console.log("   Location Input:", locationInput?.value);
    console.log("   LocationName Hidden:", locationNameInput?.value);
    console.log("   Latitude:", latitudeInput?.value);
    console.log("   Longitude:", longitudeInput?.value);
    console.log("   Image files:", imageInput?.files?.length || 0);

    // FORCED LOCATION FIX - If LocationName is empty but locationInput has value
    if (locationInput?.value && !locationNameInput?.value) {
        console.log("2. FIXING EMPTY LOCATIONNAME FIELD");
        const manualLocation = locationInput.value.trim();
        if (locationNameInput) {
            locationNameInput.value = manualLocation;
            console.log("   Set LocationName to:", manualLocation);
        }

        // Also set default coordinates if missing
        if (!latitudeInput?.value || !longitudeInput?.value) {
            if (latitudeInput) latitudeInput.value = "14.5995"; // Manila default
            if (longitudeInput) longitudeInput.value = "120.9842";
            console.log("   Set default Manila coordinates");
        }
    }

    // Check which LocationName we're using
    const locationName = locationNameInput?.value || locationInput?.value?.trim();
    console.log("3. LOCATION RESOLUTION:");
    console.log("   Final location name:", locationName);
    console.log("   Length:", locationName?.length || 0);
    console.log("   Is empty?", !locationName);

    // Check all hidden inputs
    console.log("4. ALL HIDDEN INPUTS:");
    const allHiddenInputs = document.querySelectorAll('#createPostModal input[type="hidden"]');
    allHiddenInputs.forEach(input => {
        console.log(`   ${input.name || input.id}: "${input.value}"`);
    });

    // Validate form before submission
    if (!validateFormBeforeSubmit()) {
        console.log("❌ FORM VALIDATION FAILED");
        return;
    }

    // Show loading state
    const submitBtn = document.getElementById("submitPost");
    const spinner = submitBtn.querySelector('.loading-spinner');
    const btnText = submitBtn.querySelector('.btn-text');

    submitBtn.disabled = true;
    if (spinner) spinner.style.display = 'inline-block';
    if (btnText) btnText.textContent = 'Posting...';

    try {
        // Prepare form data
        const formData = new FormData();

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Append form fields
        formData.append('ItemTitle', titleInput.value.trim());
        formData.append('CategoryId', categoryInput.value);
        formData.append('ConditionId', conditionInput.value);
        formData.append('Description', descriptionInput.value.trim());
        formData.append('LocationName', locationName); // Use resolved location name
        formData.append('Latitude', latitudeInput.value);
        formData.append('Longitude', longitudeInput.value);

        if (imageInput.files[0]) {
            formData.append('ImageFile', imageInput.files[0]);
        }

        console.log("5. FORMDATA BEING SENT:");
        for (let [key, value] of formData.entries()) {
            if (value instanceof File) {
                console.log(`   ${key}: [File] ${value.name} (${value.size} bytes)`);
            } else {
                console.log(`   ${key}: "${value}"`);
            }
        }

        // Submit to backend
        console.log("6. SENDING REQUEST...");
        const response = await fetch('/Item/Create', {
            method: 'POST',
            body: formData
        });

        console.log("7. RESPONSE STATUS:", response.status, response.statusText);

        const result = await response.json();
        console.log("8. RESPONSE DATA:", result);

        if (result.success) {
            console.log("✅ SUCCESS!");
            showSuccessMessage('Item posted successfully!');
            closeCreateModal();

            // Add to UI immediately if we have the item data
            if (result.item && window.uiUpdateSystem) {
                window.uiUpdateSystem.addItemToUI(result.item);
            } else {
                // Fallback: reload page after a short delay
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            }

        } else {
            console.log("❌ SERVER ERROR:", result.message);
            showErrorMessage(result.message || 'Failed to post item. Please try again.');
        }

    } catch (error) {
        console.error('9. NETWORK ERROR:', error);
        showErrorMessage('Network error. Please check your connection and try again.');
    } finally {
        // Reset button state
        submitBtn.disabled = false;
        if (spinner) spinner.style.display = 'none';
        if (btnText) btnText.textContent = 'Post';
    }

    console.log("=== FORM SUBMISSION DEBUG END ===");
}

// Form validation before submission - UPDATED
function validateFormBeforeSubmit() {
    const titleInput = document.getElementById("titleInput");
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");
    const descriptionInput = document.getElementById("descriptionInput");
    const imageInput = document.getElementById("imageInput");
    const locationInput = document.getElementById("locationInput");
    const latitudeInput = document.getElementById("Latitude");
    const longitudeInput = document.getElementById("Longitude");
    const locationNameInput = document.getElementById("LocationName");

    // Check required fields
    if (!titleInput?.value.trim()) {
        showErrorMessage('Please enter an item title.');
        titleInput?.focus();
        return false;
    }

    if (!categoryInput?.value) {
        showErrorMessage('Please select a category.');
        categoryInput?.focus();
        return false;
    }

    if (!conditionInput?.value) {
        showErrorMessage('Please select a condition.');
        conditionInput?.focus();
        return false;
    }

    if (!descriptionInput?.value.trim()) {
        showErrorMessage('Please enter a description.');
        descriptionInput?.focus();
        return false;
    }

    // FIXED: Check for location name from either field
    const locationName = locationNameInput?.value || locationInput?.value?.trim();
    if (!locationName) {
        showErrorMessage('Please enter a location.');
        locationInput?.focus();
        return false;
    }

    if (!imageInput?.files?.length) {
        showErrorMessage('Please upload an image.');
        imageInput?.focus();
        return false;
    }

    if (!latitudeInput?.value || !longitudeInput?.value) {
        showErrorMessage('Please wait for location detection or select a valid location.');
        return false;
    }

    return true;
}

function showSuccessMessage(message) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'success',
            title: 'Success!',
            text: message,
            background: '#F5F5F5',
            color: '#252422',
            confirmButtonColor: '#6B9080',
            timer: 2000,
            showConfirmButton: false
        });
    } else {
        alert(message);
    }
}

function showErrorMessage(message) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: message,
            background: '#F5F5F5',
            color: '#252422',
            confirmButtonColor: '#6B9080'
        });
    } else {
        alert(message);
    }
}

// Real-time form validation - UPDATED
function setupFormValidation() {
    const titleInput = document.getElementById('titleInput');
    const descriptionInput = document.getElementById('descriptionInput');
    const categoryInput = document.getElementById('categoryInput');
    const conditionInput = document.getElementById('conditionInput');
    const locationInput = document.getElementById('locationInput');
    const imageInput = document.getElementById('imageInput');

    // Add event listeners for real-time validation
    const inputs = [titleInput, descriptionInput, categoryInput, conditionInput, locationInput, imageInput];

    inputs.forEach(input => {
        if (input) {
            input.addEventListener('change', triggerFormValidation);
            input.addEventListener('input', triggerFormValidation);
        }
    });

    // Character counters
    if (titleInput) {
        addCharacterCounter(titleInput, 100);
    }

    if (descriptionInput) {
        addCharacterCounter(descriptionInput, 1000);
    }
}

function triggerFormValidation() {
    const titleInput = document.getElementById('titleInput');
    const descriptionInput = document.getElementById('descriptionInput');
    const categoryInput = document.getElementById('categoryInput');
    const conditionInput = document.getElementById('conditionInput');
    const locationInput = document.getElementById('locationInput');
    const imageInput = document.getElementById('imageInput');
    const latitudeInput = document.getElementById('Latitude');
    const longitudeInput = document.getElementById('Longitude');
    const locationNameInput = document.getElementById('LocationName');
    const submitBtn = document.getElementById('submitPost');

    if (!submitBtn) return;

    // Check all required fields
    const title = titleInput?.value.trim();
    const description = descriptionInput?.value.trim();
    const category = categoryInput?.value;
    const condition = conditionInput?.value;
    const locationName = locationNameInput?.value || locationInput?.value?.trim(); // FIXED
    const hasImage = imageInput?.files?.length > 0;
    const hasCoordinates = latitudeInput?.value && longitudeInput?.value;

    const isValid = title &&
        description &&
        category &&
        condition &&
        locationName && // FIXED: Use the combined location check
        hasImage &&
        hasCoordinates;

    // Update submit button state
    submitBtn.disabled = !isValid;

    // Add visual feedback
    if (isValid) {
        submitBtn.classList.add('valid');
        submitBtn.classList.remove('invalid');
    } else {
        submitBtn.classList.remove('valid');
        submitBtn.classList.add('invalid');
    }
}

// Character counter functionality
function addCharacterCounter(input, maxLength) {
    // Create counter element if it doesn't exist
    let counter = input.parentNode.querySelector('.character-counter');
    if (!counter) {
        counter = document.createElement('div');
        counter.className = 'character-counter';
        counter.style.cssText = `
            font-size: 12px;
            color: var(--text-color);
            text-align: right;
            margin-top: 4px;
            opacity: 0.7;
        `;
        input.parentNode.appendChild(counter);
    }

    // Update counter function
    function updateCounter() {
        const remaining = maxLength - input.value.length;
        counter.textContent = `${input.value.length}/${maxLength}`;

        if (remaining < 20) {
            counter.style.color = '#e74c3c';
        } else if (remaining < 50) {
            counter.style.color = '#f39c12';
        } else {
            counter.style.color = 'var(--text-color)';
        }
    }

    // Initial update and event listener
    updateCounter();
    input.addEventListener('input', updateCounter);
}

// FIXED Google Places Autocomplete Implementation
let selectedLat = null;
let selectedLng = null;

function initGoogleAutocomplete() {
    console.log('Initializing Google Places Autocomplete...');

    const input = document.getElementById("locationInput");
    const editInput = document.getElementById("editLocationInput");

    if (!input) {
        console.log('Location input not found');
        return;
    }

    // Enhanced API check with detailed logging
    console.log('Checking Google API availability:');
    console.log('- google object:', typeof google !== 'undefined');
    console.log('- google.maps:', typeof google !== 'undefined' && !!google.maps);
    console.log('- google.maps.places:', typeof google !== 'undefined' && !!google.maps && !!google.maps.places);
    console.log('- Autocomplete:', typeof google !== 'undefined' && !!google.maps && !!google.maps.places && !!google.maps.places.Autocomplete);

    // Check if Google Maps API is loaded
    if (typeof google === 'undefined' || !google.maps || !google.maps.places || !google.maps.places.Autocomplete) {
        console.error('Google Maps Places API not properly loaded');
        setupManualLocationEntry(input);
        return;
    }

    try {
        // Initialize for create modal
        initAutocompleteForInput(input, 'create');

        // Initialize for edit modal if it exists
        if (editInput) {
            initAutocompleteForInput(editInput, 'edit');
        }

        console.log('Google Places Autocomplete initialized successfully');
    } catch (error) {
        console.error('Error initializing autocomplete:', error);
        setupManualLocationEntry(input);
    }
}

function initAutocompleteForInput(input, modalType) {
    // Create autocomplete instance with proper options
    const autocomplete = new google.maps.places.Autocomplete(input, {
        types: ['establishment', 'geocode'], // Include both establishments and addresses
        componentRestrictions: { country: 'ph' }, // Restrict to Philippines
        fields: ['formatted_address', 'geometry', 'name', 'place_id'] // Specify required fields
    });

    // Add place changed listener
    autocomplete.addListener("place_changed", () => {
        const place = autocomplete.getPlace();
        console.log('Place selected:', place);

        if (!place || !place.geometry) {
            console.log('No valid place selected');
            clearCoordinates(modalType);
            return;
        }

        const lat = place.geometry.location.lat();
        const lng = place.geometry.location.lng();
        const locationName = place.formatted_address || place.name || input.value;

        console.log('Coordinates found:', { lat, lng, locationName });

        // Update the appropriate fields based on modal type
        updateLocationFields(locationName, lat, lng, modalType);
    });

    // Clear coordinates when user starts typing
    input.addEventListener('input', function () {
        clearCoordinates(modalType);
    });
}

// FIXED: Update location fields properly
function updateLocationFields(locationName, lat, lng, modalType = 'create') {
    console.log(`Updating location fields for ${modalType}:`, { locationName, lat, lng });

    if (modalType === 'edit') {
        const latField = document.getElementById("editLatitude");
        const lngField = document.getElementById("editLongitude");

        if (latField) latField.value = lat;
        if (lngField) lngField.value = lng;
    } else {
        const latField = document.getElementById("Latitude");
        const lngField = document.getElementById("Longitude");
        const locationNameField = document.getElementById("LocationName"); // FIXED: Set the hidden field

        if (latField) latField.value = lat;
        if (lngField) lngField.value = lng;
        if (locationNameField) {
            locationNameField.value = locationName; // FIXED: This is crucial!
            console.log("Set LocationName hidden field to:", locationName);
        } else {
            console.error("LocationName hidden field not found!");
        }
    }

    // Trigger form validation
    if (typeof triggerFormValidation === 'function') {
        triggerFormValidation();
    }
}

function clearCoordinates(modalType = 'create') {
    if (modalType === 'edit') {
        const latField = document.getElementById("editLatitude");
        const lngField = document.getElementById("editLongitude");

        if (latField) latField.value = '';
        if (lngField) lngField.value = '';
    } else {
        const latField = document.getElementById("Latitude");
        const lngField = document.getElementById("Longitude");
        const locationNameField = document.getElementById("LocationName"); // FIXED

        if (latField) latField.value = '';
        if (lngField) lngField.value = '';
        if (locationNameField) locationNameField.value = ''; // FIXED: Clear hidden field too
    }

    if (typeof triggerFormValidation === 'function') {
        triggerFormValidation();
    }
}

// Fallback for manual entry - FIXED
function setupManualLocationEntry(input) {
    console.log('Setting up manual location entry fallback');

    input.placeholder = "Enter location manually (e.g., Manila, Philippines)";

    input.addEventListener('blur', async function () {
        const locationName = this.value.trim();
        if (locationName && locationName.length > 3) {
            // Set default coordinates for Philippines if no geocoding
            const defaultLat = 14.5995;  // Manila
            const defaultLng = 120.9842;

            updateLocationFields(locationName, defaultLat, defaultLng);
            console.log('Manual location set with default coordinates');
        }
    });
}

// Enhanced error checking and debugging
function debugGoogleMapsAPI() {
    console.log('=== Google Maps API Debug Info ===');
    console.log('Google object exists:', typeof google !== 'undefined');

    if (typeof google !== 'undefined') {
        console.log('Google Maps exists:', !!google.maps);
        if (google.maps) {
            console.log('Places library exists:', !!google.maps.places);
            if (google.maps.places) {
                console.log('Autocomplete exists:', !!google.maps.places.Autocomplete);
                console.log('PlaceAutocompleteElement exists:', !!google.maps.places.PlaceAutocompleteElement);
            }
        }
    }

    const locationInput = document.getElementById("locationInput");
    console.log('Location input exists:', !!locationInput);
    console.log('==================================');
}

// Global callback for Google Places API (update this in your HTML)
window.initializeGooglePlaces = function () {
    console.log('Google Places API callback triggered');
    debugGoogleMapsAPI();

    // Small delay to ensure DOM is ready
    setTimeout(() => {
        initGoogleAutocomplete();
    }, 100);
};

// Clear coordinates when user types manually - FIXED
document.addEventListener('DOMContentLoaded', function () {
    const locationInput = document.getElementById('locationInput');
    if (locationInput) {
        locationInput.addEventListener('input', function () {
            // Clear coordinates when user starts typing
            const latField = document.getElementById("Latitude");
            const lngField = document.getElementById("Longitude");
            const locationNameField = document.getElementById("LocationName"); // FIXED

            if (latField) latField.value = '';
            if (lngField) lngField.value = '';
            if (locationNameField) locationNameField.value = ''; // FIXED

            if (typeof triggerFormValidation === 'function') {
                triggerFormValidation();
            }
        });
    }
});

// Modal control functions
function closeCreateModal() {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'none';
        resetCreateForm();
    }
}

// FIXED: Reset form properly
function resetCreateForm() {
    console.log('resetCreateForm function called');

    // Reset all form inputs
    const form = document.querySelector('#createPostModal .modal-content');
    if (form) {
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            if (input.type === 'file') {
                input.value = '';
            } else if (input.type !== 'hidden') {
                input.value = '';
            }
        });

        // FIXED: Also reset hidden fields
        const latField = document.getElementById('Latitude');
        const lngField = document.getElementById('Longitude');
        const locationNameField = document.getElementById('LocationName');
        if (latField) latField.value = '';
        if (lngField) lngField.value = '';
        if (locationNameField) locationNameField.value = ''; // FIXED

        // Reset image upload section
        resetUploadSection();

        // Reset validation state
        const submitBtn = document.getElementById('submitPost');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.classList.remove('valid', 'invalid');
        }

        // Reset price display
        const finalPriceElement = document.getElementById("finalPrice");
        if (finalPriceElement) {
            finalPriceElement.textContent = "0.00";
        }

        console.log('Form reset completed');
    }
}

// DEBUG FUNCTIONS - Call these from browser console to debug
function debugLocationDetection() {
    console.log("=== LOCATION DETECTION DEBUG ===");

    const locationInput = document.getElementById("locationInput");
    const locationNameInput = document.getElementById("LocationName");
    const latitudeInput = document.getElementById("Latitude");
    const longitudeInput = document.getElementById("Longitude");

    console.log("Current location input value:", locationInput?.value);
    console.log("Current LocationName hidden field:", locationNameInput?.value);
    console.log("Current latitude:", latitudeInput?.value);
    console.log("Current longitude:", longitudeInput?.value);

    console.log("Google Places API available?", typeof google !== 'undefined' && !!google.maps?.places);

    if (locationInput) {
        // Test manual location setting
        console.log("Testing manual location setting...");
        updateLocationFields("Test Location Manila, Philippines", 14.5995, 120.9842);

        setTimeout(() => {
            console.log("After manual setting:");
            console.log("LocationName hidden field:", locationNameInput?.value);
            console.log("Latitude:", latitudeInput?.value);
            console.log("Longitude:", longitudeInput?.value);
        }, 100);
    }

    console.log("=== END LOCATION DEBUG ===");
}

function debugImagePreview() {
    console.log("=== IMAGE PREVIEW DEBUG ===");

    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const previewImg = document.getElementById('previewImg');
    const uploadLabel = document.querySelector('.upload-label');

    console.log("Image input element:", !!imageInput);
    console.log("Image preview element:", !!imagePreview);
    console.log("Preview img element:", !!previewImg);
    console.log("Upload label element:", !!uploadLabel);

    if (imageInput) {
        console.log("Files selected:", imageInput.files.length);
        if (imageInput.files.length > 0) {
            console.log("File details:", {
                name: imageInput.files[0].name,
                size: imageInput.files[0].size,
                type: imageInput.files[0].type
            });
        }
    }

    console.log("=== END IMAGE DEBUG ===");
}

// Make debug functions available globally
window.debugLocationDetection = debugLocationDetection;
window.debugImagePreview = debugImagePreview;