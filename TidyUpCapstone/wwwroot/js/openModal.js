// Updated JavaScript for responsive price calculation
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
    const closeBtn = document.querySelector('.modal .close');
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

    // Enable/disable submit button based on required fields
    function checkFormValidity() {
        const categoryId = categoryInput?.value;
        const conditionId = conditionInput?.value;
        const title = titleInput?.value;
        const location = locationInput?.value;
        const image = imageInput?.files?.length > 0;

        if (submitBtn) {
            submitBtn.disabled = !(categoryId && conditionId && title && location && image);
        }
    }

    // Add event listeners to all required fields
    const requiredFields = [titleInput, locationInput, imageInput, categoryInput, conditionInput];
    requiredFields.forEach(field => {
        if (field) {
            field.addEventListener("change", checkFormValidity);
            field.addEventListener("input", checkFormValidity);
        }
    });

    // Initial form validation check
    checkFormValidity();
});

// ✅ UPDATED FUNCTION TO UPDATE PRICE IN CREATE MODAL
async function updateCreateFinalPrice() {
    const categorySelect = document.getElementById("categoryInput");
    const conditionSelect = document.getElementById("conditionInput");
    const finalPriceElement = document.getElementById("finalPrice");

    // Check if elements exist
    if (!categorySelect || !conditionSelect || !finalPriceElement) {
        console.error("Required elements not found for price calculation");
        return;
    }

    const categoryId = parseInt(categorySelect.value);
    const conditionId = parseInt(conditionSelect.value);

    // Clear price if either selection is invalid
    if (!categoryId || !conditionId || isNaN(categoryId) || isNaN(conditionId)) {
        finalPriceElement.textContent = "";
        return;
    }

    try {
        // Show loading state
        finalPriceElement.textContent = "Calculating...";

        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        // Update the final price display
        if (result && typeof result.finalPrice === 'number') {
            finalPriceElement.textContent = result.finalPrice.toFixed(2);
        } else {
            throw new Error("Invalid response format");
        }

        console.log("Price calculated:", result); // For debugging

    } catch (error) {
        console.error("Price calculation failed:", error);
        finalPriceElement.textContent = "Error";

        setTimeout(() => {
            finalPriceElement.textContent = "";
        }, 2000);
    }
}

// Custom dropdown functionality (only if you have custom dropdowns)
const dropdowns = document.querySelectorAll('.dropdown');

dropdowns.forEach(dropdown => {
    const select = dropdown.querySelector('.select');
    const caret = dropdown.querySelector('.caret');
    const menu = dropdown.querySelector('.menu');
    const options = dropdown.querySelectorAll('.menu li');
    const selected = dropdown.querySelector('.selected');

    if (select && caret && menu && selected) {
        select.addEventListener('click', () => {
            select.classList.toggle('select-clicked');
            caret.classList.toggle('caret-rotate');
            menu.classList.toggle('menu-open');
        });

        options.forEach(option => {
            option.addEventListener('click', () => {
                selected.innerText = option.innerText;

                select.classList.remove('select-clicked');
                caret.classList.remove('caret-rotate');
                menu.classList.remove('menu-open');

                options.forEach(opt => opt.classList.remove('active'));
                option.classList.add('active');

                // ✅ TRIGGER PRICE UPDATE FOR CUSTOM DROPDOWNS
                // If this is a category or condition dropdown, trigger price update
                const parentDropdown = option.closest('.dropdown');
                if (parentDropdown) {
                    const hiddenInput = parentDropdown.querySelector('input[type="hidden"]');
                    if (hiddenInput) {
                        hiddenInput.value = option.getAttribute('data-value');
                        updateCreateFinalPrice();
                    }
                }
            });
        });
    }
});

// Google Maps Autocomplete
document.addEventListener("DOMContentLoaded", function () {
    if (typeof google !== 'undefined' && google.maps && google.maps.places) {
        initGoogleAutocomplete();
    } else {
        console.warn("Google Maps API not loaded.");
    }
});

