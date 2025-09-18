using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Helpers
{
    public interface IQuestHelperService
    {
        Task TriggerQuestProgressAsync(int userId, string actionType, int value = 1);
        Task TriggerItemListedAsync(int userId);
        Task TriggerTransactionCompletedAsync(int userId, bool isSeller);
        Task TriggerCommentCreatedAsync(int userId);
        Task TriggerPostCreatedAsync(int userId);
        Task TriggerMarketplaceViewAsync(int userId);
    }

    public class QuestHelperService : IQuestHelperService
    {
        private readonly IQuestService _questService;
        private readonly IAchievementService _achievementService;
        private readonly IStreakService _streakService;
        private readonly ILogger<QuestHelperService> _logger;

        public QuestHelperService(
            IQuestService questService,
            IAchievementService achievementService,
            IStreakService streakService,
            ILogger<QuestHelperService> logger)
        {
            _questService = questService;
            _achievementService = achievementService;
            _streakService = streakService;
            _logger = logger;
        }

        public async Task TriggerQuestProgressAsync(int userId, string actionType, int value = 1)
        {
            try
            {
                // Get user's active quests
                var activeQuests = await _questService.GetActiveQuestsForUserAsync(userId);

                foreach (var quest in activeQuests)
                {
                    bool shouldUpdate = actionType switch
                    {
                        "item_listed" => quest.QuestObjective?.Contains("List") == true || quest.QuestObjective?.Contains("items") == true,
                        "comment_created" => quest.QuestObjective?.Contains("comment") == true,
                        "post_created" => quest.QuestObjective?.Contains("post") == true,
                        "marketplace_view" => quest.QuestObjective?.Contains("View") == true || quest.QuestObjective?.Contains("listings") == true,
                        "transaction_completed" => quest.QuestObjective?.Contains("transaction") == true,
                        _ => false
                    };

                    if (shouldUpdate)
                    {
                        await _questService.UpdateQuestProgressAsync(userId, quest.QuestId, value, actionType);
                    }
                }

                // Update achievements
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, actionType, value);

                // Update relevant streaks
                await UpdateStreaksAsync(userId, actionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error triggering quest progress for user {userId}, action {actionType}");
            }
        }

        public async Task TriggerItemListedAsync(int userId)
        {
            await TriggerQuestProgressAsync(userId, "item_listed", 1);

            // Update decluttering streak if exists
            var streakTypes = await _streakService.GetAllStreakTypesAsync();
            var declutteringStreak = streakTypes.FirstOrDefault(st => st.Name == "Daily Declutterer");
            if (declutteringStreak != null)
            {
                await _streakService.UpdateStreakAsync(userId, declutteringStreak.StreakTypeId, "item_listed");
            }
        }

        public async Task TriggerTransactionCompletedAsync(int userId, bool isSeller)
        {
            if (isSeller)
            {
                await TriggerQuestProgressAsync(userId, "transaction_completed", 1);

                // Update trading streak if exists
                var streakTypes = await _streakService.GetAllStreakTypesAsync();
                var tradingStreak = streakTypes.FirstOrDefault(st => st.Name == "Trading Streak");
                if (tradingStreak != null)
                {
                    await _streakService.UpdateStreakAsync(userId, tradingStreak.StreakTypeId, "transaction_completed");
                }
            }
        }

        public async Task TriggerCommentCreatedAsync(int userId)
        {
            await TriggerQuestProgressAsync(userId, "comment_created", 1);

            // Update community contributor streak if exists
            var streakTypes = await _streakService.GetAllStreakTypesAsync();
            var communityStreak = streakTypes.FirstOrDefault(st => st.Name == "Community Contributor");
            if (communityStreak != null)
            {
                await _streakService.UpdateStreakAsync(userId, communityStreak.StreakTypeId, "comment_created");
            }
        }

        public async Task TriggerPostCreatedAsync(int userId)
        {
            await TriggerQuestProgressAsync(userId, "post_created", 1);

            // Update community contributor streak if exists
            var streakTypes = await _streakService.GetAllStreakTypesAsync();
            var communityStreak = streakTypes.FirstOrDefault(st => st.Name == "Community Contributor");
            if (communityStreak != null)
            {
                await _streakService.UpdateStreakAsync(userId, communityStreak.StreakTypeId, "post_created");
            }
        }

        public async Task TriggerMarketplaceViewAsync(int userId)
        {
            await TriggerQuestProgressAsync(userId, "marketplace_view", 1);
        }

        private async Task UpdateStreaksAsync(int userId, string actionType)
        {
            try
            {
                var streakTypes = await _streakService.GetAllStreakTypesAsync();

                foreach (var streakType in streakTypes)
                {
                    bool shouldUpdate = (streakType.Name, actionType) switch
                    {
                        ("Daily Declutterer", "item_listed") => true,
                        ("Community Contributor", "comment_created") => true,
                        ("Community Contributor", "post_created") => true,
                        ("Trading Streak", "transaction_completed") => true,
                        _ => false
                    };

                    if (shouldUpdate)
                    {
                        await _streakService.UpdateStreakAsync(userId, streakType.StreakTypeId, actionType);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating streaks for user {userId}, action {actionType}");
            }
        }
    }
}