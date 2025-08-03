// Community Hub JavaScript Functions
document.addEventListener("DOMContentLoaded", function () {
    console.log("DOM Content Loaded - Initializing Community Hub");
    initializeCommunityHub();
    initializeCommentsModal();
});

function initializeCommunityHub() {
    // Filter functionality
    setupFilterTabs();

    // Community post creation
    setupCommunityPostCreation();

    // Post interactions (like, comment, share)
    setupPostInteractions();

    // Initialize with all posts visible
    filterPosts('all');
}

function setupFilterTabs() {
    const filterTabs = document.querySelectorAll('.filter-tab');

    filterTabs.forEach(tab => {
        tab.addEventListener('click', function () {
            // Update active tab with animation
            filterTabs.forEach(t => {
                t.classList.remove('active');
                t.style.transform = 'scale(1)';
            });

            this.classList.add('active');
            this.style.transform = 'scale(1.05)';

            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);

            const filter = this.dataset.filter;
            filterPosts(filter);
        });
    });
}

function filterPosts(filter) {
    const itemPosts = document.querySelectorAll('.item-post');
    const communityPosts = document.querySelectorAll('.community-post');
    const communityPostSection = document.getElementById('communityPostSection');
    const emptyState = document.getElementById('emptyState');
    const postsContainer = document.getElementById('postsContainer');

    let visibleCount = 0;

    // Animate out posts first
    [...itemPosts, ...communityPosts].forEach(post => {
        post.style.opacity = '0';
        post.style.transform = 'translateY(-20px)';
    });

    setTimeout(() => {
        // Show/hide community post section
        if (filter === 'items') {
            communityPostSection.style.display = 'none';
        } else {
            communityPostSection.style.display = 'block';
        }

        // Filter posts
        if (filter === 'all') {
            [...itemPosts, ...communityPosts].forEach(post => {
                showPost(post);
                visibleCount++;
            });
        } else if (filter === 'items') {
            itemPosts.forEach(post => {
                showPost(post);
                visibleCount++;
            });
            communityPosts.forEach(post => hidePost(post));
        } else if (filter === 'community') {
            communityPosts.forEach(post => {
                showPost(post);
                visibleCount++;
            });
            itemPosts.forEach(post => hidePost(post));
        }

        // Show/hide empty state
        if (visibleCount === 0) {
            emptyState.style.display = 'block';
            postsContainer.style.display = 'none';
        } else {
            emptyState.style.display = 'none';
            postsContainer.style.display = 'block';
        }
    }, 200);
}

function showPost(post) {
    post.style.display = 'block';
    setTimeout(() => {
        post.style.opacity = '1';
        post.style.transform = 'translateY(0)';
    }, 50);
}

function hidePost(post) {
    post.style.display = 'none';
    post.style.opacity = '0';
    post.style.transform = 'translateY(-20px)';
}