let selectedLat = null;
let selectedLng = null;

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
            selectedLat = place.geometry.location.lat();
            selectedLng = place.geometry.location.lng();

            // Update the hidden fields
            const latField = document.getElementById("Latitude");
            const lngField = document.getElementById("Longitude");

            if (latField) latField.value = selectedLat;
            if (lngField) lngField.value = selectedLng;
        }
    });
}

// Form submission
document.addEventListener("DOMContentLoaded", () => {
    const submitBtn = document.getElementById("submitPost");
    if (submitBtn) {
        submitBtn.addEventListener("click", async (e) => {
            e.preventDefault(); // Prevent default form submission

            const latitude = document.getElementById("Latitude")?.value;
            const longitude = document.getElementById("Longitude")?.value;

            if (!latitude || !longitude) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Location Not Ready',
                    text: 'Please wait for location detection or select a location.',
                    background: '#F5F5F5',
                    color: '#252422',
                    confirmButtonColor: '#6B9080'
                });
                return;
            }

            const formData = new FormData();
            const titleInput = document.getElementById("titleInput");
            const categoryInput = document.getElementById("categoryInput");
            const conditionInput = document.getElementById("conditionInput");
            const descriptionInput = document.getElementById("descriptionInput");
            const imageInput = document.getElementById("imageInput");
            const locationInput = document.getElementById("locationInput");

            // Validate all required fields
            if (!titleInput?.value || !categoryInput?.value || !conditionInput?.value ||
                !locationInput?.value || !imageInput?.files?.length) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Information',
                    text: 'Please fill in all required fields.',
                    background: '#F5F5F5',
                    color: '#252422',
                    confirmButtonColor: '#6B9080'
                });
                return;
            }

            formData.append("ItemTitle", titleInput.value);
            formData.append("CategoryId", parseInt(categoryInput.value));
            formData.append("ConditionId", parseInt(conditionInput.value));
            formData.append("Description", descriptionInput.value);
            formData.append("Latitude", parseFloat(latitude));
            formData.append("Longitude", parseFloat(longitude));
            formData.append("LocationName", locationInput.value);
            formData.append("ImageFile", imageInput.files[0]);

            console.log("Form data:", [...formData.entries()]);

            try {
                // Show loading state
                submitBtn.disabled = true;
                const originalText = submitBtn.textContent;
                submitBtn.textContent = "Posting...";

                const response = await fetch("/Item/Create", {
                    method: "POST",
                    body: formData
                });

                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Posted Successfully!',
                        timer: 2000,
                        toast: true,
                        position: 'top-end',
                        background: '#F5F5F5',
                        color: '#252422'
                    }).then(() => window.location.reload());
                } else {
                    const errorText = await response.text();
                    Swal.fire({
                        icon: 'error',
                        title: 'Failed to Post',
                        text: errorText || 'Try again.',
                        background: '#F5F5F5',
                        color: '#252422',
                        confirmButtonColor: '#6B9080'
                    });
                }
            } catch (err) {
                console.error("Submission error:", err);
                Swal.fire({
                    icon: 'error',
                    title: 'Network Error',
                    text: 'Check your connection and try again.',
                    background: '#F5F5F5',
                    color: '#252422',
                    confirmButtonColor: '#6B9080'
                });
            } finally {
                // Reset button state
                submitBtn.disabled = false;
                submitBtn.textContent = "Post";
            }
        });
    }
});

