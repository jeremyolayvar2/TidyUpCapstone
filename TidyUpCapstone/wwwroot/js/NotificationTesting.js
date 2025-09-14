// NotificationTesting.js - Testing module for notifications
let testingPanelOpen = false;
let isCreatingNotification = false;

// Test form data mappings
const testFormMappings = {
    transaction: {
        options: [
            { value: 'success', text: 'Transaction Success' },
            { value: 'cancel', text: 'Transaction Cancelled' },
            { value: 'confirmation', text: 'Transaction Confirmation' }
        ],
        fields: ['item-name']
    },
    social: {
        options: [
            { value: 'like', text: 'Post Liked' },
            { value: 'love', text: 'Post Loved' },
            { value: 'comment', text: 'New Comment' },
            { value: 'reply', text: 'Comment Reply' }
        ],
        fields: ['username', 'details']
    },
    gamification: {
        options: [
            { value: 'quest', text: 'Quest Completed' },
            { value: 'achievement', text: 'Achievement Unlocked' },
            { value: 'levelup', text: 'Level Up' },
            { value: 'leaderboard', text: 'Leaderboard Update' }
        ],
        fields: ['details']
    },
    communication: {
        options: [
            { value: 'message', text: 'New Message' },
            { value: 'interest', text: 'Interest Expressed' }
        ],
        fields: ['username']
    }
};

// Initialize testing functionality
document.addEventListener('DOMContentLoaded', function () {
    initializeTestingPanel();
    setupTestingEventListeners();
});

function initializeTestingPanel() {
    console.log('Initializing testing panel...');

    // Check if testing panel exists
    const testingPanel = document.getElementById('testing-panel');
    if (!testingPanel) {
        console.log('Testing panel not found - testing disabled');
        return;
    }

    // Initialize form state
    updateTestForm();

    console.log('Testing panel initialized successfully');
}

function setupTestingEventListeners() {
    // Add keyboard shortcuts for testing
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + T to toggle testing panel
        if ((e.ctrlKey || e.metaKey) && e.key === 't' && !e.shiftKey) {
            e.preventDefault();
            toggleTestingPanel();
        }

        // Ctrl/Cmd + Shift + T for bulk test
        if ((e.ctrlKey || e.metaKey) && e.key === 'T' && e.shiftKey) {
            e.preventDefault();
            createBulkNotifications();
        }
    });
}

function toggleTestingPanel() {
    const panel = document.getElementById('testing-panel');
    const toggleBtn = document.getElementById('toggle-testing-panel');
    const arrow = toggleBtn.querySelector('.toggle-arrow');

    if (!panel || !toggleBtn) return;

    testingPanelOpen = !testingPanelOpen;

    if (testingPanelOpen) {
        panel.classList.remove('hidden');
        arrow.textContent = '▲';
        toggleBtn.classList.add('active');

        // Smooth scroll into view
        panel.scrollIntoView({ behavior: 'smooth', block: 'nearest' });

        // Announce to screen readers
        announceToScreenReader('Testing panel opened');
    } else {
        panel.classList.add('hidden');
        arrow.textContent = '▼';
        toggleBtn.classList.remove('active');

        announceToScreenReader('Testing panel closed');
    }

    console.log('Testing panel toggled:', testingPanelOpen ? 'open' : 'closed');
}

