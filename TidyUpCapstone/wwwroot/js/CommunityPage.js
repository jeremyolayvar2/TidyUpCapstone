// ============================================================================
// CLEAN COMMUNITY HUB - Enhanced JavaScript (Community Posts Only)
// ============================================================================

class CommunityHub {
    constructor() {
        this.currentUserId = window.currentUserId || null;
        this.currentUserName = window.currentUserName || "You";
        this.maxPostLength = 1000;
        this.maxCommentLength = 1000;
        this.maxImageSize = 10 * 1024 * 1024; // 10MB
        this.allowedImageTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    }

    init() {
        this.setupEventListeners();
        this.initializeComponents();
        this.loadCommunityPosts();
    }

    setupEventListeners() {
        document.addEventListener("DOMContentLoaded", () => this.init());
        document.addEventListener('click', (e) => this.handleGlobalClicks(e));
        document.addEventListener('keydown', (e) => this.handleGlobalKeyboard(e));
    }

    initializeComponents() {
        this.setupFilterTabs();
        this.setupPostCreation();
        this.setupReactionSystem();
        this.setupCommentsSystem();
        this.setupDoubleClickReactions();
        this.filterPosts('all');
    }

    // ============================================================================
    // FILTER SYSTEM
    // ============================================================================

    setupFilterTabs() {
        const filterTabs = document.querySelectorAll('.filter-tab');
        filterTabs.forEach(tab => {
            tab.addEventListener('click', () => {
                this.setActiveTab(tab);
                this.filterPosts(tab.dataset.filter);
            });
        });
    }

    setActiveTab(activeTab) {
        document.querySelectorAll('.filter-tab').forEach(tab => {
            tab.classList.remove('active');
            tab.style.transform = 'scale(1)';
        });

        activeTab.classList.add('active');
        this.animateButton(activeTab);
    }

    filterPosts(filter) {
        const itemPosts = document.querySelectorAll('.item-post');
        const communityPosts = document.querySelectorAll('.community-post');
        const communitySection = document.getElementById('communityPostSection');

        this.hideAllPosts([...itemPosts, ...communityPosts]);

        setTimeout(() => {
            let visibleCount = 0;

            if (filter === 'items') {
                communitySection.style.display = 'none';
                itemPosts.forEach(post => {
                    this.showPost(post);
                    visibleCount++;
                });
            } else {
                communitySection.style.display = 'block';

                if (filter === 'all') {
                    [...itemPosts, ...communityPosts].forEach(post => {
                        this.showPost(post);
                        visibleCount++;
                    });
                } else if (filter === 'community') {
                    communityPosts.forEach(post => {
                        this.showPost(post);
                        visibleCount++;
                    });
                }
            }

            this.toggleEmptyState(visibleCount === 0);
        }, 200);
    }

    hideAllPosts(posts) {
        posts.forEach(post => {
            post.style.opacity = '0';
            post.style.transform = 'translateY(-20px)';
        });
    }

    showPost(post) {
        post.style.display = 'block';
        setTimeout(() => {
            post.style.opacity = '1';
            post.style.transform = 'translateY(0)';
        }, 50);
    }

    toggleEmptyState(show) {
        const emptyState = document.getElementById('emptyState');
        const postsContainer = document.getElementById('postsContainer');

        if (emptyState && postsContainer) {
            emptyState.style.display = show ? 'block' : 'none';
            postsContainer.style.display = show ? 'none' : 'block';
        }
    }

    // ============================================================================
    // POST CREATION
    // ============================================================================

    setupPostCreation() {
        const postInput = document.getElementById('communityPostInput');
        const postBtn = document.getElementById('postCommunityBtn');
        const addImageBtn = document.getElementById('addImageBtn');
        const imageInput = document.getElementById('communityImageInput');

        if (!postInput || !postBtn) return;

        postInput.addEventListener('input', () => this.handlePostInputChange(postInput, postBtn));
        postInput.addEventListener('focus', () => this.animatePostContainer(postInput, true));
        postInput.addEventListener('blur', () => this.animatePostContainer(postInput, false));

        postBtn.addEventListener('click', () => this.handlePostSubmit(postInput, imageInput, postBtn));

        if (addImageBtn && imageInput) {
            addImageBtn.addEventListener('click', () => {
                imageInput.click();
                this.animateButton(addImageBtn);
            });
            imageInput.addEventListener('change', (e) => this.handleImageSelect(e));
        }

        this.setupImageRemoval();
    }

    handlePostInputChange(input, button) {
        const currentLength = input.value.length;

        if (currentLength > this.maxPostLength) {
            input.value = input.value.substring(0, this.maxPostLength);
            this.showNotification(`Maximum ${this.maxPostLength} characters allowed.`, 'error');
            return;
        }

        this.autoResize(input);

        const hasContent = input.value.trim().length > 0;
        button.disabled = !hasContent;

        if (hasContent) this.animateButton(button);
    }

    animatePostContainer(input, focus) {
        const container = input.parentElement.parentElement;
        container.style.transform = focus ? 'scale(1.02)' : 'scale(1)';
    }

    async handlePostSubmit(input, imageInput, button) {
        const content = input.value.trim();

        if (content.length > this.maxPostLength) {
            this.showNotification(`Post content cannot exceed ${this.maxPostLength} characters.`, 'error');
            return;
        }

        if (content) {
            await this.createCommunityPost(content, imageInput?.files[0], button);
        }
    }

    setupImageRemoval() {
        const removeBtn = document.getElementById('removeCommunityImage');
        if (removeBtn) {
            removeBtn.addEventListener('click', () => {
                const imageInput = document.getElementById('communityImageInput');
                const imagePreview = document.getElementById('communityImagePreview');

                if (imageInput) imageInput.value = '';
                if (imagePreview) {
                    imagePreview.style.opacity = '0';
                    setTimeout(() => imagePreview.style.display = 'none', 300);
                }
            });
        }
    }

    handleImageSelect(event) {
        const file = event.target.files[0];
        if (file && this.validateImage(file)) {
            this.previewImage(file);
        }
    }

    validateImage(file) {
        if (!this.allowedImageTypes.includes(file.type)) {
            this.showNotification('Please select a valid image file (JPEG, PNG, GIF, or WebP).', 'error');
            return false;
        }

        if (file.size > this.maxImageSize) {
            this.showNotification('Image file size must be less than 10MB.', 'error');
            return false;
        }

        return true;
    }

    previewImage(file) {
        const reader = new FileReader();
        const previewImg = document.getElementById('communityPreviewImg');
        const imagePreview = document.getElementById('communityImagePreview');

        reader.onload = (e) => {
            if (previewImg && imagePreview) {
                previewImg.src = e.target.result;
                imagePreview.style.display = 'block';
                imagePreview.style.opacity = '0';
                setTimeout(() => imagePreview.style.opacity = '1', 50);
            }
        };
        reader.readAsDataURL(file);
    }

    // ============================================================================
    // REACTION SYSTEM
    // ============================================================================

    setupReactionSystem() {
        this.addHeartOverlays();
        this.setupLikeButtons();
    }

    setupDoubleClickReactions() {
        let tapCount = 0;
        let singleTapTimer = null;

        document.addEventListener('click', (e) => {
            const communityPost = e.target.closest('.community-post');
            if (!communityPost || e.target.closest('button, a, input, textarea, .post-options-dropdown')) {
                return;
            }

            tapCount++;

            if (tapCount === 1) {
                singleTapTimer = setTimeout(() => tapCount = 0, 300);
            } else if (tapCount === 2) {
                clearTimeout(singleTapTimer);
                tapCount = 0;
                this.handleDoubleClickLike(communityPost);
            }
        });
    }

    addHeartOverlays() {
        const communityPosts = document.querySelectorAll('.community-post');
        communityPosts.forEach(post => {
            if (!post.querySelector('.heart-overlay')) {
                const overlay = document.createElement('div');
                overlay.className = 'heart-overlay';
                overlay.innerHTML = '<i class="bx bxs-heart heart-animation"></i>';
                post.style.position = 'relative';
                post.appendChild(overlay);
            }
        });
    }

    setupLikeButtons() {
        // Handled in global click handler
    }

    async handleDoubleClickLike(postElement) {
        const postId = postElement.getAttribute('data-post-id');
        if (!postId) {
            return;
        }

        this.showHeartAnimation(postElement);

        try {
            const result = await this.togglePostLike(postId);
            if (result.success) {
                this.updateLikeButtonState(postElement, result.isLiked, result.reactionCount);
            }
        } catch (error) {
            console.error("Error in community post double-click like:", error);
        }
    }

    async handleLikeButtonClick(likeBtn) {
        const communityPost = likeBtn.closest('.community-post');
        if (!communityPost || likeBtn.classList.contains('processing')) return;

        const postId = likeBtn.getAttribute('data-post-id');
        if (!postId) {
            return;
        }

        likeBtn.classList.add('processing');

        try {
            const result = await this.togglePostLike(postId);

            if (result.success) {
                this.updateLikeButtonState(communityPost, result.isLiked, result.reactionCount);
                this.animateLikeButton(likeBtn, result.isLiked);
                this.showNotification(result.isLiked ? 'Post liked!' : 'Post unliked!', 'success');
            } else {
                this.showNotification(result.message || 'Error liking post', 'error');
            }
        } catch (error) {
            console.error("Error toggling community post like:", error);
            this.showNotification('Error liking post. Please try again.', 'error');
        } finally {
            likeBtn.classList.remove('processing');
        }
    }

