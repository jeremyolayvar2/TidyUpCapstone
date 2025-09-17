// filterTabSystem.js - Complete filter tab management system

class FilterTabSystem {
    constructor() {
        this.currentFilter = 'all';
        this.initializeFilterTabs();
    }

    initializeFilterTabs() {
        const filterTabs = document.querySelectorAll('.filter-tab');

        filterTabs.forEach(tab => {
            tab.addEventListener('click', (e) => {
                e.preventDefault();
                const filter = tab.getAttribute('data-filter');
                this.switchFilter(filter, tab);
            });
        });

        // Set initial filter based on active tab
        const activeTab = document.querySelector('.filter-tab.active');
        if (activeTab) {
            this.currentFilter = activeTab.getAttribute('data-filter') || 'all';
            this.applyFilter(this.currentFilter);
        }
    }

    switchFilter(filter, clickedTab) {
        // Update active tab
        document.querySelectorAll('.filter-tab').forEach(tab => {
            tab.classList.remove('active');
        });
        clickedTab.classList.add('active');

        // Apply filter
        this.currentFilter = filter;
        this.applyFilter(filter);

        // Update URL if needed (optional)
        this.updateUrl(filter);

        // Update UI system filter if available
        if (window.uiUpdateSystem) {
            window.uiUpdateSystem.setCurrentFilter(filter);
        }
    }

    applyFilter(filter) {
        const itemPosts = document.querySelectorAll('.item-post');
        const communityPosts = document.querySelectorAll('.community-post');
        const emptyState = document.getElementById('emptyState');
        const communityPostSection = document.getElementById('communityPostSection');

        let visibleCount = 0;

        // Reset all posts to hidden
        [...itemPosts, ...communityPosts].forEach(post => {
            post.style.display = 'none';
        });

        switch (filter) {
            case 'all':
                // Show all posts
                [...itemPosts, ...communityPosts].forEach(post => {
                    post.style.display = 'block';
                    visibleCount++;
                });
                // Show community post creation section
                if (communityPostSection) {
                    communityPostSection.style.display = 'block';
                }
                break;

            case 'items':
                // Show only item posts
                itemPosts.forEach(post => {
                    post.style.display = 'block';
                    visibleCount++;
                });
                // Hide community post creation section
                if (communityPostSection) {
                    communityPostSection.style.display = 'none';
                }
                break;

            case 'community':
                // Show only community posts
                communityPosts.forEach(post => {
                    post.style.display = 'block';
                    visibleCount++;
                });
                // Show community post creation section
                if (communityPostSection) {
                    communityPostSection.style.display = 'block';
                }
                break;
        }

        // Show/hide empty state
        if (emptyState) {
            emptyState.style.display = visibleCount === 0 ? 'block' : 'none';
        }

        // Update filter counts
        this.updateFilterCounts();
    }

    updateFilterCounts() {
        const itemPosts = document.querySelectorAll('.item-post').length;
        const communityPosts = document.querySelectorAll('.community-post').length;
        const totalPosts = itemPosts + communityPosts;

        // Update tab labels (keeping them simple without counts for now)
        const allTab = document.querySelector('.filter-tab[data-filter="all"]');
        const itemsTab = document.querySelector('.filter-tab[data-filter="items"]');
        const communityTab = document.querySelector('.filter-tab[data-filter="community"]');

        if (allTab) {
            const icon = allTab.querySelector('i') ? allTab.querySelector('i').outerHTML : '';
            allTab.innerHTML = `${icon} All Posts`;
        }

        if (itemsTab) {
            const icon = itemsTab.querySelector('i') ? itemsTab.querySelector('i').outerHTML : '';
            itemsTab.innerHTML = `${icon} Items`;
        }

        if (communityTab) {
            const icon = communityTab.querySelector('i') ? communityTab.querySelector('i').outerHTML : '';
            communityTab.innerHTML = `${icon} Community`;
        }
    }

    updateUrl(filter) {
        // Optional: Update URL without page reload
        const url = new URL(window.location);
        if (filter === 'all') {
            url.searchParams.delete('filter');
        } else {
            url.searchParams.set('filter', filter);
        }
        window.history.replaceState({}, '', url);
    }

    // Method to refresh current filter after adding new content
    refreshCurrentFilter() {
        this.applyFilter(this.currentFilter);
    }

    // Get current filter
    getCurrentFilter() {
        return this.currentFilter;
    }

    // Set current filter (for external use)
    setCurrentFilter(filter) {
        this.currentFilter = filter;
    }
}

// Initialize filter system
document.addEventListener('DOMContentLoaded', function () {
    window.filterTabSystem = new FilterTabSystem();

    // Integration with UI Update System
    if (window.uiUpdateSystem) {
        // Override the addItemToUI method to work with filters
        const originalAddItemToUI = window.uiUpdateSystem.addItemToUI;

        window.uiUpdateSystem.addItemToUI = function (itemData) {
            // Call original method
            originalAddItemToUI.call(this, itemData);

            // Refresh filter after adding item
            if (window.filterTabSystem) {
                window.filterTabSystem.refreshCurrentFilter();
            }
        };
    }
});