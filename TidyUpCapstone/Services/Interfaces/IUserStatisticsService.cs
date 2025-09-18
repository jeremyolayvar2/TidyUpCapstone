using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IUserStatisticsService
    {
        // Existing methods (keep these)
        Task<bool> AwardTokensAsync(int userId, decimal tokens, string reason);
        Task<bool> AwardXpAsync(int userId, int xp, string reason);
        Task<bool> AwardTokensAndXpAsync(int userId, decimal tokens, int xp, string reason);
        Task<bool> DeductTokensAsync(int userId, decimal tokens, string reason);
        Task<UserStats> GetUserStatsAsync(int userId);
        Task<UserStats> GetOrCreateUserStatsAsync(int userId);
        Task<bool> ReconcileUserStatsAsync(int userId);
        Task<bool> SyncToAppUserAsync(int userId);

        // Add these missing method signatures that your controller calls
        Task<UserStatisticsDto> GetUserStatisticsAsync(int userId);
        Task<bool> SetUserStatsAsync(int userId, decimal tokenBalance, int xp, string reason);
    }
}