    showHeartAnimation(postElement) {
        const overlay = postElement.querySelector('.heart-overlay');
        if (!overlay) return;

        const heart = overlay.querySelector('.heart-animation');
        heart.style.animation = 'none';
        overlay.style.display = 'flex';
        overlay.style.opacity = '1';

        setTimeout(() => heart.style.animation = 'heartBurstAnimation 0.8s ease-out', 10);

        setTimeout(() => {
            overlay.style.opacity = '0';
            setTimeout(() => overlay.style.display = 'none', 200);
        }, 600);
    }

    async togglePostLike(postId) {
        const formData = new FormData();
        formData.append('postId', postId);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Community/ToggleReaction', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    }

    updateLikeButtonState(postElement, isLiked, reactionCount) {
        const likeBtn = postElement.querySelector('.like-btn');
        const icon = likeBtn?.querySelector('i');
        const countElement = likeBtn?.querySelector('.likes-count');

        if (!likeBtn || !icon) return;

        if (isLiked) {
            likeBtn.classList.add('liked');
            icon.className = 'bx bxs-heart';
            likeBtn.style.color = '#e74c3c';
        } else {
            likeBtn.classList.remove('liked');
            icon.className = 'bx bx-heart';
            likeBtn.style.color = '';
        }

        if (countElement) {
            countElement.textContent = reactionCount || 0;
        }
    }

    animateLikeButton(button, isLiked) {
        const icon = button.querySelector('i');

        if (isLiked) {
            icon.style.animation = 'heartBeat 0.6s ease-in-out';
            button.style.transform = 'scale(1.1)';
            setTimeout(() => {
                button.style.transform = 'scale(1)';
                icon.style.animation = '';
            }, 600);
        } else {
            button.style.transform = 'scale(0.9)';
            setTimeout(() => button.style.transform = 'scale(1)', 150);
        }
    }

    // ============================================================================
    // COMMENTS SYSTEM
    // ============================================================================

    setupCommentsSystem() {
        this.setupCommentInput();
        this.setupCommentsModal();
    }

    setupCommentInput() {
        const input = document.getElementById('newCommentInput');
        const sendBtn = document.getElementById('sendCommentBtn');

        if (!input || !sendBtn) return;

        input.addEventListener('input', () => {
            this.validateCommentInput(input, sendBtn);
            this.autoResize(input, 120);
        });

        input.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                if (!sendBtn.disabled) {
                    const postId = sendBtn.getAttribute('data-post-id');
                    if (postId) this.submitComment(postId);
                }
            }
        });
    }

    setupCommentsModal() {
        const modal = document.getElementById('commentsModal');
        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) this.closeCommentsModal();
            });
        }
    }

    validateCommentInput(input, sendBtn) {
        const currentLength = input.value.length;

        if (currentLength > this.maxCommentLength) {
            input.value = input.value.substring(0, this.maxCommentLength);
            this.showNotification(`Maximum ${this.maxCommentLength} characters allowed for comments.`, 'error');
            return;
        }

        const hasContent = input.value.trim().length > 0;
        sendBtn.disabled = !hasContent;
    }

    async openCommentsModal(postId) {
        const modal = document.getElementById('commentsModal');
        const sendBtn = document.getElementById('sendCommentBtn');

        if (!modal) {
            return;
        }

        if (sendBtn) sendBtn.setAttribute('data-post-id', postId);

        this.showModal(modal);
        this.showCommentsLoading(true);

        try {
            await this.loadCommentsForPost(postId);
            this.updateCommentsCount();
        } catch (error) {
            console.error("Error loading comments:", error);
            this.showNotification('Error loading comments. Please try again.', 'error');
        } finally {
            this.showCommentsLoading(false);
            setTimeout(() => {
                const input = document.getElementById('newCommentInput');
                if (input) input.focus();
            }, 350);
        }
    }

    showModal(modal) {
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
        setTimeout(() => modal.classList.add('show'), 10);
    }

    closeCommentsModal() {
        const modal = document.getElementById('commentsModal');
        if (!modal) return;

        modal.classList.remove('show');
        document.body.style.overflow = 'auto';

        setTimeout(() => modal.style.display = 'none', 300);

        this.resetCommentForm();
    }

    resetCommentForm() {
        const input = document.getElementById('newCommentInput');
        const sendBtn = document.getElementById('sendCommentBtn');

        if (input) {
            input.value = '';
            input.style.height = 'auto';
            input.placeholder = 'Write a comment...';
            input.removeAttribute('data-parent-comment-id');
        }

        if (sendBtn) {
            sendBtn.disabled = true;
            sendBtn.removeAttribute('data-post-id');
        }
    }

    async loadCommentsForPost(postId) {
        const response = await fetch(`/Community/GetComments?postId=${postId}`);
        const result = await response.json();

        if (result.success && result.comments) {
            this.displayComments(result.comments);
        } else {
            this.displayEmptyCommentsState();
        }
    }

    displayComments(comments) {
        const container = document.getElementById('commentsListContainer');
        this.clearCommentsContainer(container);

        if (comments.length === 0) {
            this.displayEmptyCommentsState();
            return;
        }

        const realCommentsContainer = this.getOrCreateRealCommentsContainer(container);
        realCommentsContainer.innerHTML = '';

        comments.forEach(comment => {
            const commentElement = this.createCommentElement(comment);
            realCommentsContainer.appendChild(commentElement);
        });
    }

    clearCommentsContainer(container) {
        const loadingDiv = container.querySelector('.loading-comments');
        const emptyState = container.querySelector('.comments-empty-state');
        const demoContent = container.querySelector('.comments-demo-content');
        const existingComments = container.querySelectorAll('.comment-item:not(.demo-comment)');

        if (loadingDiv) loadingDiv.style.display = 'none';
        if (emptyState) emptyState.style.display = 'none';
        if (demoContent) demoContent.style.display = 'none';
        existingComments.forEach(comment => comment.remove());
    }

    getOrCreateRealCommentsContainer(container) {
        let realCommentsContainer = container.querySelector('.real-comments-container');
        if (!realCommentsContainer) {
            realCommentsContainer = document.createElement('div');
            realCommentsContainer.className = 'real-comments-container';
            container.appendChild(realCommentsContainer);
        }
        realCommentsContainer.style.display = 'block';
        realCommentsContainer.style.opacity = '1';
        return realCommentsContainer;
    }

    // ============================================================================
    // API CALLS
    // ============================================================================

    async createCommunityPost(content, imageFile, button) {
        const originalText = button.textContent;
        button.disabled = true;
        button.textContent = 'Posting...';
        button.style.background = '#6c757d';

        try {
            const formData = new FormData();
            formData.append('PostContent', content);
            formData.append('PostType', '0');

            if (imageFile) formData.append('ImageFile', imageFile);

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) formData.append('__RequestVerificationToken', token);

            const response = await fetch('/Community/CreatePost', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                this.resetPostForm();
                button.textContent = 'Posted!';
                button.style.background = '#28a745';
                this.showNotification('Post created successfully!', 'success');
                await this.loadCommunityPosts();
            } else {
                this.showNotification(result.message || 'Error creating post', 'error');
            }
        } catch (error) {
            console.error('Error creating post:', error);
            this.showNotification('Error creating post. Please try again.', 'error');
        } finally {
            setTimeout(() => {
                button.disabled = false;
                button.textContent = originalText;
                button.style.background = '';
            }, 2000);
        }
    }

    async loadCommunityPosts() {
        try {
            const response = await fetch('/Community/GetPosts');
            const result = await response.json();

            if (result.success && result.posts) {
                this.replaceExistingPosts(result.posts);
                this.addHeartOverlays();
            }
        } catch (error) {
            console.error('Error loading posts:', error);
        }
    }

    async submitComment(postId, parentCommentId = null) {
        const input = document.getElementById('newCommentInput');
        const content = input.value.trim();

        if (!content) {
            this.showNotification('Please enter a comment.', 'error');
            return;
        }

        if (content.length > this.maxCommentLength) {
            this.showNotification(`Comment cannot exceed ${this.maxCommentLength} characters.`, 'error');
            return;
        }

        try {
            const formData = new FormData();
            formData.append('postId', postId);
            formData.append('content', content);
            if (parentCommentId) formData.append('parentCommentId', parentCommentId);

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) formData.append('__RequestVerificationToken', token);

            const response = await fetch('/Community/CreateCommentFromForm', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                this.resetCommentForm();
                await this.loadCommentsForPost(postId);
                this.updatePostCommentCount(postId);
                this.showNotification('Comment added successfully!', 'success');
            } else {
                this.showNotification(result.message || 'Error adding comment', 'error');
            }
        } catch (error) {
            console.error("Error submitting comment:", error);
            this.showNotification('Error adding comment. Please try again.', 'error');
        }
    }

    // ============================================================================
    // GLOBAL EVENT HANDLERS
    // ============================================================================

    handleGlobalClicks(e) {
        // Like button clicks
        const likeBtn = e.target.closest('.like-btn');
        if (likeBtn && likeBtn.closest('.community-post')) {
            e.preventDefault();
            e.stopPropagation();
            this.handleLikeButtonClick(likeBtn);
            return;
        }

        // Comment button clicks
        const commentBtn = e.target.closest('.comment-btn');
        if (commentBtn && commentBtn.closest('.community-post')) {
            e.preventDefault();
            e.stopPropagation();
            const postId = commentBtn.dataset.postId || commentBtn.getAttribute('data-post-id');
            if (postId) this.openCommentsModal(postId);
            return;
        }

        // Modal close buttons
        if (e.target.closest('.close-comments-modal')) {
            this.closeCommentsModal();
            return;
        }

        // Send comment button
        const sendBtn = e.target.closest('#sendCommentBtn');
        if (sendBtn && !sendBtn.disabled) {
            const postId = sendBtn.getAttribute('data-post-id');
            if (postId) this.submitComment(postId);
            return;
        }

        // Post options dropdown
        if (!e.target.closest('.post-options-dropdown')) {
            document.querySelectorAll('.post-dropdown-menu').forEach(menu => {
                menu.style.display = 'none';
            });
        }
    }

    handleGlobalKeyboard(e) {
        if (e.key === 'Escape') {
            const modal = document.getElementById('commentsModal');
            if (modal && modal.classList.contains('show')) {
                this.closeCommentsModal();
            }
        }
    }

    // ============================================================================
    // UTILITY METHODS
    // ============================================================================

    resetPostForm() {
        const postInput = document.getElementById('communityPostInput');
        const imageInput = document.getElementById('communityImageInput');
        const imagePreview = document.getElementById('communityImagePreview');

        if (postInput) {
            postInput.value = '';
            postInput.style.height = 'auto';
        }
        if (imageInput) imageInput.value = '';
        if (imagePreview) {
            imagePreview.style.opacity = '0';
            setTimeout(() => imagePreview.style.display = 'none', 300);
        }
    }

    replaceExistingPosts(posts) {
        // Remove existing database posts
        document.querySelectorAll('.community-post[data-from-db="true"]').forEach(post => post.remove());

        // Remove sample posts
        const samplePosts = document.querySelectorAll('.community-post:not([data-from-db])');
        samplePosts.forEach(post => {
            const content = post.querySelector('.post-content p');
            if (content && (
                content.textContent.includes('Anyone know good places to donate electronics') ||
                content.textContent.includes('Just finished decluttering my closet')
            )) {
                post.remove();
            }
        });

        // Add new posts
        const postsContainer = document.getElementById('postsContainer');
        posts.forEach(post => {
            const postElement = this.createPostElement(post);
            const firstItemPost = postsContainer.querySelector('.item-post');
            if (firstItemPost) {
                postsContainer.insertBefore(postElement, firstItemPost);
            } else {
                postsContainer.appendChild(postElement);
            }
        });
    }

    createPostElement(post) {
        const postDiv = document.createElement('div');
        postDiv.className = 'community-post fade-in';
        postDiv.setAttribute('data-type', 'community');
        postDiv.setAttribute('data-post-id', post.postId);
        postDiv.setAttribute('data-from-db', 'true');

        const timeAgo = this.getTimeAgo(new Date(post.datePosted));
        const isOwner = this.currentUserId && post.authorId.toString() === this.currentUserId;
        const isLiked = post.userReaction === 0;

        postDiv.innerHTML = this.getPostHTML(post, timeAgo, isOwner, isLiked);
        return postDiv;
    }

    getPostHTML(post, timeAgo, isOwner, isLiked) {
        const likeIconClass = isLiked ? 'bx bxs-heart' : 'bx bx-heart';
        const likeButtonClass = isLiked ? 'interaction-btn like-btn liked' : 'interaction-btn like-btn';
        const likeButtonStyle = isLiked ? 'color: #e74c3c;' : '';

        return `
            <div class="post-header">
                <div class="post-user-info">
                    <img src="${post.authorAvatarUrl || '~/assets/default-avatar.svg'}" alt="User" class="post-avatar" />
                    <div class="post-user-details">
                        <span class="post-username">${this.escapeHtml(post.authorUsername)}</span>
                        <span class="post-time">${timeAgo}</span>
                        ${post.lastEdited ? '<span class="post-edited">(edited)</span>' : ''}
                    </div>
                </div>
                ${isOwner ? this.getPostOptionsHTML(post.postId) : ''}
            </div>

            <div class="post-content">
                ${post.imageUrl ? `<div class="post-image"><img src="${post.imageUrl}" alt="Post image" style="width: 100%; border-radius: 12px; margin-bottom: 10px;" /></div>` : ''}
                <p>${this.escapeHtml(post.postContent).replace(/\n/g, '<br/>')}</p>
            </div>

            <div class="post-interactions">
                <div class="interaction-buttons">
                    <button class="${likeButtonClass}" data-post-id="${post.postId}" style="${likeButtonStyle}">
                        <i class='${likeIconClass}'></i>
                        <span class="likes-count">${post.reactionCount || 0}</span>
                    </button>
                    <button class="interaction-btn comment-btn" data-post-id="${post.postId}">
                        <i class='bx bx-message-circle'></i>
                        <span class="comment-count">${post.commentCount || 0}</span>
                    </button>
                </div>
            </div>
        `;
    }

    getPostOptionsHTML(postId) {
        return `
        <div class="post-options-dropdown">
            <button class="post-options-btn" onclick="togglePostOptions(${postId})" type="button">
                <span class="material-symbols-outlined">more_horiz</span>
            </button>
            <div class="post-dropdown-menu" id="postOptions-${postId}">
                <button class="dropdown-item" onclick="editPost(${postId}, event); event.stopPropagation(); return false;" type="button">
                    <i class="bx bx-edit"></i> Edit
                </button>
                <button class="dropdown-item delete-item" onclick="deletePost(${postId}, event); event.stopPropagation(); return false;" type="button">
                    <i class="bx bx-trash"></i> Delete
                </button>
            </div>
        </div>
    `;
    }

    autoResize(element, maxHeight = 120) {
        element.style.height = 'auto';
        element.style.height = Math.min(element.scrollHeight, maxHeight) + 'px';
    }

    animateButton(button) {
        button.style.transform = 'scale(1.05)';
        setTimeout(() => button.style.transform = 'scale(1)', 150);
    }

    showCommentsLoading(show) {
        const loadingDiv = document.getElementById('commentsLoading');
        if (loadingDiv) {
            loadingDiv.style.display = show ? 'flex' : 'none';
        }
    }

    displayEmptyCommentsState() {
        const container = document.getElementById('commentsListContainer');
        const emptyState = container.querySelector('.comments-empty-state');

        if (emptyState) emptyState.style.display = 'block';

        const demoContent = container.querySelector('.comments-demo-content');
        if (demoContent) demoContent.style.display = 'none';

        const realComments = container.querySelector('.real-comments-container');
        if (realComments) realComments.style.display = 'none';
    }

    updateCommentsCount() {
        const modalCount = document.getElementById('modalCommentsCount');
        const comments = document.querySelectorAll('.comment-item:not(.demo-comment)');
        if (modalCount) {
            modalCount.textContent = comments.length;
        }
    }

    updatePostCommentCount(postId) {
        const communityPost = document.querySelector(`.community-post[data-post-id="${postId}"]`);
        if (communityPost) {
            const commentCountElement = communityPost.querySelector('.comment-count');
            if (commentCountElement) {
                const currentCount = parseInt(commentCountElement.textContent) || 0;
                commentCountElement.textContent = currentCount + 1;
            }
        }
    }

    createCommentElement(comment, isReply = false, depth = 0) {
        const div = document.createElement('div');
        div.className = `comment-item ${isReply ? 'comment-reply' : ''}`;
        div.setAttribute('data-comment-id', comment.commentId);

        const timeAgo = this.getTimeAgo(new Date(comment.dateCommented));
        const isCurrentUser = comment.isCurrentUser;
        const isDeleted = comment.content === '[deleted]';
        const hasReplies = comment.replies && comment.replies.length > 0;

        const totalReplyCount = this.calculateTotalReplies(comment);
        const directReplyCount = comment.replies ? comment.replies.length : 0;

        const maxIndentLevel = 2;
        const visualDepth = Math.min(depth, maxIndentLevel);
        const marginLeft = visualDepth * 16;

        const threadStyles = `
            margin-left: ${marginLeft}px; 
            ${visualDepth > 0 ? 'border-left: 2px solid #e1e5e9; padding-left: 12px;' : ''} 
            margin-top: ${isReply ? '6px' : '12px'};
            margin-bottom: 4px;
            opacity: 1; 
            display: block;
        `;

        div.style.cssText = threadStyles;

        let replyToText = '';
        if (isReply && comment.parentCommenterName && depth > 0) {
            replyToText = `
                <div class="reply-to-indicator" style="
                    font-size: 12px; 
                    color: #65676b; 
                    margin-bottom: 4px;
                    font-weight: 500;
                    display: flex;
                    align-items: center;
                    gap: 4px;
                ">
                    <i class="bx bx-corner-down-right" style="font-size: 14px;"></i>
                    Reply to ${this.escapeHtml(comment.parentCommenterName)}
                </div>
            `;
        }

        div.innerHTML = `
            ${replyToText}
            <div class="comment-wrapper" style="display: flex; align-items: flex-start; gap: 8px;">
                <div class="comment-avatar-wrapper" style="flex-shrink: 0;">
                    <img src="${comment.userAvatarUrl || '/assets/default-avatar.svg'}" 
                         alt="User" 
                         class="comment-avatar" 
                         style="width: ${depth > 1 ? '28px' : '32px'}; height: ${depth > 1 ? '28px' : '32px'}; border-radius: 50%; object-fit: cover;">
                </div>
                <div class="comment-content" style="flex: 1; min-width: 0;">
                    ${this.getCommentHeaderHTML(comment, timeAgo, isCurrentUser, isDeleted)}
                    ${this.getCommentContentHTML(comment, isDeleted)}
                    ${!isDeleted ? this.getCommentActionsHTML(comment, hasReplies, totalReplyCount, directReplyCount, depth) : ''}
                </div>
            </div>
            
            <div class="comment-replies" 
                 style="display: none; margin-top: 6px;" 
                 data-comment-id="${comment.commentId}">
                ${hasReplies ? comment.replies.map(reply => this.createCommentElement(reply, true, depth + 1).outerHTML).join('') : ''}
            </div>
        `;

        return div;
    }

    calculateTotalReplies(comment) {
        if (!comment.replies || comment.replies.length === 0) {
            return 0;
        }

        let total = comment.replies.length;
        comment.replies.forEach(reply => {
            total += this.calculateTotalReplies(reply);
        });

        return total;
    }

    getCommentHeaderHTML(comment, timeAgo, isCurrentUser, isDeleted) {
        return `
            <div class="comment-header" style="display: flex; align-items: center; flex-wrap: wrap; gap: 6px; margin-bottom: 4px;">
                <span class="comment-username" style="font-weight: 600; font-size: 14px; color: #050505;">
                    ${this.escapeHtml(comment.username)}
                </span>
                <span class="comment-time" style="font-size: 12px; color: #65676b;">
                    ${timeAgo}
                </span>
                ${comment.lastEdited ? '<span class="comment-edited" style="color: #65676b; font-size: 11px;">(edited)</span>' : ''}
                ${isCurrentUser && !isDeleted ? this.getCommentOptionsHTML(comment.commentId) : ''}
            </div>
        `;
    }

    getCommentContentHTML(comment, isDeleted) {
        return `
            <div class="comment-text-container" style="margin-bottom: 6px;">
                <div class="comment-bubble" style="
                    background: #f0f2f5; 
                    padding: 8px 12px; 
                    border-radius: 16px; 
                    display: inline-block;
                    max-width: 100%;
                    word-wrap: break-word;
                ">
                    <p class="comment-text" style="font-size: 14px; margin: 0; color: #050505; line-height: 1.3;">
                        ${isDeleted ? '<em style="color: #65676b;">This comment was deleted</em>' : this.escapeHtml(comment.content)}
                    </p>
                </div>
            </div>
        `;
    }

    getCommentActionsHTML(comment, hasReplies, totalReplyCount, directReplyCount, depth) {
        return `
            <div class="comment-actions" style="display: flex; align-items: center; gap: 16px; margin-top: 2px;">
                <button class="comment-action-btn reply-btn" 
                        data-action="reply" 
                        data-comment-id="${comment.commentId}" 
                        style="
                            font-size: 12px; 
                            font-weight: 600; 
                            color: #65676b; 
                            background: none; 
                            border: none; 
                            cursor: pointer; 
                            padding: 4px 6px;
                            border-radius: 4px;
                            transition: background-color 0.2s ease;
                        "
                        onmouseover="this.style.backgroundColor='#f2f3f5'"
                        onmouseout="this.style.backgroundColor='transparent'">
                    Reply
                </button>
                
                ${hasReplies ? this.getToggleRepliesButtonHTML(comment.commentId, totalReplyCount, directReplyCount, depth) : ''}
            </div>
        `;
    }

    getCommentOptionsHTML(commentId) {
        return `
            <div class="comment-options" style="margin-left: auto; display: flex; gap: 2px;">
                <button class="comment-option-btn" onclick="editComment(${commentId})" 
                        style="padding: 4px; border: none; background: none; cursor: pointer; color: #65676b; border-radius: 4px; display: flex; align-items: center; justify-content: center;">
                    <i class="bx bx-edit" style="font-size: 14px;"></i>
                </button>
                <button class="comment-option-btn delete" onclick="deleteComment(${commentId})" 
                        style="padding: 4px; border: none; background: none; cursor: pointer; color: #65676b; border-radius: 4px; display: flex; align-items: center; justify-content: center;">
                    <i class="bx bx-trash" style="font-size: 14px;"></i>
                </button>
            </div>
        `;
    }

    getToggleRepliesButtonHTML(commentId, totalReplyCount, directReplyCount, depth) {
        const displayCount = depth === 0 ? totalReplyCount : directReplyCount;

        return `
            <button class="toggle-replies-btn" 
                    data-comment-id="${commentId}" 
                    data-reply-count="${totalReplyCount}" 
                    data-direct-replies="${directReplyCount}"
                    style="
                        font-size: 12px; 
                        font-weight: 600; 
                        color: #1877f2; 
                        background: none; 
                        border: none; 
                        cursor: pointer; 
                        padding: 4px 6px;
                        display: flex;
                        align-items: center;
                        gap: 4px;
                        border-radius: 4px;
                        transition: background-color 0.2s ease;
                    "
                    onmouseover="this.style.backgroundColor='#e7f3ff'"
                    onmouseout="this.style.backgroundColor='transparent'">
                <i class="bx bx-chevron-down toggle-icon" style="font-size: 16px; transition: transform 0.2s ease;"></i>
                <span class="replies-text">
                    ${displayCount} ${displayCount === 1 ? 'reply' : 'replies'}
                </span>
            </button>
        `;
    }

    getTimeAgo(date) {
        const now = new Date();
        const serverDate = new Date(date);
        const diff = now - serverDate;
        const minutes = Math.floor(diff / 60000);
        const hours = Math.floor(diff / 3600000);
        const days = Math.floor(diff / 86400000);
        const weeks = Math.floor(days / 7);

        if (minutes < 1) return 'Just now';
        if (minutes < 60) return `${minutes}m`;
        if (hours < 24) return `${hours}h`;
        if (days < 7) return `${days}d`;
        if (weeks < 4) return `${weeks}w`;

        return serverDate.toLocaleDateString();
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : 'var(--primary-color)'};
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
}

