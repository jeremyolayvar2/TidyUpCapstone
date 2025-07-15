function initEditGoogleAutocomplete() {
    const input = document.getElementById("editLocationInput");
    if (!input) return;

    const autocomplete = new google.maps.places.Autocomplete(input, {
        types: ['geocode'],
        componentRestrictions: { country: 'ph' }
    });

    autocomplete.addListener("place_changed", () => {
        const place = autocomplete.getPlace();

        if (place.geometry && place.geometry.location) {
            document.getElementById("editLatitude").value = place.geometry.location.lat();
            document.getElementById("editLongitude").value = place.geometry.location.lng();

            // ✅ Set the location input value to the formatted address
            if (place.formatted_address) {
                input.value = place.formatted_address;
            } else if (place.name) {
                input.value = place.name;
            }
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    if (typeof google !== 'undefined' && google.maps && google.maps.places) {
        initEditGoogleAutocomplete();
    }

    // Add event listeners for price recalculation
    const catInput = document.getElementById("editCategoryInput");
    const conInput = document.getElementById("editConditionInput");

    if (catInput && conInput) {
        catInput.addEventListener("change", () => {
            console.log("Edit category changed:", catInput.value);
            updateEditFinalPrice();
        });
        conInput.addEventListener("change", () => {
            console.log("Edit condition changed:", conInput.value);
            updateEditFinalPrice();
        });
    }
});

async function openEditModal(itemId) {
    try {
        // ✅ UPDATED: Changed from /Home/Edit to /Item/Edit
        const response = await fetch(`/Item/Edit/${itemId}`);
        if (!response.ok) throw new Error("Failed to fetch item data");

        const data = await response.json();
        console.log("Edit modal data:", data);

        document.getElementById("editItemId").value = itemId;
        document.getElementById("editTitleInput").value = data.itemTitle;
        document.getElementById("editLocationInput").value = data.itemLocation;
        document.getElementById("editDescriptionInput").value = data.description;

        setDropdownByText("editCategoryInput", data.itemCategory);
        setDropdownByText("editConditionInput", data.itemCondition);

        document.getElementById("editLatitude").value = data.latitude || "";
        document.getElementById("editLongitude").value = data.longitude || "";

        document.getElementById("editPostModal").style.display = "flex";

        setTimeout(() => {
            updateEditFinalPrice();
        }, 100);

    } catch (error) {
        console.error("Error loading post:", error);
        alert("Error loading post: " + error.message);
    }
}

function setDropdownByText(selectId, textValue) {
    const select = document.getElementById(selectId);
    if (!select || !textValue) return;

    const options = Array.from(select.options);
    const match = options.find(opt => opt.text.trim().toLowerCase() === textValue.trim().toLowerCase());
    if (match) {
        select.value = match.value;
        console.log(`Set ${selectId} to value: ${match.value} (${match.text})`);
    } else {
        console.warn(`No match found for ${selectId} with text: ${textValue}`);
    }
}

async function updateEditFinalPrice() {
    const categorySelect = document.getElementById("editCategoryInput");
    const conditionSelect = document.getElementById("editConditionInput");
    const finalPriceElement = document.getElementById("editFinalPrice");

    if (!categorySelect || !conditionSelect || !finalPriceElement) {
        console.error("Required edit elements not found");
        return;
    }

    const categoryId = parseInt(categorySelect.value);
    const conditionId = parseInt(conditionSelect.value);

    console.log("Calculating edit price for:", { categoryId, conditionId });

    // Clear price if either selection is empty
    if (!categoryId || !conditionId || isNaN(categoryId) || isNaN(conditionId)) {
        finalPriceElement.textContent = "";
        return;
    }

    try {
        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log("Edit price calculation result:", result);

        if (result && typeof result.finalPrice === 'number') {
            finalPriceElement.textContent = result.finalPrice.toFixed(2);
        } else {
            console.error("Invalid edit price result:", result);
            finalPriceElement.textContent = "Error";
        }

    } catch (error) {
        console.error("Edit price calculation failed:", error);
        finalPriceElement.textContent = "Error";
    }
}

const modal = document.getElementById('editPostModal');
if (modal) {
    window.addEventListener("click", (e) => {
        if (e.target == modal) {
            modal.style.display = "none";
        }
    });
}

// Submit edited post
document.addEventListener("DOMContentLoaded", () => {
    const submitEditBtn = document.getElementById("submitEdit");
    if (submitEditBtn) {
        submitEditBtn.addEventListener("click", async () => {
            const itemId = document.getElementById("editItemId").value;
            const latitude = document.getElementById("editLatitude").value;
            const longitude = document.getElementById("editLongitude").value;

            if (!latitude || !longitude) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Missing Location',
                    text: 'Please select a valid location.',
                    background: '#F5F5F5',
                    color: '#252422',
                    confirmButtonColor: '#6B9080'
                });
                return;
            }

            const formData = new FormData();
            formData.append("ItemTitle", document.getElementById("editTitleInput").value);
            formData.append("CategoryId", document.getElementById("editCategoryInput").value);
            formData.append("ConditionId", document.getElementById("editConditionInput").value);
            formData.append("LocationName", document.getElementById("editLocationInput").value);
            formData.append("Description", document.getElementById("editDescriptionInput").value);
            formData.append("Latitude", latitude);
            formData.append("Longitude", longitude);

            const fileInput = document.getElementById("editImageInput");
            if (fileInput && fileInput.files.length > 0) {
                formData.append("ImageFile", fileInput.files[0]);
            }

            console.log("Edit form data being sent:", [...formData.entries()]);

            try {
                // ✅ UPDATED: Changed from /Home/Edit to /Item/Edit
                const response = await fetch(`/Item/Edit/${itemId}`, {
                    method: "POST",
                    body: formData
                });

                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Updated Successfully!',
                        text: 'Your item has been updated.',
                        confirmButtonColor: '#6B9080',
                        background: '#F5F5F5',
                        color: '#252422',
                        timer: 2000,
                        showConfirmButton: false,
                        toast: true,
                        position: 'top-end'
                    }).then(() => window.location.reload());
                } else {
                    const errorText = await response.text();
                    Swal.fire({
                        icon: 'error',
                        title: 'Failed to Update',
                        text: errorText || 'Something went wrong. Please try again.',
                        confirmButtonColor: '#6B9080',
                        background: '#F5F5F5',
                        color: '#252422'
                    });
                }
            } catch (err) {
                console.error("Edit error:", err);
                Swal.fire({
                    icon: 'error',
                    title: 'Network Error',
                    text: 'Check your connection and try again.',
                    confirmButtonColor: '#6B9080',
                    background: '#F5F5F5',
                    color: '#252422'
                });
            }
        });
    }
});