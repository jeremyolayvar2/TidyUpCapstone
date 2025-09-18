using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Services.Data;

namespace TidyUpCapstone.Services.Implementations
{
    public interface IActivityQuestIntegrationService
    {
        // MAIN QUEST TEMPLATE METHODS - FULLY FUNCTIONAL
        Task GenerateKonMariDailyQuestsAsync();
        Task GenerateKonMariWeeklyQuestsAsync();
        Task<List<Quest>> CreateQuestsFromTemplatesAsync(List<QuestTemplate> templates, DateTime startDate, DateTime? endDate = null);
        Task AssignQuestsToAllUsersAsync(List<Quest> quests);

        // PLACEHOLDER METHODS - Ready for item posting integration later
        Task ProcessItemListedAsync(int userId, Item item);
        Task ProcessPostCreatedAsync(int userId, Post post);
        Task ProcessCommentCreatedAsync(int userId, Comment comment);
        Task ProcessReactionCreatedAsync(int userId, Reaction reaction);
        Task ProcessTransactionCompletedAsync(int userId, bool isSeller);
        Task RecalculateAllQuestProgressAsync(int userId);
    }

    public class ActivityQuestIntegrationService : IActivityQuestIntegrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAchievementService _achievementService;
        private readonly ILogger<ActivityQuestIntegrationService> _logger;
        private readonly Random _random = new();

        public ActivityQuestIntegrationService(
            ApplicationDbContext context,
            IAchievementService achievementService,
            ILogger<ActivityQuestIntegrationService> logger)
        {
            _context = context;
            _achievementService = achievementService;
            _logger = logger;
        }

        #region FULLY FUNCTIONAL - Quest Template Generation

        public async Task GenerateKonMariDailyQuestsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Check if daily quests already exist for today
                var existingDailyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily &&
                               q.StartDate.HasValue &&
                               q.StartDate.Value.Date == today &&
                               q.IsActive)
                    .CountAsync();

                if (existingDailyQuests >= 3)
                {
                    _logger.LogInformation("Daily quests already exist for today");
                    return;
                }

                // Get quest templates and select 3 random ones from different categories
                var dailyTemplates = KonMariQuestTemplates.GetDailyQuestTemplates();
                var selectedTemplates = SelectDiverseQuestTemplates(dailyTemplates, 3);

                // Create actual quests from templates
                var createdQuests = await CreateQuestsFromTemplatesAsync(selectedTemplates, today, tomorrow);

                // Assign to all users
                await AssignQuestsToAllUsersAsync(createdQuests);

                _logger.LogInformation($"Generated {createdQuests.Count} KonMari daily quests for {today:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating KonMari daily quests");
            }
        }

        public async Task GenerateKonMariWeeklyQuestsAsync()
        {
            try
            {
                var startOfWeek = GetStartOfWeek(DateTime.UtcNow);
                var endOfWeek = startOfWeek.AddDays(7);

                // Check if weekly quests already exist for this week
                var existingWeeklyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Weekly &&
                               q.StartDate >= startOfWeek &&
                               q.StartDate < endOfWeek &&
                               q.IsActive)
                    .CountAsync();

                if (existingWeeklyQuests >= 2)
                {
                    _logger.LogInformation("Weekly quests already exist for this week");
                    return;
                }

                // Get quest templates and select 2 random ones from different categories
                var weeklyTemplates = KonMariQuestTemplates.GetWeeklyQuestTemplates();
                var selectedTemplates = SelectDiverseQuestTemplates(weeklyTemplates, 2);

                // Create actual quests from templates
                var createdQuests = await CreateQuestsFromTemplatesAsync(selectedTemplates, startOfWeek, endOfWeek);

                // Assign to all users
                await AssignQuestsToAllUsersAsync(createdQuests);

                _logger.LogInformation($"Generated {createdQuests.Count} KonMari weekly quests for week of {startOfWeek:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating KonMari weekly quests");
            }
        }

        public async Task<List<Quest>> CreateQuestsFromTemplatesAsync(List<QuestTemplate> templates, DateTime startDate, DateTime? endDate = null)
        {
            var createdQuests = new List<Quest>();

            foreach (var template in templates)
            {
                var quest = new Quest
                {
                    QuestTitle = template.QuestTitle,
                    QuestType = template.QuestType,
                    QuestDescription = template.QuestDescription,
                    QuestObjective = template.QuestObjective,
                    TokenReward = template.TokenReward,
                    XpReward = template.XpReward,
                    Difficulty = template.Difficulty,
                    TargetValue = template.TargetValue,
                    IsActive = true,
                    StartDate = startDate,
                    EndDate = endDate,
                    CreatedAt = DateTime.UtcNow,
                    // ✅ Add these new properties when you update Quest entity
                    //Category = template.Category,
                    //ValidationType = template.ValidationType,
                    //ValidationCriteria = template.ValidationCriteria
                };

                _context.Quests.Add(quest);
                createdQuests.Add(quest);
            }

            await _context.SaveChangesAsync();
            return createdQuests;
        }

        public async Task AssignQuestsToAllUsersAsync(List<Quest> quests)
        {
            var allUsers = await _context.Users.ToListAsync();

            foreach (var quest in quests)
            {
                foreach (var user in allUsers)
                {
                    // Check if user already has this quest assigned
                    var existingAssignment = await _context.UserQuests
                        .AnyAsync(uq => uq.UserId == user.Id && uq.QuestId == quest.QuestId);

                    if (!existingAssignment)
                    {
                        _context.UserQuests.Add(new UserQuest
                        {
                            UserId = user.Id,
                            QuestId = quest.QuestId,
                            CurrentProgress = 0,
                            IsCompleted = false,
                            StartedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned {quests.Count} quests to {allUsers.Count} users");
        }

        #endregion

        #region PLACEHOLDER METHODS - Ready for Future Integration

        // ✅ These methods are placeholders that won't error but are ready for item posting integration
        public async Task ProcessItemListedAsync(int userId, Item item)
        {
            try
            {
                // Get user's active quests that could be progressed by item listing
                var activeQuests = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId &&
                                !uq.IsCompleted &&
                                uq.Quest.IsActive &&
                                (uq.Quest.EndDate == null || uq.Quest.EndDate > DateTime.UtcNow))
                    .ToListAsync();

                foreach (var userQuest in activeQuests)
                {
                    // Check if this quest is related to item listing
                    var quest = userQuest.Quest;
                    bool shouldProgress = quest.QuestObjective?.ToLower().Contains("list") == true ||
                                         quest.QuestObjective?.ToLower().Contains("item") == true ||
                                         quest.QuestTitle?.ToLower().Contains("declutter") == true;

                    if (shouldProgress)
                    {
                        userQuest.CurrentProgress += 1;

                        // Check if quest is completed
                        if (userQuest.CurrentProgress >= quest.TargetValue)
                        {
                            userQuest.IsCompleted = true;
                            userQuest.CompletedAt = DateTime.UtcNow;
                            userQuest.Status = QuestStatus.Completed;

                            _logger.LogInformation($"User {userId} completed quest {quest.QuestId}: {quest.QuestTitle}");
                        }

                        _logger.LogInformation($"User {userId} progressed quest {quest.QuestId}: {userQuest.CurrentProgress}/{quest.TargetValue}");
                    }
                }

                await _context.SaveChangesAsync();

                // Update achievements
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "item_listed", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing item listed for user {userId}");
            }
        }

        public async Task ProcessPostCreatedAsync(int userId, Post post)
        {
            try
            {
                // Get user's active quests that could be progressed by posting
                var activeQuests = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId &&
                                !uq.IsCompleted &&
                                uq.Quest.IsActive &&
                                (uq.Quest.EndDate == null || uq.Quest.EndDate > DateTime.UtcNow))
                    .ToListAsync();

                foreach (var userQuest in activeQuests)
                {
                    var quest = userQuest.Quest;
                    bool shouldProgress = quest.QuestObjective?.ToLower().Contains("post") == true ||
                                         quest.QuestObjective?.ToLower().Contains("share") == true ||
                                         quest.QuestObjective?.ToLower().Contains("community") == true;

                    if (shouldProgress)
                    {
                        userQuest.CurrentProgress += 1;

                        if (userQuest.CurrentProgress >= quest.TargetValue)
                        {
                            userQuest.IsCompleted = true;
                            userQuest.CompletedAt = DateTime.UtcNow;
                            userQuest.Status = QuestStatus.Completed;

                            _logger.LogInformation($"User {userId} completed quest {quest.QuestId}: {quest.QuestTitle}");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "post_created", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing post created for user {userId}");
            }
        }

        public async Task ProcessCommentCreatedAsync(int userId, Comment comment)
        {
            try
            {
                var activeQuests = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId &&
                                !uq.IsCompleted &&
                                uq.Quest.IsActive &&
                                (uq.Quest.EndDate == null || uq.Quest.EndDate > DateTime.UtcNow))
                    .ToListAsync();

                foreach (var userQuest in activeQuests)
                {
                    var quest = userQuest.Quest;
                    bool shouldProgress = quest.QuestObjective?.ToLower().Contains("comment") == true ||
                                         quest.QuestObjective?.ToLower().Contains("help") == true ||
                                         quest.QuestObjective?.ToLower().Contains("support") == true;

                    if (shouldProgress)
                    {
                        userQuest.CurrentProgress += 1;

                        if (userQuest.CurrentProgress >= quest.TargetValue)
                        {
                            userQuest.IsCompleted = true;
                            userQuest.CompletedAt = DateTime.UtcNow;
                            userQuest.Status = QuestStatus.Completed;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "comment_created", 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing comment created for user {userId}");
            }
        }

        public async Task ProcessReactionCreatedAsync(int userId, Reaction reaction)
        {
            try
            {
                // TODO: When reaction system is ready, implement quest validation
                _logger.LogInformation($"[PLACEHOLDER] Reaction created by user {userId} - ready for quest integration");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ProcessReactionCreatedAsync placeholder for user {userId}");
            }
        }

        public async Task ProcessTransactionCompletedAsync(int userId, bool isSeller)
        {
            try
            {
                // TODO: When transaction system is ready, implement quest validation
                _logger.LogInformation($"[PLACEHOLDER] Transaction completed by user {userId}, seller: {isSeller} - ready for quest integration");

                if (isSeller)
                {
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "transaction_completed", 1);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ProcessTransactionCompletedAsync placeholder for user {userId}");
            }
        }

        public async Task RecalculateAllQuestProgressAsync(int userId)
        {
            try
            {
                // TODO: When all features are merged, implement full recalculation
                _logger.LogInformation($"[PLACEHOLDER] Quest progress recalculation for user {userId} - ready for implementation");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in RecalculateAllQuestProgressAsync placeholder for user {userId}");
            }
        }

        #endregion

        #region Helper Methods

        private List<QuestTemplate> SelectDiverseQuestTemplates(List<QuestTemplate> templates, int count)
        {
            // Group by category to ensure diversity
            var categorizedTemplates = templates.GroupBy(q => q.Category).ToList();
            var selectedTemplates = new List<QuestTemplate>();

            // First, try to select one quest from each category
            foreach (var categoryGroup in categorizedTemplates.OrderBy(x => _random.Next()))
            {
                if (selectedTemplates.Count >= count) break;

                var questsInCategory = categoryGroup.ToList();
                var randomQuest = questsInCategory[_random.Next(questsInCategory.Count)];
                selectedTemplates.Add(randomQuest);
            }

            // If we need more quests, randomly select from remaining
            while (selectedTemplates.Count < count && selectedTemplates.Count < templates.Count)
            {
                var remainingTemplates = templates.Except(selectedTemplates).ToList();
                if (!remainingTemplates.Any()) break;

                var randomQuest = remainingTemplates[_random.Next(remainingTemplates.Count)];
                selectedTemplates.Add(randomQuest);
            }

            return selectedTemplates;
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        #endregion
    }

    // Extension service for easy integration when features are ready
    public static class QuestIntegrationExtensions
    {
        public static async Task TriggerQuestUpdate(this IActivityQuestIntegrationService questService,
            string activityType, int userId, object? activityData = null)
        {
            try
            {
                switch (activityType.ToLower())
                {
                    case "item_listed":
                        if (activityData is Item item)
                            await questService.ProcessItemListedAsync(userId, item);
                        break;

                    case "post_created":
                        if (activityData is Post post)
                            await questService.ProcessPostCreatedAsync(userId, post);
                        break;

                    case "comment_created":
                        if (activityData is Comment comment)
                            await questService.ProcessCommentCreatedAsync(userId, comment);
                        break;

                    case "reaction_created":
                        if (activityData is Reaction reaction)
                            await questService.ProcessReactionCreatedAsync(userId, reaction);
                        break;

                    case "transaction_completed":
                        if (activityData is bool isSeller)
                            await questService.ProcessTransactionCompletedAsync(userId, isSeller);
                        break;

                    default:
                        Console.WriteLine($"[PLACEHOLDER] Quest trigger for {activityType} - ready for implementation");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the main operation
                Console.WriteLine($"Quest integration placeholder error: {ex.Message}");
            }
        }
    }
}