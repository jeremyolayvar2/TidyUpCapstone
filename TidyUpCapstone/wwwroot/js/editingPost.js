// Enhanced editingPost.js - Updated for new modal system with AI protection
// Works with your existing ItemController and adds AI detection protection

// Global variables for edit functionality
let currentEditingItemId = null;
let currentItemData = null;
let isAICategoryDetected = false;
let isAIConditionDetected = false;
let hasCurrentImage = false;
let removeCurrentImageFlag = false;

// ===== MAIN EDIT FUNCTIONS =====

async function openEditModal(itemId) {
    try {
        console.log('🔧 Opening edit modal for item:', itemId);
        currentEditingItemId = itemId;
        
        // Show modal with enhanced styling
        const modal = document.getElementById('editPostModal') || document.getElementById('editItemModal');
        if (!modal) {
            console.error('Edit modal not found');
            return;
        }
        
        modal.style.display = 'flex';
        setTimeout(() => modal.classList.add('show'), 10);
        
        // Fetch item data from your existing endpoint
        const response = await fetch(`/Item/Edit/${itemId}`);
        if (!response.ok) {
            throw new Error('Failed to fetch item data');
        }
        
        const data = await response.json();
        currentItemData = data;
        console.log('📋 Loaded item data:', data);
        
        // Populate form fields
        populateEditForm(data);
        
        // Set up AI protection based on item's AI processing status
        await checkAndSetupAIProtection(itemId);
        
        // Calculate initial price
        updateEditFinalPrice();
        
    } catch (error) {
        console.error('❌ Error opening edit modal:', error);
        showNotification('Failed to load item data. Please try again.', 'error');
        closeEditModal();
    }
}

function populateEditForm(data) {
    // Basic fields - using your existing IDs
    const titleInput = document.getElementById('editTitleInput') || document.getElementById('editItemTitle');
    const descriptionInput = document.getElementById('editDescriptionInput');
    const locationInput = document.getElementById('editLocationInput');

    if (titleInput) titleInput.value = data.itemTitle || '';
    if (descriptionInput) descriptionInput.value = data.description || '';
    if (locationInput) locationInput.value = data.itemLocation || '';

    // Set read-only category and condition displays
    const categoryDisplay = document.getElementById('editCategoryDisplay');
    const conditionDisplay = document.getElementById('editConditionDisplay');
    const categoryHidden = document.getElementById('editCategoryInput');
    const conditionHidden = document.getElementById('editConditionInput');

    if (categoryDisplay) categoryDisplay.textContent = data.itemCategory || 'Unknown';
    if (conditionDisplay) conditionDisplay.textContent = data.itemCondition || 'Unknown';

    // Store the IDs in hidden fields (you'll need these from your existing item data)
    if (categoryHidden) categoryHidden.value = currentItemData.categoryId || '';
    if (conditionHidden) conditionHidden.value = currentItemData.conditionId || '';
    
    // Handle latitude/longitude
    const latInput = document.getElementById('editLatitude');
    const lngInput = document.getElementById('editLongitude');
    if (latInput) latInput.value = data.latitude || '';
    if (lngInput) lngInput.value = data.longitude || '';
    
    // Handle current image display
    handleCurrentImageDisplay(data.imageUrl);
    
    // Reset new image preview
    resetNewImagePreview();
}

function handleCurrentImageDisplay(imageUrl) {
    const currentImageDisplay = document.getElementById('currentImageDisplay') || document.getElementById('currentImage');
    const currentImageContainer = document.getElementById('currentImageContainer');
    
    if (imageUrl && imageUrl !== '/assets/no-image-placeholder.svg') {
        if (currentImageDisplay) {
            currentImageDisplay.src = imageUrl;
            currentImageDisplay.style.display = 'block';
        }
        if (currentImageContainer) {
            currentImageContainer.style.display = 'block';
        }
        hasCurrentImage = true;
    } else {
        if (currentImageContainer) {
            currentImageContainer.style.display = 'none';
        }
        hasCurrentImage = false;
    }
    removeCurrentImageFlag = false;
}

function resetNewImagePreview() {
    const newImagePreview = document.getElementById('newImagePreview');
    const editImageInput = document.getElementById('editImageInput');
    
    if (newImagePreview) newImagePreview.style.display = 'none';
    if (editImageInput) editImageInput.value = '';
}

