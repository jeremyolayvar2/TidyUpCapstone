// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


//For toggling the sidebar -----------------------------------------------------------------------------------------

const toggleButton = document.getElementById('icon-toggle');
const sidebar = document.getElementById('sidebar');
const toggleIcon = toggleButton.querySelector('img');

function toggleSidebar() {
    sidebar.classList.toggle('close');
    toggleIcon.classList.toggle('rotate');
}


//Phone View -----------------------------------------------------------------------------------------

const dockItems = document.querySelectorAll(".dock-item");
const distance = 100;
const maxScale = 1.8;

const dockPanel = document.getElementById("dockPanel");

dockPanel.addEventListener("mousemove", (e) => {
    const rect = dockPanel.getBoundingClientRect();
    const mouseX = e.clientX;

    dockItems.forEach((item) => {
        const itemRect = item.getBoundingClientRect();
        const itemCenter = itemRect.left + itemRect.width / 2;
        const dist = Math.abs(mouseX - itemCenter);
        const scale = Math.max(1, maxScale - dist / distance);

        item.style.transform = `scale(${scale})`;
    });
});

dockPanel.addEventListener("mouseleave", () => {
    dockItems.forEach((item) => {
        item.style.transform = "scale(1)";
    });
});

dockItems.forEach((item) => {
    const label = item.querySelector(".dock-label");
    item.addEventListener("mouseenter", () => {
        label.style.opacity = "1";
        label.style.transform = "translateX(-50%) translateY(-5px)";
    });
    item.addEventListener("mouseleave", () => {
        label.style.opacity = "0";
        label.style.transform = "translateX(-50%)";
    });
});




document.addEventListener('DOMContentLoaded', function () {
    const appContainer = document.querySelector('.app-container');
    const chatWindow = document.querySelector('.chat-window');
    const newMessageBtn = document.getElementById('new-message-btn');
    const messageItems = document.querySelectorAll('.message-item');
    const closeBtn = document.getElementById('closeBtn');

    // Hide both on load
    appContainer.classList.remove('active');
    chatWindow.classList.remove('active');

    // Show app-container when new-message-btn is clicked
    newMessageBtn.addEventListener('click', function () {
        appContainer.classList.add('active');
        chatWindow.classList.remove('active');
    });

    // Show chat-window when a message-item is clicked
    messageItems.forEach(function (item) {
        item.addEventListener('click', function () {
            appContainer.classList.remove('active');
            chatWindow.classList.add('active');
        });
    });

    // Optional: Close app-container when closeBtn is clicked
    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            appContainer.classList.remove('active');
            chatWindow.classList.remove('active');
        });
    }
});

