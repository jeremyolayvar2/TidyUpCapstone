using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.ViewModels.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Implementations
{
    public class StreakService : IStreakService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAchievementService _achievementService;
        private readonly ILogger<StreakService> _logger;

        public StreakService(
            ApplicationDbContext context,
            IAchievementService achievementService,
            ILogger<StreakService> logger)
        {
            _context = context;
            _achievementService = achievementService;
            _logger = logger;
        }

        public async Task<bool> CheckInUserAsync(int userId)
        {
            try
            {
                var dailyCheckInStreak = await GetOrCreateDailyCheckInStreakAsync(userId);
                var today = DateTime.UtcNow.Date;

                if (await HasCheckedInTodayAsync(userId))
                {
                    _logger.LogInformation($"User {userId} already checked in today");
                    return false;
                }

                var lastActivityDate = dailyCheckInStreak.LastActivityDate?.Date;

                if (lastActivityDate == today.AddDays(-1))
                {
                    dailyCheckInStreak.CurrentStreak++;
                }
                else if (lastActivityDate < today.AddDays(-1) || lastActivityDate == null)
                {
                    dailyCheckInStreak.CurrentStreak = 1;
                }

                if (dailyCheckInStreak.CurrentStreak > dailyCheckInStreak.LongestStreak)
                {
                    dailyCheckInStreak.LongestStreak = dailyCheckInStreak.CurrentStreak;
                }

                dailyCheckInStreak.LastActivityDate = DateTime.UtcNow;

                // Award base check-in reward
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    var baseReward = dailyCheckInStreak.StreakType?.BaseRewards ?? 2.00m;
                    user.TokenBalance += baseReward;
                    _logger.LogInformation($"Awarded {baseReward} tokens to user {userId} for check-in");
                }

                // Check for milestone rewards
                await CheckAndAwardMilestoneRewardsAsync(userId, dailyCheckInStreak.StreakTypeId);

                // Update user XP (add 5 XP for daily check-in)
                await UpdateUserXpAsync(userId, 5);

                await _context.SaveChangesAsync();

                // Check achievements
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "check_in", 1);

                _logger.LogInformation($"User {userId} checked in successfully. Current streak: {dailyCheckInStreak.CurrentStreak}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking in user {userId}");
                return false;
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

                _logger.LogInformation($"User {userLevel.UserId} leveled up to level {nextLevel.LevelNumber}");
            }
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
                await CheckForLevelUpAsync(userLevel);
            }
            else
            {
                var initialLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == 1);
                if (initialLevel != null)
                {
                    var newUserLevel = new UserLevel
                    {
                        UserId = userId,
                        CurrentLevelId = initialLevel.LevelId,
                        CurrentXp = xpAmount,
                        TotalXp = xpAmount,
                        XpToNextLevel = Math.Max(0, initialLevel.XpToNext - xpAmount)
                    };
                    _context.UserLevels.Add(newUserLevel);
                }
            }
        }

        public async Task<bool> HasCheckedInTodayAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var dailyStreak = await _context.UserStreaks
                .Include(us => us.StreakType)
                .FirstOrDefaultAsync(us => us.UserId == userId &&
                                         us.StreakType.Name == "Daily Check-in" &&
                                         us.LastActivityDate >= today &&
                                         us.LastActivityDate < tomorrow);

            return dailyStreak != null;
        }

        public async Task<DateTime?> GetLastCheckInDateAsync(int userId)
        {
            var dailyStreak = await _context.UserStreaks
                .Include(us => us.StreakType)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakType.Name == "Daily Check-in");

            return dailyStreak?.LastActivityDate;
        }

        public async Task<UserStreakViewModel?> GetDailyCheckInStreakAsync(int userId)
        {
            var dailyStreak = await _context.UserStreaks
                .Include(us => us.StreakType)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakType.Name == "Daily Check-in");

            if (dailyStreak == null)
                return null;

            var nextMilestone = GetNextMilestoneAsync(dailyStreak.CurrentStreak, dailyStreak.StreakType.MilestoneInterval);
            var daysUntilMilestone = await GetDaysUntilMilestoneAsync(dailyStreak.CurrentStreak, dailyStreak.StreakType.MilestoneInterval);

            return new UserStreakViewModel
            {
                StreakId = dailyStreak.StreakId,
                StreakName = dailyStreak.StreakType.Name,
                Description = dailyStreak.StreakType.Description,
                CurrentStreak = dailyStreak.CurrentStreak,
                LongestStreak = dailyStreak.LongestStreak,
                StreakUnit = dailyStreak.StreakType.StreakUnit.ToString(),
                LastActivityDate = dailyStreak.LastActivityDate,
                NextMilestone = await nextMilestone,
                DaysUntilMilestone = daysUntilMilestone,
                MilestoneReward = dailyStreak.StreakType.MilestoneRewards,
                IsActive = IsStreakActive(dailyStreak)
            };
        }

        public async Task<List<UserStreakViewModel>> GetUserStreaksAsync(int userId)
        {
            var userStreaks = await _context.UserStreaks
                .Include(us => us.StreakType)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            var streakViewModels = new List<UserStreakViewModel>();

            foreach (var streak in userStreaks)
            {
                var nextMilestone = await GetNextMilestoneAsync(streak.CurrentStreak, streak.StreakType.MilestoneInterval);
                var daysUntilMilestone = await GetDaysUntilMilestoneAsync(streak.CurrentStreak, streak.StreakType.MilestoneInterval);

                streakViewModels.Add(new UserStreakViewModel
                {
                    StreakId = streak.StreakId,
                    StreakName = streak.StreakType.Name,
                    Description = streak.StreakType.Description,
                    CurrentStreak = streak.CurrentStreak,
                    LongestStreak = streak.LongestStreak,
                    StreakUnit = streak.StreakType.StreakUnit.ToString(),
                    LastActivityDate = streak.LastActivityDate,
                    NextMilestone = nextMilestone,
                    DaysUntilMilestone = daysUntilMilestone,
                    MilestoneReward = streak.StreakType.MilestoneRewards,
                    IsActive = IsStreakActive(streak)
                });
            }

            return streakViewModels;
        }

        public async Task<UserStreakViewModel?> GetUserStreakByTypeAsync(int userId, int streakTypeId)
        {
            var userStreak = await _context.UserStreaks
                .Include(us => us.StreakType)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakTypeId == streakTypeId);

            if (userStreak == null)
                return null;

            var nextMilestone = await GetNextMilestoneAsync(userStreak.CurrentStreak, userStreak.StreakType.MilestoneInterval);
            var daysUntilMilestone = await GetDaysUntilMilestoneAsync(userStreak.CurrentStreak, userStreak.StreakType.MilestoneInterval);

            return new UserStreakViewModel
            {
                StreakId = userStreak.StreakId,
                StreakName = userStreak.StreakType.Name,
                Description = userStreak.StreakType.Description,
                CurrentStreak = userStreak.CurrentStreak,
                LongestStreak = userStreak.LongestStreak,
                StreakUnit = userStreak.StreakType.StreakUnit.ToString(),
                LastActivityDate = userStreak.LastActivityDate,
                NextMilestone = nextMilestone,
                DaysUntilMilestone = daysUntilMilestone,
                MilestoneReward = userStreak.StreakType.MilestoneRewards,
                IsActive = IsStreakActive(userStreak)
            };
        }

        public async Task<bool> UpdateStreakAsync(int userId, int streakTypeId, string actionType)
        {
            try
            {
                var userStreak = await _context.UserStreaks
                    .Include(us => us.StreakType)
                    .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakTypeId == streakTypeId);

                if (userStreak == null)
                {
                    // Create new streak
                    var streakType = await _context.StreakTypes.FindAsync(streakTypeId);
                    if (streakType == null) return false;

                    userStreak = new UserStreak
                    {
                        UserId = userId,
                        StreakTypeId = streakTypeId,
                        CurrentStreak = 1,
                        LongestStreak = 1,
                        LastActivityDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserStreaks.Add(userStreak);
                }
                else
                {
                    // Update existing streak based on streak unit and last activity
                    var shouldIncrementStreak = ShouldIncrementStreak(userStreak, actionType);

                    if (shouldIncrementStreak)
                    {
                        userStreak.CurrentStreak++;
                        userStreak.LastActivityDate = DateTime.UtcNow;

                        if (userStreak.CurrentStreak > userStreak.LongestStreak)
                        {
                            userStreak.LongestStreak = userStreak.CurrentStreak;
                        }

                        // Check for milestone rewards
                        await CheckAndAwardMilestoneRewardsAsync(userId, streakTypeId);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating streak for user {userId}, streakType {streakTypeId}");
                return false;
            }
        }

        public async Task<List<string>> CheckAndAwardMilestoneRewardsAsync(int userId, int streakTypeId)
        {
            var rewards = new List<string>();

            try
            {
                var userStreak = await _context.UserStreaks
                    .Include(us => us.StreakType)
                    .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakTypeId == streakTypeId);

                if (userStreak == null) return rewards;

                var milestoneInterval = userStreak.StreakType.MilestoneInterval;
                if (userStreak.CurrentStreak % milestoneInterval == 0 && userStreak.CurrentStreak > 0)
                {
                    // Award milestone reward
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        var rewardAmount = userStreak.StreakType.MilestoneRewards;
                        user.TokenBalance += rewardAmount;

                        userStreak.TotalMilestonesReached++;
                        userStreak.LastMilestoneDate = DateTime.UtcNow;

                        rewards.Add($"Milestone Reward: {rewardAmount} tokens for {userStreak.CurrentStreak}-{userStreak.StreakType.StreakUnit} streak!");

                        // Check for streak-based achievements
                        if (userStreak.StreakType.Name == "Daily Check-in")
                        {
                            await _achievementService.UpdateAchievementProgressAsync(userId, "check_in_streak", userStreak.CurrentStreak);
                        }

                        _logger.LogInformation($"User {userId} reached milestone: {userStreak.CurrentStreak} {userStreak.StreakType.StreakUnit} streak");
                    }
                }

                return rewards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking milestone rewards for user {userId}, streakType {streakTypeId}");
                return rewards;
            }
        }

        public async Task<decimal> CalculateStreakRewardAsync(int streakTypeId, int currentStreak)
        {
            var streakType = await _context.StreakTypes.FindAsync(streakTypeId);
            if (streakType == null) return 0m;

            var baseReward = streakType.BaseRewards;
            var milestoneBonus = 0m;

            // Calculate milestone bonus
            if (currentStreak % streakType.MilestoneInterval == 0 && currentStreak > 0)
            {
                milestoneBonus = streakType.MilestoneRewards;
            }

            return baseReward + milestoneBonus;
        }

        public async Task<int> GetNextMilestoneAsync(int currentStreak, int milestoneInterval)
        {
            var nextMilestone = ((currentStreak / milestoneInterval) + 1) * milestoneInterval;
            return nextMilestone;
        }

        public async Task<int> GetDaysUntilMilestoneAsync(int currentStreak, int milestoneInterval)
        {
            var nextMilestone = await GetNextMilestoneAsync(currentStreak, milestoneInterval);
            return nextMilestone - currentStreak;
        }

        public async Task<bool> ResetStreakAsync(int userId, int streakTypeId)
        {
            try
            {
                var userStreak = await _context.UserStreaks
                    .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakTypeId == streakTypeId);

                if (userStreak != null)
                {
                    userStreak.CurrentStreak = 0;
                    userStreak.LastActivityDate = null;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Reset streak for user {userId}, streakType {streakTypeId}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting streak for user {userId}, streakType {streakTypeId}");
                return false;
            }
        }

        public async Task<List<StreakType>> GetAllStreakTypesAsync()
        {
            return await _context.StreakTypes.Where(st => st.IsActive).ToListAsync();
        }

        public async Task<StreakType?> GetStreakTypeByNameAsync(string name)
        {
            return await _context.StreakTypes.FirstOrDefaultAsync(st => st.Name == name && st.IsActive);
        }

        public async Task<StreakType> CreateStreakTypeAsync(StreakType streakType)
        {
            _context.StreakTypes.Add(streakType);
            await _context.SaveChangesAsync();
            return streakType;
        }

        public async Task<Dictionary<string, object>> GetStreakStatsAsync(int userId)
        {
            var stats = new Dictionary<string, object>();

            var userStreaks = await _context.UserStreaks
                .Include(us => us.StreakType)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            stats["TotalStreaks"] = userStreaks.Count;
            stats["ActiveStreaks"] = userStreaks.Count(us => IsStreakActive(us));
            stats["TotalMilestones"] = userStreaks.Sum(us => us.TotalMilestonesReached);
            stats["LongestOverallStreak"] = userStreaks.Any() ? userStreaks.Max(us => us.LongestStreak) : 0;

            var streaksByType = userStreaks
                .GroupBy(us => us.StreakType.Name)
                .ToDictionary(g => g.Key, g => g.First().CurrentStreak);

            stats["StreaksByType"] = streaksByType;

            return stats;
        }

        public async Task<List<UserStreakViewModel>> GetActiveStreaksAsync(int userId)
        {
            var activeStreaks = await GetUserStreaksAsync(userId);
            return activeStreaks.Where(s => s.IsActive).ToList();
        }

        public async Task SeedStreakTypesAsync()
        {
            if (await AreStreakTypesSeededAsync())
                return;

            var streakTypes = new List<StreakType>
            {
                new StreakType
                {
                    Name = "Daily Check-in",
                    Description = "Check in to the app daily to maintain your streak",
                    StreakUnit = StreakUnit.Days,
                    BaseRewards = 2.00m,
                    MilestoneRewards = 10.00m,
                    MilestoneInterval = 7,
                    IsActive = true
                },
                new StreakType
                {
                    Name = "Daily Declutterer",
                    Description = "List items for decluttering every day",
                    StreakUnit = StreakUnit.Days,
                    BaseRewards = 3.00m,
                    MilestoneRewards = 15.00m,
                    MilestoneInterval = 5,
                    IsActive = true
                },
                new StreakType
                {
                    Name = "Community Contributor",
                    Description = "Participate in community discussions daily",
                    StreakUnit = StreakUnit.Days,
                    BaseRewards = 2.50m,
                    MilestoneRewards = 12.00m,
                    MilestoneInterval = 7,
                    IsActive = true
                },
                new StreakType
                {
                    Name = "Trading Streak",
                    Description = "Complete successful transactions regularly",
                    StreakUnit = StreakUnit.Transactions,
                    BaseRewards = 5.00m,
                    MilestoneRewards = 25.00m,
                    MilestoneInterval = 10,
                    IsActive = true
                }
            };

            _context.StreakTypes.AddRange(streakTypes);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Seeded {streakTypes.Count} streak types");
        }

        public async Task<bool> AreStreakTypesSeededAsync()
        {
            return await _context.StreakTypes.AnyAsync();
        }

        public async Task CheckAndResetExpiredStreaksAsync()
        {
            try
            {
                var userStreaks = await _context.UserStreaks
                    .Include(us => us.StreakType)
                    .Where(us => us.LastActivityDate.HasValue)
                    .ToListAsync();

                foreach (var userStreak in userStreaks)
                {
                    if (await IsStreakBrokenAsync(userStreak.UserId, userStreak.StreakTypeId,
                        userStreak.LastActivityDate.Value, userStreak.StreakType.StreakUnit))
                    {
                        userStreak.CurrentStreak = 0;
                        _logger.LogInformation($"Reset expired streak for user {userStreak.UserId}, streak type {userStreak.StreakType.Name}");
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and resetting expired streaks");
            }
        }

        public async Task<bool> IsStreakBrokenAsync(int userId, int streakTypeId, DateTime lastActivity, StreakUnit unit)
        {
            var now = DateTime.UtcNow;
            var daysSinceLastActivity = (now.Date - lastActivity.Date).Days;

            return unit switch
            {
                StreakUnit.Days => daysSinceLastActivity > 1,
                StreakUnit.Weeks => daysSinceLastActivity > 7,
                StreakUnit.Items => false, // Item streaks don't break based on time
                StreakUnit.Transactions => false, // Transaction streaks don't break based on time
                _ => daysSinceLastActivity > 1
            };
        }

        // Private helper methods
        private async Task<UserStreak> GetOrCreateDailyCheckInStreakAsync(int userId)
        {
            var streakType = await GetStreakTypeByNameAsync("Daily Check-in");
            if (streakType == null)
            {
                throw new InvalidOperationException("Daily Check-in streak type not found. Please seed streak types first.");
            }

            var userStreak = await _context.UserStreaks
                .Include(us => us.StreakType)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.StreakTypeId == streakType.StreakTypeId);

            if (userStreak == null)
            {
                userStreak = new UserStreak
                {
                    UserId = userId,
                    StreakTypeId = streakType.StreakTypeId,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    TotalMilestonesReached = 0,
                    CreatedAt = DateTime.UtcNow,
                    StreakType = streakType
                };

                _context.UserStreaks.Add(userStreak);
            }

            return userStreak;
        }

        private static bool IsStreakActive(UserStreak userStreak)
        {
            if (!userStreak.LastActivityDate.HasValue || userStreak.CurrentStreak == 0)
                return false;

            var daysSinceLastActivity = (DateTime.UtcNow.Date - userStreak.LastActivityDate.Value.Date).Days;

            return userStreak.StreakType.StreakUnit switch
            {
                StreakUnit.Days => daysSinceLastActivity <= 1,
                StreakUnit.Weeks => daysSinceLastActivity <= 7,
                StreakUnit.Items => true, // Item streaks are always considered active if they have progress
                StreakUnit.Transactions => true, // Transaction streaks are always considered active if they have progress
                _ => daysSinceLastActivity <= 1
            };
        }

        private static bool ShouldIncrementStreak(UserStreak userStreak, string actionType)
        {
            if (userStreak.LastActivityDate == null)
                return true;

            var now = DateTime.UtcNow;
            var lastActivity = userStreak.LastActivityDate.Value;

            return userStreak.StreakType.StreakUnit switch
            {
                StreakUnit.Days => lastActivity.Date != now.Date, // Only increment once per day
                StreakUnit.Weeks => (now - lastActivity).Days >= 7, // Only increment once per week
                StreakUnit.Items => true, // Always increment for item-based streaks
                StreakUnit.Transactions => true, // Always increment for transaction-based streaks
                _ => lastActivity.Date != now.Date
            };
        }
    }
}