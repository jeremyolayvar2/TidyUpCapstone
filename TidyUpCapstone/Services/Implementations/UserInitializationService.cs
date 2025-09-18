using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Implementations
{
    public class UserInitializationService : IUserInitializationService
    {
        private readonly ApplicationDbContext _context;

        public UserInitializationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserInitializedAsync(int userId)
        {
            return await _context.UserAchievements.AnyAsync(ua => ua.UserId == userId);
        }

        public async Task InitializeUserAsync(int userId)
        {
            if (await IsUserInitializedAsync(userId)) return;

            // Only seed achievements and stats - NOT quests
            var allAchievements = await _context.Achievements.Where(a => a.IsActive).ToListAsync();

            var userAchievements = allAchievements.Select(a => new UserAchievement
            {
                UserId = userId,
                AchievementId = a.AchievementId,
                IsUnlocked = false,
                Progress = 0,
                EarnedDate = null,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.UserAchievements.AddRange(userAchievements);

            // Create user stats
            var hasStats = await _context.UserStats.AnyAsync(us => us.UserId == userId);
            if (!hasStats)
            {
                var userStats = new UserStats
                {
                    UserId = userId,
                    CurrentLevel = 1,
                    CurrentXp = 0,
                    TotalTokens = 0,
                    CurrentStreak = 0,
                    LongestStreak = 0
                };
                _context.UserStats.Add(userStats);
            }

            await _context.SaveChangesAsync();
        }
    }
}