async function checkAndSetupAIProtection(itemId) {
    try {
        // Check if this item has AI processing completed
        // Based on your ItemService.cs, items with completed AI processing should be protected
        
        // First, check if the item has AI processing status
        const hasAIProcessing = currentItemData.isAiProcessed || 
                               currentItemData.aiProcessingStatus === 'Completed' ||
                               currentItemData.aiConfidenceLevel > 0;
        
        console.log('🤖 AI Protection Check:', {
            itemId,
            hasAIProcessing,
            aiConfidenceLevel: currentItemData.aiConfidenceLevel,
            aiProcessingStatus: currentItemData.aiProcessingStatus
        });
        
        if (hasAIProcessing) {
            // Enable AI protection for category (Google Vision detected)
            enableAIProtection('category');
            isAICategoryDetected = true;
            
            // Enable AI protection for condition (Vertex AI detected) if confidence > 0.6
            if (currentItemData.aiConfidenceLevel && currentItemData.aiConfidenceLevel > 0.6) {
                enableAIProtection('condition');
                isAIConditionDetected = true;
            }
        } else {
            // No AI protection needed
            disableAIProtection('category');
            disableAIProtection('condition');
            isAICategoryDetected = false;
            isAIConditionDetected = false;
        }
        
    } catch (error) {
        console.error('❌ Error checking AI protection:', error);
        // Default to no protection on error
        disableAIProtection('category');
        disableAIProtection('condition');
    }
}

function enableAIProtection(field) {
    const fieldConfigs = {
        category: {
            select: 'editCategoryInput',
            container: document.getElementById('editCategoryInput')?.parentElement,
            message: 'This category was automatically detected by our Google Vision AI and cannot be changed.'
        },
        condition: {
            select: 'editConditionInput', 
            container: document.getElementById('editConditionInput')?.parentElement,
            message: 'This condition was automatically assessed by our Vertex AI and cannot be changed.'
        }
    };
    
    const config = fieldConfigs[field];
    if (!config) return;
    
    const selectElement = document.getElementById(config.select);
    
    if (selectElement) {
        // Disable the dropdown
        selectElement.disabled = true;
        selectElement.style.background = 'linear-gradient(135deg, #f8f9ff 0%, #e8eaff 100%)';
        selectElement.style.borderColor = '#667eea';
        selectElement.style.cursor = 'not-allowed';
        selectElement.style.opacity = '0.8';
        
        // Add AI protection badge if container exists
        if (config.container) {
            addAIProtectionBadge(config.container, field);
            addAIProtectionNotice(config.container, config.message);
        }
    }
    
    console.log(`🔒 AI protection enabled for ${field}`);
}

function disableAIProtection(field) {
    const selectId = field === 'category' ? 'editCategoryInput' : 'editConditionInput';
    const selectElement = document.getElementById(selectId);
    
    if (selectElement) {
        selectElement.disabled = false;
        selectElement.style.background = '';
        selectElement.style.borderColor = '';
        selectElement.style.cursor = '';
        selectElement.style.opacity = '';
        
        // Remove AI protection elements
        removeAIProtectionElements(selectElement.parentElement);
    }
}

function addAIProtectionBadge(container, field) {
    // Remove existing badge
    const existingBadge = container.querySelector('.ai-protection-badge');
    if (existingBadge) existingBadge.remove();
    
    const badge = document.createElement('div');
    badge.className = 'ai-protection-badge';
    badge.style.cssText = `
        position: absolute;
        top: -8px;
        right: 15px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        font-size: 11px;
        font-weight: 700;
        padding: 6px 12px;
        border-radius: 20px;
        text-transform: uppercase;
        letter-spacing: 0.5px;
        z-index: 1;
        display: flex;
        align-items: center;
        gap: 5px;
        box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
    `;
    badge.innerHTML = `<span>🤖</span> AI Detected`;
    
    container.style.position = 'relative';
    container.appendChild(badge);
}

function addAIProtectionNotice(container, message) {
    // Remove existing notice
    const existingNotice = container.querySelector('.ai-protection-notice');
    if (existingNotice) existingNotice.remove();
    
    const notice = document.createElement('div');
    notice.className = 'ai-protection-notice';
    notice.style.cssText = `
        background: linear-gradient(135deg, #e8eaff 0%, #f0f2ff 100%);
        border: 2px solid #667eea;
        border-radius: 12px;
        padding: 15px;
        margin-top: 8px;
        font-size: 13px;
        color: #4a5568;
        display: flex;
        align-items: center;
        gap: 10px;
    `;
    
    notice.innerHTML = `
        <div style="
            background: #667eea;
            color: white;
            width: 24px;
            height: 24px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 12px;
            font-weight: bold;
        ">AI</div>
        <span>${message}</span>
    `;
    
    container.appendChild(notice);
}