// ============================================================================
// USER SWITCHER
// ============================================================================

async function switchTestUser() {
    const select = document.getElementById('testUserSelect');
    const value = select.value;

    const userMap = {
        "1": "Alice",
        "2": "Bob",
        "3": "Charlie"
    };

    const username = userMap[value];
    if (!username) return;

    try {
        const formData = new FormData();
        formData.append('username', username);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Community/SwitchTestUser', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            window.location.reload();
        } else {
            alert('Error switching user: ' + result.message);
        }
    } catch (error) {
        console.error('Error switching user:', error);
        alert('Error switching user');
    }
}

// ============================================================================
// POST MANAGEMENT FUNCTIONS
// ============================================================================

function togglePostOptions(postId) {
    // Close other dropdowns
    document.querySelectorAll('.post-dropdown-menu').forEach(menu => {
        if (menu.id !== `postOptions-${postId}`) {
            menu.style.display = 'none';
        }
    });

    const dropdown = document.getElementById(`postOptions-${postId}`);
    if (dropdown) {
        const isVisible = dropdown.style.display === 'block';
        dropdown.style.display = isVisible ? 'none' : 'block';
    }
}

async function deletePost(postId) {
    if (!confirm('Are you sure you want to delete this post? This action cannot be undone.')) {
        return;
    }

    try {
        const formData = new FormData();
        formData.append('postId', postId);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Community/DeletePost', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            const postElement = document.querySelector(`[data-post-id="${postId}"]`);
            if (postElement) {
                postElement.style.opacity = '0';
                postElement.style.transform = 'translateY(-20px)';
                setTimeout(() => postElement.remove(), 300);
            }
            communityHub.showNotification('Post deleted successfully!', 'success');
        } else {
            communityHub.showNotification(result.message || 'Error deleting post', 'error');
        }
    } catch (error) {
        console.error('Error deleting post:', error);
        communityHub.showNotification('Error deleting post. Please try again.', 'error');
    }
}