function setupCommunityPostCreation() {
    const postInput = document.getElementById('communityPostInput');
    const postBtn = document.getElementById('postCommunityBtn');
    const addImageBtn = document.getElementById('addImageBtn');
    const imageInput = document.getElementById('communityImageInput');
    const imagePreview = document.getElementById('communityImagePreview');
    const previewImg = document.getElementById('communityPreviewImg');
    const removeImageBtn = document.getElementById('removeCommunityImage');

    if (!postInput || !postBtn) return;

    // Auto-resize textarea with character limit validation
    postInput.addEventListener('input', function () {
        const maxLength = parseInt(this.getAttribute('maxlength')) || 1000;
        const currentLength = this.value.length;

        // Enforce character limit
        if (currentLength > maxLength) {
            this.value = this.value.substring(0, maxLength);
            showNotification(`Maximum ${maxLength} characters allowed.`, 'error');
            return;
        }

        this.style.height = 'auto';
        this.style.height = Math.min(this.scrollHeight, 120) + 'px';

        // Enable/disable post button with animation
        const hasContent = this.value.trim().length > 0;
        postBtn.disabled = !hasContent;

        if (hasContent) {
            postBtn.style.transform = 'scale(1.05)';
            setTimeout(() => {
                postBtn.style.transform = 'scale(1)';
            }, 150);
        }
    });

    // Focus animation
    postInput.addEventListener('focus', function () {
        this.parentElement.parentElement.style.transform = 'scale(1.02)';
    });

    postInput.addEventListener('blur', function () {
        this.parentElement.parentElement.style.transform = 'scale(1)';
    });

    // Image upload
    if (addImageBtn && imageInput) {
        addImageBtn.addEventListener('click', () => {
            imageInput.click();
            addImageBtn.style.transform = 'scale(0.95)';
            setTimeout(() => {
                addImageBtn.style.transform = 'scale(1)';
            }, 150);
        });

        imageInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file && validateImageFile(file)) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    previewImg.src = e.target.result;
                    imagePreview.style.display = 'block';
                    imagePreview.style.opacity = '0';
                    setTimeout(() => {
                        imagePreview.style.opacity = '1';
                    }, 50);
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // Remove image
    if (removeImageBtn) {
        removeImageBtn.addEventListener('click', function () {
            imageInput.value = '';
            imagePreview.style.opacity = '0';
            setTimeout(() => {
                imagePreview.style.display = 'none';
            }, 300);
        });
    }

    // Post submission with validation
    postBtn.addEventListener('click', function () {
        const content = postInput.value.trim();
        const maxLength = parseInt(postInput.getAttribute('maxlength')) || 1000;

        if (content.length > maxLength) {
            showNotification(`Post content cannot exceed ${maxLength} characters.`, 'error');
            return;
        }

        if (content) {
            createCommunityPost(content, imageInput.files[0]);

            // Reset form with animation
            postInput.value = '';
            postInput.style.height = 'auto';
            if (imageInput) imageInput.value = '';
            if (imagePreview) {
                imagePreview.style.opacity = '0';
                setTimeout(() => {
                    imagePreview.style.display = 'none';
                }, 300);
            }
            postBtn.disabled = true;

            // Success animation
            this.textContent = 'Posted!';
            this.style.background = '#344e41';
            setTimeout(() => { 
                this.textContent = 'Post';
                this.style.background = '';
            }, 2000);
        }
    });
}

function createCommunityPost(content, imageFile) {
    const postsContainer = document.getElementById('postsContainer');
    const currentUser = getCurrentUserName();
    const currentUserAvatar = document.querySelector('.user-avatar img')?.src || '~/assets/default-avatar.svg';

    // Create new post element
    const newPost = document.createElement('div');
    newPost.className = 'community-post fade-in';
    newPost.dataset.type = 'community';

    const postId = Date.now(); // Simple ID generation

    newPost.innerHTML = `
        <div class="post-header">
            <div class="post-user-info">
                <img src="${currentUserAvatar}" alt="User" class="post-avatar" />
                <div class="post-user-details">
                    <span class="post-username">${currentUser}</span>
                    <span class="post-time">Just now</span>
                </div>
            </div>
            <div class="post-options-dropdown">
                <button class="post-options-btn">
                    <span class="material-symbols-outlined">more_horiz</span>
                </button>
            </div>
        </div>
        
        <div class="post-content">
            <p>${escapeHtml(content)}</p>
            ${imageFile ? `<div class="post-image"><img src="${URL.createObjectURL(imageFile)}" alt="Post image" style="width: 100%; border-radius: 12px; margin-top: 10px;" /></div>` : ''}
        </div>
        
        <div class="post-interactions">
            
            <div class="interaction-buttons">
                <button class="interaction-btn like-btn" data-post-id="${postId}">
                    <i class='bx bx-heart'></i>
                    <span class="likes-count">0</span>
                </button>
                <button class="interaction-btn comment-btn" data-post-id="${postId}">
                    <i class='bx bx-message-circle'></i>
        
                    <span class="comment-count">0</span>
                </button>

            </div>
        </div>
    `;

    // Insert at the beginning of posts container
    postsContainer.insertBefore(newPost, postsContainer.firstChild);

    // Success animation
    setTimeout(() => {
        newPost.classList.add('post-success');
    }, 100);

    // Here you would normally send to server
    console.log('New community post created:', content);
}

