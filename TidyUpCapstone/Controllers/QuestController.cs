using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers
{
    [AllowAnonymous] // Allow anonymous for testing
    public class QuestController : Controller
    {
        private readonly IQuestService _questService;
        private readonly IAchievementService _achievementService;
        private readonly IStreakService _streakService;
        private readonly IUserStatisticsService _userStatisticsService; // ADDED
        private readonly ILogger<QuestController> _logger;
        private readonly ApplicationDbContext _context;

        public QuestController(
            IQuestService questService,
            IAchievementService achievementService,
            IStreakService streakService,
            IUserStatisticsService userStatisticsService, // ADDED
            ILogger<QuestController> logger,
            ApplicationDbContext context)
        {
            _questService = questService;
            _achievementService = achievementService;
            _streakService = streakService;
            _userStatisticsService = userStatisticsService; // ADDED
            _logger = logger;
            _context = context;
        }

        private DateTime GetPhilippineTime()
        {
            var philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);
        }

        private int GetCurrentUserId()
        {
            return 1; // Always use test user ID 1
        }

        private int GetUserId()
        {
            return 1; // For testing, always return user ID 1
        }

        // Main Quest Dashboard - UPDATED to use centralized service
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();

                // Use centralized service for consistent stats
                var userStats = await _userStatisticsService.GetUserStatisticsAsync(userId);
                var userLevel = await GetOrCreateUserLevel(userId);

                var viewModel = new GamificationDashboardViewModel
                {
                    ActiveQuests = await _questService.GetActiveQuestsForUserAsync(userId),
                    CompletedQuests = await _questService.GetCompletedQuestsForUserAsync(userId),
                    RecentAchievements = await _achievementService.GetRecentAchievementsAsync(userId),
                    ActiveStreaks = await _streakService.GetActiveStreaksAsync(userId),

                    LevelProgress = new UserLevelProgressViewModel
                    {
                        CurrentLevel = userStats.CurrentLevel,
                        CurrentLevelName = userLevel.CurrentLevel?.LevelName ?? "Beginner",
                        CurrentXp = userStats.CurrentXp,
                        TotalXp = userLevel.TotalXp,
                        XpToNextLevel = userStats.XpToNextLevel,
                        XpProgress = CalculateXpProgress(userStats.CurrentXp, userStats.XpToNextLevel),
                        NextLevelName = await GetNextLevelName(userLevel.CurrentLevelId)
                    },

                    Stats = new GamificationStatsViewModel
                    {
                        TotalQuestsCompleted = await _context.UserQuests
                            .CountAsync(uq => uq.UserId == userId && uq.IsCompleted),
                        AchievementsEarned = await _context.UserAchievements
                            .CountAsync(ua => ua.UserId == userId && ua.IsUnlocked),
                        TotalAchievements = await _context.Achievements.CountAsync(a => a.IsActive),

                        // Use centralized stats
                        TokenBalance = userStats.TokenBalance,
                        TotalXpEarned = userStats.CurrentXp,
                        ActiveStreaksCount = await _context.UserStreaks
                            .CountAsync(us => us.UserId == userId && us.CurrentStreak > 0),

                        HighestRank = 0,
                        FavoriteQuestType = null,
                        RecentRewards = new List<string>()
                    }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quest dashboard");
                return View(new GamificationDashboardViewModel());
            }
        }

        private async Task<UserLevel> GetOrCreateUserLevel(int userId)
        {
            var userLevel = await _context.UserLevels
                .Include(ul => ul.CurrentLevel)
                .FirstOrDefaultAsync(ul => ul.UserId == userId);

            if (userLevel == null)
            {
                var initialLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == 1);
                if (initialLevel != null)
                {
                    userLevel = new UserLevel
                    {
                        UserId = userId,
                        CurrentLevelId = initialLevel.LevelId,
                        CurrentXp = 0,
                        TotalXp = 0,
                        XpToNextLevel = initialLevel.XpToNext
                    };
                    _context.UserLevels.Add(userLevel);
                    await _context.SaveChangesAsync();
                }
            }

            return userLevel ?? new UserLevel { UserId = userId, CurrentXp = 0, TotalXp = 0, XpToNextLevel = 100 };
        }

        private int CalculateXpProgress(int currentXp, int xpToNextLevel)
        {
            if (xpToNextLevel <= 0) return 100;

            int totalXpForCurrentLevel = currentXp + xpToNextLevel;
            return totalXpForCurrentLevel > 0 ? (currentXp * 100 / totalXpForCurrentLevel) : 0;
        }

        private async Task<string> GetNextLevelName(int currentLevelId)
        {
            var currentLevel = await _context.Levels.FindAsync(currentLevelId);
            if (currentLevel == null) return "Next Level";

            var nextLevel = await _context.Levels
                .Where(l => l.LevelNumber > currentLevel.LevelNumber)
                .OrderBy(l => l.LevelNumber)
                .FirstOrDefaultAsync();

            return nextLevel?.LevelName ?? "Max Level";
        }

        // Available Quests Page
        public async Task<IActionResult> Available()
        {
            try
            {
                var userId = GetUserId();
                var availableQuests = await _questService.GetAvailableQuestsAsync(userId);
                return View(availableQuests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available quests");
                TempData["ErrorMessage"] = "An error occurred while loading available quests.";
                return View(new List<Models.DTOs.Gamification.QuestDto>());
            }
        }

        // Quest Details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = GetUserId();
                var quest = await _questService.GetQuestByIdAsync(id, userId);
                if (quest == null)
                {
                    TempData["ErrorMessage"] = "Quest not found.";
                    return RedirectToAction("Index");
                }

                return View(quest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading quest details for quest {id}");
                TempData["ErrorMessage"] = "An error occurred while loading quest details.";
                return RedirectToAction("Index");
            }
        }

        // Start Quest
        [HttpPost]
        public async Task<IActionResult> StartQuest(int questId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _questService.StartQuestAsync(userId, questId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Quest started successfully!";
                    return Json(new { success = true, message = "Quest started successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to start quest. You may have already started this quest." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting quest {questId} for user");
                return Json(new { success = false, message = "An error occurred while starting the quest." });
            }
        }

        // FIXED: Claim Quest Reward - now uses centralized service
        [HttpPost]
        public async Task<IActionResult> ClaimReward(int questId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _questService.ClaimQuestRewardAsync(userId, questId);

                if (result)
                {
                    // Get updated stats after claiming
                    var updatedStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                    TempData["SuccessMessage"] = "Reward claimed successfully!";
                    return Json(new
                    {
                        success = true,
                        message = "Reward claimed successfully!",
                        userStats = updatedStats
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to claim reward. Quest may not be completed or reward already claimed." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error claiming reward for quest {questId}");
                return Json(new { success = false, message = "An error occurred while claiming the reward." });
            }
        }

        // FIXED: Daily Check-in - now uses centralized StreakService
        [HttpPost]
        public async Task<IActionResult> CheckIn()
        {
            try
            {
                var userId = GetCurrentUserId();

                // Use centralized streak service
                var success = await _streakService.CheckInUserAsync(userId);

                if (success)
                {
                    // Get updated information
                    var updatedStats = await _userStatisticsService.GetUserStatisticsAsync(userId);
                    var dailyStreak = await _streakService.GetDailyCheckInStreakAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = "Check-in successful!",
                        currentLevel = updatedStats.CurrentLevel,
                        currentXp = updatedStats.CurrentXp,
                        tokenBalance = updatedStats.TokenBalance,
                        streak = dailyStreak?.CurrentStreak ?? 0,
                        userStats = updatedStats
                    });
                }

                return Json(new { success = false, message = "Check-in failed or already checked in today" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check-in");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // REMOVED: UpdateUserXpAsync and CheckForLevelUpAsync methods
        // These are now handled by UserStatisticsService

        // Get Check-in Status - UPDATED to use StreakService
        [HttpGet]
        public async Task<IActionResult> CheckInStatus()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userStats = await _userStatisticsService.GetUserStatisticsAsync(userId);
                var hasCheckedIn = await _streakService.HasCheckedInTodayAsync(userId);
                var dailyStreak = await _streakService.GetDailyCheckInStreakAsync(userId);

                var today = GetPhilippineTime().Date;
                var tomorrow = today.AddDays(1);
                var timeUntilNext = tomorrow - GetPhilippineTime();

                return Json(new
                {
                    hasCheckedIn = hasCheckedIn,
                    streak = dailyStreak?.CurrentStreak ?? 0,
                    lastCheckIn = dailyStreak?.LastActivityDate,
                    tokenBalance = userStats.TokenBalance,
                    currentLevel = userStats.CurrentLevel,
                    currentXp = userStats.CurrentXp,
                    nextCheckInTime = tomorrow,
                    timeUntilNextCheckIn = new
                    {
                        hours = (int)timeUntilNext.TotalHours,
                        minutes = timeUntilNext.Minutes,
                        seconds = timeUntilNext.Seconds,
                        totalSeconds = (int)timeUntilNext.TotalSeconds
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting check-in status");
                return Json(new { hasCheckedIn = false, streak = 0 });
            }
        }

        // Achievements Page
        public async Task<IActionResult> Achievements()
        {
            try
            {
                var userId = GetUserId();
                var allAchievements = await _achievementService.GetAllAchievementsAsync(userId);

                var viewModel = new GamificationDashboardViewModel
                {
                    AllAchievements = allAchievements,
                    Stats = new GamificationStatsViewModel()
                };

                var achievementStats = await _achievementService.GetAchievementStatsAsync(userId);
                viewModel.Stats.AchievementsEarned = (int)(achievementStats.ContainsKey("TotalEarned") ? achievementStats["TotalEarned"] : 0);
                viewModel.Stats.TotalAchievements = (int)(achievementStats.ContainsKey("TotalAvailable") ? achievementStats["TotalAvailable"] : 0);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading achievements page");
                TempData["ErrorMessage"] = "An error occurred while loading achievements.";
                return View(new GamificationDashboardViewModel());
            }
        }

        public async Task<IActionResult> SeedUserAchievements(int? userId = null)
        {
            try
            {
                var targetUserId = userId ?? GetCurrentUserId();
                await _achievementService.SeedUserAchievementsAsync(targetUserId);

                return Json(new
                {
                    success = true,
                    message = $"Seeded achievements for user {targetUserId}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetAllAchievements(int userId = 1)
        {
            try
            {
                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToListAsync();

                foreach (var ua in userAchievements)
                {
                    ua.IsUnlocked = false;
                    ua.Progress = 0;
                    ua.EarnedDate = null;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Reset {userAchievements.Count} achievements to locked state"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugAchievementStatus(int userId = 1)
        {
            var achievements = await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .Select(ua => new
                {
                    Name = ua.Achievement.Name,
                    IsUnlocked = ua.IsUnlocked,
                    Progress = ua.Progress,
                    EarnedDate = ua.EarnedDate,
                    RequiredValue = ua.Achievement.CriteriaValue
                })
                .ToListAsync();

            return Json(achievements);
        }

        [HttpGet]
        public async Task<IActionResult> SeedSpecialQuests()
        {
            try
            {
                await _questService.GenerateSpecialQuestAsync();
                return Json(new { success = true, message = "Special quest generated using KonMari templates" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GenerateSpecialQuest(int? userId = null)
        {
            try
            {
                await _questService.GenerateSpecialQuestAsync();
                return Json(new { success = true, message = "Special quest generated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Streaks Page
        public async Task<IActionResult> Streaks()
        {
            try
            {
                var userId = GetUserId();
                var userStreaks = await _streakService.GetUserStreaksAsync(userId);

                var viewModel = new GamificationDashboardViewModel
                {
                    ActiveStreaks = userStreaks,
                    Stats = new GamificationStatsViewModel()
                };

                var streakStats = await _streakService.GetStreakStatsAsync(userId);
                viewModel.Stats.ActiveStreaksCount = (int)(streakStats.ContainsKey("ActiveStreaks") ? streakStats["ActiveStreaks"] : 0);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading streaks page");
                TempData["ErrorMessage"] = "An error occurred while loading streaks.";
                return View(new GamificationDashboardViewModel());
            }
        }

        // API endpoint to update quest progress
        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int questId, int progress, string? actionType = null)
        {
            try
            {
                var userId = GetUserId();
                var result = await _questService.UpdateQuestProgressAsync(userId, questId, progress, actionType);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating quest progress for quest {questId}");
                return Json(new { success = false, message = "An error occurred while updating quest progress." });
            }
        }

        // Get user's quest progress
        [HttpGet]
        public async Task<IActionResult> GetProgress(int questId)
        {
            try
            {
                var userId = GetUserId();
                var progress = await _questService.GetUserQuestProgressAsync(userId, questId);
                return Json(new { success = true, progress = progress });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting quest progress for quest {questId}");
                return Json(new { success = false, message = "An error occurred while getting quest progress." });
            }
        }

        // Get all user quest progress
        [HttpGet]
        public async Task<IActionResult> GetAllProgress()
        {
            try
            {
                var userId = GetUserId();
                var allProgress = await _questService.GetAllUserQuestProgressAsync(userId);
                return Json(new { success = true, progress = allProgress });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all quest progress");
                return Json(new { success = false, message = "An error occurred while getting quest progress." });
            }
        }

        // Debug Methods - UPDATED to use centralized service
        [HttpGet]
        public async Task<IActionResult> DebugData()
        {
            try
            {
                var userId = GetUserId();
                var userStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                var questCount = await _context.Quests.CountAsync();
                var userQuestCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId);
                var activeQuests = await _questService.GetActiveQuestsForUserAsync(userId);
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

                return Json(new
                {
                    success = true,
                    totalQuests = questCount,
                    userQuests = userQuestCount,
                    activeQuestsCount = activeQuests.Count,
                    activeQuests = activeQuests.Take(3),
                    userId = userId,
                    userExists = userExists,
                    userStats = userStats,
                    tablesExist = new
                    {
                        quests = await _context.Quests.AnyAsync(),
                        userQuests = await _context.UserQuests.AnyAsync(),
                        achievements = await _context.Achievements.AnyAsync(),
                        levels = await _context.Levels.AnyAsync(),
                        users = await _context.Users.AnyAsync()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugGenerateQuests()
        {
            try
            {
                await _questService.GenerateDailyQuestsAsync();
                await _questService.GenerateWeeklyQuestsAsync();

                var userId = GetUserId();
                var activeQuests = await _questService.GetActiveQuestsForUserAsync(userId);

                return Json(new
                {
                    success = true,
                    message = "Quests generated successfully",
                    questsGenerated = activeQuests.Count,
                    dailyQuests = activeQuests.Count(q => q.QuestType == Models.Entities.Gamification.QuestType.Daily),
                    weeklyQuests = activeQuests.Count(q => q.QuestType == Models.Entities.Gamification.QuestType.Weekly)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugCompleteQuest(int questId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _questService.UpdateQuestProgressAsync(userId, questId, 999, "debug_complete");

                return Json(new
                {
                    success = true,
                    message = "Quest completed for testing",
                    questId = questId,
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                var testUser = await _context.Users.FindAsync(1);
                if (testUser == null)
                {
                    return Json(new { success = false, message = "Test user ID 1 not found in database. Please create a user first." });
                }
                return Json(new
                {
                    success = true,
                    message = "Test user exists",
                    userId = testUser.Id,
                    username = testUser.UserName,
                    tokenBalance = testUser.TokenBalance
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // View Methods for Achievements and Streaks
        public async Task<IActionResult> ViewAllAchievements()
        {
            try
            {
                var userId = GetUserId();
                var allAchievements = await _achievementService.GetAllAchievementsAsync(userId);
                var achievementStats = await _achievementService.GetAchievementStatsAsync(userId);

                var viewModel = new GamificationDashboardViewModel
                {
                    AllAchievements = allAchievements,
                    Stats = new GamificationStatsViewModel
                    {
                        AchievementsEarned = (int)(achievementStats.ContainsKey("TotalEarned") ? achievementStats["TotalEarned"] : 0),
                        TotalAchievements = (int)(achievementStats.ContainsKey("TotalAvailable") ? achievementStats["TotalAvailable"] : 0)
                    }
                };

                return View("AllAchievements", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all achievements");
                TempData["ErrorMessage"] = "Error loading achievements";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ViewAllStreaks()
        {
            try
            {
                var userId = GetUserId();
                var userStreaks = await _streakService.GetUserStreaksAsync(userId);

                var viewModel = new GamificationDashboardViewModel
                {
                    ActiveStreaks = userStreaks,
                    Stats = new GamificationStatsViewModel
                    {
                        ActiveStreaksCount = userStreaks.Count(s => s.IsActive)
                    }
                };

                return View("AllStreaks", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all streaks");
                TempData["ErrorMessage"] = "Error loading streaks";
                return RedirectToAction("Index", "Home");
            }
        }

        // FIXED: Achievement testing methods using centralized service
        [HttpPost]
        public async Task<IActionResult> DirectUnlockAchievement(int userId = 1, int achievementId = 1)
        {
            try
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement == null)
                {
                    return Json(new { success = false, message = "Achievement not found" });
                }

                // Use centralized service for rewards
                var success = await _userStatisticsService.AwardTokensAndXpAsync(
                    userId,
                    achievement.TokenReward,
                    achievement.XpReward ?? 0,
                    $"Manual achievement unlock: {achievement.Name}");

                if (success)
                {
                    // Update achievement record
                    var userAchievement = await _context.UserAchievements
                        .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

                    if (userAchievement != null)
                    {
                        userAchievement.IsUnlocked = true;
                        userAchievement.EarnedDate = DateTime.UtcNow;
                        userAchievement.Progress = achievement.CriteriaValue;
                        userAchievement.IsNotified = false;

                        await _context.SaveChangesAsync();
                    }

                    // Get updated stats
                    var updatedStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = $"Unlocked achievement: {achievement.Name}",
                        achievement = new
                        {
                            name = achievement.Name,
                            description = achievement.Description,
                            reward = $"{achievement.TokenReward}T + {achievement.XpReward}XP"
                        },
                        userStats = updatedStats
                    });
                }

                return Json(new { success = false, message = "Failed to award tokens and XP" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockMultipleTestAchievements(int userId = 1)
        {
            try
            {
                var results = new List<string>();
                var unlockedCount = 0;

                var itemCount = await _context.Items.CountAsync(i => i.UserId == userId);
                var initialStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                var eligibleAchievementIds = new List<int>();
                if (itemCount >= 25) eligibleAchievementIds.AddRange(new[] { 1, 2, 3, 4 });
                if (itemCount >= 75) eligibleAchievementIds.AddRange(new[] { 5, 6, 7, 8 });
                if (itemCount >= 150) eligibleAchievementIds.AddRange(new[] { 9, 10, 11, 12 });

                foreach (var achievementId in eligibleAchievementIds)
                {
                    var userAchievement = await _context.UserAchievements
                        .Include(ua => ua.Achievement)
                        .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

                    if (userAchievement != null && !userAchievement.IsUnlocked)
                    {
                        // Use centralized service for rewards
                        var success = await _userStatisticsService.AwardTokensAndXpAsync(
                            userId,
                            userAchievement.Achievement.TokenReward,
                            userAchievement.Achievement.XpReward ?? 0,
                            $"Achievement: {userAchievement.Achievement.Name}");

                        if (success)
                        {
                            userAchievement.IsUnlocked = true;
                            userAchievement.EarnedDate = DateTime.UtcNow;
                            userAchievement.Progress = itemCount;
                            userAchievement.IsNotified = false;

                            results.Add($"Unlocked: {userAchievement.Achievement.Name} (+{userAchievement.Achievement.TokenReward}T, +{userAchievement.Achievement.XpReward}XP)");
                            unlockedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var finalStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                results.Add($"Total unlocked: {unlockedCount} achievements");
                results.Add($"Token balance: {initialStats.TokenBalance} → {finalStats.TokenBalance}");
                results.Add($"XP: {initialStats.CurrentXp} → {finalStats.CurrentXp}");
                results.Add($"Level: {initialStats.CurrentLevel} → {finalStats.CurrentLevel}");

                return Json(new
                {
                    success = true,
                    results = results,
                    unlockedCount = unlockedCount,
                    initialStats = initialStats,
                    finalStats = finalStats
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // FIXED: Balance verification using centralized service
        [HttpPost]
        public async Task<IActionResult> VerifyUserBalances(int userId = 1)
        {
            try
            {
                // Use centralized service for consistent stats
                var userStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                // Get raw database values for comparison
                var user = await _context.Users.FindAsync(userId);
                var rawUserStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);
                var rawUserLevel = await _context.UserLevels.FirstOrDefaultAsync(ul => ul.UserId == userId);

                var achievementsEarned = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == userId && ua.IsUnlocked)
                    .ToListAsync();

                var totalAchievementTokens = achievementsEarned.Sum(ua => ua.Achievement.TokenReward);
                var totalAchievementXp = achievementsEarned.Sum(ua => ua.Achievement.XpReward ?? 0);

                return Json(new
                {
                    success = true,
                    centralizedStats = userStats,
                    rawData = new
                    {
                        appUserTokens = user?.TokenBalance ?? 0,
                        userStatsTokens = rawUserStats?.TokenBalance ?? 0,
                        userStatsXp = rawUserStats?.CurrentXp ?? 0,
                        userLevelXp = rawUserLevel?.CurrentXp ?? 0
                    },
                    achievements = new
                    {
                        count = achievementsEarned.Count,
                        expectedTokens = totalAchievementTokens,
                        expectedXp = totalAchievementXp
                    },
                    consistency = new
                    {
                        tokensMatch = (user?.TokenBalance ?? 0) == (rawUserStats?.TokenBalance ?? 0),
                        xpMatches = (rawUserStats?.CurrentXp ?? 0) == (rawUserLevel?.CurrentXp ?? 0)
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TriggerQuestProgress(string actionType, int value = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _questService.TriggerQuestProgressByActionAsync(userId, actionType, value);

                if (result)
                {
                    return Json(new { success = true, message = $"Quest progress triggered for action: {actionType}" });
                }
                else
                {
                    return Json(new { success = false, message = "No matching quests found for this action" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error triggering quest progress for action {actionType}");
                return Json(new { success = false, message = "An error occurred while updating quest progress" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestUserStatisticsService()
        {
            try
            {
                var userId = GetCurrentUserId();
                var results = new List<string>();

                // Test getting user stats
                var stats = await _userStatisticsService.GetUserStatisticsAsync(userId);
                results.Add($"Current stats: Level {stats.CurrentLevel}, {stats.CurrentXp} XP, {stats.TokenBalance} tokens");

                // Test awarding tokens and XP
                var success = await _userStatisticsService.AwardTokensAndXpAsync(userId, 10m, 25, "UserStatisticsService test");
                results.Add($"Award test: {(success ? "Success" : "Failed")}");

                if (success)
                {
                    var newStats = await _userStatisticsService.GetUserStatisticsAsync(userId);
                    results.Add($"New stats: Level {newStats.CurrentLevel}, {newStats.CurrentXp} XP, {newStats.TokenBalance} tokens");
                    results.Add($"Changes: +{newStats.TokenBalance - stats.TokenBalance} tokens, +{newStats.CurrentXp - stats.CurrentXp} XP");
                }

                return Json(new
                {
                    success = true,
                    message = "UserStatisticsService test completed",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugWeeklyQuestGeneration()
        {
            try
            {
                var userId = GetUserId();
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                // Check existing weekly quests
                var existingWeekly = await _context.Quests
                    .Where(q => q.QuestType == Models.Entities.Gamification.QuestType.Weekly)
                    .ToListAsync();

                // Force create a weekly quest for testing
                var weeklyQuest = new Models.Entities.Gamification.Quest
                {
                    QuestTitle = "Debug Weekly Quest",
                    QuestType = Models.Entities.Gamification.QuestType.Weekly,
                    QuestDescription = "Test weekly quest",
                    QuestObjective = "Complete 3 test actions",
                    TokenReward = 25.00m,
                    XpReward = 50,
                    Difficulty = Models.Entities.Gamification.QuestDifficulty.Medium,
                    TargetValue = 3,
                    IsActive = true,
                    StartDate = startOfWeek,
                    EndDate = endOfWeek,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Quests.Add(weeklyQuest);
                await _context.SaveChangesAsync();

                // Assign to test user
                var userQuest = new Models.Entities.Gamification.UserQuest
                {
                    UserId = userId,
                    QuestId = weeklyQuest.QuestId,
                    CurrentProgress = 0,
                    IsCompleted = false,
                    StartedAt = DateTime.UtcNow
                };

                _context.UserQuests.Add(userQuest);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Weekly quest created",
                    weeklyQuestId = weeklyQuest.QuestId,
                    existingWeeklyCount = existingWeekly.Count,
                    startOfWeek = startOfWeek,
                    endOfWeek = endOfWeek
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugQuestStatus()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                var dailyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily && q.IsActive)
                    .CountAsync();

                var weeklyQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Weekly && q.IsActive)
                    .CountAsync();

                var specialQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Special && q.IsActive)
                    .CountAsync();

                var todaysQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily &&
                               q.StartDate.HasValue &&
                               q.StartDate.Value.Date == today)
                    .CountAsync();

                var thisWeeksQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Weekly &&
                               q.StartDate >= startOfWeek &&
                               q.StartDate < endOfWeek)
                    .CountAsync();

                return Json(new
                {
                    success = true,
                    timestamp = DateTime.UtcNow,
                    questCounts = new
                    {
                        totalDaily = dailyQuests,
                        totalWeekly = weeklyQuests,
                        totalSpecial = specialQuests,
                        todaysDaily = todaysQuests,
                        thisWeeksWeekly = thisWeeksQuests
                    },
                    dates = new
                    {
                        today = today.ToString("yyyy-MM-dd"),
                        startOfWeek = startOfWeek.ToString("yyyy-MM-dd"),
                        endOfWeek = endOfWeek.ToString("yyyy-MM-dd")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForceGenerateQuests()
        {
            try
            {
                await _questService.GenerateDailyQuestsAsync();
                await _questService.GenerateWeeklyQuestsAsync();

                return Json(new
                {
                    success = true,
                    message = "Quest generation triggered",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestKonMariQuests()
        {
            try
            {
                await _questService.GenerateDailyQuestsAsync();
                await _questService.GenerateWeeklyQuestsAsync();

                return Json(new
                {
                    success = true,
                    message = "KonMari quests generated!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugGenerateSpecialQuest()
        {
            try
            {
                await _questService.GenerateSpecialQuestAsync();

                // Get the generated quest info for response
                var specialQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Special && q.IsActive)
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(1)
                    .Select(q => new
                    {
                        q.QuestId,
                        q.QuestTitle,
                        q.StartDate,
                        q.EndDate,
                        q.TokenReward,
                        q.XpReward,
                        q.Difficulty
                    })
                    .FirstOrDefaultAsync();

                return Json(new
                {
                    success = true,
                    message = "Special quest generated successfully using KonMari templates",
                    quest = specialQuests,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugSpecialQuestStatus()
        {
            try
            {
                var userId = GetCurrentUserId();
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                var specialQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Special && q.IsActive)
                    .Select(q => new
                    {
                        q.QuestId,
                        q.QuestTitle,
                        q.StartDate,
                        q.EndDate,
                        q.TokenReward,
                        q.XpReward,
                        q.Difficulty,
                        q.CreatedAt
                    })
                    .ToListAsync();

                var thisMonthQuests = specialQuests
                    .Where(q => q.StartDate.HasValue &&
                               q.StartDate.Value.Month == currentMonth &&
                               q.StartDate.Value.Year == currentYear)
                    .ToList();

                return Json(new
                {
                    success = true,
                    activeSpecialQuests = specialQuests,
                    thisMonthQuests = thisMonthQuests,
                    totalActive = specialQuests.Count,
                    thisMonthCount = thisMonthQuests.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // FIXED: Reconcile stats using centralized service
        [HttpPost]
        public async Task<IActionResult> ReconcileUserStats(int? userId = null)
        {
            try
            {
                var targetUserId = userId ?? GetCurrentUserId();
                var results = new List<string>();

                // Get current state
                var initialStats = await _userStatisticsService.GetUserStatisticsAsync(targetUserId);
                results.Add($"Initial stats: Level {initialStats.CurrentLevel}, {initialStats.CurrentXp} XP, {initialStats.TokenBalance} tokens");

                // Calculate expected values from achievements and quest rewards
                var claimedQuestTokens = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == targetUserId && uq.DateClaimed != null)
                    .SumAsync(uq => uq.Quest.TokenReward);

                var claimedQuestXp = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == targetUserId && uq.DateClaimed != null)
                    .SumAsync(uq => uq.Quest.XpReward);

                var achievementTokens = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == targetUserId && ua.IsUnlocked)
                    .SumAsync(ua => ua.Achievement.TokenReward);

                var achievementXp = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == targetUserId && ua.IsUnlocked)
                    .SumAsync(ua => ua.Achievement.XpReward ?? 0);

                var checkInTokens = await _context.CheckIns
                    .Where(c => c.UserId == targetUserId)
                    .SumAsync(c => c.TokensEarned);

                var expectedTokens = claimedQuestTokens + achievementTokens + checkInTokens + 100; // Starting balance
                var expectedXp = claimedQuestXp + achievementXp;

                results.Add($"Expected tokens: {expectedTokens} (quest: {claimedQuestTokens}, achievements: {achievementTokens}, check-ins: {checkInTokens}, base: 100)");
                results.Add($"Expected XP: {expectedXp} (quest: {claimedQuestXp}, achievements: {achievementXp})");

                // Use centralized service to rebuild if needed
                var difference = Math.Abs(expectedTokens - initialStats.TokenBalance);
                if (difference > 0.01m) // Small tolerance for decimal precision
                {
                    // Award the difference
                    var tokenDiff = expectedTokens - initialStats.TokenBalance;
                    var xpDiff = expectedXp - initialStats.CurrentXp;

                    if (tokenDiff != 0 || xpDiff != 0)
                    {
                        var success = await _userStatisticsService.AwardTokensAndXpAsync(
                            targetUserId,
                            tokenDiff,
                            xpDiff,
                            "Stats reconciliation");

                        if (success)
                        {
                            results.Add($"Applied correction: {tokenDiff:F2} tokens, {xpDiff} XP");
                        }
                    }
                }

                // Get final state
                var finalStats = await _userStatisticsService.GetUserStatisticsAsync(targetUserId);
                results.Add($"Final stats: Level {finalStats.CurrentLevel}, {finalStats.CurrentXp} XP, {finalStats.TokenBalance} tokens");

                return Json(new
                {
                    success = true,
                    message = "User statistics reconciled using centralized service",
                    results = results,
                    changes = new
                    {
                        tokenChange = finalStats.TokenBalance - initialStats.TokenBalance,
                        xpChange = finalStats.CurrentXp - initialStats.CurrentXp,
                        levelChange = finalStats.CurrentLevel - initialStats.CurrentLevel
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reconciling user stats for user {userId}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestQuestTriggers()
        {
            try
            {
                var userId = GetCurrentUserId();
                var results = new List<object>();

                // Test different action types
                var actionTypes = new[] { "item_listed", "post_created", "comment_created", "check_in" };

                foreach (var actionType in actionTypes)
                {
                    var result = await _questService.TriggerQuestProgressByActionAsync(userId, actionType, 1);
                    results.Add(new
                    {
                        actionType,
                        success = result,
                        message = result ? "Quest progress triggered" : "No matching quests found"
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Quest trigger test completed",
                    results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userStats = await _userStatisticsService.GetUserStatisticsAsync(userId);

                // Check various system components
                var activeQuests = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId && !uq.IsCompleted && uq.Quest.IsActive)
                    .CountAsync();

                var totalQuests = await _context.Quests.CountAsync(q => q.IsActive);
                var user = await _context.Users.FindAsync(userId);
                var userLevel = await _context.UserLevels.FirstOrDefaultAsync(ul => ul.UserId == userId);

                return Json(new
                {
                    success = true,
                    health = new
                    {
                        questSystem = new
                        {
                            totalActiveQuests = totalQuests,
                            userActiveQuests = activeQuests,
                            dailyQuests = await _context.Quests.CountAsync(q => q.QuestType == QuestType.Daily && q.IsActive),
                            weeklyQuests = await _context.Quests.CountAsync(q => q.QuestType == QuestType.Weekly && q.IsActive),
                            specialQuests = await _context.Quests.CountAsync(q => q.QuestType == QuestType.Special && q.IsActive)
                        },
                        userProgress = new
                        {
                            level = userStats.CurrentLevel,
                            xp = userStats.CurrentXp,
                            tokens = userStats.TokenBalance,
                            streak = await _context.UserStreaks
                                .Where(us => us.UserId == userId)
                                .MaxAsync(us => (int?)us.CurrentStreak) ?? 0,
                            completedQuests = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted)
                        },
                        dataConsistency = new
                        {
                            userTokensMatch = user?.TokenBalance == userStats.TokenBalance,
                            userStatsExists = userStats != null,
                            userLevelExists = userLevel != null,
                            centralizedServiceWorking = true
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestQuestAchievementIntegration()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Test 1: Check current quest progress
                var activeQuests = await _questService.GetActiveQuestsForUserAsync(userId);
                results.Add($"Active quests: {activeQuests.Count}");

                // Test 2: Test item listing achievement trigger
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "item_listed", 1);
                results.Add("Triggered item_listed achievement check");

                // Test 3: Test category-specific achievement
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "items_listed_books_stationery", 1);
                results.Add("Triggered books_stationery category achievement check");

                // Test 4: Test quest completion achievement
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_completed", 1);
                results.Add("Triggered quest_completed achievement check");

                // Test 5: Test community achievement
                await _achievementService.CheckAndUnlockAchievementsAsync(userId, "post_created", 1);
                results.Add("Triggered post_created achievement check");

                // Test 6: Get current achievement progress
                var allAchievements = await _achievementService.GetAllAchievementsAsync(userId);
                var earnedCount = allAchievements.Count(a => a.IsEarned);
                results.Add($"Achievements earned: {earnedCount}/{allAchievements.Count}");

                // Test 7: Check category mastery achievements
                var categoryAchievements = await _achievementService.GetCategoryMasteryAchievementsAsync(userId);
                var categoryEarned = categoryAchievements.Count(a => a.IsEarned);
                results.Add($"Category mastery achievements: {categoryEarned}/{categoryAchievements.Count}");

                return Json(new
                {
                    success = true,
                    message = "Quest-Achievement integration test completed",
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing quest-achievement integration");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TestRealAchievementProgress()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Step 1: Check current progress
                var stats = await _achievementService.GetAchievementStatsAsync(userId);
                results.Add($"Starting achievements: {stats["TotalEarned"]}/{stats["TotalAvailable"]}");

                // Step 2: Simulate completing a quest (if any available)
                var activeQuests = await _questService.GetActiveQuestsForUserAsync(userId);
                if (activeQuests.Any())
                {
                    var firstQuest = activeQuests.First();
                    var success = await _questService.CompleteQuestWithAchievementCheckAsync(userId, firstQuest.QuestId);
                    results.Add($"Quest completion attempt: {(success ? "Success" : "Failed")} - {firstQuest.QuestTitle}");
                }

                // Step 3: Check progress on specific achievable goals
                var itemCount = await _context.Items.CountAsync(i => i.UserId == userId);
                results.Add($"User has {itemCount} items listed");

                var questCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"User has {questCount} quests completed");

                var postCount = await _context.Posts.CountAsync(p => p.AuthorId == userId);
                results.Add($"User has {postCount} posts created");

                // Step 4: Check if any achievements should be unlocked based on current progress
                var unlockedAchievements = new List<AchievementDto>();

                // Check basic achievements
                if (itemCount >= 25)
                {
                    var itemAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_items_listed", 1);
                    unlockedAchievements.AddRange(itemAchievements);
                }

                if (questCount >= 10)
                {
                    var questAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_quests_completed", 1);
                    unlockedAchievements.AddRange(questAchievements);
                }

                results.Add($"New achievements unlocked: {unlockedAchievements.Count}");

                // Step 5: Get updated stats
                var newStats = await _achievementService.GetAchievementStatsAsync(userId);
                results.Add($"Final achievements: {newStats["TotalEarned"]}/{newStats["TotalAvailable"]}");

                // Step 6: Get specific achievement progress
                var progressiveAchievements = await _achievementService.GetProgressiveAchievementsAsync(userId, "total_items_listed");
                var nextItemAchievement = progressiveAchievements.FirstOrDefault(a => !a.IsEarned);
                if (nextItemAchievement != null)
                {
                    results.Add($"Next item achievement: {nextItemAchievement.Name} ({itemCount}/{nextItemAchievement.CriteriaValue})");
                }

                return Json(new
                {
                    success = true,
                    message = "Real achievement progress test completed",
                    results = results,
                    unlockedCount = unlockedAchievements.Count,
                    unlockedAchievements = unlockedAchievements.Select(a => a.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing real achievement progress");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SeedTestItemsAndCheckAchievements()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Get all categories for testing
                var categories = await _context.ItemCategories.Where(c => c.IsActive).ToListAsync();
                var conditions = await _context.ItemConditions.Where(c => c.IsActive).ToListAsync();
                var locations = await _context.ItemLocations.Where(l => l.IsActive).ToListAsync();

                if (!categories.Any() || !conditions.Any() || !locations.Any())
                {
                    return Json(new { success = false, message = "Missing required data: categories, conditions, or locations" });
                }

                var defaultCondition = conditions.First();
                var defaultLocation = locations.First();

                // Seed items across different categories to test category-specific achievements
                var testItems = new List<Item>();
                var itemsPerCategory = 6; // Will create enough to test Bronze level achievements

                foreach (var category in categories.Take(5)) // Test with first 5 categories
                {
                    for (int i = 1; i <= itemsPerCategory; i++)
                    {
                        var item = new Item
                        {
                            UserId = userId,
                            CategoryId = category.CategoryId,
                            ConditionId = defaultCondition.ConditionId,
                            LocationId = defaultLocation.LocationId,
                            ItemTitle = $"Test {category.Name} Item {i}",
                            Description = $"This is a test item for {category.Name} category to test achievements. Item contains joy and brings gratitude.",
                            AdjustedTokenPrice = 10.00m,
                            FinalTokenPrice = 10.00m,
                            Status = ItemStatus.Available,
                            DatePosted = DateTime.UtcNow.AddDays(-i), // Vary dates slightly
                            AiProcessingStatus = AiProcessingStatus.Completed
                        };
                        testItems.Add(item);
                    }
                }

                // Add items to database
                _context.Items.AddRange(testItems);
                await _context.SaveChangesAsync();

                results.Add($"Seeded {testItems.Count} test items across {categories.Take(5).Count()} categories");

                // Now trigger achievement checks for item listing
                var unlockedAchievements = new List<AchievementDto>();

                // Check general item listing achievements
                var itemAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "item_listed", testItems.Count);
                unlockedAchievements.AddRange(itemAchievements);

                // Check category-specific achievements
                foreach (var category in categories.Take(5))
                {
                    var categoryKey = MapCategoryNameToKey(category.Name);
                    var categoryAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, $"items_listed_{categoryKey}", itemsPerCategory);
                    unlockedAchievements.AddRange(categoryAchievements);
                }

                // Check progressive achievements
                var progressiveAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_items_listed", 1);
                unlockedAchievements.AddRange(progressiveAchievements);

                // Check for multi-category session achievements
                var crossCategoryAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "multi_category_sessions", 1);
                unlockedAchievements.AddRange(crossCategoryAchievements);

                // Check methodology achievements (joy and gratitude mentions in descriptions)
                var joyAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "joy_mentions", 1);
                unlockedAchievements.AddRange(joyAchievements);

                var gratitudeAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "gratitude_expressions", 1);
                unlockedAchievements.AddRange(gratitudeAchievements);

                results.Add($"Triggered achievement checks for {categories.Take(5).Count()} categories");
                results.Add($"Unlocked {unlockedAchievements.Count} achievements!");

                // Get updated stats
                var stats = await _achievementService.GetAchievementStatsAsync(userId);
                results.Add($"Total achievements: {stats["TotalEarned"]}/{stats["TotalAvailable"]}");

                return Json(new
                {
                    success = true,
                    message = "Test items seeded and achievements checked",
                    results = results,
                    unlockedAchievements = unlockedAchievements.Select(a => new
                    {
                        name = a.Name,
                        description = a.Description,
                        reward = $"{a.TokenReward}T + {a.XpReward}XP"
                    }).ToList(),
                    itemsCreated = testItems.Count,
                    categoriesTested = categories.Take(5).Select(c => c.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding test items and checking achievements");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to map category names to achievement keys
        private string MapCategoryNameToKey(string categoryName)
        {
            return categoryName switch
            {
                "Books & Stationery" => "books_stationery",
                "Clothing & Accessories" => "clothing_accessories",
                "Electronics & Gadgets" => "electronics_gadgets",
                "Toys & Games" => "toys_games",
                "Home & Kitchen" => "home_kitchen",
                "Furniture" => "furniture",
                "Appliances" => "appliances",
                "Health & Beauty" => "health_beauty",
                "Crafts & DIY" => "crafts_diy",
                "School & Office" => "school_office",
                "Sentimental Items" => "sentimental",
                _ => "general"
            };
        }

        [HttpPost]
        public async Task<IActionResult> CheckDatabaseSetup()
        {
            try
            {
                var results = new List<string>();

                // Check categories
                var categories = await _context.ItemCategories.Where(c => c.IsActive).ToListAsync();
                results.Add($"Item Categories: {categories.Count} found");
                if (categories.Any())
                {
                    results.Add($"Categories: {string.Join(", ", categories.Take(5).Select(c => c.Name))}");
                }

                // Check conditions  
                var conditions = await _context.ItemConditions.Where(c => c.IsActive).ToListAsync();
                results.Add($"Item Conditions: {conditions.Count} found");
                if (conditions.Any())
                {
                    results.Add($"Conditions: {string.Join(", ", conditions.Take(5).Select(c => c.Name))}");
                }

                // Check locations
                var locations = await _context.ItemLocations.Where(l => l.IsActive).ToListAsync();
                results.Add($"Item Locations: {locations.Count} found");
                if (locations.Any())
                {
                    results.Add($"Locations: {string.Join(", ", locations.Take(5).Select(l => l.Name))}");
                }

                // Check if we can seed missing data
                var needsSeeding = !categories.Any() || !conditions.Any() || !locations.Any();

                return Json(new
                {
                    success = true,
                    results = results,
                    needsSeeding = needsSeeding,
                    categoriesCount = categories.Count,
                    conditionsCount = conditions.Count,
                    locationsCount = locations.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SeedRequiredItemData()
        {
            try
            {
                var results = new List<string>();

                // Seed ItemCategories if missing
                if (!await _context.ItemCategories.AnyAsync())
                {
                    var categories = new List<ItemCategory>
                    {
                        new ItemCategory { Name = "Books & Stationery", Description = "Books, notebooks, stationery items", IsActive = true, SortOrder = 1 },
                        new ItemCategory { Name = "Clothing & Accessories", Description = "Clothes, shoes, accessories", IsActive = true, SortOrder = 2 },
                        new ItemCategory { Name = "Electronics & Gadgets", Description = "Electronic devices and gadgets", IsActive = true, SortOrder = 3 },
                        new ItemCategory { Name = "Toys & Games", Description = "Toys, games, entertainment items", IsActive = true, SortOrder = 4 },
                        new ItemCategory { Name = "Home & Kitchen", Description = "Kitchen items and home goods", IsActive = true, SortOrder = 5 },
                        new ItemCategory { Name = "Furniture", Description = "Furniture and large home items", IsActive = true, SortOrder = 6 },
                        new ItemCategory { Name = "Appliances", Description = "Home appliances", IsActive = true, SortOrder = 7 },
                        new ItemCategory { Name = "Health & Beauty", Description = "Health and beauty products", IsActive = true, SortOrder = 8 },
                        new ItemCategory { Name = "Crafts & DIY", Description = "Craft supplies and DIY materials", IsActive = true, SortOrder = 9 },
                        new ItemCategory { Name = "School & Office", Description = "School and office supplies", IsActive = true, SortOrder = 10 },
                        new ItemCategory { Name = "Sentimental Items", Description = "Items with sentimental value", IsActive = true, SortOrder = 11 }
                    };

                    _context.ItemCategories.AddRange(categories);
                    await _context.SaveChangesAsync();
                    results.Add($"Seeded {categories.Count} item categories");
                }

                // Seed ItemConditions if missing
                if (!await _context.ItemConditions.AnyAsync())
                {
                    var conditions = new List<ItemCondition>
                    {
                        new ItemCondition { Name = "Excellent", Description = "Like new condition", ConditionMultiplier = 1.0m, IsActive = true },
                        new ItemCondition { Name = "Good", Description = "Minor wear, fully functional", ConditionMultiplier = 0.8m, IsActive = true },
                        new ItemCondition { Name = "Fair", Description = "Noticeable wear but functional", ConditionMultiplier = 0.6m, IsActive = true },
                        new ItemCondition { Name = "Poor", Description = "Heavy wear, may need repair", ConditionMultiplier = 0.4m, IsActive = true }
                    };

                    _context.ItemConditions.AddRange(conditions);
                    await _context.SaveChangesAsync();
                    results.Add($"Seeded {conditions.Count} item conditions");
                }

                // Seed ItemLocations if missing
                if (!await _context.ItemLocations.AnyAsync())
                {
                    var locations = new List<ItemLocation>
                    {
                        new ItemLocation { Name = "Metro Manila", Region = "NCR", IsActive = true },
                        new ItemLocation { Name = "Quezon City", Region = "NCR", IsActive = true },
                        new ItemLocation { Name = "Manila", Region = "NCR", IsActive = true },
                        new ItemLocation { Name = "Makati", Region = "NCR", IsActive = true },
                        new ItemLocation { Name = "Taguig", Region = "NCR", IsActive = true }
                    };

                    _context.ItemLocations.AddRange(locations);
                    await _context.SaveChangesAsync();
                    results.Add($"Seeded {locations.Count} item locations");
                }

                return Json(new
                {
                    success = true,
                    message = "Required item data seeded successfully",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ManuallyUnlockTestAchievements()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Get current item count
                var itemCount = await _context.Items.CountAsync(i => i.UserId == userId);
                results.Add($"User has {itemCount} items");

                // Manually check and unlock item-based achievements
                var unlockedAchievements = new List<AchievementDto>();

                // Force check the Bronze Item Lister achievement
                if (itemCount >= 25)
                {
                    var bronzeAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_items_listed", itemCount);
                    unlockedAchievements.AddRange(bronzeAchievements);
                }

                // Force check quest completion achievements
                var questCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                if (questCount >= 10)
                {
                    var questAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_quests_completed", questCount);
                    unlockedAchievements.AddRange(questAchievements);
                }

                results.Add($"Attempted to unlock achievements based on {itemCount} items and {questCount} quests");
                results.Add($"Unlocked {unlockedAchievements.Count} achievements");

                // Get updated stats
                var stats = await _achievementService.GetAchievementStatsAsync(userId);
                results.Add($"Updated achievement count: {stats["TotalEarned"]}/{stats["TotalAvailable"]}");

                return Json(new
                {
                    success = true,
                    message = "Manual achievement unlock test completed",
                    results = results,
                    unlockedAchievements = unlockedAchievements.Select(a => new
                    {
                        name = a.Name,
                        description = a.Description,
                        reward = $"{a.TokenReward}T + {a.XpReward}XP"
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // FIXED: Reset methods using centralized service
        [HttpPost]
        public async Task<IActionResult> ResetUserProgress(int userId = 1)
        {
            try
            {
                var results = new List<string>();

                // 1. Reset UserAchievements
                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToListAsync();

                foreach (var ua in userAchievements)
                {
                    ua.IsUnlocked = false;
                    ua.Progress = 0;
                    ua.EarnedDate = null;
                    ua.IsNotified = false;
                }
                results.Add($"Reset {userAchievements.Count} achievement records");

                // 2. Reset UserQuests
                var userQuests = await _context.UserQuests
                    .Where(uq => uq.UserId == userId)
                    .ToListAsync();

                foreach (var uq in userQuests)
                {
                    uq.IsCompleted = false;
                    uq.CurrentProgress = 0;
                    uq.DateClaimed = null;
                    uq.CompletedAt = null;
                    uq.Status = QuestStatus.Active;
                }
                results.Add($"Reset {userQuests.Count} quest records");

                // 3. Delete test items
                var testItems = await _context.Items
                    .Where(i => i.UserId == userId && i.ItemTitle.Contains("Test"))
                    .ToListAsync();

                _context.Items.RemoveRange(testItems);
                results.Add($"Deleted {testItems.Count} test items");

                // 4. Clear quest progress records
                var questProgressRecords = await _context.QuestProgresses
                    .Include(qp => qp.UserQuest)
                    .Where(qp => qp.UserQuest.UserId == userId)
                    .ToListAsync();

                _context.QuestProgresses.RemoveRange(questProgressRecords);
                results.Add($"Deleted {questProgressRecords.Count} quest progress records");

                // 5. Reset any posts/comments if they exist
                var testPosts = await _context.Posts
                    .Where(p => p.AuthorId == userId && p.PostContent.Contains("test"))
                    .ToListAsync();

                _context.Posts.RemoveRange(testPosts);
                results.Add($"Deleted {testPosts.Count} test posts");

                await _context.SaveChangesAsync();

                // 6. FIXED: Reset user stats using centralized service
                // Set to starting values
                var success = await _userStatisticsService.SetUserStatsAsync(userId, 100m, 0, "Progress reset");
                if (success)
                {
                    results.Add("Reset user stats to starting values using centralized service");
                }

                results.Add("All changes saved to database");

                return Json(new
                {
                    success = true,
                    message = "User progress reset completed using centralized service",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetAllTestData()
        {
            try
            {
                var results = new List<string>();

                // Reset for all test users (1, 2, 3)
                for (int userId = 1; userId <= 3; userId++)
                {
                    // Reset achievements
                    var userAchievements = await _context.UserAchievements
                        .Where(ua => ua.UserId == userId)
                        .ToListAsync();

                    foreach (var ua in userAchievements)
                    {
                        ua.IsUnlocked = false;
                        ua.Progress = 0;
                        ua.EarnedDate = null;
                        ua.IsNotified = false;
                    }

                    // Reset quests
                    var userQuests = await _context.UserQuests
                        .Where(uq => uq.UserId == userId)
                        .ToListAsync();

                    foreach (var uq in userQuests)
                    {
                        uq.IsCompleted = false;
                        uq.CurrentProgress = 0;
                        uq.DateClaimed = null;
                        uq.CompletedAt = null;
                        uq.Status = QuestStatus.Active;
                    }

                    // FIXED: Reset user stats using centralized service
                    await _userStatisticsService.SetUserStatsAsync(userId, 100m, 0, "Global reset");

                    results.Add($"Reset user {userId} using centralized service");
                }

                // Delete all test items
                var allTestItems = await _context.Items
                    .Where(i => i.ItemTitle.Contains("Test"))
                    .ToListAsync();

                _context.Items.RemoveRange(allTestItems);
                results.Add($"Deleted {allTestItems.Count} test items from all users");

                // Clear all quest progress
                var allQuestProgress = await _context.QuestProgresses.ToListAsync();
                _context.QuestProgresses.RemoveRange(allQuestProgress);
                results.Add($"Deleted {allQuestProgress.Count} quest progress records");

                await _context.SaveChangesAsync();
                results.Add("Complete reset finished using centralized service");

                return Json(new
                {
                    success = true,
                    message = "All test data reset completed using centralized service",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Add these methods to your QuestController for safe table debugging and reset

        [HttpPost]
        public async Task<IActionResult> SafeResetAllGamificationTables()
        {
            try
            {
                var results = new List<string>();

                using var transaction = await _context.Database.BeginTransactionAsync();

                // 1. Reset in dependency order (children first, then parents)

                // Delete QuestProgresses first
                var questProgresses = await _context.QuestProgresses.ToListAsync();
                _context.QuestProgresses.RemoveRange(questProgresses);
                results.Add($"Deleted {questProgresses.Count} QuestProgress records");

                // Delete UserQuests
                var userQuests = await _context.UserQuests.ToListAsync();
                _context.UserQuests.RemoveRange(userQuests);
                results.Add($"Deleted {userQuests.Count} UserQuest records");

                // Delete UserAchievements
                var userAchievements = await _context.UserAchievements.ToListAsync();
                _context.UserAchievements.RemoveRange(userAchievements);
                results.Add($"Deleted {userAchievements.Count} UserAchievement records");

                // Delete UserStreaks
                var userStreaks = await _context.UserStreaks.ToListAsync();
                _context.UserStreaks.RemoveRange(userStreaks);
                results.Add($"Deleted {userStreaks.Count} UserStreak records");

                // Delete CheckIns
                var checkIns = await _context.CheckIns.ToListAsync();
                _context.CheckIns.RemoveRange(checkIns);
                results.Add($"Deleted {checkIns.Count} CheckIn records");

                // Delete UserLevels
                var userLevels = await _context.UserLevels.ToListAsync();
                _context.UserLevels.RemoveRange(userLevels);
                results.Add($"Deleted {userLevels.Count} UserLevel records");

                // Delete UserStats
                var userStats = await _context.UserStats.ToListAsync();
                _context.UserStats.RemoveRange(userStats);
                results.Add($"Deleted {userStats.Count} UserStats records");

                // Reset AppUser gamification fields
                var users = await _context.Users.ToListAsync();
                foreach (var user in users)
                {
                    user.TokenBalance = 100m; // Reset to starting balance
                }
                results.Add($"Reset {users.Count} user token balances to 100");

                // Delete dynamic content (keep static reference data)
                var dynamicQuests = await _context.Quests
                    .Where(q => q.QuestType == QuestType.Daily || q.QuestType == QuestType.Weekly)
                    .ToListAsync();
                _context.Quests.RemoveRange(dynamicQuests);
                results.Add($"Deleted {dynamicQuests.Count} dynamic quests (Daily/Weekly)");

                // Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                results.Add("All gamification data reset successfully!");

                return Json(new
                {
                    success = true,
                    message = "All gamification tables reset safely",
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during safe reset");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetSpecificUser(int userId)
        {
            try
            {
                var results = new List<string>();

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Delete user-specific data only
                var userQuestProgresses = await _context.QuestProgresses
                    .Include(qp => qp.UserQuest)
                    .Where(qp => qp.UserQuest.UserId == userId)
                    .ToListAsync();
                _context.QuestProgresses.RemoveRange(userQuestProgresses);
                results.Add($"Deleted {userQuestProgresses.Count} quest progress records for user {userId}");

                var userQuests = await _context.UserQuests
                    .Where(uq => uq.UserId == userId)
                    .ToListAsync();
                _context.UserQuests.RemoveRange(userQuests);
                results.Add($"Deleted {userQuests.Count} user quest records");

                var userAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .ToListAsync();
                foreach (var ua in userAchievements)
                {
                    ua.IsUnlocked = false;
                    ua.Progress = 0;
                    ua.EarnedDate = null;
                    ua.IsNotified = false;
                }
                results.Add($"Reset {userAchievements.Count} achievement records");

                var userStreaks = await _context.UserStreaks
                    .Where(us => us.UserId == userId)
                    .ToListAsync();
                foreach (var us in userStreaks)
                {
                    us.CurrentStreak = 0;
                    us.LastActivityDate = null;
                    us.LongestStreak = 0;
                    us.TotalMilestonesReached = 0;
                    us.LastMilestoneDate = null;
                }
                results.Add($"Reset {userStreaks.Count} streak records");

                var checkIns = await _context.CheckIns
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                _context.CheckIns.RemoveRange(checkIns);
                results.Add($"Deleted {checkIns.Count} check-in records");

                var userLevel = await _context.UserLevels
                    .FirstOrDefaultAsync(ul => ul.UserId == userId);
                if (userLevel != null)
                {
                    _context.UserLevels.Remove(userLevel);
                    results.Add("Deleted user level record");
                }

                var userStat = await _context.UserStats
                    .FirstOrDefaultAsync(us => us.UserId == userId);
                if (userStat != null)
                {
                    _context.UserStats.Remove(userStat);
                    results.Add("Deleted user stats record");
                }

                // Reset user's token balance
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.TokenBalance = 100m;
                    results.Add("Reset user token balance to 100");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = $"User {userId} gamification data reset successfully",
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting user {userId} data");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DiagnoseTableState()
        {
            try
            {
                var diagnosis = new
                {
                    // Core counts
                    UserStats = await _context.UserStats.CountAsync(),
                    UserLevels = await _context.UserLevels.CountAsync(),
                    UserQuests = await _context.UserQuests.CountAsync(),
                    UserAchievements = await _context.UserAchievements.CountAsync(),
                    UserStreaks = await _context.UserStreaks.CountAsync(),
                    CheckIns = await _context.CheckIns.CountAsync(),
                    QuestProgresses = await _context.QuestProgresses.CountAsync(),

                    // Reference data counts
                    Achievements = await _context.Achievements.CountAsync(),
                    Levels = await _context.Levels.CountAsync(),
                    StreakTypes = await _context.StreakTypes.CountAsync(),

                    // Quest counts by type
                    DailyQuests = await _context.Quests.CountAsync(q => q.QuestType == QuestType.Daily),
                    WeeklyQuests = await _context.Quests.CountAsync(q => q.QuestType == QuestType.Weekly),
                    SpecialQuests = await _context.Quests.CountAsync(q => q.QuestType == QuestType.Special),

                    // User-specific data for test user
                    TestUser = new
                    {
                        TokenBalance = await _context.Users.Where(u => u.Id == 1).Select(u => u.TokenBalance).FirstOrDefaultAsync(),
                        HasUserStats = await _context.UserStats.AnyAsync(us => us.UserId == 1),
                        HasUserLevel = await _context.UserLevels.AnyAsync(ul => ul.UserId == 1),
                        ActiveQuests = await _context.UserQuests.CountAsync(uq => uq.UserId == 1 && !uq.IsCompleted),
                        CompletedQuests = await _context.UserQuests.CountAsync(uq => uq.UserId == 1 && uq.IsCompleted),
                        UnlockedAchievements = await _context.UserAchievements.CountAsync(ua => ua.UserId == 1 && ua.IsUnlocked),
                        CheckInStreak = await _context.UserStreaks.Where(us => us.UserId == 1).MaxAsync(us => (int?)us.CurrentStreak) ?? 0
                    },

                    // Data integrity checks
                    Integrity = new
                    {
                        OrphanedQuestProgresses = await _context.QuestProgresses
                            .Where(qp => qp.UserQuest == null)
                            .CountAsync(),

                        QuestsWithoutUserQuests = await _context.Quests
                            .Where(q => q.IsActive && !q.UserQuests.Any())
                            .CountAsync(),

                        AchievementsWithoutUserAchievements = await _context.Achievements
                            .Where(a => a.IsActive && !a.UserAchievements.Any())
                            .CountAsync(),

                        UsersWithoutStats = await _context.Users
                            .Where(u => !_context.UserStats.Any(us => us.UserId == u.Id))
                            .CountAsync()
                    }
                };

                return Json(new
                {
                    success = true,
                    message = "Table state diagnosed",
                    diagnosis = diagnosis,
                    recommendations = GetRecommendations(diagnosis)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<string> GetRecommendations(dynamic diagnosis)
        {
            var recommendations = new List<string>();

            if (diagnosis.TestUser.TokenBalance == 0)
                recommendations.Add("Reset test user token balance");

            if (!diagnosis.TestUser.HasUserStats)
                recommendations.Add("Create UserStats for test user");

            if (!diagnosis.TestUser.HasUserLevel)
                recommendations.Add("Create UserLevel for test user");

            if (diagnosis.DailyQuests == 0)
                recommendations.Add("Generate daily quests");

            if (diagnosis.Achievements == 0)
                recommendations.Add("Seed achievement data");

            if (diagnosis.Levels == 0)
                recommendations.Add("Seed level data");

            if (diagnosis.Integrity.OrphanedQuestProgresses > 0)
                recommendations.Add("Clean up orphaned quest progress records");

            if (diagnosis.Integrity.UsersWithoutStats > 0)
                recommendations.Add("Create missing UserStats records");

            return recommendations;
        }

        [HttpPost]
        public async Task<IActionResult> SeedReferenceData()
        {
            try
            {
                var results = new List<string>();

                // Seed Levels if missing
                if (!await _context.Levels.AnyAsync())
                {
                    var levels = new List<Level>
            {
                new Level { LevelNumber = 1, LevelName = "Beginner", XpRequired = 0, XpToNext = 100, TokenBonus = 10m },
                new Level { LevelNumber = 2, LevelName = "Organizer", XpRequired = 100, XpToNext = 150, TokenBonus = 15m },
                new Level { LevelNumber = 3, LevelName = "Declutter Pro", XpRequired = 250, XpToNext = 200, TokenBonus = 20m },
                new Level { LevelNumber = 4, LevelName = "Tidy Master", XpRequired = 450, XpToNext = 250, TokenBonus = 25m },
                new Level { LevelNumber = 5, LevelName = "Zen Minimalist", XpRequired = 700, XpToNext = 300, TokenBonus = 30m }
            };
                    _context.Levels.AddRange(levels);
                    results.Add($"Seeded {levels.Count} levels");
                }

                // Seed StreakTypes if missing
                if (!await _context.StreakTypes.AnyAsync())
                {
                    var streakTypes = new List<StreakType>
            {
                new StreakType { Name = "Daily Check-in", Description = "Check in every day", StreakUnit = StreakUnit.Days, BaseRewards = 5m, MilestoneRewards = 10m, MilestoneInterval = 7 },
                new StreakType { Name = "Listing Streak", Description = "List items consistently", StreakUnit = StreakUnit.Items, BaseRewards = 2m, MilestoneRewards = 20m, MilestoneInterval = 10 },
                new StreakType { Name = "Community Streak", Description = "Participate in community", StreakUnit = StreakUnit.Days, BaseRewards = 3m, MilestoneRewards = 15m, MilestoneInterval = 5 }
            };
                    _context.StreakTypes.AddRange(streakTypes);
                    results.Add($"Seeded {streakTypes.Count} streak types");
                }

                // Seed basic Achievements if missing
                if (!await _context.Achievements.AnyAsync())
                {
                    var achievements = new List<Achievement>
                    {
                        new Achievement { Name = "First Steps", Description = "List your first item", Category = AchievementCategory.Decluttering, CriteriaType = "total_items_listed", CriteriaValue = 1, TokenReward = 10m, XpReward = 25, Rarity = AchievementRarity.Bronze },
                        new Achievement { Name = "Getting Started", Description = "List 5 items", Category = AchievementCategory.Decluttering, CriteriaType = "total_items_listed", CriteriaValue = 5, TokenReward = 25m, XpReward = 50, Rarity = AchievementRarity.Bronze },
                        new Achievement { Name = "Item Lister", Description = "List 25 items", Category = AchievementCategory.Decluttering, CriteriaType = "total_items_listed", CriteriaValue = 25, TokenReward = 50m, XpReward = 100, Rarity = AchievementRarity.Silver },
                        new Achievement { Name = "Quest Starter", Description = "Complete your first quest", Category = AchievementCategory.Exploration, CriteriaType = "total_quests_completed", CriteriaValue = 1, TokenReward = 15m, XpReward = 30, Rarity = AchievementRarity.Bronze },
                        new Achievement { Name = "Community Helper", Description = "Create your first post", Category = AchievementCategory.Community, CriteriaType = "posts_created", CriteriaValue = 1, TokenReward = 20m, XpReward = 40, Rarity = AchievementRarity.Bronze },

                        new Achievement { Name = "Quest Explorer", Description = "Complete 5 quests", Category = AchievementCategory.Exploration, CriteriaType = "total_quests_completed", CriteriaValue = 5, TokenReward = 50m, XpReward = 100, Rarity = AchievementRarity.Silver },
                        new Achievement { Name = "Quest Master", Description = "Complete 10 quests", Category = AchievementCategory.Exploration, CriteriaType = "total_quests_completed", CriteriaValue = 10, TokenReward = 100m, XpReward = 200, Rarity = AchievementRarity.Gold },
                        new Achievement { Name = "Daily Warrior", Description = "Complete 3 daily quests", Category = AchievementCategory.Exploration, CriteriaType = "daily_quests_completed", CriteriaValue = 3, TokenReward = 40m, XpReward = 80, Rarity = AchievementRarity.Bronze },
                        new Achievement { Name = "Weekly Champion", Description = "Complete 2 weekly quests", Category = AchievementCategory.Exploration, CriteriaType = "weekly_quests_completed", CriteriaValue = 2, TokenReward = 75m, XpReward = 150, Rarity = AchievementRarity.Silver }
                    };
                    _context.Achievements.AddRange(achievements);
                    results.Add($"Seeded {achievements.Count} achievements");
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Reference data seeded successfully",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PrepareTestEnvironment(int userId = 1)
        {
            try
            {
                var results = new List<string>();

                // Ensure user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = $"User {userId} not found" });
                }

                // Create UserStats if missing
                var userStats = await _userStatisticsService.GetOrCreateUserStatsAsync(userId);
                results.Add($"UserStats ready for user {userId}");

                // Create UserLevel if missing
                var userLevel = await _context.UserLevels.FirstOrDefaultAsync(ul => ul.UserId == userId);
                if (userLevel == null)
                {
                    var firstLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == 1);
                    if (firstLevel != null)
                    {
                        userLevel = new UserLevel
                        {
                            UserId = userId,
                            CurrentLevelId = firstLevel.LevelId,
                            CurrentXp = 0,
                            TotalXp = 0,
                            XpToNextLevel = firstLevel.XpToNext
                        };
                        _context.UserLevels.Add(userLevel);
                        await _context.SaveChangesAsync();
                        results.Add("Created UserLevel record");
                    }
                }

                // Create UserAchievements if missing
                var existingAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId)
                    .Select(ua => ua.AchievementId)
                    .ToListAsync();

                var allAchievements = await _context.Achievements
                    .Where(a => a.IsActive)
                    .ToListAsync();

                var missingAchievements = allAchievements
                    .Where(a => !existingAchievements.Contains(a.AchievementId))
                    .ToList();

                foreach (var achievement in missingAchievements)
                {
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.AchievementId,
                        IsUnlocked = false,
                        Progress = 0,
                        EarnedDate = null,
                        IsNotified = false
                    });
                }

                if (missingAchievements.Any())
                {
                    await _context.SaveChangesAsync();
                    results.Add($"Created {missingAchievements.Count} UserAchievement records");
                }

                // Create UserStreaks if missing
                var existingStreaks = await _context.UserStreaks
                    .Where(us => us.UserId == userId)
                    .Select(us => us.StreakTypeId)
                    .ToListAsync();

                var allStreakTypes = await _context.StreakTypes
                    .Where(st => st.IsActive)
                    .ToListAsync();

                var missingStreaks = allStreakTypes
                    .Where(st => !existingStreaks.Contains(st.StreakTypeId))
                    .ToList();

                foreach (var streakType in missingStreaks)
                {
                    _context.UserStreaks.Add(new UserStreak
                    {
                        UserId = userId,
                        StreakTypeId = streakType.StreakTypeId,
                        CurrentStreak = 0,
                        LongestStreak = 0,
                        LastActivityDate = null,
                        TotalMilestonesReached = 0,
                        LastMilestoneDate = null
                    });
                }

                if (missingStreaks.Any())
                {
                    await _context.SaveChangesAsync();
                    results.Add($"Created {missingStreaks.Count} UserStreak records");
                }

                results.Add("Test environment prepared successfully!");

                return Json(new
                {
                    success = true,
                    message = $"Test environment prepared for user {userId}",
                    results = results
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Add to QuestController.cs
        [HttpPost]
        public async Task<IActionResult> SimulateCheckIn()
        {
            try
            {
                var userId = GetUserId();

                // Update quest progress
                var questResult = await _questService.TriggerQuestProgressByActionAsync(userId, "check_in", 1);

                // Check and unlock achievements
                var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "check_in", 1);

                return Json(new
                {
                    success = questResult || unlockedAchievements.Any(),
                    message = BuildResponseMessage("Check-in completed", questResult, unlockedAchievements),
                    action = "check_in",
                    questUpdated = questResult,
                    achievementsUnlocked = unlockedAchievements.Count,
                    achievements = unlockedAchievements.Select(a => new { a.Name, a.TokenReward, a.XpReward }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SimulateItemListing(int count = 1)
        {
            try
            {
                var userId = GetUserId();

                // Update quest progress
                var questResult = await _questService.TriggerQuestProgressByActionAsync(userId, "item_listed", count);

                // Check and unlock achievements - DECLARE FIRST
                var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "item_listed", count);

                // THEN add quest completion achievements
                var completedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_quests_completed", 1);
                unlockedAchievements.AddRange(completedAchievements);

                return Json(new
                {
                    success = questResult || unlockedAchievements.Any(),
                    message = BuildResponseMessage($"Listed {count} items", questResult, unlockedAchievements),
                    action = "item_listed",
                    count = count,
                    questUpdated = questResult,
                    achievementsUnlocked = unlockedAchievements.Count,
                    achievements = unlockedAchievements.Select(a => new { a.Name, a.TokenReward, a.XpReward }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SimulatePostCreation()
        {
            try
            {
                var userId = GetUserId();

                // Update quest progress
                var questResult = await _questService.TriggerQuestProgressByActionAsync(userId, "post_created", 1);

                // Check and unlock achievements
                var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "post_created", 1);

                return Json(new
                {
                    success = questResult || unlockedAchievements.Any(),
                    message = BuildResponseMessage("Post created", questResult, unlockedAchievements),
                    action = "post_created",
                    questUpdated = questResult,
                    achievementsUnlocked = unlockedAchievements.Count,
                    achievements = unlockedAchievements.Select(a => new { a.Name, a.TokenReward, a.XpReward }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SimulateCommentCreation()
        {
            try
            {
                var userId = GetUserId();

                // Update quest progress
                var questResult = await _questService.TriggerQuestProgressByActionAsync(userId, "comment_created", 1);

                // Check and unlock achievements
                var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "comment_created", 1);

                return Json(new
                {
                    success = questResult || unlockedAchievements.Any(),
                    message = BuildResponseMessage("Comment created", questResult, unlockedAchievements),
                    action = "comment_created",
                    questUpdated = questResult,
                    achievementsUnlocked = unlockedAchievements.Count,
                    achievements = unlockedAchievements.Select(a => new { a.Name, a.TokenReward, a.XpReward }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to build consistent response messages
        private string BuildResponseMessage(string action, bool questResult, List<AchievementDto> achievements)
        {
            var message = action + "! ";

            if (questResult)
                message += "Quest progress updated. ";

            if (achievements.Any())
                message += $"Unlocked {achievements.Count} achievement{(achievements.Count > 1 ? "s" : "")}!";
            else if (!questResult)
                message += "No progress updates.";

            return message.TrimEnd();
        }

        [HttpPost]
        public async Task<IActionResult> DebugQuestAchievement()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // 1. Check current quest completion count
                var questCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"User has {questCount} completed quests");

                // 2. Check if Quest Starter achievement exists
                var questStarterAchievement = await _context.Achievements
                    .FirstOrDefaultAsync(a => a.CriteriaType == "total_quests_completed" && a.CriteriaValue == 1);
                results.Add($"Quest Starter achievement exists: {questStarterAchievement != null}");

                if (questStarterAchievement != null)
                {
                    results.Add($"Achievement details: {questStarterAchievement.Name} - Requires {questStarterAchievement.CriteriaValue} quests");

                    // 3. Check if user already has this achievement (fixed LINQ query)
                    var userHasAchievement = await _context.UserAchievements
                        .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == questStarterAchievement.AchievementId && ua.IsUnlocked);
                    results.Add($"User already has Quest Starter: {userHasAchievement}");
                }
                else
                {
                    results.Add("No Quest Starter achievement found in database!");
                }

                // 4. Force trigger achievement check
                var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_completed", 1);
                results.Add($"Achievement check returned {unlockedAchievements.Count} unlocked achievements");

                foreach (var achievement in unlockedAchievements)
                {
                    results.Add($"Unlocked: {achievement.Name} - {achievement.Description}");
                }

                return Json(new
                {
                    success = true,
                    results = results,
                    questCount = questCount,
                    hasQuestStarterAchievement = questStarterAchievement != null,
                    unlockedCount = unlockedAchievements.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddFirstQuestAchievement()
        {
            try
            {
                // Check if it already exists
                var exists = await _context.Achievements
                    .AnyAsync(a => a.CriteriaType == "total_quests_completed" && a.CriteriaValue == 1);

                if (!exists)
                {
                    var firstQuestAchievement = new Achievement
                    {
                        Name = "Quest Beginner",
                        Description = "Complete your first quest",
                        Category = AchievementCategory.Exploration,
                        CriteriaType = "total_quests_completed",
                        CriteriaValue = 1,
                        TokenReward = 15m,
                        XpReward = 30,
                        Rarity = AchievementRarity.Bronze,
                        IsActive = true,
                        IsSecret = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Achievements.Add(firstQuestAchievement);
                    await _context.SaveChangesAsync();

                    // Now test if it unlocks
                    var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(1, "quest_completed", 1);

                    return Json(new
                    {
                        success = true,
                        message = "Added First Quest achievement",
                        unlockedCount = unlockedAchievements.Count,
                        unlockedAchievements = unlockedAchievements.Select(a => a.Name).ToList()
                    });
                }
                else
                {
                    return Json(new { success = false, message = "First quest achievement already exists" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugUserAchievementSetup()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Check if achievements exist
                var questAchievements = await _context.Achievements
                    .Where(a => a.CriteriaType == "total_quests_completed")
                    .OrderBy(a => a.CriteriaValue)
                    .ToListAsync();
                results.Add($"Found {questAchievements.Count} quest achievements in database");

                // Check if user has UserAchievement records
                var userAchievementCount = await _context.UserAchievements
                    .CountAsync(ua => ua.UserId == userId);
                results.Add($"User has {userAchievementCount} UserAchievement records");

                // Check specifically for quest achievements
                var userQuestAchievements = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == userId && ua.Achievement.CriteriaType == "total_quests_completed")
                    .ToListAsync();
                results.Add($"User has {userQuestAchievements.Count} quest-related UserAchievement records");

                // Check completed quests count
                var completedQuests = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"User has {completedQuests} completed quests");

                return Json(new
                {
                    success = true,
                    results = results,
                    questAchievements = questAchievements.Select(a => new { a.Name, a.CriteriaValue }).ToList(),
                    userQuestAchievements = userQuestAchievements.Select(ua => new
                    {
                        ua.Achievement.Name,
                        ua.Achievement.CriteriaValue,
                        ua.IsUnlocked,
                        ua.Progress
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAchievementRecords()
        {
            try
            {
                var userId = GetUserId();

                // Use the AchievementService method to seed user achievements
                await _achievementService.SeedUserAchievementsAsync(userId);

                // Now test achievement unlocking
                var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "quest_completed", 1);

                return Json(new
                {
                    success = true,
                    message = "Created user achievement records",
                    unlockedCount = unlockedAchievements.Count,
                    unlockedAchievements = unlockedAchievements.Select(a => a.Name).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetQuestProgress()
        {
            try
            {
                var userId = GetUserId();
                var activeQuests = await _questService.GetActiveQuestsForUserAsync(userId);

                var questProgress = activeQuests.Select(q => new
                {
                    q.QuestId,
                    q.QuestTitle,
                    q.QuestObjective,
                    q.CurrentProgress,
                    q.TargetValue,
                    ProgressPercentage = q.TargetValue > 0 ? (q.CurrentProgress * 100 / q.TargetValue) : 0,
                    q.IsCompleted,
                    q.QuestType,
                    q.Difficulty
                }).ToList();

                return Json(new
                {
                    success = true,
                    totalQuests = activeQuests.Count,
                    completedQuests = activeQuests.Count(q => q.IsCompleted),
                    quests = questProgress
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetQuestDetails(int questId)
        {
            try
            {
                var userId = GetUserId();
                var quest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Include(uq => uq.QuestProgresses)
                    .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

                if (quest == null)
                    return Json(new { success = false, message = "Quest not found" });

                return Json(new
                {
                    success = true,
                    questId = quest.QuestId,
                    title = quest.Quest.QuestTitle,
                    description = quest.Quest.QuestDescription,
                    objective = quest.Quest.QuestObjective,
                    currentProgress = quest.CurrentProgress,
                    targetValue = quest.Quest.TargetValue,
                    isCompleted = quest.IsCompleted,
                    startedAt = quest.StartedAt,
                    completedAt = quest.CompletedAt,
                    progressHistory = quest.QuestProgresses.Select(qp => new
                    {
                        qp.ProgressValue,
                        qp.ActionType,
                        qp.ActionTimestamp
                    }).OrderBy(qp => qp.ActionTimestamp).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugAchievementFlow()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Check initial state
                var initialQuestCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"Initial completed quests: {initialQuestCount}");

                // Check if quest completion achievements exist
                var questAchievements = await _context.Achievements
                    .Where(a => a.CriteriaType == "total_quests_completed" && a.IsActive)
                    .OrderBy(a => a.CriteriaValue)
                    .ToListAsync();
                results.Add($"Quest achievements in DB: {questAchievements.Count}");

                foreach (var ach in questAchievements)
                {
                    results.Add($"  - {ach.Name}: requires {ach.CriteriaValue} quests");
                }

                // Check user achievement records
                var userQuestAchievements = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == userId && ua.Achievement.CriteriaType == "total_quests_completed")
                    .ToListAsync();
                results.Add($"User has {userQuestAchievements.Count} quest achievement records");

                foreach (var ua in userQuestAchievements)
                {
                    results.Add($"  - {ua.Achievement.Name}: unlocked={ua.IsUnlocked}, progress={ua.Progress}");
                }

                // Test the GetCurrentProgressAsync method directly
                var currentProgress = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"Direct quest count query: {currentProgress}");

                // Simulate item listing that should complete a quest
                results.Add("--- Triggering quest progress ---");
                var questResult = await _questService.TriggerQuestProgressByActionAsync(userId, "item_listed", 1);
                results.Add($"Quest trigger result: {questResult}");

                // Check quest count after
                var finalQuestCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"Final completed quests: {finalQuestCount}");
                results.Add($"Quest completion change: {finalQuestCount - initialQuestCount}");

                // Check if any quests were actually completed
                var recentlyCompleted = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId && uq.IsCompleted && uq.CompletedAt > DateTime.UtcNow.AddMinutes(-5))
                    .Select(uq => new { uq.Quest.QuestTitle, uq.CompletedAt })
                    .ToListAsync();
                results.Add($"Recently completed quests: {recentlyCompleted.Count}");
                foreach (var quest in recentlyCompleted)
                {
                    results.Add($"  - {quest.QuestTitle} at {quest.CompletedAt}");
                }

                // Manually trigger achievement check ONLY if a quest was completed
                if (finalQuestCount > initialQuestCount)
                {
                    results.Add("--- Triggering achievement check ---");
                    var unlockedAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "total_quests_completed", 1);
                    results.Add($"Manual achievement check unlocked: {unlockedAchievements.Count}");

                    foreach (var ach in unlockedAchievements)
                    {
                        results.Add($"  - Unlocked: {ach.Name}");
                    }
                }
                else
                {
                    results.Add("No quests were completed, skipping achievement check");
                }

                return Json(new { success = true, results = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowAllAchievements()
        {
            try
            {
                var achievements = await _context.Achievements
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.Category)
                    .ThenBy(a => a.CriteriaValue)
                    .Select(a => new
                    {
                        a.AchievementId,
                        a.Name,
                        a.Description,
                        a.CriteriaType,
                        a.CriteriaValue,
                        a.TokenReward,
                        a.XpReward,
                        a.Category
                    })
                    .ToListAsync();

                return Json(new { success = true, achievements = achievements });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateExistingAchievementProgress()
        {
            try
            {
                var userId = GetUserId();
                var completedQuests = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);

                // Update all quest-related achievement progress
                var questAchievements = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == userId && ua.Achievement.CriteriaType == "total_quests_completed")
                    .ToListAsync();

                var unlockedThisTime = new List<string>();

                foreach (var userAchievement in questAchievements)
                {
                    userAchievement.Progress = completedQuests;

                    // Check if should be unlocked
                    if (completedQuests >= userAchievement.Achievement.CriteriaValue && !userAchievement.IsUnlocked)
                    {
                        userAchievement.IsUnlocked = true;
                        userAchievement.EarnedDate = DateTime.UtcNow;
                        userAchievement.IsNotified = false;

                        // Award rewards
                        await _userStatisticsService.AwardTokensAndXpAsync(
                            userId,
                            userAchievement.Achievement.TokenReward,
                            userAchievement.Achievement.XpReward ?? 0,
                            $"Achievement unlocked: {userAchievement.Achievement.Name}");

                        unlockedThisTime.Add(userAchievement.Achievement.Name);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Updated progress for {questAchievements.Count} achievements. {unlockedThisTime.Count} unlocked.",
                    completedQuests = completedQuests,
                    unlockedAchievements = unlockedThisTime,
                    achievements = questAchievements.Select(ua => new
                    {
                        ua.Achievement.Name,
                        ua.Progress,
                        ua.Achievement.CriteriaValue,
                        ua.IsUnlocked
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> FixAchievementProgress()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Get actual completed quest count
                var actualQuestCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"Actual completed quests: {actualQuestCount}");

                // Get all quest-related user achievements
                var questUserAchievements = await _context.UserAchievements
                    .Include(ua => ua.Achievement)
                    .Where(ua => ua.UserId == userId && ua.Achievement.CriteriaType == "total_quests_completed")
                    .ToListAsync();

                var unlockedCount = 0;

                // Update progress for all quest achievements
                foreach (var userAchievement in questUserAchievements)
                {
                    var oldProgress = userAchievement.Progress;
                    userAchievement.Progress = actualQuestCount;

                    results.Add($"{userAchievement.Achievement.Name}: progress {oldProgress} → {actualQuestCount}");

                    // Check if should be unlocked now
                    if (actualQuestCount >= userAchievement.Achievement.CriteriaValue && !userAchievement.IsUnlocked)
                    {
                        userAchievement.IsUnlocked = true;
                        userAchievement.EarnedDate = DateTime.UtcNow;
                        userAchievement.IsNotified = false;

                        // Award rewards
                        await _userStatisticsService.AwardTokensAndXpAsync(
                            userId,
                            userAchievement.Achievement.TokenReward,
                            userAchievement.Achievement.XpReward ?? 0,
                            $"Achievement unlocked: {userAchievement.Achievement.Name}");

                        results.Add($"  ✅ UNLOCKED! Awarded {userAchievement.Achievement.TokenReward}T + {userAchievement.Achievement.XpReward}XP");
                        unlockedCount++;
                    }
                    else if (userAchievement.IsUnlocked)
                    {
                        results.Add($"  ✅ Already unlocked");
                    }
                    else
                    {
                        results.Add($"  🔒 Needs {userAchievement.Achievement.CriteriaValue} quests");
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Fixed progress for {questUserAchievements.Count} achievements. {unlockedCount} newly unlocked.",
                    results = results,
                    actualQuestCount = actualQuestCount,
                    unlockedCount = unlockedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DebugAchievementCheckLogic()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Get current quest count
                var currentQuestCount = await _context.UserQuests.CountAsync(uq => uq.UserId == userId && uq.IsCompleted);
                results.Add($"Current completed quests: {currentQuestCount}");

                // Get quest achievements
                var questAchievements = await _context.Achievements
                    .Where(a => a.IsActive && a.CriteriaType == "total_quests_completed")
                    .ToListAsync();

                results.Add($"Found {questAchievements.Count} quest achievements");

                // Check each achievement
                foreach (var achievement in questAchievements)
                {
                    var userAchievement = await _context.UserAchievements
                        .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.AchievementId);

                    if (userAchievement == null)
                    {
                        results.Add($"❌ {achievement.Name}: No UserAchievement record found");
                        continue;
                    }

                    results.Add($"🔍 {achievement.Name}:");
                    results.Add($"   - Requires: {achievement.CriteriaValue} quests");
                    results.Add($"   - User has: {currentQuestCount} quests");
                    results.Add($"   - Progress: {userAchievement.Progress}");
                    results.Add($"   - IsUnlocked: {userAchievement.IsUnlocked}");
                    results.Add($"   - Meets criteria: {currentQuestCount >= achievement.CriteriaValue}");

                    // Manual unlock test for eligible achievements
                    if (currentQuestCount >= achievement.CriteriaValue && !userAchievement.IsUnlocked)
                    {
                        results.Add($"   - 🎯 SHOULD BE UNLOCKED - Testing manual unlock...");

                        // Update the existing record
                        userAchievement.IsUnlocked = true;
                        userAchievement.EarnedDate = DateTime.UtcNow;
                        userAchievement.Progress = currentQuestCount;
                        userAchievement.IsNotified = false;

                        // Award rewards
                        var success = await _userStatisticsService.AwardTokensAndXpAsync(
                            userId,
                            achievement.TokenReward,
                            achievement.XpReward ?? 0,
                            $"Manual achievement unlock: {achievement.Name}");

                        if (success)
                        {
                            await _context.SaveChangesAsync();
                            results.Add($"   - ✅ UNLOCKED! Awarded {achievement.TokenReward}T + {achievement.XpReward}XP");
                        }
                        else
                        {
                            results.Add($"   - ❌ Failed to award rewards");
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Achievement check debug completed",
                    results = results,
                    currentQuestCount = currentQuestCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestCommunityData()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Create test posts
                try
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        var post = new Post
                        {
                            AuthorId = userId,
                            PostContent = $"Test post {i} for achievement testing - sharing my decluttering journey!",
                            PostType = PostType.Tip,
                            DatePosted = DateTime.UtcNow
                        };
                        _context.Posts.Add(post);
                    }
                    await _context.SaveChangesAsync();
                    results.Add("Created 3 test posts");
                }
                catch (Exception ex)
                {
                    results.Add($"Posts creation failed: {ex.Message}");
                }

                // Get a post to comment on
                var testPost = await _context.Posts.FirstOrDefaultAsync();
                if (testPost != null)
                {
                    try
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            var comment = new Comment
                            {
                                PostId = testPost.PostId,
                                UserId = userId,
                                Content = $"This is a helpful comment {i} - great advice!",
                                DateCommented = DateTime.UtcNow
                            };
                            _context.Comments.Add(comment);
                        }
                        await _context.SaveChangesAsync();
                        results.Add("Created 5 test comments");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"Comments creation failed: {ex.Message}");
                    }

                    // Create test reactions
                    try
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            var reaction = new Reaction
                            {
                                PostId = testPost.PostId,
                                UserId = userId,
                                ReactionType = ReactionType.Helpful,
                                DateReacted = DateTime.UtcNow
                            };
                            _context.Reactions.Add(reaction);
                        }
                        await _context.SaveChangesAsync();
                        results.Add("Created 3 test reactions");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"Reactions creation failed: {ex.Message}");
                    }
                }

                // Now test achievement checks with real data
                var postAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "post_created", 1);
                var commentAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "comment_created", 1);
                var reactionAchievements = await _achievementService.CheckAndUnlockAchievementsAsync(userId, "reaction_created", 1);

                results.Add($"Post achievements unlocked: {postAchievements.Count}");
                results.Add($"Comment achievements unlocked: {commentAchievements.Count}");
                results.Add($"Reaction achievements unlocked: {reactionAchievements.Count}");

                // Check current counts
                var postCount = await _context.Posts.CountAsync(p => p.AuthorId == userId);
                var commentCount = await _context.Comments.CountAsync(c => c.UserId == userId);
                var reactionCount = await _context.Reactions.CountAsync(r => r.UserId == userId);

                results.Add($"Final counts - Posts: {postCount}, Comments: {commentCount}, Reactions: {reactionCount}");

                return Json(new
                {
                    success = true,
                    message = "Created test community data with correct property names",
                    results = results,
                    counts = new { posts = postCount, comments = commentCount, reactions = reactionCount },
                    achievements = new
                    {
                        posts = postAchievements.Select(a => a.Name).ToList(),
                        comments = commentAchievements.Select(a => a.Name).ToList(),
                        reactions = reactionAchievements.Select(a => a.Name).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DebugCommunityProgress()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                // Test direct database queries
                var postCount = await _context.Posts.CountAsync(p => p.AuthorId == userId);
                var commentCount = await _context.Comments.CountAsync(c => c.UserId == userId);
                var reactionCount = await _context.Reactions.CountAsync(r => r.UserId == userId);

                results.Add($"Direct DB queries:");
                results.Add($"  Posts: {postCount}");
                results.Add($"  Comments: {commentCount}");
                results.Add($"  Reactions: {reactionCount}");

                // Test GetCurrentProgressAsync for each criteria type
                var criteriaTypes = new[] { "posts_created", "helpful_comments", "positive_reactions_given", "community_engagement_total" };

                foreach (var criteriaType in criteriaTypes)
                {
                    // We need to call the private method through reflection or make it public
                    // For now, let's manually test the logic
                    int progress = criteriaType switch
                    {
                        "posts_created" => await _context.Posts.CountAsync(p => p.AuthorId == userId),
                        "helpful_comments" => await _context.Comments.CountAsync(c => c.UserId == userId),
                        "positive_reactions_given" => await _context.Reactions.CountAsync(r => r.UserId == userId),
                        "community_engagement_total" => postCount + commentCount + reactionCount,
                        _ => 0
                    };

                    results.Add($"GetCurrentProgress '{criteriaType}': {progress}");
                }

                // Check which achievements should unlock
                var communityAchievements = await _context.Achievements
                    .Where(a => a.IsActive && (a.CriteriaType == "posts_created" || a.CriteriaType == "helpful_comments" || a.CriteriaType == "positive_reactions_given" || a.CriteriaType == "community_engagement_total"))
                    .OrderBy(a => a.CriteriaValue)
                    .ToListAsync();

                results.Add($"Community achievements that should be checked:");
                foreach (var achievement in communityAchievements)
                {
                    var currentProgress = achievement.CriteriaType switch
                    {
                        "posts_created" => postCount,
                        "helpful_comments" => commentCount,
                        "positive_reactions_given" => reactionCount,
                        "community_engagement_total" => postCount + commentCount + reactionCount,
                        _ => 0
                    };

                    var shouldUnlock = currentProgress >= achievement.CriteriaValue;
                    results.Add($"  {achievement.Name}: needs {achievement.CriteriaValue}, has {currentProgress}, should unlock: {shouldUnlock}");
                }

                return Json(new
                {
                    success = true,
                    results = results,
                    counts = new { posts = postCount, comments = commentCount, reactions = reactionCount }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DebugAchievementFiltering()
        {
            try
            {
                var userId = GetUserId();
                var results = new List<string>();

                var criteriaType = "posts_created";

                var relevantAchievements = await _context.Achievements
                    .Where(a => a.IsActive && a.CriteriaType == criteriaType)
                    .ToListAsync();

                results.Add($"Found {relevantAchievements.Count} achievements for '{criteriaType}'");

                var alreadyEarned = await _context.UserAchievements
                    .Where(ua => ua.UserId == userId &&
                                ua.IsUnlocked == true &&
                                relevantAchievements.Select(a => a.AchievementId).Contains(ua.AchievementId))
                    .Select(ua => ua.AchievementId)
                    .ToListAsync();

                results.Add($"Already earned (unlocked): {alreadyEarned.Count} - IDs: [{string.Join(", ", alreadyEarned)}]");

                var filteredAchievements = relevantAchievements.Where(a => !alreadyEarned.Contains(a.AchievementId)).ToList();
                results.Add($"Achievements to check: {filteredAchievements.Count}");

                foreach (var achievement in filteredAchievements)
                {
                    var userAchievement = await _context.UserAchievements
                        .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.AchievementId);

                    var currentProgress = await _context.Posts.CountAsync(p => p.AuthorId == userId);

                    results.Add($"Checking: {achievement.Name} (ID: {achievement.AchievementId})");
                    results.Add($"  - Current progress: {currentProgress}");
                    results.Add($"  - Required: {achievement.CriteriaValue}");
                    results.Add($"  - UserAchievement exists: {userAchievement != null}");
                    results.Add($"  - UserAchievement.IsUnlocked: {userAchievement?.IsUnlocked ?? false}");
                    results.Add($"  - Should unlock: {currentProgress >= achievement.CriteriaValue}");
                }

                return Json(new { success = true, results = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DebugGenerateUserSpecialQuest(int? userId = null)
        {
            try
            {
                var targetUserId = userId ?? GetUserId();
                await _questService.GenerateNextSpecialQuestForUserAsync(targetUserId);

                return Json(new
                {
                    success = true,
                    message = $"Special quest generated for user {targetUserId}",
                    userId = targetUserId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}