// ============================================================================
// IMPROVED EDIT POST FUNCTION WITH TAILWIND UI
// ============================================================================

function editPost(postId, event) {
    // Prevent event propagation
    if (event) {
        event.stopPropagation();
        event.stopImmediatePropagation();
        event.preventDefault();
    }

    // Close dropdown first
    const dropdown = document.getElementById(`postOptions-${postId}`);
    if (dropdown) {
        dropdown.style.display = 'none';
    }

    // Close all other dropdowns
    document.querySelectorAll('.post-dropdown-menu').forEach(menu => {
        menu.style.display = 'none';
    });

    const postElement = document.querySelector(`div.community-post[data-post-id="${postId}"]`);
    if (!postElement) {
        communityHub.showNotification('Community post not found', 'error');
        return false;
    }

    const contentElement = postElement.querySelector('.post-content p');
    const currentContent = contentElement ? contentElement.textContent : '';

    const imageElement = postElement.querySelector('.post-image img, .post-content img');
    const currentImageUrl = imageElement ? imageElement.src : null;

    // Remove any existing edit modals first
    const existingModal = document.querySelector('.edit-modal-overlay');
    if (existingModal) {
        existingModal.remove();
    }

    // Create modal with enhanced image upload
    const modal = document.createElement('div');
    modal.className = 'edit-modal-overlay';
    modal.style.cssText = `
        position: fixed !important;
        top: 0 !important;
        left: 0 !important;
        right: 0 !important;
        bottom: 0 !important;
        width: 100vw !important;
        height: 100vh !important;
        background: rgba(37, 36, 34, 0.7) !important;
        backdrop-filter: blur(4px) !important;
        display: flex !important;
        align-items: center !important;
        justify-content: center !important;
        z-index: 999999 !important;
        padding: 20px !important;
        box-sizing: border-box !important;
    `;

    modal.innerHTML = `
        <div class="modal-content" id="editModal-${postId}" style="
            position: relative !important;
            background: white !important;
            border-radius: 24px !important;
            width: 100% !important;
            max-width: 600px !important;
            max-height: 85vh !important;
            overflow-y: auto !important;
            box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3) !important;
            transform: scale(0.9) !important;
            opacity: 0 !important;
            transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
            z-index: 1000000 !important;
            display: block !important;
            visibility: visible !important;
        ">
            <!-- Modal Header -->
            <div class="modal-header" style="
                padding: 24px 24px 16px 24px !important;
                border-bottom: 1px solid #e2e8f0 !important;
                position: relative !important;
                text-align: center !important;
            ">
                <h2 style="
                    font-size: 2.5rem !important;
                    font-weight: 700 !important;
                    color: var(--text-color) !important;
                    margin: 0 !important;
                    font-family: inherit !important;
                ">Edit Post</h2>
                
                <button onclick="closeEditModal(${postId})" class="close-modal" style="
                    position: absolute !important;
                    top: 16px !important;
                    right: 24px !important;
                    background: none !important;
                    border: none !important;
                    font-size: 32px !important;
                    color: #999 !important;
                    cursor: pointer !important;
                    padding: 8px !important;
                    border-radius: 50% !important;
                    width: 48px !important;
                    height: 48px !important;
                    display: flex !important;
                    align-items: center !important;
                    justify-content: center !important;
                    transition: all 0.2s ease !important;
                    z-index: 1000001 !important;
                    line-height: 1 !important;
                " onmouseover="this.style.background='#f5f5f5'; this.style.color='#666';" 
                   onmouseout="this.style.background='none'; this.style.color='#999';">
                    ×
                </button>
            </div>

            <!-- Modal Content -->
            <div style="padding: 24px !important;">
                <div style="display: flex !important; flex-direction: column !important; gap: 24px !important;">
                    
                    <!-- Current Image Section -->
                    ${currentImageUrl ? `
                    <div id="currentImageSection-${postId}" style="display: block !important;">
                        <label style="
                            display: block !important;
                            font-weight: 600 !important;
                            color: var(--text-color) !important;
                            margin-bottom: 12px !important;
                            font-size: 14px !important;
                        ">Current Image</label>
                        <div style="
                            position: relative !important;
                            border: 2px dashed var(--secondary-color) !important;
                            border-radius: 16px !important;
                            padding: 16px !important;
                            background: var(--accent-color) !important;
                        ">
                            <img id="currentImage-${postId}" src="${currentImageUrl}" style="
                                width: 100% !important;
                                height: 200px !important;
                                object-fit: cover !important;
                                border-radius: 12px !important;
                                margin-bottom: 12px !important;
                                display: block !important;
                            " alt="Current post image" />
                            <button type="button" onclick="removeCurrentImage(${postId})" style="
                                position: absolute !important;
                                top: 24px !important;
                                right: 24px !important;
                                background: rgba(220, 53, 69, 0.9) !important;
                                color: white !important;
                                border: none !important;
                                border-radius: 50% !important;
                                width: 36px !important;
                                height: 36px !important;
                                display: flex !important;
                                align-items: center !important;
                                justify-content: center !important;
                                cursor: pointer !important;
                                font-size: 18px !important;
                                transition: all 0.2s ease !important;
                                z-index: 10 !important;
                            " onmouseover="this.style.background='rgba(220, 53, 69, 1)'" 
                               onmouseout="this.style.background='rgba(220, 53, 69, 0.9)'">
                                ×
                            </button>
                        </div>
                    </div>
                    ` : ''}

                    <!-- Content Input -->
                    <div style="display: block !important;">
                        <label style="
                            display: block !important;
                            font-weight: 600 !important;
                            color: var(--text-color) !important;
                            margin-bottom: 12px !important;
                            font-size: 14px !important;
                        ">Post Content</label>
                        <div style="position: relative !important;">
                            <textarea id="editText-${postId}" 
                                placeholder="Share your thoughts..."
                                maxlength="1000"
                                style="
                                    width: 100% !important;
                                    min-height: 120px !important;
                                    max-height: 200px !important;
                                    padding: 16px !important;
                                    border: 2px solid #e2e8f0 !important;
                                    border-radius: 12px !important;
                                    font-size: 16px !important;
                                    font-family: inherit !important;
                                    color: var(--text-color) !important;
                                    background: white !important;
                                    resize: vertical !important;
                                    transition: all 0.2s ease !important;
                                    box-sizing: border-box !important;
                                    display: block !important;
                                    outline: none !important;
                                "
                                onfocus="this.style.borderColor='var(--primary-color)';"
                                onblur="this.style.borderColor='#e2e8f0';">${currentContent}</textarea>
                            <div id="charCounter-${postId}" style="
                                position: absolute !important;
                                bottom: 12px !important;
                                right: 16px !important;
                                font-size: 12px !important;
                                color: #666 !important;
                                background: rgba(255, 255, 255, 0.9) !important;
                                padding: 4px 8px !important;
                                border-radius: 12px !important;
                                backdrop-filter: blur(4px) !important;
                                pointer-events: none !important;
                            ">
                                <span id="currentCount-${postId}">${currentContent.length}</span>/1000
                            </div>
                        </div>
                    </div>

                    <!-- New Image Upload -->
                    <div style="display: block !important;">
                        <label style="
                            display: block !important;
                            font-weight: 600 !important;
                            color: var(--text-color) !important;
                            margin-bottom: 12px !important;
                            font-size: 14px !important;
                        ">Update Image (Optional)</label>
                        
                        <input type="file" id="newImageInput-${postId}" accept="image/*" 
                               onchange="previewNewImage(${postId})" style="display: none !important;">
                        
                        <!-- Enhanced Upload Label -->
                        <label for="newImageInput-${postId}" style="
                            display: flex !important;
                            flex-direction: column !important;
                            align-items: center !important;
                            justify-content: center !important;
                            height: 120px !important;
                            border: 2px dashed var(--secondary-color) !important;
                            border-radius: 16px !important;
                            cursor: pointer !important;
                            transition: all 0.2s ease !important;
                            background: var(--accent-color) !important;
                            position: relative !important;
                            overflow: hidden !important;
                        " onmouseover="this.style.borderColor='var(--primary-color)'; this.style.background='var(--background)';"
                           onmouseout="this.style.borderColor='var(--secondary-color)'; this.style.background='var(--accent-color)';">
                            <div style="
                                display: flex !important;
                                flex-direction: column !important;
                                align-items: center !important;
                                gap: 8px !important;
                                color: var(--text-color) !important;
                            ">
                   
                                <span style="font-weight: 600 !important; font-size: 14px !important;">Click to upload image</span>
                                <span style="font-size: 12px !important; color: #666 !important;">PNG, JPG, GIF up to 10MB</span>
                            </div>
                        </label>
                    </div>

                    <!-- Action Buttons -->
                    <div style="
                        display: flex !important;
                        gap: 12px !important;
                        justify-content: flex-end !important;
                        padding-top: 16px !important;
                        border-top: 1px solid #e2e8f0 !important;
                        margin-top: 8px !important;
                    ">
                        <button onclick="closeEditModal(${postId})" style="
                            padding: 12px 24px !important;
                            border: 2px solid var(--secondary-color) !important;
                            background: transparent !important;
                            color: var(--text-color) !important;
                            border-radius: 12px !important;
                            font-weight: 600 !important;
                            cursor: pointer !important;
                            transition: all 0.2s ease !important;
                            font-size: 14px !important;
                            display: inline-block !important;
                        " onmouseover="this.style.background='var(--background)'; this.style.borderColor='var(--primary-color)';"
                           onmouseout="this.style.background='transparent'; this.style.borderColor='var(--secondary-color)';">
                            Cancel
                        </button>
                        <button onclick="savePostEdit(${postId})" style="
                            padding: 12px 32px !important;
                            background: var(--primary-color) !important;
                            color: white !important;
                            border: 2px solid var(--primary-color) !important;
                            border-radius: 12px !important;
                            font-weight: 600 !important;
                            cursor: pointer !important;
                            transition: all 0.2s ease !important;
                            font-size: 14px !important;
                            display: inline-flex !important;
                            align-items: center !important;
                            gap: 8px !important;
                        " onmouseover="this.style.background='var(--dark-primary)'; this.style.borderColor='var(--dark-primary)';"
                           onmouseout="this.style.background='var(--primary-color)'; this.style.borderColor='var(--primary-color)';">
                            Save Changes
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Add to document body and show
    document.body.appendChild(modal);
    document.body.style.overflow = 'hidden';

    // Force reflow and animate in
    modal.offsetHeight;

    setTimeout(() => {
        const modalContent = modal.querySelector(`#editModal-${postId}`);
        if (modalContent) {
            modalContent.style.transform = 'scale(1)';
            modalContent.style.opacity = '1';
        }
    }, 10);

    // Setup character counter and other event handlers
    setTimeout(() => {
        const textarea = modal.querySelector(`#editText-${postId}`);
        const charCounter = modal.querySelector(`#currentCount-${postId}`);

        if (textarea && charCounter) {
            textarea.addEventListener('input', function () {
                const currentLength = this.value.length;
                charCounter.textContent = currentLength;

                const counterElement = charCounter.parentElement;
                if (currentLength > 900) {
                    counterElement.style.background = 'rgba(220, 53, 69, 0.1)';
                    counterElement.style.color = '#dc3545';
                } else if (currentLength > 800) {
                    counterElement.style.background = 'rgba(245, 158, 11, 0.1)';
                    counterElement.style.color = '#f59e0b';
                } else {
                    counterElement.style.background = 'rgba(255, 255, 255, 0.9)';
                    counterElement.style.color = '#666';
                }

                this.style.height = 'auto';
                this.style.height = Math.min(this.scrollHeight, 200) + 'px';
            });

            setTimeout(() => {
                textarea.focus();
                textarea.setSelectionRange(textarea.value.length, textarea.value.length);
            }, 300);
        }
    }, 100);

    // Event listeners for modal closing
    modal.addEventListener('click', function (e) {
        if (e.target === modal) {
            closeEditModal(postId);
        }
    });

    const escapeHandler = function (e) {
        if (e.key === 'Escape') {
            closeEditModal(postId);
            document.removeEventListener('keydown', escapeHandler);
        }
    };
    document.addEventListener('keydown', escapeHandler);

    return false;
}