function setupPostInteractions() {
    document.addEventListener('click', function (e) {
        // Share functionality
        if (e.target.closest('.share-btn')) {
            handleShare(e.target.closest('.share-btn'));
        }
    });
}

function handleShare(btn) {
    // Simple share functionality - you can enhance this
    btn.style.transform = 'scale(1.1)';
    setTimeout(() => {
        btn.style.transform = 'scale(1)';
    }, 150);

    // Show share options or copy link
    if (navigator.share) {
        navigator.share({
            title: 'Check out this post',
            text: 'Interesting post from TidyUp community',
            url: window.location.href
        });
    } else {
        // Fallback: copy to clipboard
        navigator.clipboard.writeText(window.location.href).then(() => {
            showNotification('Link copied to clipboard!');
        });
    }
}

// COMMENTS MODAL FUNCTIONALITY
function initializeCommentsModal() {
    console.log("Initializing comments modal...");

    // Setup comment button click handlers with event delegation
    document.addEventListener('click', function (e) {
        const commentBtn = e.target.closest('.comment-btn');
        if (commentBtn) {
            e.preventDefault();
            e.stopPropagation();

            const postId = commentBtn.dataset.postId || commentBtn.getAttribute('data-post-id');
            console.log("Comment button clicked for post:", postId, commentBtn);

            if (postId) {
                openCommentsModal(postId);
            } else {
                console.error("No post ID found on comment button:", commentBtn);
            }
            return;
        }

        // Handle comment action buttons
        const commentActionBtn = e.target.closest('.comment-action-btn');
        if (commentActionBtn) {
            handleCommentAction(commentActionBtn);
            return;
        }

        // Handle like buttons
        const likeBtn = e.target.closest('.like-btn');
        if (likeBtn) {
            handleLike(likeBtn);
            return;
        }

        // Handle close modal button
        const closeBtn = e.target.closest('.close-comments-modal');
        if (closeBtn) {
            closeCommentsModal();
            return;
        }

        // Handle send comment button
        const sendBtn = e.target.closest('#sendCommentBtn');
        if (sendBtn && !sendBtn.disabled) {
            addNewComment();
            return;
        }
    });

    // Setup new comment input with character limit validation
    const newCommentInput = document.getElementById('newCommentInput');
    const sendCommentBtn = document.getElementById('sendCommentBtn');

    if (newCommentInput && sendCommentBtn) {
        newCommentInput.addEventListener('input', function () {
            const maxLength = parseInt(this.getAttribute('maxlength')) || 500;
            const currentLength = this.value.length;

            // Enforce character limit
            if (currentLength > maxLength) {
                this.value = this.value.substring(0, maxLength);
                showNotification(`Maximum ${maxLength} characters allowed for comments.`, 'error');
                return;
            }

            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 120) + 'px';

            // Enable/disable send button
            const hasContent = this.value.trim().length > 0;
            sendCommentBtn.disabled = !hasContent;
        });

        // Handle Enter key with validation
        newCommentInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                if (!sendCommentBtn.disabled) {
                    addNewComment();
                }
            }
        });
    }

    // Close modal when clicking outside
    const modal = document.getElementById('commentsModal');
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === this) {
                closeCommentsModal();
            }
        });
    } else {
        console.error("Comments modal not found in DOM!");
    }

    // Close modal with ESC key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('commentsModal');
            if (modal && modal.classList.contains('show')) {
                closeCommentsModal();
            }
        }
    });
}

