using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Implementations
{
    public class UserStatisticsService : IUserStatisticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserStatisticsService> _logger;

        public UserStatisticsService(ApplicationDbContext context, ILogger<UserStatisticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // NEW METHOD: Returns UserStatisticsDto as expected by the interface
        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            try
            {
                var userStats = await GetOrCreateUserStatsAsync(userId);
                var userLevel = await _context.UserLevels
                    .Include(ul => ul.CurrentLevel)
                    .FirstOrDefaultAsync(ul => ul.UserId == userId);

                if (userLevel == null)
                {
                    userLevel = await CreateUserLevelAsync(userId);
                }

                return new UserStatisticsDto
                {
                    UserId = userId,
                    CurrentLevel = userLevel.CurrentLevel?.LevelNumber ?? 1,
                    CurrentXp = userStats.CurrentXp,
                    XpToNextLevel = userLevel.XpToNextLevel,
                    TokenBalance = userStats.TotalTokens,
                    CurrentStreak = userStats.CurrentStreak,
                    LongestStreak = userStats.LongestStreak
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user statistics for user {userId}");
                throw;
            }
        }

        // NEW METHOD: Sets absolute values for user stats
        public async Task<bool> SetUserStatsAsync(int userId, decimal tokenBalance, int xp, string reason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userStats = await GetOrCreateUserStatsAsync(userId);
                userStats.TotalTokens = tokenBalance;
                userStats.CurrentXp = xp;

                // Sync to AppUser
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TokenBalance = tokenBalance;
                }

                // Update UserLevel if it exists
                var userLevel = await _context.UserLevels.FirstOrDefaultAsync(ul => ul.UserId == userId);
                if (userLevel != null)
                {
                    userLevel.CurrentXp = xp;
                    userLevel.TotalXp = xp;
                    await UpdateLevelProgressAsync(userLevel);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Set user stats for user {userId}: {tokenBalance} tokens, {xp} XP. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Failed to set user stats for user {userId}");
                return false;
            }
        }

        // EXISTING METHODS - keep these as they are
        public async Task<bool> AwardTokensAsync(int userId, decimal tokens, string reason)
        {
            if (tokens <= 0) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userStats = await GetOrCreateUserStatsAsync(userId);
                userStats.TotalTokens += tokens;

                // Sync to AppUser for backward compatibility
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TokenBalance = userStats.TotalTokens;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Awarded {tokens} tokens to user {userId}. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Failed to award {tokens} tokens to user {userId}");
                return false;
            }
        }

        public async Task<bool> AwardXpAsync(int userId, int xp, string reason)
        {
            if (xp <= 0) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userStats = await GetOrCreateUserStatsAsync(userId);
                userStats.CurrentXp += xp;

                // Update UserLevel if it exists
                var userLevel = await _context.UserLevels.FirstOrDefaultAsync(ul => ul.UserId == userId);
                if (userLevel != null)
                {
                    userLevel.CurrentXp = userStats.CurrentXp;
                    userLevel.TotalXp += xp;
                    await CheckForLevelUpAsync(userLevel);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Awarded {xp} XP to user {userId}. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Failed to award {xp} XP to user {userId}");
                return false;
            }
        }

        public async Task<bool> AwardTokensAndXpAsync(int userId, decimal tokens, int xp, string reason)
        {
            if (tokens <= 0 && xp <= 0) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userStats = await GetOrCreateUserStatsAsync(userId);

                if (tokens > 0)
                {
                    userStats.TotalTokens += tokens;
                }

                if (xp > 0)
                {
                    userStats.CurrentXp += xp;
                }

                // Sync to AppUser
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TokenBalance = userStats.TotalTokens;
                }

                // Update UserLevel if it exists
                if (xp > 0)
                {
                    var userLevel = await _context.UserLevels.FirstOrDefaultAsync(ul => ul.UserId == userId);
                    if (userLevel != null)
                    {
                        userLevel.CurrentXp = userStats.CurrentXp;
                        userLevel.TotalXp += xp;
                        await CheckForLevelUpAsync(userLevel);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Awarded {tokens} tokens and {xp} XP to user {userId}. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Failed to award rewards to user {userId}");
                return false;
            }
        }

        public async Task<bool> DeductTokensAsync(int userId, decimal tokens, string reason)
        {
            if (tokens <= 0) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userStats = await GetUserStatsAsync(userId);
                if (userStats == null || userStats.TotalTokens < tokens)
                {
                    _logger.LogWarning($"Insufficient tokens for user {userId}. Has: {userStats?.TotalTokens ?? 0}, Needs: {tokens}");
                    return false;
                }

                userStats.TotalTokens -= tokens;

                // Sync to AppUser
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TokenBalance = userStats.TotalTokens;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Deducted {tokens} tokens from user {userId}. Reason: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Failed to deduct {tokens} tokens from user {userId}");
                return false;
            }
        }

        public async Task<UserStats> GetUserStatsAsync(int userId)
        {
            return await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);
        }

        public async Task<UserStats> GetOrCreateUserStatsAsync(int userId)
        {
            var userStats = await GetUserStatsAsync(userId);

            if (userStats == null)
            {
                userStats = new UserStats
                {
                    UserId = userId,
                    CurrentLevel = 1,
                    CurrentXp = 0,
                    TotalTokens = 100, // Starting balance
                    CurrentStreak = 0,
                    LongestStreak = 0
                };

                _context.UserStats.Add(userStats);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created UserStats for user {userId}");
            }

            return userStats;
        }

        public async Task<bool> ReconcileUserStatsAsync(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Calculate correct values from transaction history
                var claimedQuestTokens = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId && uq.DateClaimed != null)
                    .SumAsync(uq => uq.Quest.TokenReward);

                var checkInTokens = await _context.CheckIns
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.TokensEarned);

                var correctTokenBalance = 100 + claimedQuestTokens + checkInTokens; // 100 starting balance

                var userStats = await GetOrCreateUserStatsAsync(userId);
                userStats.TotalTokens = correctTokenBalance;

                // Sync to AppUser
                await SyncToAppUserAsync(userId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Reconciled user stats for user {userId}. Token balance: {correctTokenBalance}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Failed to reconcile user stats for user {userId}");
                return false;
            }
        }

        public async Task<bool> SyncToAppUserAsync(int userId)
        {
            try
            {
                var userStats = await GetUserStatsAsync(userId);
                var user = await _context.Users.FindAsync(userId);

                if (userStats != null && user != null)
                {
                    user.TokenBalance = userStats.TotalTokens;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to sync UserStats to AppUser for user {userId}");
                return false;
            }
        }

        // PRIVATE HELPER METHODS
        private async Task<UserLevel> CreateUserLevelAsync(int userId)
        {
            var initialLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == 1);
            if (initialLevel == null)
            {
                // Create default level if none exists
                initialLevel = new Level
                {
                    LevelNumber = 1,
                    LevelName = "Beginner",
                    XpRequired = 0,
                    XpToNext = 100
                };
                _context.Levels.Add(initialLevel);
                await _context.SaveChangesAsync();
            }

            var userLevel = new UserLevel
            {
                UserId = userId,
                CurrentLevelId = initialLevel.LevelId,
                CurrentXp = 0,
                TotalXp = 0,
                XpToNextLevel = initialLevel.XpToNext,
                LevelUpDate = DateTime.UtcNow,
                TotalLevelUps = 0
            };

            _context.UserLevels.Add(userLevel);
            await _context.SaveChangesAsync();

            // Load the relationship
            await _context.Entry(userLevel)
                .Reference(ul => ul.CurrentLevel)
                .LoadAsync();

            return userLevel;
        }

        private async Task UpdateLevelProgressAsync(UserLevel userLevel)
        {
            // Load current level if not loaded
            if (userLevel.CurrentLevel == null)
            {
                await _context.Entry(userLevel)
                    .Reference(ul => ul.CurrentLevel)
                    .LoadAsync();
            }

            // Check for level up
            await CheckForLevelUpAsync(userLevel);

            // Update XP to next level
            var nextLevel = await _context.Levels
                .Where(l => l.LevelNumber > userLevel.CurrentLevel.LevelNumber)
                .OrderBy(l => l.LevelNumber)
                .FirstOrDefaultAsync();

            if (nextLevel != null)
            {
                userLevel.XpToNextLevel = Math.Max(0, nextLevel.XpRequired - userLevel.CurrentXp);
            }
            else
            {
                userLevel.XpToNextLevel = 0; // Max level reached
            }
        }

        private async Task CheckForLevelUpAsync(UserLevel userLevel)
        {
            // Load current level if not loaded
            if (userLevel.CurrentLevel == null)
            {
                await _context.Entry(userLevel)
                    .Reference(ul => ul.CurrentLevel)
                    .LoadAsync();
            }

            bool leveledUp = false;

            // Keep checking for multiple level ups
            while (true)
            {
                var nextLevel = await _context.Levels
                    .Where(l => l.LevelNumber > userLevel.CurrentLevel.LevelNumber)
                    .OrderBy(l => l.LevelNumber)
                    .FirstOrDefaultAsync();

                if (nextLevel != null && userLevel.CurrentXp >= nextLevel.XpRequired)
                {
                    userLevel.CurrentLevelId = nextLevel.LevelId;
                    userLevel.CurrentLevel = nextLevel;
                    userLevel.LevelUpDate = DateTime.UtcNow;
                    userLevel.TotalLevelUps++;
                    leveledUp = true;

                    _logger.LogInformation($"User {userLevel.UserId} leveled up to level {nextLevel.LevelNumber} ({nextLevel.LevelName})");
                }
                else
                {
                    break; // No more level ups possible
                }
            }

            if (leveledUp)
            {
                // Update XP to next level after all level ups
                var nextLevel = await _context.Levels
                    .Where(l => l.LevelNumber > userLevel.CurrentLevel.LevelNumber)
                    .OrderBy(l => l.LevelNumber)
                    .FirstOrDefaultAsync();

                userLevel.XpToNextLevel = nextLevel != null ?
                    Math.Max(0, nextLevel.XpRequired - userLevel.CurrentXp) : 0;
            }
        }
    }
}