// Updated closeEditModal function
function closeEditModal(postId) {
    const modal = document.querySelector('.edit-modal-overlay');
    if (!modal) {
        console.warn('Edit modal not found for closing');
        return;
    }

    const modalContent = modal.querySelector(`#editModal-${postId}`);
    if (modalContent) {
        modalContent.style.transform = 'scale(0.9)';
        modalContent.style.opacity = '0';
    }

    setTimeout(() => {
        if (modal.parentNode) {
            modal.remove();
        }
        document.body.style.overflow = '';
    }, 300);

    console.log('Edit modal closed');
}


function removeCurrentImage(postId) {
    const currentImageSection = document.getElementById(`currentImageSection-${postId}`);
    const currentImage = document.getElementById(`currentImage-${postId}`);

    if (currentImageSection && currentImage) {
        // Add removal animation
        currentImageSection.style.opacity = '0';
        currentImageSection.style.transform = 'scale(0.95)';
        currentImageSection.style.transition = 'all 0.3s ease';

        setTimeout(() => {
            currentImageSection.style.display = 'none';
            // Mark for removal when saving
            currentImage.setAttribute('data-remove', 'true');
        }, 300);
    }
}

function previewNewImage(postId) {
    const input = document.getElementById(`newImageInput-${postId}`);
    const uploadLabel = input.nextElementSibling; // The label element

    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Validate file size (10MB)
        if (file.size > 10 * 1024 * 1024) {
            communityHub.showNotification('Image file size must be less than 10MB', 'error');
            input.value = '';
            return;
        }

        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            communityHub.showNotification('Please select a valid image file (JPEG, PNG, GIF, or WebP)', 'error');
            input.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            // Transform the upload label to show the image
            uploadLabel.innerHTML = `
                <div style="
                    position: relative !important;
                    width: 100% !important;
                    height: 100% !important;
                    display: flex !important;
                    align-items: center !important;
                    justify-content: center !important;
                ">
                    <img style="
                        width: 100% !important;
                        height: 100% !important;
                        object-fit: cover !important;
                        border-radius: 12px !important;
                        display: block !important;
                    " src="${e.target.result}" alt="Selected image" />
                    
                    <!-- Remove button -->
                    <button type="button" onclick="clearNewImage(${postId})" style="
                        position: absolute !important;
                        top: 8px !important;
                        right: 8px !important;
                        background: rgba(220, 53, 69, 0.9) !important;
                        color: white !important;
                        border: none !important;
                        border-radius: 50% !important;
                        width: 32px !important;
                        height: 32px !important;
                        display: flex !important;
                        align-items: center !important;
                        justify-content: center !important;
                        cursor: pointer !important;
                        font-size: 16px !important;
                        transition: all 0.2s ease !important;
                        z-index: 10 !important;
                        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3) !important;
                    " onmouseover="this.style.background='rgba(220, 53, 69, 1)'"
                       onmouseout="this.style.background='rgba(220, 53, 69, 0.9)'">
                        ×
                    </button>
                    
                    <!-- Replace button -->
                    <button type="button" onclick="document.getElementById('newImageInput-${postId}').click()" style="
                        position: absolute !important;
                        bottom: 8px !important;
                        right: 8px !important;
                        background: rgba(107, 144, 128, 0.9) !important;
                        color: white !important;
                        border: none !important;
                        border-radius: 16px !important;
                        padding: 6px 12px !important;
                        display: flex !important;
                        align-items: center !important;
                        justify-content: center !important;
                        cursor: pointer !important;
                        font-size: 12px !important;
                        font-weight: 600 !important;
                        transition: all 0.2s ease !important;
                        z-index: 10 !important;
                        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3) !important;
                        gap: 4px !important;
                    " onmouseover="this.style.background='var(--dark-primary)'"
                       onmouseout="this.style.background='rgba(107, 144, 128, 0.9)'">
                        Replace
                    </button>
                </div>
            `;

            // Update the label styling to show it has an image
            uploadLabel.style.border = '2px solid var(--primary-color)';
            uploadLabel.style.background = 'white';
            uploadLabel.style.padding = '0';
        };
        reader.readAsDataURL(file);
    }
}

