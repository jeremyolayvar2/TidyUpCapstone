using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IQuestService
    {
        // Quest Management
        Task<List<QuestDto>> GetActiveQuestsForUserAsync(int userId);
        Task<List<QuestDto>> GetCompletedQuestsForUserAsync(int userId);
        Task<QuestDto?> GetQuestByIdAsync(int questId, int userId);
        Task<bool> StartQuestAsync(int userId, int questId);
        Task<bool> UpdateQuestProgressAsync(int userId, int questId, int progressIncrement, string? actionType = null);
        Task<bool> CompleteQuestAsync(int userId, int questId);
        Task<bool> ClaimQuestRewardAsync(int userId, int questId);

        // Quest Generation
        Task GenerateDailyQuestsAsync();
        Task GenerateWeeklyQuestsAsync();
        Task GenerateSpecialQuestAsync();
        Task<bool> CheckAndExpireQuestsAsync();

        // User Progress
        Task<UserQuestProgressDto?> GetUserQuestProgressAsync(int userId, int questId);
        Task<List<UserQuestProgressDto>> GetAllUserQuestProgressAsync(int userId);

        // Quest Analytics
        Task<Dictionary<string, object>> GetQuestStatsAsync(int userId);
        Task<List<QuestDto>> GetAvailableQuestsAsync(int userId);

        // Admin Functions
        Task<List<Quest>> GetAllQuestsAsync();
        Task<Quest> CreateQuestAsync(Quest quest);
        Task<bool> UpdateQuestAsync(Quest quest);
        Task<bool> DeleteQuestAsync(int questId);
        Task<bool> ToggleQuestActiveStatusAsync(int questId);

        Task<bool> TriggerQuestProgressByActionAsync(int userId, string actionType, int value = 1, object? actionData = null);
        Task<bool> ValidateQuestCompletionAsync(int userId, int questId, string actionType, object? actionData = null);

        Task<bool> CompleteQuestWithAchievementCheckAsync(int userId, int questId);
    }
}