using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Helpers;
using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    /// <summary>
    /// Clean controller for managing community posts with CRUD operations, reactions, and comments
    /// Production version - requires authentication for all operations
    /// </summary>
    [Authorize]
    public class CommunityController : BaseController
    {
        private readonly ICommunityService _communityService;
        private readonly ICommentService _commentService;
        private readonly IReactionService _reactionService;
        private readonly IImageService _imageService;
        private readonly IViewModelService _viewModelService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(
            ICommunityService communityService,
            ICommentService commentService,
            IReactionService reactionService,
            IImageService imageService,
            IViewModelService viewModelService,
            UserManager<AppUser> userManager,
            ILogger<CommunityController> logger)
            : base(userManager)
        {
            _communityService = communityService;
            _commentService = commentService;
            _reactionService = reactionService;
            _imageService = imageService;
            _viewModelService = viewModelService;
            _userManager = userManager;
            _logger = logger;
        }

        #region Helper Methods

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }
            return await _userManager.GetUserAsync(User);
        }

        #endregion

        #region Traditional MVC Actions

        /// <summary>
        /// Display community posts with pagination and filtering
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Index(PostType? filterType = null, string? searchQuery = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var (validatedPage, validatedPageSize) = ValidationHelper.PaginationValidation.ValidateAndNormalizePagination(page, pageSize);

                var currentUser = await GetCurrentUserAsync();
                var currentUserId = currentUser?.Id;

                // Get posts and pinned posts
                var posts = await _communityService.GetPostsAsync(currentUserId, filterType, validatedPage, validatedPageSize);
                var pinnedPosts = validatedPage == 1 ? await _communityService.GetPinnedPostsAsync(currentUserId) : new List<PostDto>();
                var stats = await _communityService.GetCommunityStatsAsync();

                // Build view model
                var viewModel = await _viewModelService.BuildCommunityViewModelAsync(
                    posts, pinnedPosts, filterType, searchQuery, validatedPage, validatedPageSize,
                    posts.Count, User.Identity.IsAuthenticated, stats);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading community posts. FilterType: {FilterType}, Page: {Page}", filterType, page);
                TempData["ErrorMessage"] = "An error occurred while loading posts. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// Display detailed view of a single post with comments
        /// </summary>
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                var currentUserId = currentUser?.Id;

                var post = await _communityService.GetPostByIdAsync(id, currentUserId);
                if (post == null)
                {
                    TempData["ErrorMessage"] = "Post not found.";
                    return RedirectToAction(nameof(Index));
                }

                var comments = await _commentService.GetCommentsForPostAsync(id, currentUserId);
                var relatedPosts = await _communityService.GetPostsAsync(currentUserId, post.PostType, 1, 5);

                var viewModel = await _viewModelService.BuildPostDetailsViewModelAsync(
                    post, comments, relatedPosts,
                    canEdit: currentUserId.HasValue && post.AuthorId == currentUserId.Value,
                    canDelete: currentUserId.HasValue && post.AuthorId == currentUserId.Value,
                    canComment: User.Identity.IsAuthenticated,
                    canReact: User.Identity.IsAuthenticated);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading post details. PostId: {PostId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the post. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region AJAX API Endpoints - Posts

        /// <summary>
        /// API endpoint for creating posts via AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Please fill in all required fields correctly.", errors });
                }

                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to create posts." });

                // Validate content
                var (isValidContent, contentError) = ValidationHelper.PostValidation.ValidatePostContent(model.PostContent);
                if (!isValidContent)
                    return Json(new { success = false, message = contentError });

                // Validate and save image
                var (isValidImage, imageError) = ValidationHelper.ImageValidation.ValidateImageFile(model.ImageFile);
                if (!isValidImage)
                    return Json(new { success = false, message = imageError });

                string? imageUrl = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = await _imageService.SaveImageAsync(model.ImageFile);
                }

                var postDto = await _communityService.CreatePostAsync(model, currentUser.Id, imageUrl);

                _logger.LogInformation("Post created via API successfully. PostId: {PostId}, AuthorId: {AuthorId}",
                    postDto.PostId, currentUser.Id);

                return Json(new { success = true, message = "Post created successfully!", post = postDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in CreatePost");
                return Json(new { success = false, message = "You must be logged in to create posts." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post via API");
                return Json(new { success = false, message = "An error occurred while creating your post. Please try again." });
            }
        }

        /// <summary>
        /// API endpoint for editing posts via AJAX
        /// </summary>
        [HttpPost("Community/EditPost/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [FromForm] string PostContent, [FromForm] string PostType,
            [FromForm] string? RemoveImage, [FromForm] IFormFile? ImageFile)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to edit posts." });

                // Validate content
                var (isValidContent, contentError) = ValidationHelper.PostValidation.ValidatePostContent(PostContent);
                if (!isValidContent)
                    return Json(new { success = false, message = contentError });

                // Validate post type
                var (isValidType, typeError) = ValidationHelper.PostValidation.ValidatePostType(PostType, out var postTypeEnum);
                if (!isValidType)
                    return Json(new { success = false, message = typeError });

                // Validate image if provided
                var (isValidImage, imageError) = ValidationHelper.ImageValidation.ValidateImageFile(ImageFile);
                if (!isValidImage)
                    return Json(new { success = false, message = imageError });

                // Handle image operations
                string? finalImageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    finalImageUrl = await _imageService.SaveImageAsync(ImageFile);
                }
                else if (RemoveImage?.ToLower() != "true")
                {
                    // Keep existing image if not removing and no new image
                    var existingPost = await _communityService.GetPostByIdAsync(id, currentUser.Id);
                    finalImageUrl = existingPost?.ImageUrl;
                }

                var updatedPost = await _communityService.UpdatePostAsync(id, PostContent, postTypeEnum, currentUser.Id, finalImageUrl);

                if (updatedPost == null)
                    return Json(new { success = false, message = "Post not found or you don't have permission to edit it." });

                return Json(new
                {
                    success = true,
                    message = "Post updated successfully!",
                    newImageUrl = finalImageUrl,
                    imageRemoved = string.IsNullOrEmpty(finalImageUrl)
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in EditPost: {PostId}", id);
                return Json(new { success = false, message = "You must be logged in to edit posts." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post via API. PostId: {PostId}", id);
                return Json(new { success = false, message = "An error occurred while updating your post. Please try again." });
            }
        }

        /// <summary>
        /// API endpoint for deleting posts via AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost([FromForm] int postId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to delete posts." });

                var success = await _communityService.DeletePostAsync(postId, currentUser.Id);

                if (!success)
                    return Json(new { success = false, message = "Post not found or you don't have permission to delete it." });

                _logger.LogInformation("Post deleted via API successfully. PostId: {PostId}, AuthorId: {AuthorId}",
                    postId, currentUser.Id);

                return Json(new { success = true, message = "Post deleted successfully!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in DeletePost: {PostId}", postId);
                return Json(new { success = false, message = "You must be logged in to delete posts." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post via API. PostId: {PostId}", postId);
                return Json(new { success = false, message = "An error occurred while deleting your post. Please try again." });
            }
        }

        /// <summary>
        /// API endpoint for loading posts dynamically
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosts(PostType? filterType = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var (validatedPage, validatedPageSize) = ValidationHelper.PaginationValidation.ValidateAndNormalizePagination(page, pageSize);
                var currentUser = await GetCurrentUserAsync();
                var posts = await _communityService.GetPostsAsync(currentUser?.Id, filterType, validatedPage, validatedPageSize);

                return Json(new { success = true, posts = posts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading posts via API");
                return Json(new { success = false, message = "An error occurred while loading posts." });
            }
        }

        #endregion

        #region AJAX API Endpoints - Reactions

        /// <summary>
        /// Toggle like/unlike for a post
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleReaction([FromForm] int postId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to react to posts." });

                var (isLiked, reactionCount) = await _reactionService.ToggleReactionAsync(postId, currentUser.Id);

                return Json(new
                {
                    success = true,
                    isLiked = isLiked,
                    reactionCount = reactionCount,
                    message = isLiked ? "Post liked!" : "Post unliked!"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in ToggleReaction: {PostId}", postId);
                return Json(new { success = false, message = "You must be logged in to react to posts." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling reaction for post {PostId}", postId);
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }

        #endregion

        #region AJAX API Endpoints - Comments

        /// <summary>
        /// Get all comments for a specific post
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int postId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                var comments = await _commentService.GetCommentsForPostAsync(postId, currentUser?.Id);

                return Json(new { success = true, comments = comments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading comments for post {PostId}", postId);
                return Json(new { success = false, message = "Error loading comments." });
            }
        }

        /// <summary>
        /// Create a new comment or reply
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCommentFromForm([FromForm] int postId, [FromForm] string content, [FromForm] int? parentCommentId = null)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to comment." });

                // Validate content
                var (isValid, error) = ValidationHelper.CommentValidation.ValidateCommentContent(content);
                if (!isValid)
                    return Json(new { success = false, message = error });

                var model = new CreateCommentDto
                {
                    PostId = postId,
                    Content = content,
                    ParentCommentId = parentCommentId
                };

                var commentDto = await _commentService.CreateCommentAsync(model, currentUser.Id);

                return Json(new { success = true, message = "Comment added successfully!", comment = commentDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in CreateCommentFromForm: {PostId}", postId);
                return Json(new { success = false, message = "You must be logged in to comment." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId}", postId);
                return Json(new { success = false, message = "Error creating comment. Please try again." });
            }
        }

        /// <summary>
        /// Update an existing comment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment([FromForm] int commentId, [FromForm] string content)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to edit comments." });

                // Validate content
                var (isValid, error) = ValidationHelper.CommentValidation.ValidateCommentContent(content);
                if (!isValid)
                    return Json(new { success = false, message = error });

                var success = await _commentService.UpdateCommentAsync(commentId, content, currentUser.Id);

                if (!success)
                    return Json(new { success = false, message = "Comment not found or you don't have permission to edit it." });

                return Json(new { success = true, message = "Comment updated successfully!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in UpdateComment: {CommentId}", commentId);
                return Json(new { success = false, message = "You must be logged in to edit comments." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
                return Json(new { success = false, message = "Error updating comment. Please try again." });
            }
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment([FromForm] int commentId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "You must be logged in to delete comments." });

                var success = await _commentService.DeleteCommentAsync(commentId, currentUser.Id);

                if (!success)
                    return Json(new { success = false, message = "Comment not found or you don't have permission to delete it." });

                return Json(new { success = true, message = "Comment deleted successfully!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in DeleteComment: {CommentId}", commentId);
                return Json(new { success = false, message = "You must be logged in to delete comments." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                return Json(new { success = false, message = "Error deleting comment. Please try again." });
            }
        }

        #endregion
    }
}