function clearNewImage(postId) {
    const input = document.getElementById(`newImageInput-${postId}`);
    const uploadLabel = input.nextElementSibling;

    // Clear the input
    input.value = '';

    // Animate out the current image
    const imageContainer = uploadLabel.querySelector('div');
    if (imageContainer) {
        imageContainer.style.opacity = '0';
        imageContainer.style.transform = 'scale(0.95)';

        setTimeout(() => {
            // Reset the upload label to its original state
            uploadLabel.innerHTML = `
                <div style="
                    display: flex !important;
                    flex-direction: column !important;
                    align-items: center !important;
                    gap: 8px !important;
                    color: var(--text-color) !important;
                ">
                    <div style="
                        width: 48px !important;
                        height: 48px !important;
                        background: var(--secondary-color) !important;
                        border-radius: 12px !important;
                        display: flex !important;
                        align-items: center !important;
                        justify-content: center !important;
                        color: white !important;
                        font-size: 24px !important;
                    "></div>
                    <span style="font-weight: 600 !important; font-size: 14px !important;">Click to upload image</span>
                    <span style="font-size: 12px !important; color: #666 !important;">PNG, JPG, GIF up to 10MB</span>
                </div>
            `;

            // Reset label styling
            uploadLabel.style.border = '2px dashed var(--secondary-color)';
            uploadLabel.style.background = 'var(--accent-color)';
            uploadLabel.style.padding = '16px';
            uploadLabel.style.opacity = '1';
            uploadLabel.style.transform = 'scale(1)';
        }, 200);
    }
}

function closeEditModal(postId) {
    // Remove modal-open class
    document.body.classList.remove('modal-open');

    // Hide backdrop with your color palette
    const backdrop = document.getElementById('edit-modal-backdrop');
    if (backdrop) {
        backdrop.style.opacity = '0';
        setTimeout(() => backdrop.remove(), 300);
    }

    // Find the modal using the correct class name
    const modal = document.querySelector('.edit-modal-overlay');
    if (!modal) {
        console.warn('Edit modal not found for closing');
        return;
    }

    const modalContent = modal.querySelector(`#editModal-${postId}`);
    if (!modalContent) {
        console.warn(`Modal content not found for post ${postId}`);
        // Fallback: remove the modal anyway
        modal.remove();
        return;
    }

    // Animate out
    modalContent.style.transform = 'scale(0.95)';
    modalContent.style.opacity = '0';
    modalContent.style.transition = 'all 0.3s ease';

    setTimeout(() => {
        if (modal.parentNode) {
            modal.remove();
        }
        // Restore body scroll
        document.body.style.overflow = '';
    }, 300);
}

