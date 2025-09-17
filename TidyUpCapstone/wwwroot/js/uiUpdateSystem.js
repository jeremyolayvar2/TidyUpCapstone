// uiUpdateSystem.js - Complete UI update system for dynamic post management
class UIUpdateSystem {
    constructor() {
        this.postsContainer = document.getElementById('postsContainer');
        this.emptyState = document.getElementById('emptyState');
        this.currentFilter = 'all';
        this.initialize();
    }

    initialize() {
        // Get current filter from active tab
        const activeTab = document.querySelector('.filter-tab.active');
        if (activeTab) {
            this.currentFilter = activeTab.getAttribute('data-filter') || 'all';
        }
    }

    // Add a new item post to the UI
    addItemToUI(itemData) {
        if (!this.postsContainer) {
            console.error('Posts container not found');
            return;
        }

        console.log('Adding item to UI:', itemData);

        // Hide empty state if visible
        if (this.emptyState) {
            this.emptyState.style.display = 'none';
        }

        // Create the item HTML
        const itemHtml = this.createItemPostHtml(itemData);

        // Insert at the beginning of posts container
        this.postsContainer.insertAdjacentHTML('afterbegin', itemHtml);

        // Add fade-in animation
        const newPost = this.postsContainer.querySelector('.item-post:first-child');
        if (newPost) {
            newPost.style.opacity = '0';
            newPost.style.transform = 'translateY(-20px)';
            newPost.style.transition = 'all 0.5s ease';

            setTimeout(() => {
                newPost.style.opacity = '1';
                newPost.style.transform = 'translateY(0)';
            }, 100);

            // Apply current filter to new post
            this.applyFilterToPost(newPost);
        }

        // Update filter counts
        this.updateFilterCounts();
    }