// Open comments modal
function openCommentsModal(postId) {
    console.log("Opening modal for post:", postId);
    const modal = document.getElementById('commentsModal');
    const modalCount = document.getElementById('modalCommentsCount');

    if (!modal) {
        console.error("Modal element not found!");
        return;
    }

    // Get comment count from the button
    const commentBtn = document.querySelector(`[data-post-id="${postId}"].comment-btn`);
    let commentCount = '0';

    if (commentBtn) {
        const countElement = commentBtn.querySelector('.comment-count');
        commentCount = countElement ? countElement.textContent.trim() : '0';
    }

    if (modalCount) {
        modalCount.textContent = commentCount;
    }

    // Show modal with proper display and class
    modal.style.display = 'flex';
    // Use setTimeout to ensure display is applied first
    setTimeout(() => {
        modal.classList.add('show');
    }, 10);

    document.body.style.overflow = 'hidden';

    // Focus on comment input after animation
    setTimeout(() => {
        const input = document.getElementById('newCommentInput');
        if (input) input.focus();
    }, 350);

    console.log("Modal opened successfully");
}

// Close comments modal
function closeCommentsModal() {
    console.log("Closing modal");
    const modal = document.getElementById('commentsModal');
    if (modal) {
        modal.classList.remove('show');

        // Wait for animation to complete before hiding
        setTimeout(() => {
            modal.style.display = 'none';
        }, 300);

        document.body.style.overflow = 'auto';

        // Clear input
        const input = document.getElementById('newCommentInput');
        const sendBtn = document.getElementById('sendCommentBtn');
        if (input) {
            input.value = '';
            input.style.height = 'auto';
        }
        if (sendBtn) {
            sendBtn.disabled = true;
        }
    }
}

// Add new comment with enhanced validation
function addNewComment() {
    const input = document.getElementById('newCommentInput');
    const comment = input.value.trim();
    const maxLength = parseInt(input.getAttribute('maxlength')) || 500;

    if (!comment) {
        showNotification('Please enter a comment.', 'error');
        return;
    }

    if (comment.length > maxLength) {
        showNotification(`Comment cannot exceed ${maxLength} characters.`, 'error');
        input.value = input.value.substring(0, maxLength);
        return;
    }

    console.log("Adding comment:", comment);

    // Create new comment element
    const commentItem = document.createElement('div');
    commentItem.className = 'comment-item';
    commentItem.innerHTML = `
        <div class="comment-avatar-wrapper">
            <img src="${getCurrentUserAvatar()}" alt="User" class="comment-avatar">
        </div>
        <div class="comment-content">
            <div class="comment-header">
                <span class="comment-username">${getCurrentUserName()}</span>
                <span class="comment-time">Just now</span>
            </div>
            <p class="comment-text">${escapeHtml(comment)}</p>
            <div class="comment-actions">
                <button class="comment-action-btn" data-action="like" data-comment-id="new-${Date.now()}">
                    <i class='bx bx-heart'></i>
                 
                </button>
                <button class="comment-action-btn" data-action="reply" data-comment-id="new-${Date.now()}">
                     <i class='bx  bx-message-bubble-reply'></i>
                    Reply
                </button>
            </div>
        </div>
    `;

    // Add to comments list
    const container = document.getElementById('commentsListContainer');
    const demoContent = container.querySelector('.comments-demo-content');

    if (demoContent) {
        demoContent.appendChild(commentItem);
    } else {
        container.appendChild(commentItem);
    }

    // Update comment count in modal
    const modalCount = document.getElementById('modalCommentsCount');
    if (modalCount) {
        const currentCount = parseInt(modalCount.textContent) || 0;
        modalCount.textContent = currentCount + 1;
    }

    // Clear input
    input.value = '';
    input.style.height = 'auto';
    document.getElementById('sendCommentBtn').disabled = true;

    // Scroll to new comment
    commentItem.scrollIntoView({ behavior: 'smooth' });
}

