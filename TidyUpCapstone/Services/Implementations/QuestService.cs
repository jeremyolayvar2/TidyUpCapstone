using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Services.Data;

namespace TidyUpCapstone.Services.Implementations
{
    public class QuestService : IQuestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAchievementService _achievementService;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly ILogger<QuestService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new();

        public QuestService(
            ApplicationDbContext context,
            IAchievementService achievementService,
            IUserStatisticsService userStatisticsService,
            ILogger<QuestService> logger,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _achievementService = achievementService;
            _userStatisticsService = userStatisticsService;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<List<QuestDto>> GetActiveQuestsForUserAsync(int userId)
        {
            var userQuests = await _context.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId &&
                             uq.Quest.IsActive &&
                             !uq.IsCompleted &&
                             (uq.Quest.EndDate == null || uq.Quest.EndDate > DateTime.UtcNow))
                .Select(uq => new QuestDto
                {
                    QuestId = uq.Quest.QuestId,
                    QuestTitle = uq.Quest.QuestTitle,
                    QuestType = uq.Quest.QuestType,
                    QuestDescription = uq.Quest.QuestDescription,
                    QuestObjective = uq.Quest.QuestObjective,
                    TokenReward = uq.Quest.TokenReward,
                    XpReward = uq.Quest.XpReward,
                    Difficulty = uq.Quest.Difficulty,
                    TargetValue = uq.Quest.TargetValue,
                    IsActive = uq.Quest.IsActive,
                    StartDate = uq.Quest.StartDate,
                    EndDate = uq.Quest.EndDate,
                    IsCompleted = uq.IsCompleted,
                    CurrentProgress = uq.CurrentProgress,
                    IsClaimed = uq.DateClaimed != null,
                    CompletedAt = uq.CompletedAt,
                    ProgressPercentage = uq.Quest.TargetValue > 0
                    ? (uq.CurrentProgress * 100 / uq.Quest.TargetValue)
                    : 0,
                                IsAvailable = true,
                                // ✅ ADD THIS - Can't call method in LINQ, so inline the logic
                                StatusMessage = uq.IsCompleted
                    ? (uq.DateClaimed != null ? "Completed & Rewards Claimed" : "Completed - Ready to Claim")
                    : (uq.CurrentProgress == 0 ? "Not Started" : "In Progress")
                })
                .ToListAsync();

            return userQuests;
        }

        public async Task<List<QuestDto>> GetCompletedQuestsForUserAsync(int userId)
        {
            var completedQuests = await _context.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId && uq.IsCompleted)
                .OrderByDescending(uq => uq.CompletedAt)
                .Select(uq => new QuestDto
                {
                    QuestId = uq.Quest.QuestId,
                    QuestTitle = uq.Quest.QuestTitle,
                    QuestType = uq.Quest.QuestType,
                    QuestDescription = uq.Quest.QuestDescription,
                    QuestObjective = uq.Quest.QuestObjective,
                    TokenReward = uq.Quest.TokenReward,
                    XpReward = uq.Quest.XpReward,
                    Difficulty = uq.Quest.Difficulty,
                    TargetValue = uq.Quest.TargetValue,
                    IsActive = uq.Quest.IsActive,
                    StartDate = uq.Quest.StartDate,
                    EndDate = uq.Quest.EndDate,
                    IsCompleted = uq.IsCompleted,
                    CurrentProgress = uq.CurrentProgress,
                    IsClaimed = uq.DateClaimed != null,
                    CompletedAt = uq.CompletedAt,
                    ProgressPercentage = 100,
                    IsAvailable = false,
                    StatusMessage = uq.DateClaimed != null ? "Reward Claimed" : "Ready to Claim"
                })
                .ToListAsync();

