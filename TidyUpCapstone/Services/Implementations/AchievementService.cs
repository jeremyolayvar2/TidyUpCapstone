using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Services.Implementations
{
    public class AchievementService : IAchievementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(ApplicationDbContext context, ILogger<AchievementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AchievementDto>> GetUserAchievementsAsync(int userId)
        {
            var userAchievements = await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .Select(ua => new AchievementDto
                {
                    AchievementId = ua.AchievementId,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    Category = ua.Achievement.Category,
                    CriteriaType = ua.Achievement.CriteriaType,
                    CriteriaValue = ua.Achievement.CriteriaValue,
                    TokenReward = ua.Achievement.TokenReward,
                    XpReward = ua.Achievement.XpReward,
                    BadgeImageUrl = ua.Achievement.BadgeImageUrl,
                    Rarity = ua.Achievement.Rarity,
                    IsSecret = ua.Achievement.IsSecret,
                    IsEarned = true,
                    EarnedDate = ua.EarnedDate,
                    Progress = ua.Progress,
                    ProgressPercentage = 100
                })
                .OrderByDescending(a => a.EarnedDate)
                .ToListAsync();

            return userAchievements;
        }

        public async Task<List<AchievementDto>> GetAllAchievementsAsync(int userId)
        {
            var userAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var userAchievements = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .ToDictionaryAsync(ua => ua.AchievementId, ua => ua);

            var allAchievements = await _context.Achievements
                .Where(a => a.IsActive)
                .Select(a => new AchievementDto
                {
                    AchievementId = a.AchievementId,
                    Name = a.Name,
                    Description = a.Description,
                    Category = a.Category,
                    CriteriaType = a.CriteriaType,
                    CriteriaValue = a.CriteriaValue,
                    TokenReward = a.TokenReward,
                    XpReward = a.XpReward,
                    BadgeImageUrl = a.BadgeImageUrl,
                    Rarity = a.Rarity,
                    IsSecret = a.IsSecret,
                    // FIXED: Check both existence AND IsUnlocked status
                    IsEarned = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked,
                    EarnedDate = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].EarnedDate : null,
                    Progress = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].Progress : 0,
                    // FIXED: Only show 100% if actually unlocked
                    ProgressPercentage = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked ? 100 : 0
                })
                        .ToListAsync();

            // Hide secret achievements that haven't been earned
            return allAchievements.Where(a => !a.IsSecret || a.IsEarned).ToList();
        }

        public async Task<bool> UpdateAchievementProgressAsync(int userId, string criteriaType, int progressValue)
        {
            try
            {
                var relevantAchievements = await _context.Achievements
                    .Where(a => a.IsActive && a.CriteriaType == criteriaType)
                    .ToListAsync();

                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId && relevantAchievements.Select(a => a.AchievementId).Contains(ua.AchievementId))
                    .ToDictionaryAsync(ua => ua.AchievementId, ua => ua);

                bool hasUpdates = false;

                foreach (var achievement in relevantAchievements)
                {
                    if (userAchievements.ContainsKey(achievement.AchievementId))
                    {
                        // Already earned, skip
                        continue;
                    }

                    // Calculate current progress
                    int currentProgress = await GetCurrentProgressAsync(userId, achievement.CriteriaType);

                    // Check if achievement should be unlocked
                    if (currentProgress >= achievement.CriteriaValue)
                    {
                        await UnlockAchievementAsync(userId, achievement.AchievementId, currentProgress);
                        hasUpdates = true;
                    }
                }

                return hasUpdates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating achievement progress for user {userId}, criteria {criteriaType}");
                return false;
            }
        }

        public async Task<List<AchievementDto>> CheckAndUnlockAchievementsAsync(int userId, string actionType, int value = 1)
        {
            var unlockedAchievements = new List<AchievementDto>();

            try
            {
                // Map action types to criteria types
                var criteriaTypes = MapActionToCriteriaTypes(actionType);

                foreach (var criteriaType in criteriaTypes)
                {
                    var relevantAchievements = await _context.Achievements
                        .Where(a => a.IsActive && a.CriteriaType == criteriaType)
                        .ToListAsync();

                    var alreadyEarned = await _context.UserAchievements
                        .Where(ua => ua.UserId == userId && relevantAchievements.Select(a => a.AchievementId).Contains(ua.AchievementId))
                        .Select(ua => ua.AchievementId)
                        .ToListAsync();

                    foreach (var achievement in relevantAchievements.Where(a => !alreadyEarned.Contains(a.AchievementId)))
                    {
                        int currentProgress = await GetCurrentProgressAsync(userId, achievement.CriteriaType);

                        if (currentProgress >= achievement.CriteriaValue)
                        {
                            var unlockedAchievement = await UnlockAchievementAsync(userId, achievement.AchievementId, currentProgress);
                            if (unlockedAchievement != null)
                            {
                                unlockedAchievements.Add(unlockedAchievement);
                            }
                        }
                    }
                }

                return unlockedAchievements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking achievements for user {userId}, action {actionType}");
                return unlockedAchievements;
            }
        }

        private async Task<AchievementDto?> UnlockAchievementAsync(int userId, int achievementId, int progress)
        {
            try
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement == null) return null;

                var userAchievement = new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievementId,
                    EarnedDate = DateTime.UtcNow,
                    Progress = progress,
                    IsNotified = false
                };

                _context.UserAchievements.Add(userAchievement);

                // Award tokens and XP
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TokenBalance += achievement.TokenReward;

                    // Update user XP
                    if (achievement.XpReward.HasValue)
                    {
                        await UpdateUserXpAsync(userId, achievement.XpReward.Value);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {userId} unlocked achievement: {achievement.Name}");

                return new AchievementDto
                {
                    AchievementId = achievement.AchievementId,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    Category = achievement.Category,
                    CriteriaType = achievement.CriteriaType,
                    CriteriaValue = achievement.CriteriaValue,
                    TokenReward = achievement.TokenReward,
                    XpReward = achievement.XpReward,
                    BadgeImageUrl = achievement.BadgeImageUrl,
                    Rarity = achievement.Rarity,
                    IsSecret = achievement.IsSecret,
                    IsEarned = true,
                    EarnedDate = userAchievement.EarnedDate,
                    Progress = progress,
                    ProgressPercentage = 100
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unlocking achievement {achievementId} for user {userId}");
                return null;
            }
        }

        public async Task<List<AchievementDto>> GetRecentAchievementsAsync(int userId, int limit = 5)
        {
            return await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.EarnedDate)
                .Take(limit)
                .Select(ua => new AchievementDto
                {
                    AchievementId = ua.AchievementId,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    Category = ua.Achievement.Category,
                    CriteriaType = ua.Achievement.CriteriaType,
                    CriteriaValue = ua.Achievement.CriteriaValue,
                    TokenReward = ua.Achievement.TokenReward,
                    XpReward = ua.Achievement.XpReward,
                    BadgeImageUrl = ua.Achievement.BadgeImageUrl,
                    Rarity = ua.Achievement.Rarity,
                    IsSecret = ua.Achievement.IsSecret,
                    IsEarned = true,
                    EarnedDate = ua.EarnedDate,
                    Progress = ua.Progress,
                    ProgressPercentage = 100
                })
                .ToListAsync();
        }

        public async Task SeedAchievementsAsync()
        {
            if (await AreAchievementsSeededAsync())
                return;

            var achievements = new List<Achievement>
            {
                // Decluttering Achievements
                new Achievement
                {
                    Name = "First Steps",
                    Description = "List your first item for decluttering",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_listed",
                    CriteriaValue = 1,
                    TokenReward = 5.00m,
                    XpReward = 10,
                    Rarity = AchievementRarity.Bronze,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Getting Started",
                    Description = "List 10 items for decluttering",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_listed",
                    CriteriaValue = 10,
                    TokenReward = 15.00m,
                    XpReward = 25,
                    Rarity = AchievementRarity.Bronze,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Decluttering Enthusiast",
                    Description = "List 50 items for decluttering",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_listed",
                    CriteriaValue = 50,
                    TokenReward = 50.00m,
                    XpReward = 75,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Decluttering Master",
                    Description = "List 200 items for decluttering",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_listed",
                    CriteriaValue = 200,
                    TokenReward = 150.00m,
                    XpReward = 200,
                    Rarity = AchievementRarity.Gold,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Minimalist Legend",
                    Description = "List 500 items for decluttering",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_listed",
                    CriteriaValue = 500,
                    TokenReward = 300.00m,
                    XpReward = 500,
                    Rarity = AchievementRarity.Platinum,
                    IsSecret = false
                },

                // Quest Achievements
                new Achievement
                {
                    Name = "Quest Beginner",
                    Description = "Complete your first quest",
                    Category = AchievementCategory.Special,
                    CriteriaType = "total_quests_completed",
                    CriteriaValue = 1,
                    TokenReward = 10.00m,
                    XpReward = 15,
                    Rarity = AchievementRarity.Bronze,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Daily Dedication",
                    Description = "Complete 10 daily quests",
                    Category = AchievementCategory.Special,
                    CriteriaType = "daily_quest_completed",
                    CriteriaValue = 10,
                    TokenReward = 25.00m,
                    XpReward = 50,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Weekly Warrior",
                    Description = "Complete 5 weekly quests",
                    Category = AchievementCategory.Special,
                    CriteriaType = "weekly_quest_completed",
                    CriteriaValue = 5,
                    TokenReward = 75.00m,
                    XpReward = 100,
                    Rarity = AchievementRarity.Gold,
                    IsSecret = false
                },

                // Community Achievements
                new Achievement
                {
                    Name = "Social Butterfly",
                    Description = "Make 25 comments in the community",
                    Category = AchievementCategory.Community,
                    CriteriaType = "comments_made",
                    CriteriaValue = 25,
                    TokenReward = 20.00m,
                    XpReward = 40,
                    Rarity = AchievementRarity.Bronze,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Community Helper",
                    Description = "Create 10 helpful posts",
                    Category = AchievementCategory.Community,
                    CriteriaType = "posts_created",
                    CriteriaValue = 10,
                    TokenReward = 30.00m,
                    XpReward = 60,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = false
                },

                // Trading Achievements
                new Achievement
                {
                    Name = "First Sale",
                    Description = "Complete your first successful transaction as seller",
                    Category = AchievementCategory.Trading,
                    CriteriaType = "successful_sales",
                    CriteriaValue = 1,
                    TokenReward = 15.00m,
                    XpReward = 25,
                    Rarity = AchievementRarity.Bronze,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Merchant",
                    Description = "Complete 10 successful transactions as seller",
                    Category = AchievementCategory.Trading,
                    CriteriaType = "successful_sales",
                    CriteriaValue = 10,
                    TokenReward = 50.00m,
                    XpReward = 100,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Trading Tycoon",
                    Description = "Complete 50 successful transactions as seller",
                    Category = AchievementCategory.Trading,
                    CriteriaType = "successful_sales",
                    CriteriaValue = 50,
                    TokenReward = 200.00m,
                    XpReward = 300,
                    Rarity = AchievementRarity.Gold,
                    IsSecret = false
                },

                // Streak Achievements
                new Achievement
                {
                    Name = "Consistent Visitor",
                    Description = "Maintain a 7-day check-in streak",
                    Category = AchievementCategory.Special,
                    CriteriaType = "check_in_streak",
                    CriteriaValue = 7,
                    TokenReward = 25.00m,
                    XpReward = 50,
                    Rarity = AchievementRarity.Bronze,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Dedicated User",
                    Description = "Maintain a 30-day check-in streak",
                    Category = AchievementCategory.Special,
                    CriteriaType = "check_in_streak",
                    CriteriaValue = 30,
                    TokenReward = 100.00m,
                    XpReward = 150,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = false
                },
                new Achievement
                {
                    Name = "Loyalty Champion",
                    Description = "Maintain a 100-day check-in streak",
                    Category = AchievementCategory.Special,
                    CriteriaType = "check_in_streak",
                    CriteriaValue = 100,
                    TokenReward = 500.00m,
                    XpReward = 750,
                    Rarity = AchievementRarity.Platinum,
                    IsSecret = false
                },

                // Secret Achievements
                new Achievement
                {
                    Name = "Early Bird",
                    Description = "Check in before 6 AM",
                    Category = AchievementCategory.Special,
                    CriteriaType = "early_check_in",
                    CriteriaValue = 1,
                    TokenReward = 20.00m,
                    XpReward = 30,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = true
                },
                new Achievement
                {
                    Name = "Night Owl",
                    Description = "Check in after 11 PM",
                    Category = AchievementCategory.Special,
                    CriteriaType = "late_check_in",
                    CriteriaValue = 1,
                    TokenReward = 20.00m,
                    XpReward = 30,
                    Rarity = AchievementRarity.Silver,
                    IsSecret = true
                }
            };

            _context.Achievements.AddRange(achievements);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {achievements.Count} achievements");
        }

        public async Task<bool> AreAchievementsSeededAsync()
        {
            return await _context.Achievements.AnyAsync();
        }

        public async Task<AchievementDto?> GetAchievementByIdAsync(int achievementId, int userId)
        {
            var achievement = await _context.Achievements.FindAsync(achievementId);
            if (achievement == null) return null;

            var userAchievement = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            var isEarned = userAchievement != null;
            var progress = isEarned ? userAchievement.Progress : await GetCurrentProgressAsync(userId, achievement.CriteriaType);

            return new AchievementDto
            {
                AchievementId = achievement.AchievementId,
                Name = achievement.Name,
                Description = achievement.Description,
                Category = achievement.Category,
                CriteriaType = achievement.CriteriaType,
                CriteriaValue = achievement.CriteriaValue,
                TokenReward = achievement.TokenReward,
                XpReward = achievement.XpReward,
                BadgeImageUrl = achievement.BadgeImageUrl,
                Rarity = achievement.Rarity,
                IsSecret = achievement.IsSecret,
                IsEarned = isEarned,
                EarnedDate = userAchievement?.EarnedDate,
                Progress = progress,
                ProgressPercentage = achievement.CriteriaValue > 0 ? (progress * 100 / achievement.CriteriaValue) : 0
            };
        }

        public async Task<List<AchievementDto>> GetAchievementsByCategoryAsync(AchievementCategory category, int userId)
        {
            var userAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var userAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToDictionaryAsync(ua => ua.AchievementId, ua => ua);

            return await _context.Achievements
                .Where(a => a.IsActive && a.Category == category)
                .Select(a => new AchievementDto
                {
                    AchievementId = a.AchievementId,
                    Name = a.Name,
                    Description = a.Description,
                    Category = a.Category,
                    CriteriaType = a.CriteriaType,
                    CriteriaValue = a.CriteriaValue,
                    TokenReward = a.TokenReward,
                    XpReward = a.XpReward,
                    BadgeImageUrl = a.BadgeImageUrl,
                    Rarity = a.Rarity,
                    IsSecret = a.IsSecret,
                    IsEarned = userAchievementIds.Contains(a.AchievementId),
                    EarnedDate = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].EarnedDate : null,
                    Progress = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].Progress : 0,
                    ProgressPercentage = userAchievements.ContainsKey(a.AchievementId) ? 100 : 0
                })
                .ToListAsync();
        }

        public async Task<List<AchievementDto>> GetSecretAchievementsAsync(int userId)
        {
            return await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId && ua.Achievement.IsSecret)
                .Select(ua => new AchievementDto
                {
                    AchievementId = ua.AchievementId,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    Category = ua.Achievement.Category,
                    CriteriaType = ua.Achievement.CriteriaType,
                    CriteriaValue = ua.Achievement.CriteriaValue,
                    TokenReward = ua.Achievement.TokenReward,
                    XpReward = ua.Achievement.XpReward,
                    BadgeImageUrl = ua.Achievement.BadgeImageUrl,
                    Rarity = ua.Achievement.Rarity,
                    IsSecret = ua.Achievement.IsSecret,
                    IsEarned = true,
                    EarnedDate = ua.EarnedDate,
                    Progress = ua.Progress,
                    ProgressPercentage = 100
                })
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetAchievementStatsAsync(int userId)
        {
            var stats = new Dictionary<string, object>();

            var userAchievements = await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId && ua.IsUnlocked == true) // Add IsUnlocked check
                .ToListAsync();


            var totalAchievements = await _context.Achievements.CountAsync(a => a.IsActive);

            stats["TotalEarned"] = userAchievements.Count;
            stats["TotalAvailable"] = totalAchievements;
            stats["CompletionPercentage"] = totalAchievements > 0 ? (userAchievements.Count * 100 / totalAchievements) : 0;
            stats["TotalTokensFromAchievements"] = userAchievements.Sum(ua => ua.Achievement.TokenReward);
            stats["TotalXpFromAchievements"] = userAchievements.Sum(ua => ua.Achievement.XpReward ?? 0);

            var achievementsByRarity = userAchievements
                .GroupBy(ua => ua.Achievement.Rarity)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            stats["AchievementsByRarity"] = achievementsByRarity;

            var achievementsByCategory = userAchievements
                .GroupBy(ua => ua.Achievement.Category)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            stats["AchievementsByCategory"] = achievementsByCategory;

            return stats;
        }

        public async Task<int> GetCompletionPercentageAsync(int userId)
        {
            var earnedCount = await _context.UserAchievements.CountAsync(ua => ua.UserId == userId);
            var totalCount = await _context.Achievements.CountAsync(a => a.IsActive);

            return totalCount > 0 ? (earnedCount * 100 / totalCount) : 0;
        }

        public async Task<bool> NotifyUserOfNewAchievementAsync(int userId, int achievementId)
        {
            try
            {
                var userAchievement = await _context.UserAchievements
                    .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

                if (userAchievement != null)
                {
                    userAchievement.IsNotified = true;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking achievement {achievementId} as notified for user {userId}");
                return false;
            }
        }

        // Admin Functions
        public async Task<Achievement> CreateAchievementAsync(Achievement achievement)
        {
            achievement.CreatedAt = DateTime.UtcNow;
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();
            return achievement;
        }

        public async Task<bool> UpdateAchievementAsync(Achievement achievement)
        {
            try
            {
                _context.Achievements.Update(achievement);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating achievement {achievement.AchievementId}");
                return false;
            }
        }

        public async Task<bool> DeleteAchievementAsync(int achievementId)
        {
            try
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement != null)
                {
                    _context.Achievements.Remove(achievement);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting achievement {achievementId}");
                return false;
            }
        }

        public async Task<bool> ToggleAchievementActiveStatusAsync(int achievementId)
        {
            try
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement != null)
                {
                    achievement.IsActive = !achievement.IsActive;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling achievement status {achievementId}");
                return false;
            }
        }

        // Private helper methods
        private async Task<int> GetCurrentProgressAsync(int userId, string criteriaType)
        {
            return criteriaType switch
            {
                "items_listed" => await _context.Items.CountAsync(i => i.UserId == userId),
                "total_quests_completed" => await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted),
                "daily_quest_completed" => await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .CountAsync(uq => uq.UserId == userId && uq.IsCompleted && uq.Quest.QuestType == QuestType.Daily),
                "weekly_quest_completed" => await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .CountAsync(uq => uq.UserId == userId && uq.IsCompleted && uq.Quest.QuestType == QuestType.Weekly),
                "comments_made" => await _context.Comments.CountAsync(c => c.UserId == userId),
                "posts_created" => await _context.Posts.CountAsync(p => p.AuthorId == userId),
                "successful_sales" => await _context.Transactions
                    .CountAsync(t => t.SellerId == userId && t.TransactionStatus == TransactionStatus.Completed),
                "check_in_streak" => await GetCurrentCheckInStreakAsync(userId),
                "rewards_claimed" => await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.DateClaimed != null),
                "tokens_earned" => (int)await _context.Users.Where(u => u.Id == userId).Select(u => u.TokenBalance).FirstOrDefaultAsync(),
                "level_up" => await _context.UserLevels.Where(ul => ul.UserId == userId).Select(ul => ul.TotalLevelUps).FirstOrDefaultAsync(),
                "early_check_in" => await CountEarlyCheckInsAsync(userId),
                "late_check_in" => await CountLateCheckInsAsync(userId),
                _ => 0
            };
        }

        private async Task<int> GetCurrentCheckInStreakAsync(int userId)
        {
            var checkInStreak = await _context.UserStreaks
                .Include(us => us.StreakType)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakType.Name == "Daily Check-in");

            return checkInStreak?.CurrentStreak ?? 0;
        }

        private async Task<int> CountEarlyCheckInsAsync(int userId)
        {
            // This would require a check-in log table to track exact times
            // For now, return 0 as placeholder
            // You could implement this by creating a CheckInLog table and tracking check-in times
            return 0;
        }

        private async Task<int> CountLateCheckInsAsync(int userId)
        {
            // This would require a check-in log table to track exact times
            // For now, return 0 as placeholder  
            // You could implement this by creating a CheckInLog table and tracking check-in times
            return 0;
        }

        private static List<string> MapActionToCriteriaTypes(string actionType)
        {
            return actionType switch
            {
                "item_listed" => new List<string> { "items_listed" },
                "quest_completed" => new List<string> { "total_quests_completed" },
                "daily_quest_completed" => new List<string> { "daily_quest_completed", "total_quests_completed" },
                "weekly_quest_completed" => new List<string> { "weekly_quest_completed", "total_quests_completed" },
                "comment_created" => new List<string> { "comments_made" },
                "post_created" => new List<string> { "posts_created" },
                "transaction_completed" => new List<string> { "successful_sales" },
                "check_in" => new List<string> { "check_in_streak" },
                "reward_claimed" => new List<string> { "rewards_claimed" },
                "tokens_earned" => new List<string> { "tokens_earned" },
                "level_up" => new List<string> { "level_up" },
                "early_check_in" => new List<string> { "early_check_in" },
                "late_check_in" => new List<string> { "late_check_in" },
                _ => new List<string>()
            };
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

                // Check for level up logic here if needed
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

            await _context.SaveChangesAsync();
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

                _logger.LogInformation($"User {userLevel.UserId} leveled up to level {nextLevel.LevelNumber}");
            }
        }
    }
}