function updateTestForm() {
    const categorySelect = document.getElementById('test-category');
    const typeSelect = document.getElementById('test-type');
    const additionalFields = document.getElementById('additional-fields');
    const createBtn = document.getElementById('create-advanced-btn');

    if (!categorySelect || !typeSelect) return;

    const selectedCategory = categorySelect.value;

    // Clear type options
    typeSelect.innerHTML = '<option value="">Select Type</option>';

    if (selectedCategory && testFormMappings[selectedCategory]) {
        // Enable type select and populate options
        typeSelect.disabled = false;

        testFormMappings[selectedCategory].options.forEach(option => {
            const optionElement = document.createElement('option');
            optionElement.value = option.value;
            optionElement.textContent = option.text;
            typeSelect.appendChild(optionElement);
        });

        // Show additional fields
        additionalFields.style.display = 'block';

        // Show/hide specific fields based on category
        const itemNameGroup = document.getElementById('test-item-name').closest('.form-group');
        const usernameGroup = document.getElementById('test-username').closest('.form-group');
        const detailsGroup = document.getElementById('test-details').closest('.form-group');

        const requiredFields = testFormMappings[selectedCategory].fields;

        itemNameGroup.style.display = requiredFields.includes('item-name') ? 'block' : 'none';
        usernameGroup.style.display = requiredFields.includes('username') ? 'block' : 'none';
        detailsGroup.style.display = requiredFields.includes('details') ? 'block' : 'none';

    } else {
        // Disable type select and hide additional fields
        typeSelect.disabled = true;
        additionalFields.style.display = 'none';
    }

    // Enable/disable create button
    createBtn.disabled = !selectedCategory || !typeSelect.value;
}