// Handle comment actions
function handleCommentAction(btn) {
    const action = btn.dataset.action;
    const commentId = btn.dataset.commentId;

    console.log("Comment action:", action, "for comment:", commentId);

    if (action === 'like') {
        const icon = btn.querySelector('i');
        const isLiked = btn.classList.contains('liked');

        if (isLiked) {
            btn.classList.remove('liked');
            icon.className = 'bx bx-heart';
            btn.innerHTML = '<i class="bx bx-heart"></i>';
        } else {
            btn.classList.add('liked');
            icon.className = 'bx bxs-heart';
            btn.innerHTML = '<i class="bx bxs-heart"></i>';
        }
    } else if (action === 'reply') {
        // Focus on comment input for reply
        const input = document.getElementById('newCommentInput');
        if (input) {
            input.focus();
        }
    }
}

// Handle like functionality
function handleLike(btn) {
    const icon = btn.querySelector('i');
    const postElement = btn.closest('.community-post, .item-post');
    const likesCount = postElement.querySelector('.likes-count');

    // Animation
    btn.style.transform = 'scale(1.2)';
    setTimeout(() => {
        btn.style.transform = 'scale(1)';
    }, 150);

    if (btn.classList.contains('liked')) {
        // Unlike
        btn.classList.remove('liked');
        icon.className = 'bx bx-heart';
        btn.style.color = '';

        // Update count
        const currentCount = parseInt(likesCount.textContent) || 0;
        likesCount.textContent = `${Math.max(0, currentCount - 1)}`;
    } else {
        // Like
        btn.classList.add('liked');
        icon.className = 'bx bxs-heart';
        btn.style.color = '#e74c3c';

        // Heart animation
        icon.style.animation = 'heartBeat 0.6s ease-in-out';
        setTimeout(() => {
            icon.style.animation = '';
        }, 600);

        // Update count
        const currentCount = parseInt(likesCount.textContent) || 0;
        likesCount.textContent = `${currentCount + 1}`;
    }
}

// Utility functions
function validateImageFile(file) {
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    const maxSize = 10 * 1024 * 1024; // 10MB

    if (!allowedTypes.includes(file.type)) {
        showNotification('Please select a valid image file (JPEG, PNG, GIF, or WebP).', 'error');
        return false;
    }

    if (file.size > maxSize) {
        showNotification('Image file size must be less than 10MB.', 'error');
        return false;
    }

    return true;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function getCurrentUserAvatar() {
    return document.querySelector('.user-avatar img')?.src || '/assets/default-avatar.svg';
}

function getCurrentUserName() {
    return window.currentUserName || "You";
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'success' ? '#27ae60' : type === 'error' ? '#e74c3c' : 'var(--primary-color)'};
        color: white;
        padding: 12px 20px;
        border-radius: 8px;
        z-index: 10000;
        animation: slideInRight 0.3s ease;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        max-width: 300px;
        font-weight: 500;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// CSS for notifications and animations
const notificationStyles = `
@keyframes slideInRight {
    from { transform: translateX(100%); opacity: 0; }
    to { transform: translateX(0); opacity: 1; }
}

@keyframes slideOutRight {
    from { transform: translateX(0); opacity: 1; }
    to { transform: translateX(100%); opacity: 0; }
}

@keyframes heartBeat {
    0% { transform: scale(1); }
    14% { transform: scale(1.3); }
    28% { transform: scale(1); }
    42% { transform: scale(1.3); }
    70% { transform: scale(1); }
}
`;

// Add notification styles to document if not already added
if (!document.getElementById('notification-styles')) {
    const styleSheet = document.createElement('style');
    styleSheet.id = 'notification-styles';
    styleSheet.textContent = notificationStyles;
    document.head.appendChild(styleSheet);
}

// Make functions globally available
window.openCommentsModal = openCommentsModal;
window.closeCommentsModal = closeCommentsModal;
window.addNewComment = addNewComment;