// Enhanced image preview functionality with proper sizing
document.addEventListener("DOMContentLoaded", function () {
    // Image preview functionality for create modal
    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const previewImg = document.getElementById('previewImg');
    const uploadSection = document.querySelector('.upload-section');
    const uploadLabel = document.querySelector('.upload-label');

    if (imageInput && imagePreview && previewImg && uploadLabel) {
        // Handle file input change
        imageInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                handleImagePreview(file, imagePreview, previewImg, uploadLabel, uploadSection);
            } else {
                // If no file selected, reset the upload section
                resetUploadSection();
            }
        });

        // Add drag and drop functionality
        setupDragAndDrop(uploadLabel, imageInput, imagePreview, previewImg, uploadSection);
    }

    // Set up remove image button click handler
    setupRemoveImageButton();

    // Image preview functionality for edit modal
    const editImageInput = document.getElementById('editImageInput');
    const editCurrentImage = document.getElementById('currentImage');

    if (editImageInput && editCurrentImage) {
        editImageInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                handleEditImagePreview(file, editCurrentImage);
            }
        });
    }

    // Form validation with image requirements
    setupFormValidation();
});

function setupRemoveImageButton() {
    // Set up the remove image button click handler using event delegation
    document.addEventListener('click', function (e) {
        if (e.target.closest('.remove-image')) {
            e.preventDefault();
            e.stopPropagation();
            removeImage();
        }
    });
}

function handleImagePreview(file, previewContainer, previewImg, uploadLabel, uploadSection) {
    // Validate file type and size
    if (!validateImageFile(file)) {
        return;
    }

    // Show loading state
    uploadSection.classList.add('loading');

    const reader = new FileReader();

    reader.onload = function (e) {
        // Set the image source
        previewImg.src = e.target.result;

        // Hide upload label and show preview
        uploadLabel.style.display = 'none';
        previewContainer.style.display = 'flex';

        // Add class for styling
        uploadSection.classList.add('has-image');
        uploadSection.classList.remove('loading', 'reset');

        // Trigger form validation
        triggerFormValidation();
    };

    reader.onerror = function () {
        showError('Failed to load image. Please try again.');
        uploadSection.classList.remove('loading');
        resetUploadSection();
    };

    reader.readAsDataURL(file);
}

// Enhanced removeImage function with proper centering reset
function removeImage() {
    console.log('removeImage function called');

    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const uploadLabel = document.querySelector('.upload-label');
    const uploadSection = document.querySelector('.upload-section');

    if (!imageInput || !imagePreview || !uploadLabel || !uploadSection) {
        console.error('Required elements not found:', {
            imageInput: !!imageInput,
            imagePreview: !!imagePreview,
            uploadLabel: !!uploadLabel,
            uploadSection: !!uploadSection
        });
        return;
    }

    // Clear the file input
    imageInput.value = '';

    // Hide preview
    imagePreview.style.display = 'none';

    // Show and properly reset the upload label with explicit flex properties
    uploadLabel.style.display = 'flex';
    uploadLabel.style.flexDirection = 'column';
    uploadLabel.style.alignItems = 'center';
    uploadLabel.style.justifyContent = 'center';
    uploadLabel.style.textAlign = 'center';
    uploadLabel.style.width = '100%';
    uploadLabel.style.height = '100%';
    uploadLabel.style.minHeight = '150px';

    // Remove styling classes and add reset class
    uploadSection.classList.remove('has-image', 'drag-over', 'loading');
    uploadSection.classList.add('reset');

    // Force a reflow to ensure styles are applied
    uploadSection.offsetHeight;

    // Trigger form validation
    triggerFormValidation();

    console.log('Image removed and upload section properly reset');
}

// Alternative reset function for upload section
function resetUploadSection() {
    console.log('resetUploadSection function called');

    const imageInput = document.getElementById('imageInput');
    const imagePreview = document.getElementById('imagePreview');
    const uploadLabel = document.querySelector('.upload-label');
    const uploadSection = document.querySelector('.upload-section');

    if (imageInput && imagePreview && uploadLabel && uploadSection) {
        // Clear the file input
        imageInput.value = '';

        // Hide preview
        imagePreview.style.display = 'none';

        // Show and reset upload label
        uploadLabel.style.display = 'flex';
        uploadLabel.style.flexDirection = 'column';
        uploadLabel.style.alignItems = 'center';
        uploadLabel.style.justifyContent = 'center';
        uploadLabel.style.textAlign = 'center';
        uploadLabel.style.width = '100%';
        uploadLabel.style.height = '100%';
        uploadLabel.style.minHeight = '150px';

        // Reset classes
        uploadSection.classList.remove('has-image', 'drag-over', 'loading');
        uploadSection.classList.add('reset');

        // Force reflow
        uploadSection.offsetHeight;

        console.log('Upload section reset completed');
    }
}