function removeAIProtectionElements(container) {
    if (!container) return;
    
    const badge = container.querySelector('.ai-protection-badge');
    const notice = container.querySelector('.ai-protection-notice');
    
    if (badge) badge.remove();
    if (notice) notice.remove();
}

// ===== PRICE CALCULATION =====

async function updateEditFinalPrice() {
    const categorySelect = document.getElementById('editCategoryInput');
    const conditionSelect = document.getElementById('editConditionInput');
    const finalPriceElement = document.getElementById('editFinalPrice') || document.getElementById('editPriceAmount');

    if (!categorySelect || !conditionSelect || !finalPriceElement) {
        console.error('Required edit elements not found');
        return;
    }

    const categoryId = parseInt(categorySelect.value);
    const conditionId = parseInt(conditionSelect.value);

    if (!categoryId || !conditionId || isNaN(categoryId) || isNaN(conditionId)) {
        finalPriceElement.textContent = "0.00";
        return;
    }

    try {
        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result && typeof result.finalPrice === 'number') {
            finalPriceElement.textContent = result.finalPrice.toFixed(2);
        } else {
            console.error('Invalid edit price result:', result);
            finalPriceElement.textContent = "Error";
        }

    } catch (error) {
        console.error('Edit price calculation failed:', error);
        finalPriceElement.textContent = "Error";
    }
}

// ===== FORM SUBMISSION =====

async function saveItemChanges() {
    try {
        const submitBtn = document.getElementById('submitEdit') || document.getElementById('saveEditBtn');
        if (!submitBtn) {
            console.error('Submit button not found');
            return;
        }

        // Show loading state
        const originalContent = submitBtn.innerHTML;
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="loading-spinner"></span><span class="btn-text">Saving...</span>';

        // Validate form
        const validation = validateEditForm();
        if (!validation.isValid) {
            showNotification(validation.message, 'error');
            return;
        }

        // Get form values
        const titleInput = document.getElementById('editTitleInput');
        const descriptionInput = document.getElementById('editDescriptionInput');
        const locationInput = document.getElementById('editLocationInput');
        const categorySelect = document.getElementById('editCategoryInput');
        const conditionSelect = document.getElementById('editConditionInput');
        const latInput = document.getElementById('editLatitude');
        const lngInput = document.getElementById('editLongitude');
        const imageInput = document.getElementById('editImageInput');

        // DEBUG: Log what we're about to send
        console.log('Form data to send:', {
            itemId: currentEditingItemId,
            title: titleInput?.value,
            description: descriptionInput?.value,
            location: locationInput?.value,
            category: categorySelect?.value,
            condition: conditionSelect?.value,
            lat: latInput?.value,
            lng: lngInput?.value,
            hasNewImage: imageInput?.files?.length > 0
        });

        // Prepare form data
        const formData = new FormData();
        formData.append('ItemTitle', titleInput?.value || '');
        formData.append('Description', descriptionInput?.value || '');
        formData.append('CategoryId', categorySelect?.value || '');
        formData.append('ConditionId', conditionSelect?.value || '');
        formData.append('LocationName', locationInput?.value || '');
        formData.append('Latitude', latInput?.value || '');
        formData.append('Longitude', lngInput?.value || '');

        // Handle image
        if (imageInput?.files?.length > 0) {
            formData.append('ImageFile', imageInput.files[0]);
        }

        if (removeCurrentImageFlag) {
            formData.append('RemoveImage', 'true');
        }

        // Add anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        console.log('📤 Sending edit request for item:', currentEditingItemId);

        // DEBUG: Log the actual FormData contents
        console.log('FormData contents:');
        for (let [key, value] of formData.entries()) {
            console.log(`${key}: ${value}`);
        }

        // Send update request to your existing endpoint
        const response = await fetch(`/Item/Edit/${currentEditingItemId}`, {
            method: 'POST',
            body: formData
        });

        console.log('Response status:', response.status);
        console.log('Response ok:', response.ok);

        if (!response.ok) {
            const errorText = await response.text();
            console.error('Response error text:', errorText);
            throw new Error(errorText || 'Failed to update item');
        }

        const result = await response.json();
        console.log('Response result:', result);

        if (result.success) {
            showNotification('Item updated successfully!', 'success');
            closeEditModal();

            // Refresh page to show updated item
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        } else {
            throw new Error(result.message || 'Failed to update item');
        }

    } catch (error) {
        console.error('Error updating item:', error);
        showNotification(error.message || 'Failed to save changes. Please try again.', 'error');
    } finally {
        // Reset button state
        const submitBtn = document.getElementById('submitEdit') || document.getElementById('saveEditBtn');
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<span class="btn-text">Save Changes</span>';
        }
    }
}
function validateEditForm() {
    const titleInput = document.getElementById('editTitleInput');
    const descriptionInput = document.getElementById('editDescriptionInput');
    const locationInput = document.getElementById('editLocationInput');
    const latInput = document.getElementById('editLatitude');
    const lngInput = document.getElementById('editLongitude');

    const title = titleInput?.value?.trim() || '';
    const description = descriptionInput?.value?.trim() || '';
    const location = locationInput?.value?.trim() || '';
    const latitude = latInput?.value || '';
    const longitude = lngInput?.value || '';

    if (!title) return { isValid: false, message: 'Item title is required' };
    if (!description) return { isValid: false, message: 'Description is required' };
    if (!location) return { isValid: false, message: 'Location is required' };
    if (!latitude || !longitude) return { isValid: false, message: 'Location coordinates are required' };

    return { isValid: true };
}