async function savePostEdit(postId) {
    try {
        const content = document.getElementById(`editText-${postId}`).value.trim();
        const newImageFile = document.getElementById(`newImageInput-${postId}`).files[0];
        const removeCurrentImage = document.getElementById(`currentImage-${postId}`)?.getAttribute('data-remove') === 'true';

        if (!content) {
            communityHub.showNotification('Post content is required!', 'error');
            return;
        }

        const saveBtn = document.querySelector(`button[onclick="savePostEdit(${postId})"]`);
        const originalText = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<i class="bx bx-loader-alt animate-spin text-lg"></i> <span>Saving...</span>';

        const formData = new FormData();
        formData.append('PostContent', content);
        formData.append('PostType', '0');

        if (newImageFile) formData.append('ImageFile', newImageFile);
        if (removeCurrentImage) formData.append('RemoveImage', 'true');

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch(`/Community/EditPost/${postId}`, {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            updatePostInDOM(postId, content, result);
            communityHub.showNotification('Community post updated successfully!', 'success');
            closeEditModal(postId);
        } else {
            communityHub.showNotification('Error: ' + (result.message || 'Failed to update post'), 'error');
        }
    } catch (error) {
        console.error('Error updating community post:', error);
        communityHub.showNotification('Error updating post. Please try again.', 'error');
    } finally {
        const saveBtn = document.querySelector(`button[onclick="savePostEdit(${postId})"]`);
        if (saveBtn) {
            saveBtn.disabled = false;
            saveBtn.innerHTML = '<i class="bx bx-save text-lg"></i> <span>Save Changes</span>';
        }
    }
}

function updatePostInDOM(postId, content, result) {
    const postElement = document.querySelector(`div.community-post[data-post-id="${postId}"]`);
    if (!postElement) return;

    const contentElement = postElement.querySelector('.post-content p');
    if (contentElement) {
        contentElement.innerHTML = communityHub.escapeHtml(content).replace(/\n/g, '<br/>');
    }

    const existingImageContainer = postElement.querySelector('.post-image');

    if (result.imageRemoved) {
        if (existingImageContainer) existingImageContainer.remove();
    } else if (result.newImageUrl) {
        if (existingImageContainer) {
            const img = existingImageContainer.querySelector('img');
            if (img) img.src = result.newImageUrl;
        } else {
            const postContentDiv = postElement.querySelector('.post-content');
            const imageHtml = `<div class="post-image"><img src="${result.newImageUrl}" alt="Post image" style="width: 100%; border-radius: 12px; margin-bottom: 10px;" /></div>`;
            postContentDiv.insertAdjacentHTML('afterbegin', imageHtml);
        }
    }

    const userDetails = postElement.querySelector('.post-user-details');
    if (userDetails && !userDetails.querySelector('.post-edited')) {
        userDetails.insertAdjacentHTML('beforeend', '<span class="post-edited" style="color: #666; font-size: 12px; margin-left: 8px;">(edited)</span>');
    }
}

// ============================================================================
// COMMENT MANAGEMENT FUNCTIONS
// ============================================================================

function editComment(commentId) {
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    if (!commentElement) {
        communityHub.showNotification('Comment not found', 'error');
        return;
    }

    const commentTextElement = commentElement.querySelector('.comment-text');
    const currentText = commentTextElement.textContent;

    if (currentText.includes('This comment was deleted')) {
        communityHub.showNotification('Cannot edit deleted comments', 'error');
        return;
    }

    const editForm = createCommentEditForm(commentId, currentText);
    const commentTextContainer = commentElement.querySelector('.comment-text-container');
    commentTextContainer.innerHTML = '';
    commentTextContainer.appendChild(editForm);

    const textarea = editForm.querySelector('.edit-comment-input');
    textarea.focus();
    textarea.setSelectionRange(textarea.value.length, textarea.value.length);

    setupCommentEditHandlers(editForm, commentId, commentElement, currentText);
}

function createCommentEditForm(commentId, currentText) {
    const editForm = document.createElement('div');
    editForm.className = 'comment-edit-form';
    editForm.innerHTML = `
        <textarea class="edit-comment-input" maxlength="1000" style="
            width: 100%; 
            min-height: 60px; 
            padding: 8px; 
            border: 2px solid #007bff; 
            border-radius: 8px; 
            resize: vertical;
            font-family: inherit;
            font-size: 14px;
            margin-bottom: 8px;
        ">${currentText}</textarea>
        <div class="edit-comment-actions" style="display: flex; gap: 8px;">
            <button class="save-edit-btn" style="
                padding: 6px 12px; 
                background: #28a745; 
                color: white; 
                border: none; 
                border-radius: 4px; 
                cursor: pointer;
                font-size: 12px;
            ">Save</button>
            <button class="cancel-edit-btn" style="
                padding: 6px 12px; 
                background: #6c757d; 
                color: white; 
                border: none; 
                border-radius: 4px; 
                cursor: pointer;
                font-size: 12px;
            ">Cancel</button>
        </div>
    `;
    return editForm;
}

function setupCommentEditHandlers(editForm, commentId, commentElement, currentText) {
    const textarea = editForm.querySelector('.edit-comment-input');
    const saveBtn = editForm.querySelector('.save-edit-btn');
    const cancelBtn = editForm.querySelector('.cancel-edit-btn');

    saveBtn.addEventListener('click', async () => {
        const newContent = textarea.value.trim();

        if (!newContent) {
            communityHub.showNotification('Comment cannot be empty', 'error');
            return;
        }

        if (newContent.length > 1000) {
            communityHub.showNotification('Comment cannot exceed 1000 characters', 'error');
            return;
        }

        await saveCommentEdit(commentId, newContent, commentElement);
    });

    cancelBtn.addEventListener('click', () => {
        cancelCommentEdit(commentElement, currentText);
    });

    textarea.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && e.ctrlKey) {
            e.preventDefault();
            saveBtn.click();
        } else if (e.key === 'Escape') {
            e.preventDefault();
            cancelBtn.click();
        }
    });
}

async function saveCommentEdit(commentId, newContent, commentElement) {
    try {
        const formData = new FormData();
        formData.append('commentId', commentId);
        formData.append('content', newContent);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Community/UpdateComment', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            updateCommentInDOM(commentElement, newContent);
            communityHub.showNotification('Comment updated successfully!', 'success');
        } else {
            communityHub.showNotification(result.message || 'Error updating comment', 'error');
            restoreOriginalCommentText(commentElement);
        }
    } catch (error) {
        console.error('Error updating comment:', error);
        communityHub.showNotification('Error updating comment. Please try again.', 'error');
    }
}

function updateCommentInDOM(commentElement, newContent) {
    const commentTextContainer = commentElement.querySelector('.comment-text-container');
    commentTextContainer.innerHTML = `
        <div class="comment-bubble" style="
            background: #f0f2f5; 
            padding: 8px 12px; 
            border-radius: 16px; 
            display: inline-block;
            max-width: 100%;
            word-wrap: break-word;
        ">
            <p class="comment-text" style="font-size: 14px; margin: 0; color: #050505; line-height: 1.3;">
                ${communityHub.escapeHtml(newContent)}
            </p>
        </div>
    `;

    const commentHeader = commentElement.querySelector('.comment-header');
    let editedSpan = commentHeader.querySelector('.comment-edited');
    if (!editedSpan) {
        editedSpan = document.createElement('span');
        editedSpan.className = 'comment-edited';
        editedSpan.textContent = '(edited)';
        editedSpan.style.color = '#65676b';
        editedSpan.style.fontSize = '11px';
        editedSpan.style.marginLeft = '8px';
        commentHeader.appendChild(editedSpan);
    }
}

function cancelCommentEdit(commentElement, originalText) {
    const commentTextContainer = commentElement.querySelector('.comment-text-container');
    commentTextContainer.innerHTML = `
        <div class="comment-bubble" style="
            background: #f0f2f5; 
            padding: 8px 12px; 
            border-radius: 16px; 
            display: inline-block;
            max-width: 100%;
            word-wrap: break-word;
        ">
            <p class="comment-text" style="font-size: 14px; margin: 0; color: #050505; line-height: 1.3;">
                ${communityHub.escapeHtml(originalText)}
            </p>
        </div>
    `;
}

function restoreOriginalCommentText(commentElement) {
    const originalText = commentElement.getAttribute('data-original-text') || 'Error loading comment';
    const commentTextContainer = commentElement.querySelector('.comment-text-container');
    commentTextContainer.innerHTML = `
        <div class="comment-bubble" style="
            background: #f0f2f5; 
            padding: 8px 12px; 
            border-radius: 16px; 
            display: inline-block;
            max-width: 100%;
            word-wrap: break-word;
        ">
            <p class="comment-text" style="font-size: 14px; margin: 0; color: #050505; line-height: 1.3;">
                ${communityHub.escapeHtml(originalText)}
            </p>
        </div>
    `;
}

async function deleteComment(commentId) {
    if (!confirm('Are you sure you want to delete this comment? This action cannot be undone.')) {
        return;
    }

    try {
        const formData = new FormData();
        formData.append('commentId', commentId);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Community/DeleteComment', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            updateDeletedCommentInDOM(commentId);
            communityHub.showNotification('Comment deleted successfully!', 'success');
        } else {
            communityHub.showNotification(result.message || 'Error deleting comment', 'error');
        }
    } catch (error) {
        console.error('Error deleting comment:', error);
        communityHub.showNotification('Error deleting comment. Please try again.', 'error');
    }
}

function updateDeletedCommentInDOM(commentId) {
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    if (!commentElement) return;

    const commentText = commentElement.querySelector('.comment-text');
    if (commentText) {
        commentText.innerHTML = '<em style="color: #65676b;">This comment was deleted</em>';
        commentText.style.fontStyle = 'italic';
        commentText.style.color = '#666';
    }

    const commentOptions = commentElement.querySelector('.comment-options');
    if (commentOptions) commentOptions.remove();

    const commentActions = commentElement.querySelector('.comment-actions');
    if (commentActions) commentActions.remove();
}

