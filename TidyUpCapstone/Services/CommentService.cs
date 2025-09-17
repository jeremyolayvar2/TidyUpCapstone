using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.Entities.Community;

namespace TidyUpCapstone.Services
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetCommentsForPostAsync(int postId, int? currentUserId);
        Task<CommentDto> CreateCommentAsync(CreateCommentDto model, int userId);
        Task<bool> UpdateCommentAsync(int commentId, string content, int userId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }

    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ApplicationDbContext context, ILogger<CommentService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<List<CommentDto>> GetCommentsForPostAsync(int postId, int? currentUserId)
        {
            try
            {
                _logger.LogInformation("🔍 GetCommentsForPost called with postId: {PostId}, currentUserId: {UserId}", postId, currentUserId);

                // Load all comments for this post (including replies) in one query
                var allComments = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.PostId == postId && c.Content != "[deleted]")
                    .OrderBy(c => c.DateCommented)
                    .ToListAsync();

                _logger.LogInformation("📊 Total comments found: {Count}", allComments.Count);

                // Create a dictionary for quick parent lookup
                var commentLookup = allComments.ToDictionary(c => c.CommentId, c => c);

                // Get only top-level comments
                var topLevelComments = allComments
                    .Where(c => c.ParentCommentId == null)
                    .Select(c => MapToCommentDtoWithParentLookup(c, currentUserId, commentLookup))
                    .ToList();

                return topLevelComments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GetCommentsForPostAsync for postId: {PostId}", postId);
                throw;
            }
        }

        public async Task<CommentDto> CreateCommentAsync(CreateCommentDto model, int userId)
        {
            // Verify post exists
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == model.PostId && !p.IsDeleted);

            if (post == null)
                throw new ArgumentException("Post not found.");

            // Verify parent comment exists if this is a reply
            if (model.ParentCommentId.HasValue)
            {
                var parentComment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.CommentId == model.ParentCommentId.Value);

                if (parentComment == null)
                    throw new ArgumentException("Parent comment not found.");
            }

            // Create new comment
            var comment = new Comment
            {
                PostId = model.PostId,
                UserId = userId,
                ParentCommentId = model.ParentCommentId,
                Content = model.Content.Trim(),
                DateCommented = GetPhilippinesTime()
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Load the comment with user info for response
            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == comment.CommentId);

            _logger.LogInformation("Comment created successfully. CommentId: {CommentId}, PostId: {PostId}, UserId: {UserId}",
                comment.CommentId, model.PostId, userId);

            return MapToCommentDto(createdComment!, userId);
        }

        public async Task<bool> UpdateCommentAsync(int commentId, string content, int userId)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                return false;

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null || comment.UserId != userId)
                return false;

            comment.Content = content.Trim();
            comment.LastEdited = GetPhilippinesTime();

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment updated successfully. CommentId: {CommentId}, UserId: {UserId}",
                commentId, userId);

            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null || comment.UserId != userId)
                return false;

            // Soft delete: Replace content with [deleted] instead of removing
            comment.Content = "[deleted]";
            comment.LastEdited = GetPhilippinesTime();

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Comment soft deleted successfully. CommentId: {CommentId}, UserId: {UserId}",
                commentId, userId);

            return true;
        }

        private CommentDto MapToCommentDto(Comment comment, int? currentUserId)
        {
            var replies = comment.Replies?
                .Where(r => r.Content != "[deleted]")
                .OrderBy(r => r.DateCommented)
                .Select(r => MapToCommentDto(r, currentUserId))
                .ToList() ?? new List<CommentDto>();

            return new CommentDto
            {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                UserId = comment.UserId,
                Username = comment.User.UserName ?? "Unknown User",
                UserAvatarUrl = "/assets/default-avatar.svg",
                ParentCommentId = comment.ParentCommentId,

                // TEMPORARILY COMMENT OUT THESE LINES:
                // ParentCommenterName = comment.ParentComment?.User?.UserName,
                // ParentCommenterId = comment.ParentComment?.UserId,

                Content = comment.Content,
                DateCommented = comment.DateCommented,
                LastEdited = comment.LastEdited,
                Replies = replies,
                IsCurrentUser = currentUserId.HasValue && comment.UserId == currentUserId.Value
            };
        }
        private CommentDto MapToCommentDtoWithParentLookup(Comment comment, int? currentUserId, Dictionary<int, Comment> commentLookup)
        {
            // Find replies for this comment
            var replies = commentLookup.Values
                .Where(c => c.ParentCommentId == comment.CommentId)
                .OrderBy(c => c.DateCommented)
                .Select(r => MapToCommentDtoWithParentLookup(r, currentUserId, commentLookup))
                .ToList();

            // Find parent comment if it exists
            Comment parentComment = null;
            if (comment.ParentCommentId.HasValue && commentLookup.ContainsKey(comment.ParentCommentId.Value))
            {
                parentComment = commentLookup[comment.ParentCommentId.Value];
            }

            return new CommentDto
            {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                UserId = comment.UserId,
                Username = comment.User.UserName ?? "Unknown User",
                UserAvatarUrl = "/assets/default-avatar.svg",
                ParentCommentId = comment.ParentCommentId,

                // Add parent commenter information safely
                ParentCommenterName = parentComment?.User?.UserName,
                ParentCommenterId = parentComment?.UserId,

                Content = comment.Content,
                DateCommented = comment.DateCommented,
                LastEdited = comment.LastEdited,
                Replies = replies,
                IsCurrentUser = currentUserId.HasValue && comment.UserId == currentUserId.Value
            };
        }
        private static DateTime GetPhilippinesTime()
        {
            var utcTime = DateTime.UtcNow;
            var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, philippinesTimeZone);
        }
    }
}