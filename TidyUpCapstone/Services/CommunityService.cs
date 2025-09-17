using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels.Community;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public interface ICommunityService
    {
        Task<List<PostDto>> GetPostsAsync(int? currentUserId, PostType? filterType = null, int page = 1, int pageSize = 10);
        Task<PostDto> CreatePostAsync(CreatePostDto model, int userId, string? imageUrl);
        Task<PostDto?> UpdatePostAsync(int postId, string content, PostType postType, int userId, string? imageUrl);
        Task<bool> DeletePostAsync(int postId, int userId);
        Task<PostDto?> GetPostByIdAsync(int postId, int? currentUserId);
        Task<bool> ToggleReactionAsync(int postId, int userId);
        Task<(bool isLiked, int reactionCount)> GetReactionStatusAsync(int postId, int userId);
        Task<List<PostDto>> GetPinnedPostsAsync(int? currentUserId);
        Task<CommunityStatsViewModel> GetCommunityStatsAsync();
    }

    public class CommunityService : ICommunityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommunityService> _logger;

        public CommunityService(ApplicationDbContext context, ILogger<CommunityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PostDto>> GetPostsAsync(int? currentUserId, PostType? filterType = null, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var query = _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.IsDeleted && !p.IsPinned); // Regular posts only

            if (filterType.HasValue)
            {
                query = query.Where(p => p.PostType == filterType.Value);
            }

            var postEntities = await query
                .OrderByDescending(p => p.DatePosted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var posts = new List<PostDto>();
            foreach (var post in postEntities)
            {
                var postDto = await MapToPostDtoAsync(post, currentUserId);
                posts.Add(postDto);
            }

            return posts;
        }

        public async Task<List<PostDto>> GetPinnedPostsAsync(int? currentUserId)
        {
            var pinnedPostEntities = await _context.Posts
                .Include(p => p.Author)
                .Where(p => !p.IsDeleted && p.IsPinned)
                .OrderByDescending(p => p.DatePosted)
                .ToListAsync();

            var pinnedPosts = new List<PostDto>();
            foreach (var post in pinnedPostEntities)
            {
                var postDto = await MapToPostDtoAsync(post, currentUserId);
                pinnedPosts.Add(postDto);
            }

            return pinnedPosts;
        }

        public async Task<PostDto> CreatePostAsync(CreatePostDto model, int userId, string? imageUrl)
        {
            var post = new Post
            {
                PostContent = model.PostContent?.Trim() ?? string.Empty,
                PostType = model.PostType,
                ImageUrl = imageUrl,
                AuthorId = userId,
                DatePosted = GetPhilippinesTime(),
                IsDeleted = false,
                IsPinned = false
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Load the post with author info for response
            var createdPost = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            return await MapToPostDtoAsync(createdPost!, userId);
        }

        public async Task<PostDto?> UpdatePostAsync(int postId, string content, PostType postType, int userId, string? imageUrl)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

            if (post == null || post.AuthorId != userId)
                return null;

            post.PostContent = content.Trim();
            post.PostType = postType;
            post.ImageUrl = imageUrl;
            post.LastEdited = GetPhilippinesTime();

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return await MapToPostDtoAsync(post, userId);
        }

        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

            if (post == null || post.AuthorId != userId)
                return false;

            post.IsDeleted = true;
            post.LastEdited = GetPhilippinesTime();

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId, int? currentUserId)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Where(p => p.PostId == postId && !p.IsDeleted)
                .FirstOrDefaultAsync();

            if (post == null) return null;

            return await MapToPostDtoAsync(post, currentUserId);
        }

        public async Task<bool> ToggleReactionAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

            if (post == null) return false;

            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

            if (existingReaction != null)
            {
                _context.Reactions.Remove(existingReaction);
            }
            else
            {
                var newReaction = new Reaction
                {
                    PostId = postId,
                    UserId = userId,
                    ReactionType = ReactionType.Like,
                    DateReacted = GetPhilippinesTime()
                };
                _context.Reactions.Add(newReaction);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool isLiked, int reactionCount)> GetReactionStatusAsync(int postId, int userId)
        {
            var isLiked = await _context.Reactions
                .AnyAsync(r => r.PostId == postId && r.UserId == userId);

            var reactionCount = await _context.Reactions
                .CountAsync(r => r.PostId == postId);

            return (isLiked, reactionCount);
        }

        public async Task<CommunityStatsViewModel> GetCommunityStatsAsync()
        {
            try
            {
                var totalPosts = await _context.Posts.CountAsync(p => !p.IsDeleted);
                var totalComments = await _context.Comments.CountAsync(c => c.Content != "[deleted]");
                var totalReactions = await _context.Reactions.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.Status == "active");

                return new CommunityStatsViewModel
                {
                    TotalPosts = totalPosts,
                    TotalComments = totalComments,
                    TotalReactions = totalReactions,
                    ActiveUsers = activeUsers,
                    MostActiveUsers = new List<string>(),
                    TrendingPosts = new List<PostDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting community stats");
                return new CommunityStatsViewModel();
            }
        }

        private async Task<PostDto> MapToPostDtoAsync(Post post, int? currentUserId)
        {
            var reactionCount = await _context.Reactions
                .CountAsync(r => r.PostId == post.PostId);

            var commentCount = await _context.Comments
                .CountAsync(c => c.PostId == post.PostId && c.Content != "[deleted]");

            ReactionType? userReaction = null;
            if (currentUserId.HasValue)
            {
                var reaction = await _context.Reactions
                    .FirstOrDefaultAsync(r => r.PostId == post.PostId && r.UserId == currentUserId.Value);
                userReaction = reaction?.ReactionType;
            }

            var reactionSummary = await _context.Reactions
                .Where(r => r.PostId == post.PostId)
                .GroupBy(r => r.ReactionType)
                .Select(g => new ReactionSummaryDto
                {
                    ReactionType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new PostDto
            {
                PostId = post.PostId,
                AuthorId = post.AuthorId,
                AuthorUsername = post.Author.UserName ?? "Unknown User",
                AuthorAvatarUrl = "/assets/default-avatar.svg",
                PostContent = post.PostContent,
                PostType = post.PostType,
                ImageUrl = post.ImageUrl,
                IsPinned = post.IsPinned,
                DatePosted = post.DatePosted,
                LastEdited = post.LastEdited,
                CommentCount = commentCount,
                ReactionCount = reactionCount,
                UserReaction = userReaction,
                ReactionSummary = reactionSummary
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