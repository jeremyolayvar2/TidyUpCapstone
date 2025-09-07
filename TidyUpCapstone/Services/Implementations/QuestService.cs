using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Implementations
{
    public class QuestService : IQuestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAchievementService _achievementService;
        private readonly ILogger<QuestService> _logger;

        public QuestService(
            ApplicationDbContext context,
            IAchievementService achievementService,
            ILogger<QuestService> logger)
        {
            _context = context;
            _achievementService = achievementService;
            _logger = logger;
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

                    // ✅ Use quest start/end dates, not userquest.createdAt
                    StartDate = uq.Quest.StartDate,
                    EndDate = uq.Quest.EndDate,

                    // ✅ UserQuest-specific info
                    IsCompleted = uq.IsCompleted,
                    CurrentProgress = uq.CurrentProgress,
                    IsClaimed = uq.DateClaimed != null,
                    CompletedAt = uq.CompletedAt,

                    // ✅ Derived
                    ProgressPercentage = uq.Quest.TargetValue > 0
                        ? (uq.CurrentProgress * 100 / uq.Quest.TargetValue)
                        : 0,
                    IsAvailable = true,
                    StatusMessage = GetQuestStatusMessage(uq)
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
                // Check if user already has this quest
                var existingUserQuest = await _context.UserQuests
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

                if (existingUserQuest != null)
                    return false; // Already started

                // Check if quest exists and is active
                var quest = await _context.Quests.FindAsync(questId);
                if (quest == null || !quest.IsActive)
                    return false;

                // Create new user quest
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

                // Update progress
                userQuest.CurrentProgress += progressIncrement;

                // Ensure progress doesn't exceed target
                if (userQuest.CurrentProgress > userQuest.Quest.TargetValue)
                    userQuest.CurrentProgress = userQuest.Quest.TargetValue;

                // Create progress tracking record
                var progressRecord = new QuestProgress
                {
                    UserQuestId = userQuest.UserQuestId,
                    ProgressValue = progressIncrement,
                    GoalValue = userQuest.Quest.TargetValue,
                    ActionType = actionType,
                    ActionTimestamp = DateTime.UtcNow
                };

                _context.QuestProgresses.Add(progressRecord);

                // Check if quest is completed
                if (userQuest.CurrentProgress >= userQuest.Quest.TargetValue)
                {
                    await CompleteQuestAsync(userId, questId);
                }

                await _context.SaveChangesAsync();

                // Check for achievements related to quest progress
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_progress", progressIncrement);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating quest progress for user {userId}, quest {questId}");
                return false;
            }
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
                userQuest.CurrentProgress = userQuest.Quest.TargetValue; // Ensure progress is at target

                await _context.SaveChangesAsync();

                // Check for achievements related to quest completion
                var questType = userQuest.Quest.QuestType.ToString().ToLower();
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, $"{questType}_quest_completed", 1);
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_quests_completed", 1);

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
                    .Include(uq => uq.User)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId &&
                                            uq.IsCompleted && uq.DateClaimed == null);

                if (userQuest == null)
                    return false;

                // Award tokens
                userQuest.User.TokenBalance += userQuest.Quest.TokenReward;

                // Award XP (this would typically update user level/XP in a separate service)
                await UpdateUserXpAsync(userId, userQuest.Quest.XpReward);

                // Mark as claimed
                userQuest.DateClaimed = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Check for achievements related to reward claiming
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "rewards_claimed", 1);
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "tokens_earned", (int)userQuest.Quest.TokenReward);

                _logger.LogInformation($"User {userId} claimed reward for quest {questId}: {userQuest.Quest.TokenReward} tokens, {userQuest.Quest.XpReward} XP");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error claiming reward for quest {questId} by user {userId}");
                return false;
            }
        }

        // ---------------- DAILY QUESTS ----------------
        public async Task GenerateDailyQuestsAsync()
        {
            try
            {
                // Remove expired daily quests
                var expiredDailyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily &&
                               q.EndDate.HasValue && q.EndDate.Value < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var quest in expiredDailyQuests)
                {
                    quest.IsActive = false;
                }

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var dailyQuestTemplates = new List<Quest>
                {
                    new Quest
                    {
                        QuestTitle = "Daily Declutterer",
                        QuestType = QuestType.Daily,
                        QuestDescription = "List items for decluttering today",
                        QuestObjective = "List 3 items for decluttering",
                        TokenReward = 5.00m,
                        XpReward = 10,
                        Difficulty = QuestDifficulty.Easy,
                        TargetValue = 3,
                        IsActive = true,
                        StartDate = today,
                        EndDate = tomorrow
                    },
                    new Quest
                    {
                        QuestTitle = "Community Helper",
                        QuestType = QuestType.Daily,
                        QuestDescription = "Help other users in the community",
                        QuestObjective = "Make 2 helpful comments on posts",
                        TokenReward = 7.50m,
                        XpReward = 15,
                        Difficulty = QuestDifficulty.Easy,
                        TargetValue = 2,
                        IsActive = true,
                        StartDate = today,
                        EndDate = tomorrow
                    },
                    new Quest
                    {
                        QuestTitle = "Marketplace Explorer",
                        QuestType = QuestType.Daily,
                        QuestDescription = "Browse and engage with marketplace listings",
                        QuestObjective = "View 10 marketplace listings",
                        TokenReward = 3.00m,
                        XpReward = 8,
                        Difficulty = QuestDifficulty.Easy,
                        TargetValue = 10,
                        IsActive = true,
                        StartDate = today,
                        EndDate = tomorrow
                    }
                };

                // Only add quests that don't already exist for today
                foreach (var template in dailyQuestTemplates)
                {
                    var exists = await _context.Quests
                        .AnyAsync(q => q.QuestTitle == template.QuestTitle &&
                                     q.QuestType == QuestType.Daily &&
                                     q.StartDate.HasValue && q.StartDate.Value.Date == today);

                    if (!exists)
                    {
                        _context.Quests.Add(template);
                    }
                }

                await _context.SaveChangesAsync();

                // Assign quests to all users
                var users = await _context.Users.ToListAsync();
                var todayQuestIds = dailyQuestTemplates.Select(q => q.QuestId).ToList();

                foreach (var user in users)
                {
                    foreach (var questId in todayQuestIds)
                    {
                        bool alreadyAssigned = await _context.UserQuests
                            .AnyAsync(uq => uq.UserId == user.Id && uq.QuestId == questId);

                        if (!alreadyAssigned)
                        {
                            _context.UserQuests.Add(new UserQuest
                            {
                                UserId = user.Id,
                                QuestId = questId,
                                CurrentProgress = 0,
                                StartedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Daily quests generated and assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily quests");
            }
        }

        // ---------------- WEEKLY QUESTS ----------------
        public async Task GenerateWeeklyQuestsAsync()
        {
            try
            {
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                var existingWeeklyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Weekly &&
                                q.StartDate >= startOfWeek && q.StartDate < endOfWeek &&
                                q.IsActive)
                    .ToListAsync();

                if (existingWeeklyQuests.Any())
                {
                    _logger.LogInformation("Weekly quests already exist for this week");
                    return;
                }

                var weeklyQuestTemplates = new List<Quest>
        {
            new Quest
            {
                QuestTitle = "Weekly Warrior",
                QuestType = QuestType.Weekly,
                QuestDescription = "Complete multiple transactions this week",
                QuestObjective = "Complete 5 successful transactions",
                TokenReward = 25.00m,
                XpReward = 50,
                Difficulty = QuestDifficulty.Medium,
                TargetValue = 5,
                IsActive = true,
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                CreatedAt = DateTime.UtcNow
            }
            // Add other weekly quests...
        };

                _context.Quests.AddRange(weeklyQuestTemplates);
                await _context.SaveChangesAsync();

                // Get fresh quest IDs after saving
                var savedQuestIds = weeklyQuestTemplates.Select(q => q.QuestId).ToList();

                // Assign to all users
                var allUsers = await _context.Users.ToListAsync();
                foreach (var questId in savedQuestIds)
                {
                    foreach (var user in allUsers)
                    {
                        var existingAssignment = await _context.UserQuests
                            .AnyAsync(uq => uq.UserId == user.Id && uq.QuestId == questId);

                        if (!existingAssignment)
                        {
                            _context.UserQuests.Add(new UserQuest
                            {
                                UserId = user.Id,
                                QuestId = questId,
                                CurrentProgress = 0,
                                IsCompleted = false,
                                StartedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Generated and assigned {weeklyQuestTemplates.Count} weekly quests");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly quests");
            }
        }

        // ---------------- SPECIAL QUESTS ----------------
        public async Task<Quest> CreateSpecialQuestAsync(Quest quest)
        {
            quest.QuestType = QuestType.Special;
            quest.StartDate = DateTime.UtcNow;

            _context.Quests.Add(quest);
            await _context.SaveChangesAsync();

            // Assign to all users
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                _context.UserQuests.Add(new UserQuest
                {
                    UserId = user.Id,
                    QuestId = quest.QuestId,
                    CurrentProgress = 0,
                    StartedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Special quest created and assigned: {quest.QuestTitle}");
            return quest;
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

        // Additional implementation methods...
        public async Task<QuestDto?> GetQuestByIdAsync(int questId, int userId)
        {
            var userQuest = await _context.UserQuests
                .Include(uq => uq.Quest)
                .FirstOrDefaultAsync(uq => uq.QuestId == questId && uq.UserId == userId);

            if (userQuest == null)
            {
                // Check if quest exists but user hasn't started it
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
                ProgressPercentage = userQuest.Quest.TargetValue > 0 ? (userQuest.CurrentProgress * 100 / userQuest.Quest.TargetValue) : 0,
                IsAvailable = !userQuest.IsCompleted,
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

        // Private helper methods
        private static string GetQuestStatusMessage(UserQuest userQuest)
        {
            if (userQuest.IsCompleted)
            {
                return userQuest.DateClaimed != null ? "Reward Claimed" : "Ready to Claim";
            }

            if (userQuest.Quest.EndDate.HasValue && userQuest.Quest.EndDate.Value < DateTime.UtcNow)
            {
                return "Expired";
            }

            var progress = userQuest.Quest.TargetValue > 0 ? (userQuest.CurrentProgress * 100 / userQuest.Quest.TargetValue) : 0;
            return $"In Progress ({progress}%)";
        }

        private async Task UpdateUserXpAsync(int userId, int xpAmount)
        {
            var userLevel = await _context.UserLevels
                .Include(ul => ul.CurrentLevel)
                .FirstOrDefaultAsync(ul => ul.UserId == userId);

            if (userLevel != null)
            {
                userLevel.CurrentXp += xpAmount;
                userLevel.TotalXp += xpAmount;

                // Check for level up logic here
                await CheckForLevelUpAsync(userLevel);
            }
            else
            {
                // Create initial user level if it doesn't exist
                var initialLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == 1);
                if (initialLevel != null)
                {
                    var newUserLevel = new UserLevel
                    {
                        UserId = userId,
                        CurrentLevelId = initialLevel.LevelId,
                        CurrentXp = xpAmount,
                        TotalXp = xpAmount,
                        XpToNextLevel = initialLevel.XpToNext - xpAmount
                    };
                    _context.UserLevels.Add(newUserLevel);
                }
            }
        }

        private async Task CheckForLevelUpAsync(UserLevel userLevel)
        {
            var nextLevel = await _context.Levels
                .Where(l => l.LevelNumber > userLevel.CurrentLevel.LevelNumber)
                .OrderBy(l => l.LevelNumber)
                .FirstOrDefaultAsync();

            if (nextLevel != null && userLevel.CurrentXp >= nextLevel.XpRequired)
            {
                userLevel.CurrentLevelId = nextLevel.LevelId;
                userLevel.LevelUpDate = DateTime.UtcNow;
                userLevel.TotalLevelUps++;

                var nextNextLevel = await _context.Levels
                    .Where(l => l.LevelNumber > nextLevel.LevelNumber)
                    .OrderBy(l => l.LevelNumber)
                    .FirstOrDefaultAsync();

                userLevel.XpToNextLevel = nextNextLevel != null ?
                    nextNextLevel.XpRequired - userLevel.CurrentXp : 0;

                // Award level up achievements
                await _achievementService.CheckAndUnlockAchievementsAsync(userLevel.UserId, "level_up", 1);
            }
        }
    }
}