// Quick notification creation functions
async function createQuickNotification(category, type) {
    if (isCreatingNotification) return;

    console.log('Creating quick notification:', category, type);

    const button = event.target;
    const originalText = button.textContent;

    try {
        isCreatingNotification = true;

        // Update button state
        button.textContent = 'Creating...';
        button.disabled = true;

        const requestData = {
            notificationType: category,
            subType: type,
            itemName: getRandomItemName(),
            username: getRandomUsername(),
            postTitle: getRandomPostTitle(),
            details: getRandomDetails(type),
            count: 1
        };

        const response = await fetch(`/api/notification/test/${category}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        showTestingResult(`✅ Created ${type} notification successfully!`, 'success');

        // Refresh notifications after short delay
        setTimeout(() => {
            refreshNotifications();
        }, 500);

    } catch (error) {
        console.error('Error creating quick notification:', error);
        showTestingResult(`❌ Failed to create notification: ${error.message}`, 'error');
    } finally {
        // Reset button state
        button.textContent = originalText;
        button.disabled = false;
        isCreatingNotification = false;
    }
}

async function createAdvancedNotification() {
    if (isCreatingNotification) return;

    const categorySelect = document.getElementById('test-category');
    const typeSelect = document.getElementById('test-type');
    const itemNameInput = document.getElementById('test-item-name');
    const usernameInput = document.getElementById('test-username');
    const detailsInput = document.getElementById('test-details');
    const countInput = document.getElementById('test-count');
    const createBtn = document.getElementById('create-advanced-btn');

    const category = categorySelect.value;
    const type = typeSelect.value;
    const count = parseInt(countInput.value) || 1;

    if (!category || !type) {
        showTestingResult('❌ Please select category and type', 'error');
        return;
    }

    console.log('Creating advanced notification:', { category, type, count });

    const originalText = createBtn.textContent;

    try {
        isCreatingNotification = true;

        // Update button state
        createBtn.textContent = 'Creating...';
        createBtn.disabled = true;

        const requestData = {
            notificationType: category,
            subType: type,
            itemName: itemNameInput.value || getRandomItemName(),
            username: usernameInput.value || getRandomUsername(),
            postTitle: getRandomPostTitle(),
            details: detailsInput.value || getRandomDetails(type),
            count: count
        };

        const response = await fetch(`/api/notification/test/${category}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        const message = count === 1
            ? `✅ Created ${type} notification successfully!`
            : `✅ Created ${count} ${type} notifications successfully!`;

        showTestingResult(message, 'success');

        // Clear form
        clearAdvancedForm();

        // Refresh notifications after short delay
        setTimeout(() => {
            refreshNotifications();
        }, 500);

    } catch (error) {
        console.error('Error creating advanced notification:', error);
        showTestingResult(`❌ Failed to create notification: ${error.message}`, 'error');
    } finally {
        // Reset button state
        createBtn.textContent = originalText;
        createBtn.disabled = false;
        isCreatingNotification = false;
    }
}

async function createBulkNotifications() {
    if (isCreatingNotification) return;

    const countInput = document.getElementById('bulk-count');
    const count = parseInt(countInput.value) || 5;

    console.log('Creating bulk notifications:', count);

    const button = event.target;
    const originalText = button.textContent;

    try {
        isCreatingNotification = true;

        // Update button state
        button.textContent = `Creating ${count} notifications...`;
        button.disabled = true;

        const requestData = {
            count: Math.min(count, 50) // Limit to 50
        };

        const response = await fetch('/api/notification/test/bulk', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        showTestingResult(`✅ Created ${result.notifications?.length || count} random notifications successfully!`, 'success');

        // Refresh notifications after short delay
        setTimeout(() => {
            refreshNotifications();
        }, 1000);

    } catch (error) {
        console.error('Error creating bulk notifications:', error);
        showTestingResult(`❌ Failed to create bulk notifications: ${error.message}`, 'error');
    } finally {
        // Reset button state
        button.textContent = originalText;
        button.disabled = false;
        isCreatingNotification = false;
    }
}

// Management functions
async function markAllAsRead() {
    console.log('Marking all notifications as read');

    const button = event.target;
    const originalText = button.textContent;

    try {
        button.textContent = 'Marking as read...';
        button.disabled = true;

        const response = await fetch('/Notification/MarkAllAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            showTestingResult('✅ All notifications marked as read', 'success');

            // Update UI
            const notifications = document.querySelectorAll('.notification-card');
            notifications.forEach(card => {
                card.dataset.read = 'true';
                card.classList.add('read');

                // Remove mark as read button
                const markReadBtn = card.querySelector('.mark-read-btn');
                if (markReadBtn) {
                    markReadBtn.remove();
                }
            });

            // Update unread count
            updateUnreadCountDisplay(0);

            // Update current tab if showing unread
            if (currentTab === 'unread') {
                setTimeout(() => switchTab('unread'), 300);
            }
        } else {
            showTestingResult('⚠️ ' + result.message, 'warning');
        }

    } catch (error) {
        console.error('Error marking all as read:', error);
        showTestingResult(`❌ Failed to mark all as read: ${error.message}`, 'error');
    } finally {
        button.textContent = originalText;
        button.disabled = false;
    }
}

async function clearAllNotifications() {
    if (!confirm('Are you sure you want to delete ALL notifications? This action cannot be undone.')) {
        return;
    }

    console.log('Clearing all notifications');

    const button = event.target;
    const originalText = button.textContent;

    try {
        button.textContent = 'Clearing...';
        button.disabled = true;

        const response = await fetch('/api/notification/user/1/clear-all', {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        showTestingResult('✅ All notifications cleared successfully', 'success');

        // Clear UI
        const container = document.getElementById('notifications-container');
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">
                    <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="#A4C3B2" />
                    </svg>
                </div>
                <h3>No notifications</h3>
                <p>All notifications have been cleared. Create new test notifications using the testing panel.</p>
                <button onclick="toggleTestingPanel()" class="empty-test-btn">
                    🧪 Open Testing Panel to Create Test Notifications
                </button>
            </div>
        `;

        // Update unread count
        updateUnreadCountDisplay(0);

    } catch (error) {
        console.error('Error clearing notifications:', error);
        showTestingResult(`❌ Failed to clear notifications: ${error.message}`, 'error');
    } finally {
        button.textContent = originalText;
        button.disabled = false;
    }
}

async function refreshNotifications() {
    console.log('Refreshing notifications');

    const button = event?.target;
    let originalText;

    if (button) {
        originalText = button.textContent;
        button.textContent = 'Refreshing...';
        button.disabled = true;
    }

    try {
        // Get current filter
        const activeTab = document.querySelector('.tab-button.active');
        const filter = activeTab ? activeTab.id.replace('tab-', '') : 'all';

        const response = await fetch(`/Notification/GetNotifications?filter=${filter}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            updateNotificationsList(result.notifications);
            showTestingResult('✅ Notifications refreshed', 'success');

            // Update unread count
            const unreadResponse = await fetch('/Notification/GetUnreadCount');
            if (unreadResponse.ok) {
                const unreadResult = await unreadResponse.json();
                updateUnreadCountDisplay(unreadResult.count);
            }
        } else {
            showTestingResult('❌ Failed to refresh notifications', 'error');
        }

    } catch (error) {
        console.error('Error refreshing notifications:', error);
        showTestingResult(`❌ Failed to refresh: ${error.message}`, 'error');
    } finally {
        if (button) {
            button.textContent = originalText;
            button.disabled = false;
        }
    }
}

async function getNotificationSummary() {
    console.log('Getting notification summary');

    const button = event.target;
    const originalText = button.textContent;

    try {
        button.textContent = 'Loading summary...';
        button.disabled = true;

        const response = await fetch('/api/notification/test/summary', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const summary = await response.json();

        // Display summary in results
        const summaryHtml = `
            <div class="summary-display">
                <h5>📊 Notification Summary</h5>
                <div class="summary-stats">
                    <div class="stat-item">
                        <span class="stat-label">Total:</span>
                        <span class="stat-value">${summary.totalNotifications}</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-label">Unread:</span>
                        <span class="stat-value">${summary.unreadCount}</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-label">Today:</span>
                        <span class="stat-value">${summary.todayCount}</span>
                    </div>
                    <div class="stat-item">
                        <span class="stat-label">This Week:</span>
                        <span class="stat-value">${summary.thisWeekCount}</span>
                    </div>
                </div>
                ${summary.typeStats.length > 0 ? `
                    <div class="type-stats">
                        <h6>By Type:</h6>
                        ${summary.typeStats.map(stat => `
                            <div class="type-stat">
                                <span>${stat.typeName}:</span>
                                <span>${stat.count} (${stat.unreadCount} unread)</span>
                            </div>
                        `).join('')}
                    </div>
                ` : ''}
            </div>
        `;

        showTestingResult(summaryHtml, 'info', false);

    } catch (error) {
        console.error('Error getting summary:', error);
        showTestingResult(`❌ Failed to get summary: ${error.message}`, 'error');
    } finally {
        button.textContent = originalText;
        button.disabled = false;
    }
}

// Helper functions
function clearAdvancedForm() {
    document.getElementById('test-category').value = '';
    document.getElementById('test-type').value = '';
    document.getElementById('test-item-name').value = '';
    document.getElementById('test-username').value = '';
    document.getElementById('test-details').value = '';
    document.getElementById('test-count').value = '1';

    updateTestForm();
}

function updateNotificationsList(notifications) {
    const container = document.getElementById('notifications-container');

    if (notifications.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">
                    <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="#A4C3B2" />
                    </svg>
                </div>
                <h3>No notifications</h3>
                <p id="empty-message">You're all caught up! Check back later for new updates.</p>
                <button onclick="toggleTestingPanel()" class="empty-test-btn">
                    🧪 Open Testing Panel to Create Test Notifications
                </button>
            </div>
        `;
        return;
    }

    container.innerHTML = notifications.map(notification => `
        <div class="notification-card ${notification.category}-notification" 
             data-category="all" 
             data-read="${notification.isRead}" 
             data-type="${notification.category}"
             data-id="${notification.id}"
             onclick="handleNotificationClick(this, ${notification.id})">
            
            <div class="notification-content">
                <div class="notification-main">
                    <div class="notification-title-section">
                        <h3 class="notification-card-title">${notification.title}</h3>
                        <span class="notification-time">${notification.timeAgo}</span>
                    </div>
                    <div class="notification-message">
                        <p class="message-text">${notification.message}</p>
                    </div>
                </div>
                
                <div class="notification-actions">
                    ${!notification.isRead ? `
                        <button onclick="markAsRead(${notification.id}, event)" 
                                class="action-btn mark-read-btn" 
                                title="Mark as read">
                            👁️
                        </button>
                    ` : ''}
                    <button onclick="deleteNotification(${notification.id}, event)" 
                            class="action-btn delete-btn" 
                            title="Delete notification">
                        🗑️
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function updateUnreadCountDisplay(count) {
    const unreadTab = document.getElementById('tab-unread');
    if (unreadTab) {
        const badgeText = count > 0 ? ` (${count})` : '';
        unreadTab.innerHTML = `Unread${badgeText > 0 ? `<span class="unread-badge">${badgeText}</span>` : ''}`;
    }
}

function showTestingResult(message, type = 'info', isText = true) {
    const resultsContainer = document.getElementById('testing-results');
    const resultsContent = document.getElementById('results-content');

    if (!resultsContainer || !resultsContent) return;

    resultsContainer.classList.remove('hidden');

    const resultDiv = document.createElement('div');
    resultDiv.className = `testing-result ${type}`;

    if (isText) {
        resultDiv.textContent = message;
    } else {
        resultDiv.innerHTML = message;
    }

    // Add timestamp
    const timestamp = document.createElement('span');
    timestamp.className = 'result-timestamp';
    timestamp.textContent = ` - ${new Date().toLocaleTimeString()}`;
    resultDiv.appendChild(timestamp);

    // Add to top of results
    resultsContent.insertBefore(resultDiv, resultsContent.firstChild);

    // Limit to 10 results
    const results = resultsContent.children;
    if (results.length > 10) {
        resultsContent.removeChild(results[results.length - 1]);
    }

    // Auto-hide after 5 seconds for non-info messages
    if (type !== 'info') {
        setTimeout(() => {
            if (resultDiv.parentNode) {
                resultDiv.style.opacity = '0.5';
            }
        }, 5000);
    }

    // Scroll results into view
    resultsContainer.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

// Random data generators
function getRandomItemName() {
    const items = [
        'Wireless Headphones', 'Vintage Camera', 'Gaming Mouse', 'Bluetooth Speaker',
        'Coffee Maker', 'Laptop Stand', 'Desk Lamp', 'Phone Charger',
        'Tablet Case', 'Mechanical Keyboard', 'Monitor Stand', 'USB Cable'
    ];
    return items[Math.floor(Math.random() * items.length)];
}

function getRandomUsername() {
    const usernames = [
        'Ana Santos', 'Juan Dela Cruz', 'Maria Rodriguez', 'Carlos Miguel',
        'Sofia Chen', 'Miguel Torres', 'Elena Gonzalez', 'Diego Morales',
        'Isabella Kim', 'Lucas Wang', 'Carmen Lopez', 'Adrian Tan'
    ];
    return usernames[Math.floor(Math.random() * usernames.length)];
}

function getRandomPostTitle() {
    const titles = [
        'Vintage Camera Collection', 'Gaming Setup Showcase', 'Coffee Corner Design',
        'Book Organization Tips', 'Minimalist Workspace', 'Home Office Ideas',
        'Tech Gadget Review', 'DIY Project Results', 'Travel Photography',
        'Cooking Adventures', 'Fitness Journey', 'Art and Creativity'
    ];
    return titles[Math.floor(Math.random() * titles.length)];
}

function getRandomDetails(type) {
    const details = {
        quest: ['Daily Login Streak', 'First Sale Achievement', 'Community Helper', 'Photo Upload Master', 'Transaction Expert'],
        achievement: ['First Sale', 'Community Star', 'Photography Pro', 'Deal Master', 'Helper Badge'],
        levelup: [`Level ${Math.floor(Math.random() * 18) + 2}`, `Level ${Math.floor(Math.random() * 25) + 5}`],
        leaderboard: ['You moved up 5 positions!', 'New personal best ranking!', 'Top 10 this week!', 'Climbing the ranks!']
    };

    const options = details[type] || ['Great achievement!', 'Awesome progress!', 'Keep it up!'];
    return options[Math.floor(Math.random() * options.length)];
}

// Individual notification actions (extend existing functions)
// Replace the markAsRead function:
async function markAsRead(notificationId, event) {
    if (event) {
        event.stopPropagation();
    }

    try {
        const response = await fetch('/Notification/MarkAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ notificationId: notificationId })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                // Update UI
                const card = document.querySelector(`[data-id="${notificationId}"]`);
                if (card) {
                    card.dataset.read = 'true';
                    card.classList.add('read');

                    // Remove mark as read button
                    const markReadBtn = card.querySelector('.mark-read-btn');
                    if (markReadBtn) {
                        markReadBtn.remove();
                    }
                }

                // Update unread count
                const unreadResponse = await fetch('/Notification/GetUnreadCount');
                if (unreadResponse.ok) {
                    const unreadResult = await unreadResponse.json();
                    updateUnreadCountDisplay(unreadResult.count);
                }

                showEnhancedNotificationToast('Marked as read', 'success');
            }
        }
    } catch (error) {
        console.error('Error marking as read:', error);
        showEnhancedNotificationToast('Failed to mark as read', 'error');
    }
}
async function deleteNotification(notificationId, event) {
    if (event) {
        event.stopPropagation();
    }

    if (!confirm('Are you sure you want to delete this notification?')) {
        return;
    }

    try {
        const response = await fetch('/Notification/Delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ notificationId: notificationId })
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                // Remove from UI with animation
                const card = document.querySelector(`[data-id="${notificationId}"]`);
                if (card) {
                    card.style.transition = 'all 0.3s ease';
                    card.style.transform = 'translateX(100%)';
                    card.style.opacity = '0';

                    setTimeout(() => {
                        card.remove();

                        // Check if container is empty
                        const container = document.getElementById('notifications-container');
                        const remainingCards = container.querySelectorAll('.notification-card');

                        if (remainingCards.length === 0) {
                            container.innerHTML = `
                                <div class="empty-state">
                                    <div class="empty-icon">
                                        <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="#A4C3B2" />
                                        </svg>
                                    </div>
                                    <h3>No notifications</h3>
                                    <p>You're all caught up! Check back later for new updates.</p>
                                    <button onclick="toggleTestingPanel()" class="empty-test-btn">
                                        🧪 Open Testing Panel to Create Test Notifications
                                    </button>
                                </div>
                            `;
                        }
                    }, 300);
                }

                // Update unread count
                const unreadResponse = await fetch('/Notification/GetUnreadCount');
                if (unreadResponse.ok) {
                    const unreadResult = await unreadResponse.json();
                    updateUnreadCountDisplay(unreadResult.count);
                }

                showEnhancedNotificationToast('Notification deleted', 'success');
            }
        }
    } catch (error) {
        console.error('Error deleting notification:', error);
        showEnhancedNotificationToast('Failed to delete notification', 'error');
    }
}



// Export functions for global access
window.toggleTestingPanel = toggleTestingPanel;
window.updateTestForm = updateTestForm;
window.createQuickNotification = createQuickNotification;
window.createAdvancedNotification = createAdvancedNotification;
window.createBulkNotifications = createBulkNotifications;
window.markAllAsRead = markAllAsRead;
window.clearAllNotifications = clearAllNotifications;
window.refreshNotifications = refreshNotifications;
window.getNotificationSummary = getNotificationSummary;
window.markAsRead = markAsRead;
window.deleteNotification = deleteNotification;
window.handleNotificationClick = handleNotificationClick;

console.log('NotificationTesting.js loaded successfully');