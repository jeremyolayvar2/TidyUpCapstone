using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Templates;

namespace TidyUpCapstone.Services.Implementations
{
    public class AchievementService : IAchievementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(
            ApplicationDbContext context,
            IUserStatisticsService userStatisticsService,
            ILogger<AchievementService> logger)
        {
            _context = context;
            _userStatisticsService = userStatisticsService;
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
                    IsEarned = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked,
                    EarnedDate = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].EarnedDate : null,
                    Progress = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].Progress : 0,
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
                        .Where(ua => ua.UserId == userId &&
                                    ua.IsUnlocked == true && // Add this condition
                                    relevantAchievements.Select(a => a.AchievementId).Contains(ua.AchievementId))
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

        // CORRECTED: Use centralized statistics service
        private async Task<AchievementDto?> UnlockAchievementAsync(int userId, int achievementId, int progress)
        {
            try
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement == null) return null;

                // Check if user already has this achievement record
                var existingUserAchievement = await _context.UserAchievements
                    .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

                if (existingUserAchievement != null)
                {
                    // Update existing record
                    existingUserAchievement.IsUnlocked = true;
                    existingUserAchievement.EarnedDate = DateTime.UtcNow;
                    existingUserAchievement.Progress = progress;
                    existingUserAchievement.IsNotified = false;
                }
                else
                {
                    // Create new record
                    var userAchievement = new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievementId,
                        EarnedDate = DateTime.UtcNow,
                        Progress = progress,
                        IsUnlocked = true,
                        IsNotified = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.UserAchievements.Add(userAchievement);
                }

                // CORRECTED: Use centralized statistics service
                var success = await _userStatisticsService.AwardTokensAndXpAsync(
                    userId,
                    achievement.TokenReward,
                    achievement.XpReward ?? 0,
                    $"Achievement unlocked: {achievement.Name}");

                if (success)
                {
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"User {userId} unlocked achievement: {achievement.Name} - Awarded {achievement.TokenReward} tokens, {achievement.XpReward} XP");

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
                        EarnedDate = DateTime.UtcNow,
                        Progress = progress,
                        ProgressPercentage = 100
                    };
                }

                return null;
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
            try
            {
                // First, seed any existing basic achievements you have
                var areBasicSeeded = await AreAchievementsSeededAsync();
                if (!areBasicSeeded)
                {
                    // Add your existing basic achievement seeding logic here if any
                    _logger.LogInformation("Seeding basic achievements...");
                    // ... your existing seeding code ...
                }

                // Then, seed KonMari-specific achievements
                var areKonMariSeeded = await AreKonMariAchievementsSeededAsync();
                if (!areKonMariSeeded)
                {
                    await SeedKonMariAchievementsAsync();
                }

                _logger.LogInformation("Achievement seeding completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in complete achievement seeding process");
                throw;
            }
        }

        public async Task SeedKonMariAchievementsAsync()
        {
            try
            {
                _logger.LogInformation("Starting KonMari achievements seeding...");

                // Get all KonMari achievement templates
                var templates = KonMariAchievementTemplates.GetAllKonMariAchievements();

                var achievementsToAdd = new List<Achievement>();

                foreach (var template in templates)
                {
                    // Check if achievement already exists by name
                    var existingAchievement = await _context.Achievements
                        .FirstOrDefaultAsync(a => a.Name == template.Name);

                    if (existingAchievement == null)
                    {
                        if (template.IsValid())
                        {
                            achievementsToAdd.Add(template.ToAchievement());
                        }
                        else
                        {
                            _logger.LogWarning($"Invalid achievement template: {template.Name}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Achievement already exists: {template.Name}");
                    }
                }

                if (achievementsToAdd.Any())
                {
                    _context.Achievements.AddRange(achievementsToAdd);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully seeded {achievementsToAdd.Count} KonMari achievements");
                }
                else
                {
                    _logger.LogInformation("No new KonMari achievements to seed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding KonMari achievements");
                throw;
            }
        }

        public async Task<bool> AreKonMariAchievementsSeededAsync()
        {
            try
            {
                // Check for specific KonMari achievements that should exist
                var konMariIndicators = new[]
                {
                    "Literary Curator",
                    "Style Curator",
                    "Memory Keeper",
                    "Item Lister Bronze"
                };

                var existingCount = await _context.Achievements
                    .Where(a => konMariIndicators.Contains(a.Name))
                    .CountAsync();

                return existingCount >= konMariIndicators.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking KonMari achievements seeding status");
                return false;
            }
        }

        public async Task<List<AchievementDto>> GetCategoryMasteryAchievementsAsync(int userId)
        {
            try
            {
                var categoryMasteryTypes = KonMariAchievementTemplates.GetValidCategories()
                    .Select(cat => $"category_mastery_{cat}")
                    .ToList();

                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToDictionaryAsync(ua => ua.AchievementId, ua => ua);

                var achievements = await _context.Achievements
                    .Where(a => categoryMasteryTypes.Contains(a.CriteriaType) && a.IsActive)
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
                        IsEarned = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked,
                        EarnedDate = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].EarnedDate : null,
                        Progress = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].Progress : 0,
                        ProgressPercentage = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked ? 100 : 0
                    })
                    .ToListAsync();

                return achievements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting category mastery achievements for user {userId}");
                return new List<AchievementDto>();
            }
        }

        public async Task<List<AchievementDto>> GetProgressiveAchievementsAsync(int userId, string criteriaType)
        {
            try
            {
                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToDictionaryAsync(ua => ua.AchievementId, ua => ua);

                var achievements = await _context.Achievements
                    .Where(a => a.CriteriaType == criteriaType && a.IsActive)
                    .OrderBy(a => a.CriteriaValue)
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
                        IsEarned = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked,
                        EarnedDate = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].EarnedDate : null,
                        Progress = userAchievements.ContainsKey(a.AchievementId) ? userAchievements[a.AchievementId].Progress : 0,
                        ProgressPercentage = userAchievements.ContainsKey(a.AchievementId) && userAchievements[a.AchievementId].IsUnlocked ? 100 : 0
                    })
                    .ToListAsync();

                return achievements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting progressive achievements for user {userId}, criteria {criteriaType}");
                return new List<AchievementDto>();
            }
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
                .Where(ua => ua.UserId == userId && ua.IsUnlocked == true)
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

        public async Task SeedUserAchievementsAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Seeding user achievement tracking for user {userId}...");

                var allAchievements = await _context.Achievements
                    .Where(a => a.IsActive)
                    .ToListAsync();

                var existingUserAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .Select(ua => ua.AchievementId)
                    .ToListAsync();

                var userAchievementsToAdd = new List<UserAchievement>();

                foreach (var achievement in allAchievements)
                {
                    if (!existingUserAchievements.Contains(achievement.AchievementId))
                    {
                        userAchievementsToAdd.Add(new UserAchievement
                        {
                            UserId = userId,
                            AchievementId = achievement.AchievementId,
                            Progress = 0,
                            IsUnlocked = false,
                            IsNotified = false,
                            EarnedDate = null,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (userAchievementsToAdd.Any())
                {
                    _context.UserAchievements.AddRange(userAchievementsToAdd);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully seeded {userAchievementsToAdd.Count} user achievement records for user {userId}");
                }
                else
                {
                    _logger.LogInformation($"No new user achievement records needed for user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error seeding user achievements for user {userId}");
                throw;
            }
        }

        public async Task SeedAllUsersAchievementsAsync()
        {
            try
            {
                var allUserIds = await _context.Users.Select(u => u.Id).ToListAsync();

                foreach (var userId in allUserIds)
                {
                    await SeedUserAchievementsAsync(userId);
                }

                _logger.LogInformation($"Completed seeding achievements for {allUserIds.Count} users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding achievements for all users");
                throw;
            }
        }

        // All the progress calculation methods remain the same...
        // [Include all your existing helper methods like GetKonMariProgressAsync, GetCategoryItemCountAsync, etc.]
        // I'm omitting them here for brevity, but keep them exactly as they were

        private async Task<int> GetKonMariProgressAsync(int userId, string criteriaType)
        {
            return criteriaType switch
            {
                // Category-specific item listings
                "items_listed_books_stationery" => await GetCategoryItemCountAsync(userId, "Books & Stationery"),
                "items_listed_clothing_accessories" => await GetCategoryItemCountAsync(userId, "Clothing & Accessories"),
                "items_listed_electronics_gadgets" => await GetCategoryItemCountAsync(userId, "Electronics & Gadgets"),
                "items_listed_toys_games" => await GetCategoryItemCountAsync(userId, "Toys & Games"),
                "items_listed_home_kitchen" => await GetCategoryItemCountAsync(userId, "Home & Kitchen"),
                "items_listed_furniture" => await GetCategoryItemCountAsync(userId, "Furniture"),
                "items_listed_appliances" => await GetCategoryItemCountAsync(userId, "Appliances"),
                "items_listed_health_beauty" => await GetCategoryItemCountAsync(userId, "Health & Beauty"),
                "items_listed_crafts_diy" => await GetCategoryItemCountAsync(userId, "Crafts & DIY"),
                "items_listed_school_office" => await GetCategoryItemCountAsync(userId, "School & Office"),
                "items_listed_sentimental" => await GetCategoryItemCountAsync(userId, "Sentimental Items"),

                // Category mastery (requires both quest completion AND item threshold)
                "category_mastery_books_stationery" => await GetCategoryMasteryProgressAsync(userId, "Books & Stationery", 20),
                "category_mastery_clothing_accessories" => await GetCategoryMasteryProgressAsync(userId, "Clothing & Accessories", 25),
                "category_mastery_electronics_gadgets" => await GetCategoryMasteryProgressAsync(userId, "Electronics & Gadgets", 15),
                "category_mastery_toys_games" => await GetCategoryMasteryProgressAsync(userId, "Toys & Games", 20),
                "category_mastery_home_kitchen" => await GetCategoryMasteryProgressAsync(userId, "Home & Kitchen", 30),
                "category_mastery_furniture" => await GetCategoryMasteryProgressAsync(userId, "Furniture", 10),
                "category_mastery_appliances" => await GetCategoryMasteryProgressAsync(userId, "Appliances", 8),
                "category_mastery_health_beauty" => await GetCategoryMasteryProgressAsync(userId, "Health & Beauty", 25),
                "category_mastery_crafts_diy" => await GetCategoryMasteryProgressAsync(userId, "Crafts & DIY", 20),
                "category_mastery_school_office" => await GetCategoryMasteryProgressAsync(userId, "School & Office", 15),
                "category_mastery_sentimental" => await GetCategoryMasteryProgressAsync(userId, "Sentimental Items", 10),

                // Progressive achievements
                "total_items_listed" => await _context.Items.CountAsync(i => i.UserId == userId),
                "categories_mastered" => await GetMasteredCategoriesCountAsync(userId),
                "community_engagement_total" => await GetCommunityEngagementCountAsync(userId),

                // KonMari methodology tracking
                "joy_mentions" => await CountContentMentionsAsync(userId, new[] { "joy", "spark joy", "brings joy" }),
                "gratitude_expressions" => await CountContentMentionsAsync(userId, new[] { "grateful", "thankful", "thank you" }),
                "transformation_posts" => await CountPostsByTypeAsync(userId, PostType.Achievement),
                "story_sharing_posts" => await CountPostsByContentAsync(userId, new[] { "story", "memory", "remember" }),

                // Community achievements
                "helpful_comments" => await _context.Comments.CountAsync(c => c.UserId == userId),
                "inspiring_posts" => await CountPostsByTypeAsync(userId, PostType.Tip),
                "positive_reactions_given" => await _context.Reactions.CountAsync(r => r.UserId == userId),
                "wisdom_sharing_posts" => await CountPostsByContentAsync(userId, new[] { "tip", "advice", "suggest" }),

                // Cross-category achievements
                "multi_category_sessions" => await CountMultiCategorySessionsAsync(userId),
                "all_categories_monthly" => await CheckAllCategoriesInTimeframeAsync(userId, 30),

                // Default fallback to existing logic
                _ => await GetCurrentProgressAsync(userId, criteriaType)
            };
        }

        private async Task<int> GetCategoryItemCountAsync(int userId, string categoryName)
        {
            return await _context.Items
                .Include(i => i.Category)
                .CountAsync(i => i.UserId == userId && i.Category.Name == categoryName);
        }

        private async Task<int> GetCategoryMasteryProgressAsync(int userId, string categoryName, int requiredItems)
        {
            var hasCompletedCategoryQuest = await _context.UserQuests
                .Include(uq => uq.Quest)
                .AnyAsync(uq => uq.UserId == userId &&
                               uq.IsCompleted &&
                               uq.Quest.QuestObjective.ToLower().Contains(categoryName.ToLower()));

            if (!hasCompletedCategoryQuest)
                return 0;

            var itemCount = await GetCategoryItemCountAsync(userId, categoryName);
            return itemCount >= requiredItems ? 1 : 0;
        }

        private async Task<int> GetMasteredCategoriesCountAsync(int userId)
        {
            var categories = new[]
            {
                "Books & Stationery", "Clothing & Accessories", "Electronics & Gadgets",
                "Toys & Games", "Home & Kitchen", "Furniture", "Appliances",
                "Health & Beauty", "Crafts & DIY", "School & Office", "Sentimental Items"
            };

            int masteredCount = 0;

            foreach (var category in categories)
            {
                var requiredItems = category == "Sentimental Items" ? 10 :
                                   category == "Furniture" ? 10 :
                                   category == "Appliances" ? 8 : 20;

                var isMastered = await GetCategoryMasteryProgressAsync(userId, category, requiredItems);
                if (isMastered == 1)
                    masteredCount++;
            }

            return masteredCount;
        }

        private async Task<int> GetCommunityEngagementCountAsync(int userId)
        {
            var posts = await _context.Posts.CountAsync(p => p.AuthorId == userId);
            var comments = await _context.Comments.CountAsync(c => c.UserId == userId);
            var reactions = await _context.Reactions.CountAsync(r => r.UserId == userId);

            return posts + comments + reactions;
        }

        private async Task<int> CountContentMentionsAsync(int userId, string[] keywords)
        {
            int count = 0;
            foreach (var keyword in keywords)
            {
                count += await _context.Posts
                    .Where(p => p.AuthorId == userId && p.PostContent.ToLower().Contains(keyword.ToLower()))
                    .CountAsync();

                count += await _context.Items
                    .Where(i => i.UserId == userId && (i.Description ?? "").ToLower().Contains(keyword.ToLower()))
                    .CountAsync();
            }
            return count;
        }

        private async Task<int> CountPostsByTypeAsync(int userId, PostType postType)
        {
            return await _context.Posts.CountAsync(p => p.AuthorId == userId && p.PostType == postType);
        }

        private async Task<int> CountPostsByContentAsync(int userId, string[] keywords)
        {
            var posts = await _context.Posts
                .Where(p => p.AuthorId == userId)
                .ToListAsync();

            return posts.Count(p => keywords.Any(keyword =>
                p.PostContent.ToLower().Contains(keyword.ToLower())));
        }

        private async Task<int> CountMultiCategorySessionsAsync(int userId)
        {
            var itemsByDate = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.UserId == userId)
                .GroupBy(i => i.DatePosted.Date)
                .Select(g => new { Date = g.Key, Categories = g.Select(i => i.Category.Name).Distinct().Count() })
                .Where(x => x.Categories >= 3)
                .CountAsync();

            return itemsByDate;
        }

        private async Task<int> CheckAllCategoriesInTimeframeAsync(int userId, int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var categoriesInTimeframe = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.UserId == userId && i.DatePosted >= cutoffDate)
                .Select(i => i.Category.Name)
                .Distinct()
                .CountAsync();

            return categoriesInTimeframe >= 11 ? 1 : 0;
        }
        private async Task<int> GetCurrentCheckInStreakAsync(int userId)
        {
            try
            {
                // Get the user's current daily check-in streak
                var dailyStreak = await _context.UserStreaks
                    .Include(us => us.StreakType)
                    .Where(us => us.UserId == userId && us.StreakType.Name == "Daily Check-in")
                    .FirstOrDefaultAsync();

                return dailyStreak?.CurrentStreak ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting check-in streak for user {userId}");
                return 0;
            }
        }
        private async Task<int> GetCurrentProgressAsync(int userId, string criteriaType)
        {
            if (criteriaType.StartsWith("category_mastery_") ||
                criteriaType.StartsWith("items_listed_") ||
                criteriaType.Contains("joy_") ||
                criteriaType.Contains("gratitude_") ||
                criteriaType.Contains("community_") ||
                criteriaType.Contains("multi_category"))
            {
                return await GetKonMariProgressAsync(userId, criteriaType);
            }

            return criteriaType switch
            {
                "items_listed" => await _context.Items.CountAsync(i => i.UserId == userId),
                "total_items_listed" => await _context.Items.CountAsync(i => i.UserId == userId),
                "total_quests_completed" => await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted),
                "daily_quests_completed" => await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .CountAsync(uq => uq.UserId == userId && uq.IsCompleted && uq.Quest.QuestType == QuestType.Daily),
                "weekly_quests_completed" => await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .CountAsync(uq => uq.UserId == userId && uq.IsCompleted && uq.Quest.QuestType == QuestType.Weekly),
                "daily_checkin_streak" => await GetCurrentCheckInStreakAsync(userId),
                "engaging_posts_created" => await CountEngagingPostsAsync(userId),
                "quest_categories_completed" => await CountQuestCategoriesCompletedAsync(userId),
                "player_level" => await GetPlayerLevelAsync(userId),
                "weekend_quests" => await CountWeekendQuestsAsync(userId),
                "evening_quests" => await CountEveningQuestsAsync(userId),
                "morning_quests" => await CountMorningQuestsAsync(userId),
                "quick_start" => await CheckQuickStartAsync(userId),
                "daily_quest_burst" => await GetMaxQuestsInSingleDayAsync(userId),
                "return_activity" => await CheckReturnActivityAsync(userId),
                _ => 0
            };
        }

        private static List<string> MapActionToCriteriaTypes(string actionType)
        {
            return actionType switch
            {
                "item_listed" => new List<string> { "items_listed", "total_items_listed" },
                "quest_completed" => new List<string> { "total_quests_completed" },
                "daily_quest_completed" => new List<string> { "daily_quests_completed", "total_quests_completed" },
                "weekly_quest_completed" => new List<string> { "weekly_quests_completed", "total_quests_completed" },
                "special_quest_completed" => new List<string> { "special_quest_completed", "total_quests_completed" },

                // FIX: Use correct criteria types that actually exist
                "comment_created" => new List<string> { "helpful_comments", "community_engagement_total" },
                "post_created" => new List<string> { "posts_created", "community_engagement_total" },
                "reaction_created" => new List<string> { "positive_reactions_given", "community_engagement_total" }, // Fix: was "reaction_given"

                "check_in" => new List<string> { "daily_checkin_streak" },
                "tokens_earned" => new List<string> { "tokens_earned" },
                "level_up" => new List<string> { "player_level" },
                "engaging_post_created" => new List<string> { "engaging_posts_created" },
                _ => new List<string>()
            };
        }

        private async Task<int> CountEngagingPostsAsync(int userId)
        {
            // Fix: Use correct property name for Post entity
            return await _context.Posts
                .Where(p => p.AuthorId == userId) // Change from UserId to AuthorId
                .CountAsync(p => p.Comments.Count + p.Reactions.Count >= 3);
        }

        private async Task<int> CountQuestCategoriesCompletedAsync(int userId)
        {
            // Fix: Quest might not have Category property, use a different approach
            var completedQuests = await _context.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId && uq.IsCompleted)
                .Select(uq => uq.Quest.QuestTitle) // Use title to infer category
                .Distinct()
                .ToListAsync();

            // Count unique categories based on quest content
            var categories = new HashSet<string>();
            foreach (var questTitle in completedQuests)
            {
                var category = InferCategoryFromQuestTitle(questTitle);
                if (!string.IsNullOrEmpty(category))
                    categories.Add(category);
            }

            return categories.Count;
        }

        private string InferCategoryFromQuestTitle(string questTitle)
        {
            var title = questTitle.ToLower();
            if (title.Contains("clothing") || title.Contains("wardrobe")) return "clothing";
            if (title.Contains("book") || title.Contains("literary")) return "books";
            if (title.Contains("kitchen") || title.Contains("cooking")) return "kitchen";
            if (title.Contains("bathroom") || title.Contains("beauty")) return "bathroom";
            if (title.Contains("garage") || title.Contains("tool")) return "garage";
            if (title.Contains("paper") || title.Contains("document")) return "papers";
            return "miscellaneous";
        }

        private async Task<int> GetPlayerLevelAsync(int userId)
        {
            var userStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);
            return userStats?.CurrentLevel ?? 1;
        }



        private async Task<int> CountWeekendQuestsAsync(int userId)
        {
            return await _context.UserQuests
                .Where(uq => uq.UserId == userId && uq.IsCompleted &&
                            uq.CompletedAt.HasValue &&
                            (uq.CompletedAt.Value.DayOfWeek == DayOfWeek.Saturday ||
                             uq.CompletedAt.Value.DayOfWeek == DayOfWeek.Sunday))
                .CountAsync();
        }

        private async Task<int> CountEveningQuestsAsync(int userId)
        {
            return await _context.UserQuests
                .Where(uq => uq.UserId == userId && uq.IsCompleted &&
                            uq.CompletedAt.HasValue &&
                            uq.CompletedAt.Value.Hour >= 20)
                .CountAsync();
        }

        private async Task<int> CountMorningQuestsAsync(int userId)
        {
            return await _context.UserQuests
                .Where(uq => uq.UserId == userId && uq.IsCompleted &&
                             uq.CompletedAt.HasValue &&
                            uq.CompletedAt.Value.Hour <= 10)
                .CountAsync();
        }

        private async Task<int> CheckQuickStartAsync(int userId)
        {
            // Check if user completed their first quest within 24 hours of their first activity
            var firstQuest = await _context.UserQuests
                .Where(uq => uq.UserId == userId && uq.IsCompleted)
                .OrderBy(uq => uq.CompletedAt)
                .FirstOrDefaultAsync();

            if (firstQuest?.CompletedAt != null)
            {
                // Check if completed within 24 hours of starting their first quest
                var timeDiff = firstQuest.CompletedAt.Value - firstQuest.StartedAt;
                return timeDiff.TotalHours <= 24 ? 1 : 0;
            }

            return 0;
        }

        private async Task<int> GetMaxQuestsInSingleDayAsync(int userId)
        {
            // Get the maximum number of quests completed in a single day
            var questsByDay = await _context.UserQuests
                .Where(uq => uq.UserId == userId && uq.IsCompleted && uq.CompletedAt.HasValue)
                .GroupBy(uq => uq.CompletedAt.Value.Date)
                .Select(g => g.Count())
                .ToListAsync();

            return questsByDay.Any() ? questsByDay.Max() : 0;
        }

        private async Task<int> CheckReturnActivityAsync(int userId)
        {
            // Check if user returned after 7+ days and completed a quest
            var allQuests = await _context.UserQuests
                .Where(uq => uq.UserId == userId && uq.IsCompleted && uq.CompletedAt.HasValue)
                .OrderBy(uq => uq.CompletedAt)
                .ToListAsync();

            for (int i = 1; i < allQuests.Count; i++)
            {
                var gap = allQuests[i].CompletedAt.Value - allQuests[i - 1].CompletedAt.Value;
                if (gap.TotalDays >= 7)
                {
                    return 1; // Found a comeback
                }
            }

            return 0;
        }
    }
}