    // Create HTML for a new item post
    createItemPostHtml(itemData) {
        const currentUserId = window.currentUserId || '';
        const currentUserName = window.currentUserName || 'Anonymous';

        // Generate image URL or placeholder
        const imageHtml = itemData.imageUrl ?
            `<img src="${itemData.imageUrl}" alt="${this.escapeHtml(itemData.itemTitle)}" />` :
            `<div class="no-image-placeholder">No Image Available</div>`;

        // Owner actions (edit/delete) if current user owns the item
        const ownerActions = itemData.userId.toString() === currentUserId ? `
            <div class="dropdown-menu">
                <button class="dropdown-btn" onclick="toggleDropdown(this)">
                    <img src="/assets/dot-horiz.svg" alt="dot-horiz" />
                </button>
                <div class="dropdown-content">
                    <button class="dropdown-item" onclick="openEditModal(${itemData.itemId})">Edit</button>
                    <button class="dropdown-item delete" onclick="deleteItem(${itemData.itemId})">Delete</button>
                    <button class="dropdown-item report">Report</button>
                </div>
            </div>
        ` : '';

        // Interest button or status badge
        const actionButton = itemData.status === 'Available' && !itemData.isExpired ?
            `<button class="interested-btn" onclick="createChatBubble('${itemData.userId}', '${this.escapeHtml(itemData.username)}', ${itemData.itemId}, '${this.escapeHtml(itemData.itemTitle)}')">
                <span>Interested</span>
            </button>` :
            `<div class="status-badge ${(itemData.statusDisplayName || 'unavailable').toLowerCase()}">
                ${itemData.statusDisplayName || 'Unavailable'}
            </div>`;

        return `
            <div class="item-post" data-type="item" id="item-${itemData.itemId}">
                <div class="mid-container-child">
                    <div class="listing-container1">
                        <div class="listing-container1-child1">
                            <img src="${itemData.userAvatarUrl || '/assets/default-avatar.svg'}" alt="person" />
                            <span>${this.escapeHtml(itemData.username)}</span>
                            <img src="/assets/verified-icon.svg" alt="verified" />
                            <span class="date-span">Just now</span>
                        </div>
                        ${ownerActions}
                    </div>

                    <div class="listing-container2">
                        ${imageHtml}
                    </div>

                    <div class="listing-container3">
                        <div class="listing-container3-child1">
                            <span class="material-symbols-outlined">category</span>
                            <span>${this.escapeHtml(itemData.categoryName)}</span>
                        </div>

                        <div class="listing-container3-child2">
                            <span class="material-symbols-outlined">action_key</span>
                            <span>${this.escapeHtml(itemData.conditionName)}</span>
                        </div>

                        <div class="listing-container3-child3">
                            <span class="material-symbols-outlined">location_on</span>
                            <span>${this.escapeHtml(itemData.locationName)}</span>
                        </div>
                    </div>

                    <div class="listing-container4">
                        <h3>${this.escapeHtml(itemData.itemTitle)}</h3>
                        <p>${this.escapeHtml(itemData.description)}</p>
                    </div>

                    <div class="listing-container5">
                        <div class="listing-container5-child1">
                            <span>${(itemData.finalTokenPrice || 0).toFixed(2)}</span>
                            <img src="/assets/game-icons_token.svg" alt="token" />
                        </div>

                        ${actionButton}

                        ${itemData.isAiProcessed && itemData.aiConfidenceLevel ? `
                            <div class="ai-confidence" title="AI Confidence Level">
                                <span class="material-symbols-outlined">psychology</span>
                                <span>${Math.round(itemData.aiConfidenceLevel * 100)}%</span>
                            </div>
                        ` : ''}
                    </div>

                    <!-- Add interaction buttons for item posts -->
                    <div class="post-interactions" style="border-top: 1px solid rgba(107, 144, 128, 0.1); padding-top: 15px; margin-top: 15px;">
                        <div class="interaction-stats">
                            <span class="likes-count">0 likes</span>
                            <span class="comments-count">0 comments</span>
                        </div>

                        <div class="interaction-buttons">
                            <button class="interaction-btn like-btn" data-post-id="${itemData.itemId}">
                                <i class='bx bx-heart'></i>
                                Like
                            </button>
                            <button class="interaction-btn comment-btn" data-post-id="${itemData.itemId}">
                                <i class='bx bx-message-circle'></i>
                                Comments
                                <span class="comment-count">0</span>
                            </button>
                            <button class="interaction-btn share-btn" data-post-id="${itemData.itemId}">
                                <i class='bx bx-share'></i>
                                Share
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Apply current filter to a specific post
    applyFilterToPost(post) {
        const postType = post.getAttribute('data-type');

        switch (this.currentFilter) {
            case 'all':
                post.style.display = 'block';
                break;
            case 'items':
                post.style.display = postType === 'item' ? 'block' : 'none';
                break;
            case 'community':
                post.style.display = postType === 'community' ? 'block' : 'none';
                break;
        }
    }

    // Update filter tab counts
    updateFilterCounts() {
        const itemPosts = document.querySelectorAll('.item-post').length;
        const communityPosts = document.querySelectorAll('.community-post').length;
        const totalPosts = itemPosts + communityPosts;

        // Update tab text (if you have count indicators)
        const allTab = document.querySelector('.filter-tab[data-filter="all"]');
        const itemsTab = document.querySelector('.filter-tab[data-filter="items"]');
        const communityTab = document.querySelector('.filter-tab[data-filter="community"]');

        if (allTab) {
            allTab.innerHTML = `
                <i class='bx bx-apps-alt'></i>
                All Posts
            `;
        }

        if (itemsTab) {
            itemsTab.innerHTML = `
                <i class='bx bx-shopping-bag'></i>
                Items
            `;
        }

        if (communityTab) {
            communityTab.innerHTML = `
                <i class='bx bx-community'></i>
                Community
            `;
        }
    }

    // Remove an item from UI
    removeItemFromUI(itemId) {
        const itemElement = document.getElementById(`item-${itemId}`);
        if (itemElement) {
            itemElement.style.transition = 'all 0.3s ease';
            itemElement.style.opacity = '0';
            itemElement.style.transform = 'translateX(-20px)';

            setTimeout(() => {
                itemElement.remove();
                this.updateFilterCounts();
                this.checkEmptyState();
            }, 300);
        }
    }

    // Check if we should show empty state
    checkEmptyState() {
        const visiblePosts = document.querySelectorAll('.item-post:not([style*="display: none"]), .community-post:not([style*="display: none"])');

        if (this.emptyState) {
            this.emptyState.style.display = visiblePosts.length === 0 ? 'block' : 'none';
        }
    }

    // Update current filter
    setCurrentFilter(filter) {
        this.currentFilter = filter;
    }

    // Show loading state on submit button
    showLoadingState(button) {
        const spinner = button.querySelector('.loading-spinner');
        const btnText = button.querySelector('.btn-text');

        button.disabled = true;
        if (spinner) spinner.style.display = 'inline-block';
        if (btnText) btnText.textContent = 'Posting...';
    }

    // Reset button state
    resetButtonState(button) {
        const spinner = button.querySelector('.loading-spinner');
        const btnText = button.querySelector('.btn-text');

        button.disabled = false;
        if (spinner) spinner.style.display = 'none';
        if (btnText) btnText.textContent = 'Post';
    }

    // Show notification (integrate with existing notification system)
    showNotification(message, type = 'info') {
        // Use SweetAlert2 if available
        if (typeof Swal !== 'undefined') {
            const icon = type === 'error' ? 'error' : type === 'success' ? 'success' : 'info';
            Swal.fire({
                icon: icon,
                title: type === 'error' ? 'Error' : 'Success',
                text: message,
                background: '#F5F5F5',
                color: '#252422',
                confirmButtonColor: '#6B9080',
                timer: type === 'success' ? 2000 : undefined,
                showConfirmButton: type !== 'success'
            });
        } else {
            // Fallback to console and alert
            console.log(`${type.toUpperCase()}: ${message}`);
            alert(message);
        }
    }

    // Trigger post list refresh (fallback)
    refreshPostsList() {
        // Could be used as fallback or for real-time updates
        window.location.reload();
    }

    // Escape HTML to prevent XSS
    escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function (m) { return map[m]; });
    }
}

// Initialize the UI update system
document.addEventListener('DOMContentLoaded', function () {
    window.uiUpdateSystem = new UIUpdateSystem();
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UIUpdateSystem;
}