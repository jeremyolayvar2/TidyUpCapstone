// afterTax.js - Complete price calculation system for both create and edit modals

// Price calculation for create modal
async function updateCreateFinalPrice() {
    const categorySelect = document.getElementById("categoryInput");
    const conditionSelect = document.getElementById("conditionInput");
    const finalPriceElement = document.getElementById("finalPrice");
    const priceBreakdownElement = document.getElementById("priceBreakdown");
    const priceDetailsElement = document.getElementById("priceDetails");

    if (!categorySelect || !conditionSelect || !finalPriceElement) {
        console.error("Required pricing elements not found");
        return;
    }

    const categoryId = parseInt(categorySelect.value);
    const conditionId = parseInt(conditionSelect.value);

    if (!categoryId || !conditionId || isNaN(categoryId) || isNaN(conditionId)) {
        finalPriceElement.textContent = "0.00";
        if (priceBreakdownElement) priceBreakdownElement.style.display = "none";
        return;
    }

    try {
        // Call pricing API
        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.error) {
            console.error("Pricing API error:", data.error);
            finalPriceElement.textContent = "Error";
            return;
        }

        // Update price display
        finalPriceElement.textContent = data.finalPrice?.toFixed(2) || "0.00";

        // Show price breakdown if elements exist
        if (priceBreakdownElement && priceDetailsElement && data.adjustedPrice !== data.finalPrice) {
            priceDetailsElement.innerHTML = `
                Base: ${data.adjustedPrice?.toFixed(2)} tokens<br>
                Tax: -${data.taxAmount?.toFixed(2)} tokens<br>
                <strong>Final: ${data.finalPrice?.toFixed(2)} tokens</strong>
            `;
            priceBreakdownElement.style.display = "block";
        }

        // Trigger form validation after price update
        if (typeof triggerFormValidation === 'function') {
            triggerFormValidation();
        }

    } catch (error) {
        console.error("Error calculating price:", error);
        finalPriceElement.textContent = "Error";
        if (priceBreakdownElement) priceBreakdownElement.style.display = "none";
    }
}

// Price calculation for edit modal
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

    if (!categoryId || !conditionId || isNaN(categoryId) || isNaN(conditionId)) {
        finalPriceElement.textContent = "";
        return;
    }

    try {
        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.error) {
            console.error("Pricing API error:", data.error);
            finalPriceElement.textContent = "Error";
            return;
        }

        finalPriceElement.textContent = data.finalPrice?.toFixed(2) || "0.00";

    } catch (error) {
        console.error("Error calculating edit price:", error);
        finalPriceElement.textContent = "Error";
    }
}

// Debounced price calculation to prevent too many API calls
let priceCalculationTimeout;

function debouncedPriceCalculation(isEdit = false) {
    clearTimeout(priceCalculationTimeout);
    priceCalculationTimeout = setTimeout(() => {
        if (isEdit) {
            updateEditFinalPrice();
        } else {
            updateCreateFinalPrice();
        }
    }, 300); // 300ms delay
}

// Enhanced initialization
document.addEventListener("DOMContentLoaded", function () {
    // Set up create modal price calculation
    const categoryInput = document.getElementById("categoryInput");
    const conditionInput = document.getElementById("conditionInput");

    if (categoryInput && conditionInput) {
        categoryInput.addEventListener("change", () => debouncedPriceCalculation(false));
        conditionInput.addEventListener("change", () => debouncedPriceCalculation(false));
    }

    // Set up edit modal price calculation
    const editCategoryInput = document.getElementById("editCategoryInput");
    const editConditionInput = document.getElementById("editConditionInput");

    if (editCategoryInput && editConditionInput) {
        editCategoryInput.addEventListener("change", () => debouncedPriceCalculation(true));
        editConditionInput.addEventListener("change", () => debouncedPriceCalculation(true));
    }
});

// Export functions for external use
window.updateCreateFinalPrice = updateCreateFinalPrice;
window.updateEditFinalPrice = updateEditFinalPrice;