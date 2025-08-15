using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Community;

namespace TidyUpCapstone.Services
{
    public interface IReactionService
    {
        Task<(bool isLiked, int reactionCount)> ToggleReactionAsync(int postId, int userId);
        Task<(bool isLiked, int reactionCount)> GetReactionStatusAsync(int postId, int userId);
        Task<int> GetReactionCountAsync(int postId);
    }

    public class ReactionService : IReactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReactionService> _logger;

        public ReactionService(ApplicationDbContext context, ILogger<ReactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool isLiked, int reactionCount)> ToggleReactionAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

            if (post == null)
                throw new ArgumentException("Post not found");

            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

            bool isLiked;

            if (existingReaction != null)
            {
                // Unlike: Remove the reaction
                _context.Reactions.Remove(existingReaction);
                isLiked = false;
                _logger.LogInformation("Removed reaction for user {UserId} on post {PostId}", userId, postId);
            }
            else
            {
                // Like: Add new reaction
                var newReaction = new Reaction
                {
                    PostId = postId,
                    UserId = userId,
                    ReactionType = ReactionType.Like,
                    DateReacted = GetPhilippinesTime()
                };
                _context.Reactions.Add(newReaction);
                isLiked = true;
                _logger.LogInformation("Added reaction for user {UserId} on post {PostId}", userId, postId);
            }

            await _context.SaveChangesAsync();

            // Get updated reaction count
            var reactionCount = await GetReactionCountAsync(postId);

            return (isLiked, reactionCount);
        }

        public async Task<(bool isLiked, int reactionCount)> GetReactionStatusAsync(int postId, int userId)
        {
            var isLiked = await _context.Reactions
                .AnyAsync(r => r.PostId == postId && r.UserId == userId);

            var reactionCount = await GetReactionCountAsync(postId);

            return (isLiked, reactionCount);
        }

        public async Task<int> GetReactionCountAsync(int postId)
        {
            return await _context.Reactions
                .CountAsync(r => r.PostId == postId);
        }

        private static DateTime GetPhilippinesTime()
        {
            var utcTime = DateTime.UtcNow;
            var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, philippinesTimeZone);
        }
    }
}