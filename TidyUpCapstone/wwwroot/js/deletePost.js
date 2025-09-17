async function deleteItem(itemId) {
    const confirmResult = await Swal.fire({
        title: 'Delete Item?',
        text: "Are you sure you want to delete this item?",
        icon: 'warning',
        background: '#F5F5F5',
        color: '#252422',
        showCancelButton: true,
        confirmButtonColor: '#B0413E',
        cancelButtonColor: '#A4C3B2',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel'
    });

    if (!confirmResult.isConfirmed) return;

    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenInput ? tokenInput.value : '';

    try {
     
        const response = await fetch(`/Item/Delete/${itemId}`, {
            method: "POST",
            headers: {
                "RequestVerificationToken": token
            }
        });

        if (response.ok) {
            const itemContainer = document.getElementById(`item-${itemId}`);
            if (itemContainer) {
                itemContainer.style.transition = "opacity 0.5s ease";
                itemContainer.style.opacity = 0;
                setTimeout(() => itemContainer.remove(), 500);
            }

            Swal.fire({
                icon: 'success',
                title: 'Deleted Successfully!',
                text: 'Your item has been permanently removed.',
                background: '#F5F5F5',
                color: '#B0413E',
                confirmButtonColor: '#B0413E',
                timer: 2000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });

        } else {
            const errText = await response.text();
            throw new Error(errText);
        }
    } catch (err) {
        Swal.fire({
            icon: 'error',
            title: 'Deletion Failed',
            text: err.message || 'An error occurred while deleting.',
            background: '#F5F5F5',
            color: '#252422',
            confirmButtonColor: '#6B9080'
        });
    }
}