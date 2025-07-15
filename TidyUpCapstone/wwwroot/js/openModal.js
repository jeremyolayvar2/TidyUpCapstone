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
        }
    });

    // Add close button functionality (optional)
    const closeBtn = document.querySelector('.modal .close');
    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            modal.style.display = "none";
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

        // Optional: Show user-friendly error message
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
                    text: 'Please wait for location detection or select a location.'
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
                    text: 'Please fill in all required fields.'
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
                submitBtn.textContent = "Posting...";

                // ✅ UPDATED: Changed from /Home/Create to /Item/Create
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
                        position: 'top-end'
                    }).then(() => window.location.reload());
                } else {
                    const errorText = await response.text();
                    Swal.fire({
                        icon: 'error',
                        title: 'Failed to Post',
                        text: errorText || 'Try again.'
                    });
                }
            } catch (err) {
                console.error("Submission error:", err);
                Swal.fire({
                    icon: 'error',
                    title: 'Network Error',
                    text: 'Check your connection and try again.'
                });
            } finally {
                // Reset button state
                submitBtn.disabled = false;
                submitBtn.textContent = "Post";
            }
        });
    }
});