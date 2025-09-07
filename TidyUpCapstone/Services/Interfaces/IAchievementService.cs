using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IAchievementService
    {
        // Achievement Management
        Task<List<AchievementDto>> GetUserAchievementsAsync(int userId);
        Task<List<AchievementDto>> GetAllAchievementsAsync(int userId);
        Task<AchievementDto?> GetAchievementByIdAsync(int achievementId, int userId);

        // Achievement Progress and Unlocking
        Task<bool> UpdateAchievementProgressAsync(int userId, string criteriaType, int progressValue);
        Task<List<AchievementDto>> CheckAndUnlockAchievementsAsync(int userId, string actionType, int value = 1);
        Task<bool> NotifyUserOfNewAchievementAsync(int userId, int achievementId);

        // Achievement Categories
        Task<List<AchievementDto>> GetAchievementsByCategoryAsync(AchievementCategory category, int userId);
        Task<List<AchievementDto>> GetSecretAchievementsAsync(int userId);
        Task<List<AchievementDto>> GetRecentAchievementsAsync(int userId, int limit = 5);

        // Achievement Statistics
        Task<Dictionary<string, object>> GetAchievementStatsAsync(int userId);
        Task<int> GetCompletionPercentageAsync(int userId);

        // Database Seeding
        Task SeedAchievementsAsync();
        Task<bool> AreAchievementsSeededAsync();

        // Admin Functions
        Task<Achievement> CreateAchievementAsync(Achievement achievement);
        Task<bool> UpdateAchievementAsync(Achievement achievement);
        Task<bool> DeleteAchievementAsync(int achievementId);
        Task<bool> ToggleAchievementActiveStatusAsync(int achievementId);
    }
}