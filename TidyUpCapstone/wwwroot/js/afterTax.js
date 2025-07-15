// Change the pricing API endpoint to match new structure
async function updateEditFinalPrice() {
    const categorySelect = document.getElementById("editCategoryInput");
    const conditionSelect = document.getElementById("conditionInput");
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
        // ✅ UPDATED: Use new API endpoint structure
        const response = await fetch(`/api/Pricing/Calculate?categoryId=${categoryId}&conditionId=${conditionId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

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

function getBaseCap(category) {
    switch (category.toLowerCase()) {
        case "books & stationery": return 50;
        case "electronics & gadgets": return 200;
        case "toys & games": return 80;
        case "home & kitchen": return 100;
        case "furniture": return 150;
        case "appliances": return 180;
        case "health & beauty": return 70;
        case "crafts & diy supplies": return 60;
        case "school & office supplies": return 60;
        case "sentimental items": return 90;
        case "miscellaneous": return 75;
        default: return 50;
    }
}

function getConditionModifier(condition) {
    switch (condition.toLowerCase()) {
        case "brand new": return 0.25;
        case "like new": return 0.15;
        case "gently used": return 0.05;
        case "visible wear": return -0.10;
        case "for repair/parts": return -0.25;
        default: return 0;
    }
}

function calculateAfterTax(adjustedCap) {
    let taxRate = 0;
    if (adjustedCap <= 50) taxRate = 0.05;
    else if (adjustedCap <= 100) taxRate = 0.10;
    else if (adjustedCap <= 200) taxRate = 0.15;

    return adjustedCap - (adjustedCap * taxRate);
}

function updateFinalPrice() {
    const category = document.getElementById("categoryInput").value;
    const condition = document.getElementById("conditionInput").value;
    const postButton = document.getElementById("submitPost");

    if (!category || !condition) {
        document.getElementById("finalPrice").innerText = "—";
        postButton.disabled = true;
        return;
    }

    const base = getBaseCap(category);
    const modifier = getConditionModifier(condition);
    const adjustedCap = base + (base * modifier);
    const finalPrice = calculateAfterTax(adjustedCap);

    document.getElementById("finalPrice").innerText = finalPrice.toFixed(2);
    postButton.disabled = false;
}

document.getElementById("categoryInput").addEventListener("change", updateFinalPrice);
document.getElementById("conditionInput").addEventListener("change", updateFinalPrice);


document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("editCategoryInput").addEventListener("change", updateEditFinalPrice);
    document.getElementById("editConditionInput").addEventListener("change", updateEditFinalPrice);
});