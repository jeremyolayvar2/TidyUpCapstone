// Enhanced editingPost.js - Improved UI Consistency & Better Error Handling
// ============================================================================

// Global variables
let currentEditingItemId = null;
let currentItemData = null;
let isAICategoryDetected = false;
let isAIConditionDetected = false;
let hasCurrentImage = false;
let removeCurrentImageFlag = false;

// ============================================================================
// MAIN EDIT FUNCTIONS
// ============================================================================
async function openEditModal(itemId) {
    try {
        console.log('🔧 Opening edit modal for item:', itemId);
        currentEditingItemId = itemId;

        const modal = document.getElementById('editPostModal') || document.getElementById('editItemModal');
        if (!modal) {
            console.error('Edit modal not found');
            showNotification('Edit modal not found', 'error');
            return;
        }

        // Show modal with enhanced animation
        modal.style.display = 'flex';
        setTimeout(() => modal.classList.add('show'), 10);

        // Prevent body scroll
        document.body.style.overflow = 'hidden';

        // Show loading state
        showLoadingState(modal, true);

        // Fetch item data
        const response = await fetch(`/Item/Edit/${itemId}`);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: Failed to fetch item data`);
        }

        const data = await response.json();
        currentItemData = data;
        console.log('📋 Loaded item data:', data);

        // Hide loading state
        showLoadingState(modal, false);

        // Populate form fields
        populateEditForm(data);

        // Set up AI protection
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
    // Basic fields
    const titleInput = document.getElementById('editTitleInput') || document.getElementById('editItemTitle');
    const descriptionInput = document.getElementById('editDescriptionInput');
    const locationInput = document.getElementById('editLocationInput');

    if (titleInput) titleInput.value = data.itemTitle || '';
    if (descriptionInput) descriptionInput.value = data.description || '';
    if (locationInput) locationInput.value = data.itemLocation || '';

    // Enhanced category and condition display
    const categoryDisplay = document.getElementById('editCategoryDisplay');
    const conditionDisplay = document.getElementById('editConditionDisplay');
    const categoryHidden = document.getElementById('editCategoryInput');
    const conditionHidden = document.getElementById('editConditionInput');

    if (categoryDisplay) {
        categoryDisplay.innerHTML = `
            <div class="read-only-field-content">
                <i class='bx bx-category'></i>
                <span>${data.itemCategory || 'Unknown'}</span>
            </div>
        `;
    }

    if (conditionDisplay) {
        conditionDisplay.innerHTML = `
            <div class="read-only-field-content">
                <i class='bx bx-info-circle'></i>
                <span>${data.itemCondition || 'Unknown'}</span>
            </div>
        `;
    }

    // Store IDs in hidden fields
    if (categoryHidden) categoryHidden.value = currentItemData.categoryId || '';
    if (conditionHidden) conditionHidden.value = currentItemData.conditionId || '';

    // Handle coordinates
    const latInput = document.getElementById('editLatitude');
    const lngInput = document.getElementById('editLongitude');
    if (latInput) latInput.value = data.latitude || '';
    if (lngInput) lngInput.value = data.longitude || '';

    // Handle current image
    handleCurrentImageDisplay(data.imageUrl);
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
            enableAIProtection('category');
            isAICategoryDetected = true;

            if (currentItemData.aiConfidenceLevel && currentItemData.aiConfidenceLevel > 0.6) {
                enableAIProtection('condition');
                isAIConditionDetected = true;
            }
        } else {
            disableAIProtection('category');
            disableAIProtection('condition');
            isAICategoryDetected = false;
            isAIConditionDetected = false;
        }

    } catch (error) {
        console.error('❌ Error checking AI protection:', error);
        disableAIProtection('category');
        disableAIProtection('condition');
    }
}

function enableAIProtection(field) {
    const fieldConfigs = {
        category: {
            displayId: 'editCategoryDisplay',
            message: 'This category was automatically detected by our Google Vision AI and cannot be changed.'
        },
        condition: {
            displayId: 'editConditionDisplay',
            message: 'This condition was automatically assessed by our Vertex AI and cannot be changed.'
        }
    };

    const config = fieldConfigs[field];
    if (!config) return;

    const displayElement = document.getElementById(config.displayId);

    if (displayElement) {
        // Enhanced AI protection styling
        displayElement.style.cssText = `
            padding: 14px;
            background: linear-gradient(135deg, #f8f9ff 0%, #e8eaff 100%);
            border: 2px solid #667eea;
            border-radius: 12px;
            color: #4a5568;
            font-weight: 500;
            position: relative;
            overflow: hidden;
        `;

        // Add AI protection badge
        addAIProtectionBadge(displayElement, field);
        addAIProtectionNotice(displayElement.parentElement, config.message);
    }

    console.log(`🔒 AI protection enabled for ${field}`);
}

function disableAIProtection(field) {
    const displayIds = {
        category: 'editCategoryDisplay',
        condition: 'editConditionDisplay'
    };

    const displayElement = document.getElementById(displayIds[field]);

    if (displayElement) {
        displayElement.style.cssText = `
            padding: 14px;
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            color: #6c757d;
            font-weight: 500;
        `;

        // Remove AI protection elements
        removeAIProtectionElements(displayElement.parentElement);
    }
}

function addAIProtectionBadge(container, field) {
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
    badge.innerHTML = `<i class='bx bx-bot'></i> AI Detected`;

    container.style.position = 'relative';
    container.appendChild(badge);
}

function addAIProtectionNotice(container, message) {
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
        "><i class='bx bx-bot'></i></div>
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

// ============================================================================
// PRICE CALCULATION
// ============================================================================
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
            console.error('Invalid price result:', result);
            finalPriceElement.textContent = "Error";
        }

    } catch (error) {
        console.error('Price calculation failed:', error);
        finalPriceElement.textContent = "Error";
    }
}

// ============================================================================
// ENHANCED FORM SUBMISSION
// ============================================================================
async function saveItemChanges() {
    try {
        const submitBtn = document.getElementById('submitEdit') || document.getElementById('saveEditBtn');
        if (!submitBtn) {
            console.error('Submit button not found');
            return;
        }

        // Validate form first
        const validation = validateEditForm();
        if (!validation.isValid) {
            showNotification(validation.message, 'error');
            return;
        }

        // Show enhanced loading state
        const originalContent = submitBtn.innerHTML;
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="loading-spinner"></span><span class="btn-text">Saving Changes...</span>';
        submitBtn.style.background = 'linear-gradient(135deg, #6c757d, #495057)';

        // Prepare form data
        const formData = new FormData();
        const formFields = {
            'ItemTitle': document.getElementById('editTitleInput'),
            'Description': document.getElementById('editDescriptionInput'),
            'LocationName': document.getElementById('editLocationInput'),
            'CategoryId': document.getElementById('editCategoryInput'),
            'ConditionId': document.getElementById('editConditionInput'),
            'Latitude': document.getElementById('editLatitude'),
            'Longitude': document.getElementById('editLongitude')
        };

        // Add form data
        Object.entries(formFields).forEach(([name, element]) => {
            if (element && element.value) {
                formData.append(name, element.value);
            }
        });

        // Handle image upload
        const imageInput = document.getElementById('editImageInput');
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

        // Send update request
        const response = await fetch(`/Item/Edit/${currentEditingItemId}`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || 'Failed to update item');
        }

        const result = await response.json();

        if (result.success) {
            // Success animation
            submitBtn.style.background = 'linear-gradient(135deg, #28a745, #20a744)';
            submitBtn.innerHTML = '<i class="bx bx-check"></i><span class="btn-text">Saved!</span>';

            setTimeout(() => {
                showNotification('Item updated successfully! 🎉', 'success');
                closeEditModal();

                // Refresh page
                setTimeout(() => window.location.reload(), 1500);
            }, 1000);
        } else {
            throw new Error(result.message || 'Failed to update item');
        }

    } catch (error) {
        console.error('❌ Error updating item:', error);
        showNotification(error.message || 'Failed to save changes. Please try again.', 'error');
    } finally {
        // Reset button state after delay
        setTimeout(() => {
            const submitBtn = document.getElementById('submitEdit') || document.getElementById('saveEditBtn');
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.style.background = '';
                submitBtn.innerHTML = '<span class="btn-text">Save Changes</span>';
            }
        }, 2000);
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
    if (title.length < 3) return { isValid: false, message: 'Title must be at least 3 characters' };
    if (!description) return { isValid: false, message: 'Description is required' };
    if (description.length < 10) return { isValid: false, message: 'Description must be at least 10 characters' };
    if (!location) return { isValid: false, message: 'Location is required' };
    if (!latitude || !longitude) return { isValid: false, message: 'Location coordinates are required' };

    return { isValid: true };
}

// ============================================================================
// ENHANCED DELETE FUNCTION
// ============================================================================
async function deleteItem(itemId) {
    // Enhanced confirmation dialog
    const result = await showConfirmDialog(
        'Delete Item',
        'Are you sure you want to delete this item? This action cannot be undone.',
        'Delete',
        'danger'
    );

    if (!result) return;

    try {
        console.log('🗑️ Deleting item:', itemId);

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
            showNotification('Item deleted successfully! 🗑️', 'success');
            removeItemFromDOM(itemId);
        } else {
            throw new Error(result.message || 'Failed to delete item');
        }

    } catch (error) {
        console.error('❌ Error deleting item:', error);
        showNotification(error.message || 'Failed to delete item. Please try again.', 'error');
    }
}

function removeItemFromDOM(itemId) {
    const itemElement = document.querySelector(`#item-${itemId}`);
    if (itemElement) {
        // Enhanced removal animation
        itemElement.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
        itemElement.style.opacity = '0';
        itemElement.style.transform = 'translateY(-20px) scale(0.95)';

        setTimeout(() => {
            itemElement.remove();

            // Check for empty state
            const remainingItems = document.querySelectorAll('.item-post');
            if (remainingItems.length === 0) {
                const emptyState = document.getElementById('emptyState');
                if (emptyState) {
                    emptyState.style.display = 'block';
                    emptyState.style.animation = 'fadeIn 0.5s ease-out';
                }
            }
        }, 500);
    }
}

// ============================================================================
// MODAL CONTROL FUNCTIONS
// ============================================================================
function closeEditModal() {
    const modal = document.getElementById('editPostModal') || document.getElementById('editItemModal');
    if (modal) {
        modal.classList.remove('show');

        setTimeout(() => {
            modal.style.display = 'none';
            document.body.style.overflow = '';
            resetEditForm();
        }, 300);
    }
}

function resetEditForm() {
    console.log("🔄 Resetting edit form...");

    // Reset form fields
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

    // Reset displays
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

    console.log("✅ Edit form reset completed");
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================
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

    // Auto-remove
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

function showConfirmDialog(title, message, confirmText = 'Confirm', type = 'primary') {
    return new Promise((resolve) => {
        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.6);
            backdrop-filter: blur(8px);
            z-index: 15000;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            animation: fadeIn 0.3s ease;
        `;

        const dialog = document.createElement('div');
        dialog.style.cssText = `
            background: white;
            border-radius: 16px;
            padding: 24px;
            max-width: 400px;
            width: 100%;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            transform: scale(0.9);
            transition: transform 0.3s ease;
        `;

        const colors = {
            primary: '#6B9080',
            danger: '#dc3545',
            warning: '#ffc107'
        };

        dialog.innerHTML = `
            <div style="text-align: center; margin-bottom: 20px;">
                <h3 style="margin: 0 0 12px 0; color: #333; font-size: 20px;">${title}</h3>
                <p style="margin: 0; color: #666; line-height: 1.5;">${message}</p>
            </div>
            <div style="display: flex; gap: 12px; justify-content: center;">
                <button id="cancelBtn" style="
                    padding: 12px 24px;
                    border: 2px solid #ddd;
                    background: white;
                    color: #666;
                    border-radius: 8px;
                    cursor: pointer;
                    font-weight: 600;
                    transition: all 0.3s ease;
                ">Cancel</button>
                <button id="confirmBtn" style="
                    padding: 12px 24px;
                    border: none;
                    background: ${colors[type] || colors.primary};
                    color: white;
                    border-radius: 8px;
                    cursor: pointer;
                    font-weight: 600;
                    transition: all 0.3s ease;
                ">${confirmText}</button>
            </div>
        `;

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        setTimeout(() => {
            dialog.style.transform = 'scale(1)';
        }, 10);

        // Event handlers
        const cleanup = () => {
            overlay.style.opacity = '0';
            setTimeout(() => {
                if (overlay.parentNode) {
                    document.body.removeChild(overlay);
                }
            }, 300);
        };

        dialog.querySelector('#cancelBtn').onclick = () => {
            cleanup();
            resolve(false);
        };

        dialog.querySelector('#confirmBtn').onclick = () => {
            cleanup();
            resolve(true);
        };

        overlay.onclick = (e) => {
            if (e.target === overlay) {
                cleanup();
                resolve(false);
            }
        };
    });
}

function showLoadingState(modal, show) {
    const existingLoader = modal.querySelector('.modal-loader');

    if (show) {
        if (!existingLoader) {
            const loader = document.createElement('div');
            loader.className = 'modal-loader';
            loader.style.cssText = `
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(255, 255, 255, 0.9);
                backdrop-filter: blur(4px);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 1000;
            `;
            loader.innerHTML = `
                <div style="text-align: center;">
                    <div class="loading-spinner" style="
                        width: 40px;
                        height: 40px;
                        border: 4px solid #f3f3f3;
                        border-top: 4px solid #6B9080;
                        border-radius: 50%;
                        animation: spin 1s linear infinite;
                        margin: 0 auto 16px;
                    "></div>
                    <p style="color: #666; margin: 0;">Loading item data...</p>
                </div>
            `;
            modal.appendChild(loader);
        }
    } else {
        if (existingLoader) {
            existingLoader.remove();
        }
    }
}

function reportItem(itemId) {
    showConfirmDialog(
        'Report Item',
        'Are you sure you want to report this item for violating community guidelines?',
        'Report',
        'danger'
    ).then(async (confirmed) => {
        if (confirmed) {
            try {
                const formData = new FormData();
                formData.append('itemId', itemId);
                formData.append('reason', 'inappropriate');

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (token) formData.append('__RequestVerificationToken', token);

                const response = await fetch('/Item/Report', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                if (result.success) {
                    showNotification('Thank you for helping keep our community safe. 🛡️', 'success');
                } else {
                    throw new Error(result.message);
                }
            } catch (error) {
                console.error('Error submitting report:', error);
                showNotification('Failed to submit report. Please try again.', 'error');
            }
        }
    });
}

// ============================================================================
// EVENT LISTENERS
// ============================================================================
document.addEventListener('DOMContentLoaded', function () {
    console.log('🎯 Enhanced editingPost.js loaded');

    // Save button event listener
    const submitBtn = document.getElementById('submitEdit');
    if (submitBtn) {
        submitBtn.addEventListener('click', saveItemChanges);
        console.log('✅ Save button event listener added');
    }

    // Modal close handlers
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

    console.log('✅ Event listeners initialized');
});

// ============================================================================
// GLOBAL EXPORTS
// ============================================================================
window.openEditModal = openEditModal;
window.closeEditModal = closeEditModal;
window.deleteItem = deleteItem;
window.saveItemChanges = saveItemChanges;
window.updateEditFinalPrice = updateEditFinalPrice;
window.reportItem = reportItem;

console.log('✅ Enhanced editingPost.js with improved UI consistency loaded successfully');