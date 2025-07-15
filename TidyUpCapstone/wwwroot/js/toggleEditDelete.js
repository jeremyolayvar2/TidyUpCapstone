function toggleDropdown(image) {
    const dropdown = image.closest('.dropdown-menu');
    const isActive = dropdown.classList.contains('active');

    // Close all other dropdowns
    document.querySelectorAll('.dropdown-menu.active').forEach(menu => {
        menu.classList.remove('active');
    });

    // Toggle current dropdown
    if (!isActive) {
        dropdown.classList.add('active');
    }
}

document.addEventListener('click', function (event) {
    // Close dropdown if click is outside
    if (!event.target.closest('.dropdown-menu')) {
        document.querySelectorAll('.dropdown-menu.active').forEach(menu => {
            menu.classList.remove('active');
        });
    }
});

//function openEditModal(itemId) {
//    console.log(`Edit item ${itemId}`);
//    // add your modal logic here
//}

//function deleteItem(itemId) {
//    if (confirm('Are you sure you want to delete this item?')) {
//        window.location.href = `/Item/Delete/${itemId}`;
//    }
//}