            return completedQuests;
        }

        public async Task<bool> StartQuestAsync(int userId, int questId)
        {
            try
            {
                var existingUserQuest = await _context.UserQuests
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

                if (existingUserQuest != null)
                    return false;

                var quest = await _context.Quests.FindAsync(questId);
                if (quest == null || !quest.IsActive)
                    return false;

                var userQuest = new UserQuest
                {
                    UserId = userId,
                    QuestId = questId,
                    CurrentProgress = 0,
                    StartedAt = DateTime.UtcNow
                };

                _context.UserQuests.Add(userQuest);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {userId} started quest {questId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting quest {questId} for user {userId}");
                return false;
            }
        }

        public async Task<bool> UpdateQuestProgressAsync(int userId, int questId, int progressIncrement, string? actionType = null)
        {
            try
            {
                var userQuest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId && !uq.IsCompleted);

                if (userQuest == null)
                    return false;

                userQuest.CurrentProgress += progressIncrement;

                if (userQuest.CurrentProgress > userQuest.Quest.TargetValue)
                    userQuest.CurrentProgress = userQuest.Quest.TargetValue;

                var progressRecord = new QuestProgress
                {
                    UserQuestId = userQuest.UserQuestId,
                    ProgressValue = progressIncrement,
                    GoalValue = userQuest.Quest.TargetValue,
                    ActionType = actionType,
                    ActionTimestamp = DateTime.UtcNow
                };
                _context.QuestProgresses.Add(progressRecord);

                // Auto-complete quest if target reached
                if (userQuest.CurrentProgress >= userQuest.Quest.TargetValue)
                {
                    userQuest.IsCompleted = true;
                    userQuest.CompletedAt = DateTime.UtcNow;
                    userQuest.Status = QuestStatus.Completed;
                    userQuest.DateClaimed = DateTime.UtcNow; // Auto-claim on completion

                    _logger.LogInformation($"Quest auto-completed: User {userId}, Quest {questId} - {userQuest.Quest.QuestTitle}");

                    // Award tokens and XP immediately upon completion
                    await _userStatisticsService.AwardTokensAndXpAsync(
                        userId,
                        userQuest.Quest.TokenReward,
                        userQuest.Quest.XpReward,
                        $"Quest completed: {userQuest.Quest.QuestTitle}");
                }

                // Save changes FIRST
                await _context.SaveChangesAsync();

                // THEN trigger achievement checks (after database is updated)
                if (userQuest.CurrentProgress >= userQuest.Quest.TargetValue)
                {
                    // ✅ UPDATE ACHIEVEMENT PROGRESS FIRST
                    var questAchievements = await _context.UserAchievements
                        .Include(ua => ua.Achievement)
                        .Where(ua => ua.UserId == userId && ua.Achievement.CriteriaType == "total_quests_completed")
                        .ToListAsync();

                    var newQuestCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);

                    foreach (var questAchievement in questAchievements)
                    {
                        questAchievement.Progress = newQuestCount;
                    }

                    await _context.SaveChangesAsync(); // Save progress updates

                    // ✅ THEN CHECK ACHIEVEMENTS
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_completed", 1);
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_quests_completed", 1);
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "tokens_earned", (int)userQuest.Quest.TokenReward);
                    await TriggerQuestCompletionAchievementsAsync(userId, userQuest.Quest);
                }

                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_progress", progressIncrement);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating quest progress for user {userId}, quest {questId}");
                return false;
            }
        }

        public async Task<bool> TriggerQuestProgressByActionAsync(int userId, string actionType, int value = 1, object? actionData = null)
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

                bool anyProgressMade = false;

                foreach (var userQuest in activeQuests)
                {
                    bool shouldProgress = ShouldQuestProgressForAction(userQuest.Quest, actionType);

                    if (shouldProgress)
                    {
                        await UpdateQuestProgressAsync(userId, userQuest.QuestId, value, actionType);
                        anyProgressMade = true;

                        _logger.LogInformation($"Quest progress triggered: User {userId}, Action {actionType}, Quest {userQuest.Quest.QuestTitle}");
                    }
                }

                return anyProgressMade;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error triggering quest progress for user {userId}, action {actionType}");
                return false;
            }
        }

        // In QuestService.cs - Replace the incomplete method
        private static bool ShouldQuestProgressForAction(Quest quest, string actionType)
        {
            var objective = quest.QuestObjective?.ToLower() ?? "";
            var title = quest.QuestTitle?.ToLower() ?? "";
            var description = quest.QuestDescription?.ToLower() ?? "";

            return actionType.ToLower() switch
            {
                "check_in" => objective.Contains("check") || objective.Contains("intention") ||
                             title.Contains("mindfulness") || title.Contains("morning") ||
                             objective.Contains("daily"),

                "item_listed" => objective.Contains("list") || objective.Contains("declutter") ||
                                objective.Contains("item") || title.Contains("declutter") ||
                                objective.Contains("clothing") || objective.Contains("books") ||
                                objective.Contains("kitchen") || objective.Contains("bathroom") ||
                                objective.Contains("garage") || objective.Contains("miscellaneous"),

                "post_created" => objective.Contains("share") || objective.Contains("post") ||
                                 title.Contains("share") || title.Contains("tip") ||
                                 objective.Contains("community") || objective.Contains("wisdom") ||
                                 objective.Contains("inspiration"),

                "comment_created" => objective.Contains("help") || objective.Contains("support") ||
                                    objective.Contains("comment") || title.Contains("helper") ||
                                    objective.Contains("encouraging") || objective.Contains("community"),

                "reaction_created" => objective.Contains("react") || objective.Contains("positive") ||
                                     title.Contains("support") || title.Contains("cheerleader") ||
                                     objective.Contains("positively"),

                "category_completed" => objective.Contains("category") || title.Contains("master") ||
                                       title.Contains("transformation") || objective.Contains("complete"),

                "quest_completed" => quest.QuestType == QuestType.Special ||
                                    objective.Contains("milestone") || title.Contains("mastery"),

                _ => false
            };
        }

        public async Task<bool> CompleteQuestAsync(int userId, int questId)
        {
            try
            {
                var userQuest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId && !uq.IsCompleted);

                if (userQuest == null)
                    return false;

                userQuest.IsCompleted = true;
                userQuest.CompletedAt = DateTime.UtcNow;
                userQuest.CurrentProgress = userQuest.Quest.TargetValue;

                await _context.SaveChangesAsync();

                await TriggerQuestCompletionAchievementsAsync(userId, userQuest.Quest);

                _logger.LogInformation($"User {userId} completed quest {questId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing quest {questId} for user {userId}");
                return false;
            }
        }

        public async Task<bool> ClaimQuestRewardAsync(int userId, int questId)
        {
            try
            {
                var userQuest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId &&
                                              uq.IsCompleted && uq.DateClaimed == null);

                if (userQuest == null)
                    return false;

                // Use centralized statistics service
                var success = await _userStatisticsService.AwardTokensAndXpAsync(
                    userId,
                    userQuest.Quest.TokenReward,
                    userQuest.Quest.XpReward,
                    $"Quest completed: {userQuest.Quest.QuestTitle}");

                if (success)
                {
                    userQuest.DateClaimed = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "rewards_claimed", 1);
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "tokens_earned", (int)userQuest.Quest.TokenReward);

                    _logger.LogInformation($"User {userId} claimed reward for quest {questId}: {userQuest.Quest.TokenReward} tokens, {userQuest.Quest.XpReward} XP");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error claiming reward for quest {questId} by user {userId}");
                return false;
            }
        }

        public async Task GenerateDailyQuestsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Remove expired daily quests
                var expiredDailyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily &&
                               q.EndDate.HasValue && q.EndDate.Value < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var quest in expiredDailyQuests)
                {
                    quest.IsActive = false;
                }

                // Check if daily quests already exist for today
                var existingDailyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily &&
                               q.StartDate.HasValue &&
                               q.StartDate.Value.Date == today &&
                               q.IsActive)
                    .CountAsync();

                if (existingDailyQuests >= 5)
                {
                    _logger.LogInformation("Daily quests already exist for today");
                    return;
                }

                // Use KonMari quest templates
                var dailyTemplates = KonMariQuestTemplates.GetDailyQuestTemplates();
                var selectedTemplates = SelectDiverseQuestTemplates(dailyTemplates, 3);

                var questsToAdd = selectedTemplates.Select(template => new Quest
                {
                    QuestTitle = template.QuestTitle,
                    QuestType = QuestType.Daily,
                    QuestDescription = template.QuestDescription,
                    QuestObjective = template.QuestObjective,
                    TokenReward = template.TokenReward,
                    XpReward = template.XpReward,
                    Difficulty = template.Difficulty,
                    TargetValue = template.TargetValue,
                    IsActive = true,
                    StartDate = today,
                    EndDate = tomorrow,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.Quests.AddRange(questsToAdd);
                await _context.SaveChangesAsync();

                // Assign to all users
                await AssignQuestsToAllUsersAsync(questsToAdd);

                _logger.LogInformation($"Generated {questsToAdd.Count} KonMari daily quests for {today:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily quests");
            }
        }

        public async Task GenerateWeeklyQuestsAsync()
        {
            try
            {
                var startOfWeek = GetStartOfWeek(DateTime.UtcNow);
                var endOfWeek = startOfWeek.AddDays(7);

                var existingWeeklyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Weekly &&
                                q.StartDate >= startOfWeek && q.StartDate < endOfWeek &&
                                q.IsActive)
                    .CountAsync();

                if (existingWeeklyQuests >= 2)
                {
                    _logger.LogInformation("Weekly quests already exist for this week");
                    return;
                }

                // Use KonMari quest templates
                var weeklyTemplates = KonMariQuestTemplates.GetWeeklyQuestTemplates();
                var selectedTemplates = SelectDiverseQuestTemplates(weeklyTemplates, 2);

                var questsToAdd = selectedTemplates.Select(template => new Quest
                {
                    QuestTitle = template.QuestTitle,
                    QuestType = QuestType.Weekly,
                    QuestDescription = template.QuestDescription,
                    QuestObjective = template.QuestObjective,
                    TokenReward = template.TokenReward,
                    XpReward = template.XpReward,
                    Difficulty = template.Difficulty,
                    TargetValue = template.TargetValue,
                    IsActive = true,
                    StartDate = startOfWeek,
                    EndDate = endOfWeek,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.Quests.AddRange(questsToAdd);
                await _context.SaveChangesAsync();

                // Assign to all users
                await AssignQuestsToAllUsersAsync(questsToAdd);

                _logger.LogInformation($"Generated {questsToAdd.Count} KonMari weekly quests for week of {startOfWeek:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly quests");
            }
        }

        public async Task GenerateSpecialQuestAsync()
        {
            try
            {
                // Check if there are already active special quests
                var activeSpecialQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Special && q.IsActive)
                    .CountAsync();

                // Limit to 1 active special quest at a time
                if (activeSpecialQuests >= 1)
                {
                    _logger.LogInformation("Special quest already exists");
                    return;
                }

                // Get available special quest templates
                var specialTemplates = KonMariQuestTemplates.GetSpecialQuestTemplates();

                // Filter out templates that already have active quests
                var availableTemplates = new List<QuestTemplate>();

                foreach (var template in specialTemplates)
                {
                    var existingQuest = await _context.Quests
                        .Where(q => q.QuestTitle == template.QuestTitle &&
                                   q.IsActive &&
                                   q.QuestType == QuestType.Special)
                        .FirstOrDefaultAsync();

                    if (existingQuest == null)
                    {
                        availableTemplates.Add(template);
                    }
                }

                if (!availableTemplates.Any())
                {
                    _logger.LogInformation("No available special quest templates");
                    return;
                }

                // Check User 1's eligibility for special quests
                var userId = 1; // Since we're using User 1
                var completedQuests = await _context.UserQuests
                    .Where(uq => uq.UserId == userId && uq.IsCompleted)
                    .CountAsync();

                // Select appropriate template based on user progress
                QuestTemplate selectedTemplate = null;

                if (completedQuests >= 20) // High-level users
                {
                    selectedTemplate = availableTemplates
                        .Where(t => t.Difficulty == QuestDifficulty.Hard)
                        .OrderBy(r => Guid.NewGuid())
                        .FirstOrDefault();
                }
                else if (completedQuests >= 10) // Mid-level users  
                {
                    selectedTemplate = availableTemplates
                        .Where(t => t.Difficulty == QuestDifficulty.Medium)
                        .OrderBy(r => Guid.NewGuid())
                        .FirstOrDefault();
                }
                else if (completedQuests >= 3) // Entry-level users
                {
                    selectedTemplate = availableTemplates
                        .Where(t => t.Difficulty == QuestDifficulty.Easy)
                        .OrderBy(r => Guid.NewGuid())
                        .FirstOrDefault();
                }

                // If no suitable template found, pick any available easy one
                if (selectedTemplate == null)
                {
                    selectedTemplate = availableTemplates
                        .Where(t => t.Difficulty == QuestDifficulty.Easy)
                        .FirstOrDefault();
                }

                if (selectedTemplate == null)
                {
                    _logger.LogInformation("No suitable special quest template for user progress level");
                    return;
                }

                // Create quest from template
                var specialQuest = new Quest
                {
                    QuestTitle = selectedTemplate.QuestTitle,
                    QuestDescription = selectedTemplate.QuestDescription,
                    QuestObjective = selectedTemplate.QuestObjective,
                    QuestType = QuestType.Special,
                    Difficulty = selectedTemplate.Difficulty,
                    TargetValue = selectedTemplate.TargetValue,
                    TokenReward = selectedTemplate.TokenReward,
                    XpReward = selectedTemplate.XpReward,
                    IsActive = true,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(GetSpecialQuestDurationDays(selectedTemplate.Difficulty)),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Quests.Add(specialQuest);
                await _context.SaveChangesAsync();

                // Assign to users using the new assignment logic
                await AssignQuestsToAllUsersAsync(new List<Quest> { specialQuest });

                _logger.LogInformation($"Generated special quest for users with {completedQuests}+ completed quests: {specialQuest.QuestTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating special quest");
            }
        }

        private static int GetSpecialQuestDurationDays(QuestDifficulty difficulty)
        {
            return difficulty switch
            {
                QuestDifficulty.Easy => 21,    // 3 weeks
                QuestDifficulty.Medium => 35,  // 5 weeks  
                QuestDifficulty.Hard => 60,    // 2 months
                _ => 30                         // Default 1 month
            };
        }

        public async Task<bool> CheckAndExpireQuestsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredQuests = await _context.Quests
                    .Where(q => q.IsActive && q.EndDate.HasValue && q.EndDate.Value < now)
                    .ToListAsync();

                foreach (var quest in expiredQuests)
                {
                    quest.IsActive = false;
                }

                if (expiredQuests.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Expired {expiredQuests.Count} quests");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and expiring quests");
                return false;
            }
        }

        public async Task<List<QuestDto>> GetAvailableQuestsAsync(int userId)
        {
            var userQuestIds = await _context.UserQuests
                .Where(uq => uq.UserId == userId)
                .Select(uq => uq.QuestId)
                .ToListAsync();

            var availableQuests = await _context.Quests
                .Where(q => q.IsActive &&
                           !userQuestIds.Contains(q.QuestId) &&
                           (q.EndDate == null || q.EndDate > DateTime.UtcNow))
                .Select(q => new QuestDto
                {
                    QuestId = q.QuestId,
                    QuestTitle = q.QuestTitle,
                    QuestType = q.QuestType,
                    QuestDescription = q.QuestDescription,
                    QuestObjective = q.QuestObjective,
                    TokenReward = q.TokenReward,
                    XpReward = q.XpReward,
                    Difficulty = q.Difficulty,
                    TargetValue = q.TargetValue,
                    IsActive = q.IsActive,
                    StartDate = q.StartDate,
                    EndDate = q.EndDate,
                    IsCompleted = false,
                    CurrentProgress = 0,
                    IsClaimed = false,
                    ProgressPercentage = 0,
                    IsAvailable = true,
                    StatusMessage = "Available to Start"
                })
                .ToListAsync();

            return availableQuests;
        }

        public async Task<QuestDto?> GetQuestByIdAsync(int questId, int userId)
        {
            var userQuest = await _context.UserQuests
                .Include(uq => uq.Quest)
                .FirstOrDefaultAsync(uq => uq.QuestId == questId && uq.UserId == userId);

            if (userQuest == null)
            {
                var quest = await _context.Quests.FindAsync(questId);
                if (quest != null)
                {
                    return new QuestDto
                    {
                        QuestId = quest.QuestId,
                        QuestTitle = quest.QuestTitle,
                        QuestType = quest.QuestType,
                        QuestDescription = quest.QuestDescription,
                        QuestObjective = quest.QuestObjective,
                        TokenReward = quest.TokenReward,
                        XpReward = quest.XpReward,
                        Difficulty = quest.Difficulty,
                        TargetValue = quest.TargetValue,
                        IsActive = quest.IsActive,
                        StartDate = quest.StartDate,
                        EndDate = quest.EndDate,
                        IsCompleted = false,
                        CurrentProgress = 0,
                        IsClaimed = false,
                        ProgressPercentage = 0,
                        IsAvailable = true,
                        StatusMessage = "Not Started"
                    };
                }
                return null;
            }

            return new QuestDto
            {
                QuestId = userQuest.Quest.QuestId,
                QuestTitle = userQuest.Quest.QuestTitle,
                QuestType = userQuest.Quest.QuestType,
                QuestDescription = userQuest.Quest.QuestDescription,
                QuestObjective = userQuest.Quest.QuestObjective,
                TokenReward = userQuest.Quest.TokenReward,
                XpReward = userQuest.Quest.XpReward,
                Difficulty = userQuest.Quest.Difficulty,
                TargetValue = userQuest.Quest.TargetValue,
                IsActive = userQuest.Quest.IsActive,
                StartDate = userQuest.Quest.StartDate,
                EndDate = userQuest.Quest.EndDate,
                IsCompleted = userQuest.IsCompleted,
                CurrentProgress = userQuest.CurrentProgress,
                IsClaimed = userQuest.DateClaimed != null,
                CompletedAt = userQuest.CompletedAt,
                ProgressPercentage = userQuest.Quest.TargetValue > 0 ?
                (userQuest.CurrentProgress * 100 / userQuest.Quest.TargetValue) : 0,
                        IsAvailable = !userQuest.IsCompleted,
                // ✅ UPDATE THIS LINE
                StatusMessage = GetQuestStatusMessage(userQuest)
            };
        }

        public async Task<UserQuestProgressDto?> GetUserQuestProgressAsync(int userId, int questId)
        {
            var userQuest = await _context.UserQuests
                .Include(uq => uq.Quest)
                .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

            if (userQuest == null)
                return null;

            return new UserQuestProgressDto
            {
                UserQuestId = userQuest.UserQuestId,
                QuestId = userQuest.QuestId,
                QuestTitle = userQuest.Quest.QuestTitle,
                CurrentProgress = userQuest.CurrentProgress,
                TargetValue = userQuest.Quest.TargetValue,
                IsCompleted = userQuest.IsCompleted,
                CompletedAt = userQuest.CompletedAt,
                IsClaimed = userQuest.DateClaimed != null,
                TokenReward = userQuest.Quest.TokenReward,
                XpReward = userQuest.Quest.XpReward
            };
        }

        public async Task<List<UserQuestProgressDto>> GetAllUserQuestProgressAsync(int userId)
        {
            return await _context.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId)
                .Select(uq => new UserQuestProgressDto
                {
                    UserQuestId = uq.UserQuestId,
                    QuestId = uq.QuestId,
                    QuestTitle = uq.Quest.QuestTitle,
                    CurrentProgress = uq.CurrentProgress,
                    TargetValue = uq.Quest.TargetValue,
                    IsCompleted = uq.IsCompleted,
                    CompletedAt = uq.CompletedAt,
                    IsClaimed = uq.DateClaimed != null,
                    TokenReward = uq.Quest.TokenReward,
                    XpReward = uq.Quest.XpReward
                })
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetQuestStatsAsync(int userId)
        {
            var stats = new Dictionary<string, object>();

            var userQuests = await _context.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId)
                .ToListAsync();

            stats["TotalQuestsStarted"] = userQuests.Count;
            stats["CompletedQuests"] = userQuests.Count(uq => uq.IsCompleted);
            stats["ActiveQuests"] = userQuests.Count(uq => !uq.IsCompleted);
            stats["ClaimedRewards"] = userQuests.Count(uq => uq.DateClaimed != null);
            stats["TotalTokensEarned"] = userQuests.Where(uq => uq.DateClaimed != null).Sum(uq => uq.Quest.TokenReward);
            stats["TotalXpEarned"] = userQuests.Where(uq => uq.DateClaimed != null).Sum(uq => uq.Quest.XpReward);

            var questsByType = userQuests
                .Where(uq => uq.IsCompleted)
                .GroupBy(uq => uq.Quest.QuestType)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            stats["QuestsByType"] = questsByType;

            return stats;
        }

        // Admin Functions
        public async Task<List<Quest>> GetAllQuestsAsync()
        {
            return await _context.Quests.ToListAsync();
        }

        public async Task<Quest> CreateQuestAsync(Quest quest)
        {
            quest.CreatedAt = DateTime.UtcNow;
            _context.Quests.Add(quest);
            await _context.SaveChangesAsync();
            return quest;
        }

        public async Task<bool> UpdateQuestAsync(Quest quest)
        {
            try
            {
                _context.Quests.Update(quest);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating quest {quest.QuestId}");
                return false;
            }
        }

        public async Task<bool> DeleteQuestAsync(int questId)
        {
            try
            {
                var quest = await _context.Quests.FindAsync(questId);
                if (quest != null)
                {
                    _context.Quests.Remove(quest);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting quest {questId}");
                return false;
            }
        }

        public async Task<bool> ToggleQuestActiveStatusAsync(int questId)
        {
            try
            {
                var quest = await _context.Quests.FindAsync(questId);
                if (quest != null)
                {
                    quest.IsActive = !quest.IsActive;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling quest status {questId}");
                return false;
            }
        }

        public async Task<bool> ValidateQuestCompletionAsync(int userId, int questId, string actionType, object? actionData = null)
        {
            try
            {
                var userQuest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

                if (userQuest == null || userQuest.IsCompleted)
                    return false;

                var quest = userQuest.Quest;
                bool isValid = false;

                // Validate based on quest type and action
                switch (actionType.ToLower())
                {
                    case "item_listed":
                        isValid = ValidateItemListingAction(quest, actionData);
                        break;
                    case "post_created":
                        isValid = ValidatePostCreationAction(quest, actionData);
                        break;
                    case "comment_created":
                        isValid = ValidateCommentAction(quest, actionData);
                        break;
                    case "check_in":
                        isValid = ValidateCheckInAction(quest);
                        break;
                    default:
                        isValid = true; // Default validation for other actions
                        break;
                }

                if (isValid)
                {
                    await UpdateQuestProgressAsync(userId, questId, 1, actionType);
                    _logger.LogInformation($"Quest validation passed: User {userId}, Quest {questId}, Action {actionType}");
                    return true;
                }
                else
                {
                    _logger.LogInformation($"Quest validation failed: User {userId}, Quest {questId}, Action {actionType}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating quest completion for user {userId}, quest {questId}");
                return false;
            }
        }

        // CORRECTED: Use centralized statistics service
        public async Task<bool> CompleteQuestWithAchievementCheckAsync(int userId, int questId)
        {
            try
            {
                var userQuest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId &&
                                              uq.QuestId == questId &&
                                              !uq.IsCompleted);

                if (userQuest == null)
                    return false;

                // Mark quest as completed
                userQuest.IsCompleted = true;
                userQuest.CompletedAt = DateTime.UtcNow;
                userQuest.Status = QuestStatus.Completed;

                // Use centralized statistics service
                var success = await _userStatisticsService.AwardTokensAndXpAsync(
                    userId,
                    userQuest.Quest.TokenReward,
                    userQuest.Quest.XpReward,
                    $"Quest completion: {userQuest.Quest.QuestTitle}");

                if (success)
                {
                    await _context.SaveChangesAsync();

                    // Trigger comprehensive achievement checks
                    await TriggerQuestCompletionAchievementsAsync(userId, userQuest.Quest);

                    _logger.LogInformation($"Quest completed with achievement check: User {userId}, Quest {questId}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing quest with achievement check for user {userId}, quest {questId}");
                return false;
            }
        }

        // Helper methods
        private List<QuestTemplate> SelectDiverseQuestTemplates(List<QuestTemplate> templates, int count)
        {
            var categorizedTemplates = templates.GroupBy(q => q.Category).ToList();
            var selectedTemplates = new List<QuestTemplate>();

            foreach (var categoryGroup in categorizedTemplates.OrderBy(x => _random.Next()))
            {
                if (selectedTemplates.Count >= count) break;

                var questsInCategory = categoryGroup.ToList();
                var randomQuest = questsInCategory[_random.Next(questsInCategory.Count)];
                selectedTemplates.Add(randomQuest);
            }

            while (selectedTemplates.Count < count && selectedTemplates.Count < templates.Count)
            {
                var remainingTemplates = templates.Except(selectedTemplates).ToList();
                if (!remainingTemplates.Any()) break;

                var randomQuest = remainingTemplates[_random.Next(remainingTemplates.Count)];
                selectedTemplates.Add(randomQuest);
            }

            return selectedTemplates;
        }

        private async Task AssignQuestsToAllUsersAsync(List<Quest> quests)
        {
            var allUsers = await _context.Users.ToListAsync();

            foreach (var quest in quests)
            {
                foreach (var user in allUsers)
                {
                    var existingActiveQuests = await _context.UserQuests
                        .Include(uq => uq.Quest)
                        .Where(uq => uq.UserId == user.Id &&
                                   !uq.IsCompleted &&
                                   uq.Quest.IsActive &&
                                   (uq.Quest.EndDate == null || uq.Quest.EndDate > DateTime.UtcNow))
                        .ToListAsync();

                    var dailyCount = existingActiveQuests.Count(uq => uq.Quest.QuestType == QuestType.Daily);
                    var weeklyCount = existingActiveQuests.Count(uq => uq.Quest.QuestType == QuestType.Weekly);
                    var specialCount = existingActiveQuests.Count(uq => uq.Quest.QuestType == QuestType.Special);

                    bool shouldAssign = quest.QuestType switch
                    {
                        QuestType.Daily => dailyCount < 3,
                        QuestType.Weekly => weeklyCount < 2,
                        QuestType.Special => specialCount < 1,
                        _ => false
                    };

                    if (!shouldAssign)
                    {
                        continue;
                    }

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
                            StartedAt = DateTime.UtcNow,
                            Status = QuestStatus.Active
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Assigned quests with limits: 3 daily, 2 weekly, 1 special per user");
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        // Find and replace the GetQuestStatusMessage method in QuestService.cs
        private string GetQuestStatusMessage(UserQuest userQuest)
        {
            if (userQuest.IsCompleted)
            {
                if (userQuest.DateClaimed != null)
                {
                    return "Completed & Rewards Claimed";
                }
                else
                {
                    return "Completed - Ready to Claim";
                }
            }

            if (userQuest.CurrentProgress == 0)
            {
                return "Not Started";
            }

            var progress = userQuest.Quest.TargetValue > 0
                ? (userQuest.CurrentProgress * 100 / userQuest.Quest.TargetValue)
                : 0;
            return $"In Progress ({progress}%)";
        }

        private static bool ValidateItemListingAction(Quest quest, object? actionData)
        {
            var objective = quest.QuestObjective?.ToLower() ?? "";
            var title = quest.QuestTitle?.ToLower() ?? "";

            if (objective.Contains("clothing") || title.Contains("clothing"))
            {
                return true;
            }

            if (objective.Contains("book") || title.Contains("book"))
            {
                return true;
            }

            if (objective.Contains("kitchen") || title.Contains("kitchen"))
            {
                return true;
            }

            if (objective.Contains("list") || objective.Contains("item") || objective.Contains("declutter"))
            {
                return true;
            }

            return false;
        }

        private static bool ValidatePostCreationAction(Quest quest, object? actionData)
        {
            var objective = quest.QuestObjective?.ToLower() ?? "";
            var title = quest.QuestTitle?.ToLower() ?? "";

            if (objective.Contains("tip") || title.Contains("tip"))
            {
                return true;
            }

            if (objective.Contains("achievement") || title.Contains("achievement"))
            {
                return true;
            }

            if (objective.Contains("post") || objective.Contains("share") || objective.Contains("community"))
            {
                return true;
            }

            return false;
        }

        private static bool ValidateCommentAction(Quest quest, object? actionData)
        {
            var objective = quest.QuestObjective?.ToLower() ?? "";

            if (objective.Contains("helpful") || objective.Contains("support"))
            {
                return true;
            }

            if (objective.Contains("encouraging") || objective.Contains("positive"))
            {
                return true;
            }

            if (objective.Contains("comment") || objective.Contains("help"))
            {
                return true;
            }

            return false;
        }

        private static bool ValidateCheckInAction(Quest quest)
        {
            var objective = quest.QuestObjective?.ToLower() ?? "";
            var title = quest.QuestTitle?.ToLower() ?? "";

            if (objective.Contains("early") || title.Contains("early"))
            {
                var currentHour = DateTime.UtcNow.Hour;
                return currentHour < 6;
            }

            if (objective.Contains("late") || title.Contains("late"))
            {
                var currentHour = DateTime.UtcNow.Hour;
                return currentHour > 23;
            }

            if (objective.Contains("check") || title.Contains("daily"))
            {
                return true;
            }

            return false;
        }

        // Add this method to QuestService.cs
        private async Task TriggerQuestCompletionAchievementsAsync(int userId, Quest quest)
        {
            try
            {
                // Quest completion achievements
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_completed", 1);
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "daily_quest_completed",
                    quest.QuestType == QuestType.Daily ? 1 : 0);
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "weekly_quest_completed",
                    quest.QuestType == QuestType.Weekly ? 1 : 0);
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "special_quest_completed",
                    quest.QuestType == QuestType.Special ? 1 : 0);

                // Category-specific achievements based on quest objective
                var objective = quest.QuestObjective?.ToLower() ?? "";
                if (objective.Contains("clothing") || objective.Contains("wardrobe"))
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "clothing_category_progress", 1);
                if (objective.Contains("book") || objective.Contains("reading"))
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "books_category_progress", 1);
                if (objective.Contains("kitchen") || objective.Contains("cooking"))
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, "kitchen_category_progress", 1);

                // Difficulty-based achievements
                switch (quest.Difficulty)
                {
                    case QuestDifficulty.Easy:
                        await _achievementService.CheckAndUnlockAchievementsAsync(userId, "easy_quest_completed", 1);
                        break;
                    case QuestDifficulty.Medium:
                        await _achievementService.CheckAndUnlockAchievementsAsync(userId, "medium_quest_completed", 1);
                        break;
                    case QuestDifficulty.Hard:
                        await _achievementService.CheckAndUnlockAchievementsAsync(userId, "hard_quest_completed", 1);
                        break;
                }

                _logger.LogInformation($"Triggered quest completion achievements for user {userId}, quest: {quest.QuestTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error triggering quest completion achievements for user {userId}");
            }
        }

        private List<string> GetCategoryAchievementsFromQuest(Quest quest)
        {
            var achievements = new List<string>();
            var objective = quest.QuestObjective?.ToLower() ?? "";
            var title = quest.QuestTitle?.ToLower() ?? "";
            var description = quest.QuestDescription?.ToLower() ?? "";

            var content = $"{objective} {title} {description}";

            var categoryMappings = new Dictionary<string, string>
            {
                { "books", "books_stationery" },
                { "stationery", "books_stationery" },
                { "clothing", "clothing_accessories" },
                { "accessories", "clothing_accessories" },
                { "electronics", "electronics_gadgets" },
                { "gadgets", "electronics_gadgets" },
                { "toys", "toys_games" },
                { "games", "toys_games" },
                { "home", "home_kitchen" },
                { "kitchen", "home_kitchen" },
                { "furniture", "furniture" },
                { "appliances", "appliances" },
                { "health", "health_beauty" },
                { "beauty", "health_beauty" },
                { "crafts", "crafts_diy" },
                { "diy", "crafts_diy" },
                { "school", "school_office" },
                { "office", "school_office" },
                { "sentimental", "sentimental" },
                { "memory", "sentimental" }
            };

            foreach (var mapping in categoryMappings)
            {
                if (content.Contains(mapping.Key))
                {
                    achievements.Add($"items_listed_{mapping.Value}");
                    achievements.Add($"category_progress_{mapping.Value}");
                }
            }

            return achievements;
        }

        private List<string> GetMethodologyAchievementsFromQuest(Quest quest)
        {
            var achievements = new List<string>();
            var content = $"{quest.QuestObjective} {quest.QuestTitle} {quest.QuestDescription}".ToLower();

            if (content.Contains("joy") || content.Contains("spark"))
                achievements.Add("joy_mentions");

            if (content.Contains("grateful") || content.Contains("thankful"))
                achievements.Add("gratitude_expressions");

            if (content.Contains("community") || content.Contains("share") || content.Contains("post"))
                achievements.Add("community_engagement_total");

            if (content.Contains("mindful") || content.Contains("thoughtful"))
                achievements.Add("mindful_pacing");

            if (content.Contains("transformation") || content.Contains("before") || content.Contains("after"))
                achievements.Add("transformation_posts");

            return achievements;
        }

        private async Task CheckCategoryMasteryAchievementsAsync(int userId, Quest completedQuest)
        {
            try
            {
                var categories = new[]
                {
                    "books_stationery", "clothing_accessories", "electronics_gadgets",
                    "toys_games", "home_kitchen", "furniture", "appliances",
                    "health_beauty", "crafts_diy", "school_office", "sentimental"
                };

                foreach (var category in categories)
                {
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, $"category_mastery_{category}", 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking category mastery achievements for user {userId}");
            }
        }

        private async Task CheckProgressiveAchievementsAsync(int userId)
        {
            try
            {
                var progressiveTypes = new[]
                {
                    "total_items_listed",
                    "categories_mastered",
                    "community_engagement_total",
                    "daily_activity_streak"
                };

                foreach (var type in progressiveTypes)
                {
                    await _achievementService.CheckAndUnlockAchievementsAsync(userId, type, 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking progressive achievements for user {userId}");
            }
        }
    }
}