function handleEditImagePreview(file, currentImageContainer) {
    if (!validateImageFile(file)) {
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        // Create or update the image element
        let img = currentImageContainer.querySelector('img');
        if (!img) {
            img = document.createElement('img');
            img.id = 'currentImageDisplay';
            img.alt = 'Current image';
            currentImageContainer.appendChild(img);
        }

        img.src = e.target.result;
        img.style.display = 'block';
    };

    reader.readAsDataURL(file);
}

function setupDragAndDrop(uploadLabel, fileInput, previewContainer, previewImg, uploadSection) {
    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        uploadLabel.addEventListener(eventName, preventDefaults, false);
        document.body.addEventListener(eventName, preventDefaults, false);
    });

    // Highlight drop area when item is dragged over it
    ['dragenter', 'dragover'].forEach(eventName => {
        uploadLabel.addEventListener(eventName, () => {
            uploadSection.classList.add('drag-over');
        }, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        uploadLabel.addEventListener(eventName, () => {
            uploadSection.classList.remove('drag-over');
        }, false);
    });

    // Handle dropped files
    uploadLabel.addEventListener('drop', function (e) {
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            const file = files[0];
            if (validateImageFile(file)) {
                // Update the file input
                const dt = new DataTransfer();
                dt.items.add(file);
                fileInput.files = dt.files;

                // Handle the preview
                handleImagePreview(file, previewContainer, previewImg, uploadLabel, uploadSection);
            }
        }
    }, false);
}

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

function validateImageFile(file) {
    // Check file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
        showError('Please select a valid image file (JPEG, PNG, GIF, or WebP).');
        return false;
    }

    // Check file size (10MB limit)
    const maxSize = 10 * 1024 * 1024; // 10MB in bytes
    if (file.size > maxSize) {
        showError('Image file size must be less than 10MB.');
        return false;
    }

    return true;
}

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
    const submitBtn = document.getElementById('submitPost');

    if (!submitBtn) return;

    // Check all required fields
    const title = titleInput?.value.trim();
    const description = descriptionInput?.value.trim();
    const category = categoryInput?.value;
    const condition = conditionInput?.value;
    const location = locationInput?.value.trim();
    const hasImage = imageInput?.files?.length > 0;
    const hasCoordinates = latitudeInput?.value && longitudeInput?.value;

    const isValid = title &&
        description &&
        category &&
        condition &&
        location &&
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

function showError(message) {
    // Using SweetAlert2 if available, otherwise fallback to alert
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'error',
            title: 'Invalid File',
            text: message,
            background: '#F5F5F5',
            color: '#252422',
            confirmButtonColor: '#6B9080'
        });
    } else {
        alert(message);
    }
}

function closeCreateModal() {
    const modal = document.getElementById('createPostModal');
    if (modal) {
        modal.style.display = 'none';
        // Reset form when closing
        resetCreateForm();
    }
}

function closeEditModal() {
    const modal = document.getElementById('editPostModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Single, comprehensive resetCreateForm function
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

        // Reset image upload section
        resetUploadSection();

        // Reset hidden coordinate fields
        const latField = document.getElementById('Latitude');
        const lngField = document.getElementById('Longitude');
        if (latField) latField.value = '';
        if (lngField) lngField.value = '';

        // Reset validation state
        const submitBtn = document.getElementById('submitPost');
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.classList.remove('valid', 'invalid');
            submitBtn.textContent = 'Post'; // Reset button text
        }

        // Reset price display
        const finalPriceElement = document.getElementById("finalPrice");
        if (finalPriceElement) {
            finalPriceElement.textContent = "0.00";
        }

        console.log('Form reset completed');
    }
}