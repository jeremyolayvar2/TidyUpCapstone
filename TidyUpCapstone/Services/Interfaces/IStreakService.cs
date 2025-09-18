using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.ViewModels.Gamification;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IStreakService
    {
        // Daily Check-in Streak
        Task<bool> CheckInUserAsync(int userId);
        Task<UserStreakViewModel?> GetDailyCheckInStreakAsync(int userId);
        Task<bool> HasCheckedInTodayAsync(int userId);
        Task<DateTime?> GetLastCheckInDateAsync(int userId);

        // General Streak Management
        Task<List<UserStreakViewModel>> GetUserStreaksAsync(int userId);
        Task<UserStreakViewModel?> GetUserStreakByTypeAsync(int userId, int streakTypeId);
        Task<bool> UpdateStreakAsync(int userId, int streakTypeId, string actionType);
        Task<bool> ResetStreakAsync(int userId, int streakTypeId);

        // Streak Rewards and Milestones
        Task<List<string>> CheckAndAwardMilestoneRewardsAsync(int userId, int streakTypeId);
        Task<decimal> CalculateStreakRewardAsync(int streakTypeId, int currentStreak);
        Task<int> GetNextMilestoneAsync(int currentStreak, int milestoneInterval);
        Task<int> GetDaysUntilMilestoneAsync(int currentStreak, int milestoneInterval);

        // Streak Types Management
        Task<List<StreakType>> GetAllStreakTypesAsync();
        Task<StreakType?> GetStreakTypeByNameAsync(string name);
        Task<StreakType> CreateStreakTypeAsync(StreakType streakType);

        // Streak Statistics
        Task<Dictionary<string, object>> GetStreakStatsAsync(int userId);
        Task<List<UserStreakViewModel>> GetActiveStreaksAsync(int userId);

        // Database Seeding
        Task SeedStreakTypesAsync();
        Task<bool> AreStreakTypesSeededAsync();

        // Maintenance
        Task CheckAndResetExpiredStreaksAsync();
        Task<bool> IsStreakBrokenAsync(int userId, int streakTypeId, DateTime lastActivity, StreakUnit unit);
    }
}