// ===== DELETE FUNCTIONS =====

async function deleteItem(itemId) {
    if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
        return;
    }
    
    try {
        console.log('Deleting item:', itemId);
        
        // Prepare form data with anti-forgery token
        const formData = new FormData();
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }
        
        const response = await fetch(`/Item/Delete/${itemId}`, {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            showNotification('Item deleted successfully!', 'success');
            
            // Remove item from DOM
            removeItemFromDOM(itemId);
        } else {
            throw new Error(result.message || 'Failed to delete item');
        }
        
    } catch (error) {
        console.error('Error deleting item:', error);
        showNotification(error.message || 'Failed to delete item. Please try again.', 'error');
    }
}

function removeItemFromDOM(itemId) {
    const itemElement = document.querySelector(`#item-${itemId}`);
    if (itemElement) {
        // Add fade-out animation
        itemElement.style.transition = 'all 0.5s ease';
        itemElement.style.opacity = '0';
        itemElement.style.transform = 'translateY(-20px)';
        
        setTimeout(() => {
            itemElement.remove();
            
            // Check if no items remain and show empty state
            const remainingItems = document.querySelectorAll('.item-post');
            if (remainingItems.length === 0) {
                const emptyState = document.getElementById('emptyState');
                if (emptyState) {
                    emptyState.style.display = 'block';
                }
            }
        }, 500);
    }
}

// ===== MODAL CONTROL FUNCTIONS =====

function closeEditModal() {
    const modal = document.getElementById('editPostModal') || document.getElementById('editItemModal');
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => {
            modal.style.display = 'none';
            resetEditForm();
        }, 300);
    }
}

function resetEditForm() {
    // Reset all form fields
    const fields = [
        'editTitleInput', 'editItemTitle',
        'editDescriptionInput', 'editItemDescription', 
        'editLocationInput', 'editItemLocation',
        'editCategoryInput', 'editConditionInput',
        'editLatitude', 'editLongitude', 'editImageInput'
    ];
    
    fields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            if (field.type === 'file') {
                field.value = '';
            } else if (field.tagName === 'SELECT') {
                field.selectedIndex = 0;
            } else {
                field.value = '';
            }
        }
    });
    
    // Reset image displays
    handleCurrentImageDisplay(null);
    resetNewImagePreview();
    
    // Reset AI protection
    disableAIProtection('category');
    disableAIProtection('condition');
    
    // Reset global variables
    currentEditingItemId = null;
    currentItemData = null;
    isAICategoryDetected = false;
    isAIConditionDetected = false;
    hasCurrentImage = false;
    removeCurrentImageFlag = false;
    
    // Reset price display
    const priceDisplay = document.getElementById('editFinalPrice') || document.getElementById('editPriceAmount');
    if (priceDisplay) priceDisplay.textContent = '0.00';
}

// ===== IMAGE HANDLING FUNCTIONS =====

function removeCurrentImage() {
    const currentImageContainer = document.getElementById('currentImageContainer');
    if (currentImageContainer) {
        currentImageContainer.style.display = 'none';
    }
    removeCurrentImageFlag = true;
    hasCurrentImage = false;
}

