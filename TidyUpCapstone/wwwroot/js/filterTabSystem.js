// Updated FilterTabSystem.js - Fixed version that doesn't hide posts on initial load

class FilterTabSystem {
    constructor() {
        this.currentFilter = 'all';
        this.initialized = false;
        this.initializeFilterTabs();
    }

    initializeFilterTabs() {
        console.log('FilterTabSystem: Initializing...');

        const filterTabs = document.querySelectorAll('.filter-tab');

        filterTabs.forEach(tab => {
            tab.addEventListener('click', (e) => {
                e.preventDefault();
                const filter = tab.getAttribute('data-filter');
                this.switchFilter(filter, tab);
            });
        });

        // Set initial filter based on active tab - but DON'T apply it immediately
        const activeTab = document.querySelector('.filter-tab.active');
        if (activeTab) {
            this.currentFilter = activeTab.getAttribute('data-filter') || 'all';
        }

        // Mark as initialized but don't apply filter yet
        this.initialized = true;
        console.log('FilterTabSystem: Initialized with filter:', this.currentFilter);

        // Only apply filter after a short delay to ensure posts are rendered
        setTimeout(() => {
            if (this.currentFilter === 'all') {
                // For 'all' filter, use the safe method that doesn't hide posts first
                this.applyFilterSafely(this.currentFilter);
            } else {
                this.applyFilter(this.currentFilter);
            }
        }, 300);
    }

    switchFilter(filter, clickedTab) {
        console.log('FilterTabSystem: Switching to filter:', filter);

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
        console.log('FilterTabSystem: Applying filter:', filter);

        const itemPosts = document.querySelectorAll('.item-post');
        const communityPosts = document.querySelectorAll('.community-post');
        const emptyState = document.getElementById('emptyState');
        const communityPostSection = document.getElementById('communityPostSection');

        let visibleCount = 0;

        // For 'all' filter, use the safe method
        if (filter === 'all') {
            this.applyFilterSafely(filter);
            return;
        }

        // For specific filters, hide posts first then show relevant ones
        [...itemPosts, ...communityPosts].forEach(post => {
            post.style.display = 'none';
        });

        switch (filter) {
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

    // Safe apply filter method that doesn't hide posts first (used for 'all' filter)
    applyFilterSafely(filter) {
        console.log('FilterTabSystem: Applying filter safely:', filter);

        const itemPosts = document.querySelectorAll('.item-post');
        const communityPosts = document.querySelectorAll('.community-post');
        const emptyState = document.getElementById('emptyState');
        const communityPostSection = document.getElementById('communityPostSection');

        let visibleCount = 0;

        if (filter === 'all') {
            // Show all posts without hiding them first
            [...itemPosts, ...communityPosts].forEach(post => {
                post.style.display = 'block';
                post.style.opacity = '1';
                post.style.visibility = 'visible';
                visibleCount++;
            });

            // Show community post creation section
            if (communityPostSection) {
                communityPostSection.style.display = 'block';
            }
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
            const icon = communityTab.querySelector('i') ?
                communityTab.querySelector('i').outerHTML : '';
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
        if (this.currentFilter === 'all') {
            this.applyFilterSafely(this.currentFilter);
        } else {
            this.applyFilter(this.currentFilter);
        }
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

// Initialize filter system with improved timing
document.addEventListener('DOMContentLoaded', function () {
    console.log('FilterTabSystem: DOM loaded, initializing...');

    // Wait a bit for posts to be rendered
    setTimeout(() => {
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
    }, 100);
});

// Expose for global use
window.FilterTabSystem = FilterTabSystem;