// ============================================================================
// REPLY SYSTEM
// ============================================================================

function handleCommentAction(btn) {
    const action = btn.dataset.action;
    const commentId = btn.dataset.commentId;

    if (action === 'reply') {
        showReplyForm(commentId);
    }
}

function showReplyForm(parentCommentId) {
    const parentComment = document.querySelector(`[data-comment-id="${parentCommentId}"]`);
    if (!parentComment) {
        communityHub.showNotification('Comment not found', 'error');
        return;
    }

    const parentUsername = parentComment.querySelector('.comment-username')?.textContent?.trim() || 'someone';
    const existingForm = parentComment.querySelector('.reply-form');

    if (existingForm) {
        existingForm.querySelector('.reply-input').focus();
        return;
    }

    const replyForm = createReplyForm(parentCommentId, parentUsername);
    const commentWrapper = parentComment.querySelector('.comment-wrapper');
    if (commentWrapper) {
        commentWrapper.insertAdjacentElement('afterend', replyForm);
    }

    setupReplyFormHandlers(replyForm, parentCommentId);
}

function createReplyForm(parentCommentId, parentUsername) {
    const replyForm = document.createElement('div');
    replyForm.className = 'reply-form';
    replyForm.innerHTML = `
        <div style="margin-top: 8px; margin-left: 40px;">
            <div class="reply-context" style="
                font-size: 12px; 
                color: #65676b; 
                margin-bottom: 8px;
                display: flex;
                align-items: center;
                gap: 4px;
            ">
                <i class="bx bx-corner-down-right" style="font-size: 14px;"></i>
                <span>Replying to ${communityHub.escapeHtml(parentUsername)}</span>
            </div>
            <div style="display: flex; align-items: flex-start; gap: 8px;">
                <img src="/assets/default-avatar.svg" alt="Your avatar" style="width: 28px; height: 28px; border-radius: 50%; object-fit: cover;">
                <div style="flex: 1;">
                    <div class="reply-input-container" style="
                        background: #f0f2f5; 
                        border-radius: 16px; 
                        padding: 8px 12px;
                        border: 1px solid transparent;
                        transition: all 0.2s ease;
                    ">
                        <textarea class="reply-input" 
                                  placeholder="Write a reply..." 
                                  maxlength="1000" 
                                  style="
                                      width: 100%; 
                                      min-height: 20px;
                                      max-height: 120px;
                                      background: none;
                                      border: none;
                                      outline: none;
                                      resize: none;
                                      font-family: inherit;
                                      font-size: 14px;
                                      line-height: 1.4;
                                      color: #050505;
                                  "></textarea>
                    </div>
                    <div style="display: flex; gap: 8px; margin-top: 6px; justify-content: flex-end;">
                        <button class="reply-cancel-btn" style="
                            padding: 6px 16px; 
                            background: #e4e6ea; 
                            color: #050505; 
                            border: none; 
                            border-radius: 6px; 
                            cursor: pointer;
                            font-size: 13px;
                            font-weight: 600;
                        ">Cancel</button>
                        <button class="reply-submit-btn" style="
                            padding: 6px 16px; 
                            background: #1877f2; 
                            color: white; 
                            border: none; 
                            border-radius: 6px; 
                            cursor: pointer;
                            font-size: 13px;
                            font-weight: 600;
                        " disabled>Reply</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    return replyForm;
}

function setupReplyFormHandlers(replyForm, parentCommentId) {
    const replyInput = replyForm.querySelector('.reply-input');
    const replySubmitBtn = replyForm.querySelector('.reply-submit-btn');
    const replyCancelBtn = replyForm.querySelector('.reply-cancel-btn');
    const inputContainer = replyForm.querySelector('.reply-input-container');

    replyInput.addEventListener('input', function () {
        this.style.height = 'auto';
        this.style.height = Math.min(this.scrollHeight, 120) + 'px';

        const hasContent = this.value.trim().length > 0;
        replySubmitBtn.disabled = !hasContent;
        replySubmitBtn.style.opacity = hasContent ? '1' : '0.5';

        if (this.value.length > 1000) {
            this.value = this.value.substring(0, 1000);
            communityHub.showNotification('Reply cannot exceed 1000 characters', 'error');
        }
    });

    replyInput.addEventListener('focus', () => {
        inputContainer.style.borderColor = '#1877f2';
        inputContainer.style.backgroundColor = '#ffffff';
    });

    replyInput.addEventListener('blur', () => {
        inputContainer.style.borderColor = 'transparent';
        inputContainer.style.backgroundColor = '#f0f2f5';
    });

    replySubmitBtn.addEventListener('click', async () => {
        const content = replyInput.value.trim();
        if (!content) return;
        await submitReply(parentCommentId, content, replyForm);
    });

    replyCancelBtn.addEventListener('click', () => {
        replyForm.remove();
    });

    replyInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            if (!replySubmitBtn.disabled) {
                replySubmitBtn.click();
            }
        } else if (e.key === 'Escape') {
            e.preventDefault();
            replyCancelBtn.click();
        }
    });

    replyInput.focus();
}

async function submitReply(parentCommentId, content, replyForm) {
    try {
        const sendBtn = document.getElementById('sendCommentBtn');
        const postId = sendBtn ? sendBtn.getAttribute('data-post-id') : null;

        if (!postId) {
            communityHub.showNotification('Error: Post ID not found', 'error');
            return;
        }

        const formData = new FormData();
        formData.append('postId', postId);
        formData.append('content', content);
        formData.append('parentCommentId', parentCommentId);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) formData.append('__RequestVerificationToken', token);

        const response = await fetch('/Community/CreateCommentFromForm', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            replyForm.remove();
            await communityHub.loadCommentsForPost(postId);
            communityHub.updatePostCommentCount(postId);
            communityHub.showNotification('Reply added successfully!', 'success');
        } else {
            communityHub.showNotification(result.message || 'Error adding reply', 'error');
        }
    } catch (error) {
        console.error('Error submitting reply:', error);
        communityHub.showNotification('Error adding reply. Please try again.', 'error');
    }
}

// ============================================================================
// REPLY TOGGLING SYSTEM
// ============================================================================

function initializeReplyToggling() {
    document.addEventListener('click', function (e) {
        const toggleBtn = e.target.closest('.toggle-replies-btn');
        if (!toggleBtn) return;

        e.preventDefault();
        e.stopPropagation();

        const commentId = toggleBtn.getAttribute('data-comment-id');
        const totalReplyCount = parseInt(toggleBtn.getAttribute('data-reply-count')) || 0;
        const directReplyCount = parseInt(toggleBtn.getAttribute('data-direct-replies')) || 0;
        const repliesContainer = document.querySelector(`.comment-replies[data-comment-id="${commentId}"]`);
        const icon = toggleBtn.querySelector('.toggle-icon');
        const text = toggleBtn.querySelector('.replies-text');

        if (!repliesContainer) return;

        const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
        const isTopLevel = !commentElement.classList.contains('comment-reply');
        const displayCount = isTopLevel ? totalReplyCount : directReplyCount;

        const isHidden = repliesContainer.style.display === 'none';

        if (isHidden) {
            showReplies(repliesContainer, icon);
            text.textContent = 'Hide replies';
        } else {
            hideReplies(repliesContainer, icon, displayCount);
        }
    });
}

function showReplies(repliesContainer, icon) {
    repliesContainer.style.display = 'block';
    repliesContainer.style.opacity = '0';
    repliesContainer.style.transform = 'translateY(-10px)';

    setTimeout(() => {
        repliesContainer.style.transition = 'all 0.3s ease';
        repliesContainer.style.opacity = '1';
        repliesContainer.style.transform = 'translateY(0)';
    }, 10);

    icon.style.transform = 'rotate(180deg)';
}

function hideReplies(repliesContainer, icon, displayCount) {
    repliesContainer.style.transition = 'all 0.3s ease';
    repliesContainer.style.opacity = '0';
    repliesContainer.style.transform = 'translateY(-10px)';

    setTimeout(() => {
        repliesContainer.style.display = 'none';
    }, 300);

    icon.style.transform = 'rotate(0deg)';
}

// ============================================================================
// INITIALIZATION
// ============================================================================

// Create global instance
const communityHub = new CommunityHub();

// Global function assignments for onclick handlers
window.switchTestUser = switchTestUser;
window.togglePostOptions = togglePostOptions;
window.deletePost = deletePost;
window.editPost = editPost;
window.removeCurrentImage = removeCurrentImage;
window.previewNewImage = previewNewImage;
window.clearNewImage = clearNewImage;
window.closeEditModal = closeEditModal;
window.savePostEdit = savePostEdit;
window.editComment = editComment;
window.deleteComment = deleteComment;

// Initialize comment action handlers
document.addEventListener('click', function (e) {
    const commentActionBtn = e.target.closest('.comment-action-btn');
    if (commentActionBtn) {
        handleCommentAction(commentActionBtn);
        return;
    }
});

// Initialize reply toggling
initializeReplyToggling();

// Initialize the community hub when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => communityHub.init());
} else {
    communityHub.init();
}