function previewNewImage(event) {
    const file = event.target.files[0];
    if (!file) return;
    
    // Validate file
    if (!validateImageFile(file)) {
        event.target.value = '';
        return;
    }
    
    const reader = new FileReader();
    reader.onload = function(e) {
        const newImageDisplay = document.getElementById('newImageDisplay');
        const newImagePreview = document.getElementById('newImagePreview');
        
        if (newImageDisplay && newImagePreview) {
            newImageDisplay.src = e.target.result;
            newImagePreview.style.display = 'inline-block';
        }
    };
    reader.readAsDataURL(file);
}

function validateImageFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
        showNotification('Image size must be less than 10MB', 'error');
        return false;
    }
    
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
        showNotification('Please select a valid image file (JPEG, PNG, or WebP)', 'error');
        return false;
    }
    
    return true;
}

// ===== UTILITY FUNCTIONS =====

function setDropdownByText(selectId, textValue) {
    const selectElement = document.getElementById(selectId);
    if (!selectElement || !textValue) return;
    
    for (let option of selectElement.options) {
        if (option.text === textValue) {
            option.selected = true;
            break;
        }
    }
}

function showNotification(message, type) {
    // Create notification element
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        border-radius: 12px;
        color: white;
        font-weight: 600;
        z-index: 20000;
        max-width: 400px;
        box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2);
        animation: slideInRight 0.4s ease;
    `;
    
    // Set background based on type
    if (type === 'success') {
        notification.style.background = 'linear-gradient(135deg, #10b981 0%, #059669 100%)';
    } else {
        notification.style.background = 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)';
    }
    
    notification.textContent = message;
    document.body.appendChild(notification);
    
    // Auto-remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.4s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 400);
    }, 3000);
}
function reportItem(itemId) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Report Item',
            text: "Why are you reporting this item?",
            input: 'select',
            inputOptions: {
                'inappropriate': 'Inappropriate content',
                'spam': 'Spam',
                'fraud': 'Fraudulent listing',
                'other': 'Other'
            },
            inputPlaceholder: 'Select a reason',
            showCancelButton: true,
            confirmButtonText: 'Submit Report',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
        }).then(async (result) => {
            if (result.isConfirmed && result.value) {
                try {
                    const formData = new FormData();
                    formData.append('itemId', itemId);
                    formData.append('reason', result.value);

                    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                    if (token) formData.append('__RequestVerificationToken', token);

                    const response = await fetch('/Item/Report', {
                        method: 'POST',
                        body: formData
                    });

                    const apiResult = await response.json();

                    if (apiResult.success) {
                        Swal.fire({
                            title: 'Report Submitted',
                            text: 'Thank you for helping keep our community safe.',
                            icon: 'success',
                            timer: 2000,
                            showConfirmButton: false
                        });
                    } else {
                        throw new Error(apiResult.message);
                    }
                } catch (error) {
                    console.error('Error submitting report:', error);
                    Swal.fire({
                        title: 'Error',
                        text: 'Failed to submit report. Please try again.',
                        icon: 'error'
                    });
                }
            }
        });
    }
} 

// ===== EVENT LISTENERS =====

document.addEventListener('DOMContentLoaded', function () {
    console.log('Enhanced editingPost.js loaded');

    // Add event listener for save button - THIS IS THE NEW ADDITION
    const submitBtn = document.getElementById('submitEdit');
    if (submitBtn) {
        submitBtn.addEventListener('click', saveItemChanges);
        console.log('Save button event listener added');
    }

    // Remove price calculation listeners since category/condition are now disabled
    // (Comment out or remove the price calculation code)

    // Close modal on outside click
    const modal = document.getElementById('editPostModal') || document.getElementById('editItemModal');
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                closeEditModal();
            }
        });
    }

    // Keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const editModal = document.getElementById('editPostModal') || document.getElementById('editItemModal');
            if (editModal && editModal.style.display === 'flex') {
                closeEditModal();
            }
        }
    });
});

// ===== GLOBAL FUNCTION EXPORTS =====
window.openEditModal = openEditModal;
window.closeEditModal = closeEditModal;
window.deleteItem = deleteItem;
window.saveItemChanges = saveItemChanges;
window.removeCurrentImage = removeCurrentImage;
window.previewNewImage = previewNewImage;
window.updateEditFinalPrice = updateEditFinalPrice;
window.setDropdownByText = setDropdownByText;
window.reportItem = reportItem;

console.log('Enhanced Item Edit & Delete System with AI Protection loaded successfully');
console.log('reportItem function available:', typeof window.reportItem);
console.log('reportItem function:', window.reportItem);