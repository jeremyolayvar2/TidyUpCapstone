using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Gamification;
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
        private readonly ILogger<QuestController> _logger;
        private readonly ApplicationDbContext _context;

        public QuestController(
            IQuestService questService,
            IAchievementService achievementService,
            IStreakService streakService,
            ILogger<QuestController> logger,
            ApplicationDbContext context)
        {
            _questService = questService;
            _achievementService = achievementService;
            _streakService = streakService;
            _logger = logger;
            _context = context;
        }

        // Add these methods to your QuestController class
        private DateTime GetPhilippineTime()
        {
            var philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);
        }

        private async Task<UserStats> GetUserStats(int userId)
        {
            return await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId)
                   ?? new UserStats { UserId = userId, CurrentLevel = 1, CurrentXp = 0, TotalTokens = 0, CurrentStreak = 0, LongestStreak = 0 };
        }

        // Simple hardcoded user for testing - no session needed
        private int GetCurrentUserId()
        {
            return 1; // Always use test user ID 1
        }



        private int GetUserId()
        {
            // For testing, always return user ID 1
            return 1;
        }

        // Main Quest Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userStats = await _context.UserStats
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                var userLevel = await GetOrCreateUserLevel(userId);

                if (userStats == null)
                {
                    userStats = new UserStats { UserId = userId };
                    _context.UserStats.Add(userStats);
                    await _context.SaveChangesAsync();
                }

                var viewModel = new GamificationDashboardViewModel
                {
                    ActiveQuests = await _questService.GetActiveQuestsForUserAsync(userId),
                    CompletedQuests = await _questService.GetCompletedQuestsForUserAsync(userId),
                    RecentAchievements = await _achievementService.GetRecentAchievementsAsync(userId),
                    ActiveStreaks = await _streakService.GetActiveStreaksAsync(userId),

                    LevelProgress = new UserLevelProgressViewModel
                    {
                        CurrentLevel = userLevel.CurrentLevel?.LevelNumber ?? 1,
                        CurrentLevelName = userLevel.CurrentLevel?.LevelName ?? "Beginner",
                        CurrentXp = userLevel.CurrentXp,
                        TotalXp = userLevel.TotalXp,
                        XpToNextLevel = userLevel.XpToNextLevel,
                        XpProgress = CalculateXpProgress(userLevel.CurrentXp, userLevel.XpToNextLevel),
                        NextLevelName = await GetNextLevelName(userLevel.CurrentLevelId)
                    },

                    Stats = new GamificationStatsViewModel
                    {
                        TotalQuestsCompleted = await _context.UserQuests
                        .CountAsync(uq => uq.UserId == userId && uq.IsCompleted),
                                        AchievementsEarned = await _context.UserAchievements
                        .CountAsync(ua => ua.UserId == userId && ua.IsUnlocked),
                                        TotalAchievements = await _context.Achievements.CountAsync(a => a.IsActive),

                        // 🔹 Pulled from UserStats table
                        TokenBalance = userStats.TotalTokens,
                        TotalXpEarned = userStats.CurrentXp,
                        ActiveStreaksCount = userStats.CurrentStreak > 0 ? 1 : 0,

                        // 🔹 You can fill these in later if needed
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

        // Claim Quest Reward
        [HttpPost]
        public async Task<IActionResult> ClaimReward(int questId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _questService.ClaimQuestRewardAsync(userId, questId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Reward claimed successfully!";
                    return Json(new { success = true, message = "Reward claimed successfully!" });
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

        // Daily Check-in
        [HttpPost]
        public async Task<IActionResult> CheckIn()
        {
            try
            {
                var userId = GetCurrentUserId();
                var today = GetPhilippineTime().Date;
                var userStats = await GetOrCreateUserStats(userId);

                // Check if already checked in
                var existingCheckIn = await _context.CheckIns
                    .Where(c => c.UserId == userId && c.CheckInDate.Date == today)
                    .FirstOrDefaultAsync();

                if (existingCheckIn != null)
                {
                    return Json(new { success = false, message = "Already checked in today" });
                }

                // Update streak in UserStats
                var yesterday = today.AddDays(-1);
                var checkedInYesterday = await _context.CheckIns
                    .AnyAsync(c => c.UserId == userId && c.CheckInDate.Date == yesterday);

                if (checkedInYesterday)
                {
                    userStats.CurrentStreak++;
                }
                else
                {
                    userStats.CurrentStreak = 1;
                }

                if (userStats.CurrentStreak > userStats.LongestStreak)
                {
                    userStats.LongestStreak = userStats.CurrentStreak;
                }

                // Calculate rewards
                decimal tokenReward = CalculateCheckInTokenReward(userStats.CurrentStreak);
                int xpReward = CalculateCheckInXpReward(userStats.CurrentStreak);

                // Update UserStats (tokens and streak)
                userStats.TotalTokens += tokenReward;
                userStats.LastCheckIn = GetPhilippineTime();

                // Update UserLevel (XP)
                await UpdateUserXpAsync(userId, xpReward);

                // Create check-in record
                var checkIn = new CheckIn
                {
                    UserId = userId,
                    CheckInDate = GetPhilippineTime(),
                    TokensEarned = tokenReward,
                    StreakDay = userStats.CurrentStreak
                };

                _context.CheckIns.Add(checkIn);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Check-in successful!",
                    tokensEarned = tokenReward,
                    xpEarned = xpReward,
                    streak = userStats.CurrentStreak
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int CalculateCheckInXpReward(int streakDay)
        {
            int baseXp = 10; // Base XP for checking in
            int streakBonus = Math.Min(streakDay * 2, 50); // Max 50 bonus XP
            return baseXp + streakBonus;
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
            }
        }

        private decimal CalculateCheckInTokenReward(int streakDay)
        {
            // Base reward + streak bonus (inspired by KonMari's gradual progress philosophy)
            decimal baseReward = 2.0m;
            decimal streakMultiplier = Math.Min(streakDay * 0.5m, 10m); // Max 10x multiplier

            return baseReward + streakMultiplier;
        }

        private async Task<UserStats> GetOrCreateUserStats(int userId)
        {
            var userStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == userId);

            if (userStats == null)
            {
                userStats = new UserStats
                {
                    UserId = userId,
                    CurrentLevel = 1,
                    CurrentXp = 0,
                    TotalTokens = 0,
                    CurrentStreak = 0,
                    LongestStreak = 0
                };

                _context.UserStats.Add(userStats);
                await _context.SaveChangesAsync();
            }

            return userStats;
        }


        // Get Check-in Status
        [HttpGet]
        public async Task<IActionResult> CheckInStatus()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userStats = await GetOrCreateUserStats(userId);
                var today = GetPhilippineTime().Date;
                var tomorrow = today.AddDays(1);

                var hasCheckedIn = await _context.CheckIns.AnyAsync(c => c.UserId == userId && c.CheckInDate.Date == today);

                // Calculate time until next check-in (midnight Philippine time)
                var nextCheckIn = tomorrow;
                var timeUntilNext = nextCheckIn - GetPhilippineTime();

                return Json(new
                {
                    hasCheckedIn = hasCheckedIn,
                    streak = userStats.CurrentStreak,
                    lastCheckIn = userStats.LastCheckIn,
                    totalTokens = userStats.TotalTokens,
                    nextCheckInTime = nextCheckIn,
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

                // Populate achievement stats
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

                // Get all achievements
                var allAchievements = await _context.Achievements.Where(a => a.IsActive).ToListAsync();

                // Get existing user achievements
                var existingAchievements = await _context.UserAchievements
                    .Where(ua => ua.UserId == targetUserId)
                    .Select(ua => ua.AchievementId)
                    .ToListAsync();

                var userAchievementsToAdd = new List<UserAchievement>();

                foreach (var achievement in allAchievements)
                {
                    if (!existingAchievements.Contains(achievement.AchievementId))
                    {
                        userAchievementsToAdd.Add(new UserAchievement
                        {
                            UserId = targetUserId,
                            AchievementId = achievement.AchievementId,
                            IsUnlocked = false,
                            Progress = 0,
                            EarnedDate = null,
                            CreatedAt = GetPhilippineTime()
                        });
                    }
                }

                if (userAchievementsToAdd.Any())
                {
                    _context.UserAchievements.AddRange(userAchievementsToAdd);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Seeded {userAchievementsToAdd.Count} achievements for user {targetUserId}"
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
                    ua.EarnedDate = null; // This will now work since it's nullable
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
                var specialQuests = new List<Quest>
        {
            new Quest {
                QuestTitle = "Complete KonMari Home Transformation",
                QuestDescription = "Apply the complete KonMari Method to your entire home, category by category.",
                QuestObjective = "Transform entire home using KonMari Method.",
                QuestType = QuestType.Special,
                Difficulty = QuestDifficulty.Hard,
                TargetValue = 1,
                TokenReward = 25m,
                XpReward = 250,
                IsActive = true,
                CreatedAt = GetPhilippineTime()
            },
            new Quest {
                QuestTitle = "Joy-Spark Life Festival",
                QuestDescription = "Celebrate items that spark joy with a gratitude ceremony.",
                QuestObjective = "Celebrate joy-sparking possessions.",
                QuestType = QuestType.Special,
                Difficulty = QuestDifficulty.Medium,
                TargetValue = 1,
                TokenReward = 15m,
                XpReward = 150,
                IsActive = true,
                CreatedAt = GetPhilippineTime()
            },
            new Quest {
                QuestTitle = "Master of Gratitude Challenge",
                QuestDescription = "Thank 100 items before letting them go.",
                QuestObjective = "Thank 100 items before release.",
                QuestType = QuestType.Special,
                Difficulty = QuestDifficulty.Easy,
                TargetValue = 100,
                TokenReward = 20m,
                XpReward = 200,
                IsActive = true,
                CreatedAt = GetPhilippineTime()
            }
        };

                _context.Quests.AddRange(specialQuests);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Seeded {specialQuests.Count} special quests" });
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
                var targetUserId = userId ?? GetCurrentUserId();

                // Check if user already has an active special quest
                var existingSpecialQuest = await _context.UserQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == targetUserId &&
                                uq.Quest.QuestType == QuestType.Special &&
                                uq.Status == QuestStatus.Active)
                    .FirstOrDefaultAsync();

                if (existingSpecialQuest != null)
                {
                    return Json(new { success = false, message = "User already has an active special quest" });
                }

                // Check user's progress/level to determine eligibility
                var userStats = await GetUserStats(targetUserId);
                var completedQuests = await _context.UserQuests
                    .Where(uq => uq.UserId == targetUserId && uq.Status == QuestStatus.Completed)
                    .CountAsync();

                // Replace the eligibility logic with this for testing:
                Quest specialQuest = null;

                // For testing - always allow at least one special quest
                if (completedQuests >= 50) // High-level users
                {
                    specialQuest = await _context.Quests
                        .Where(q => q.QuestType == QuestType.Special &&
                                   q.Difficulty == QuestDifficulty.Hard &&
                                   q.IsActive)
                        .OrderBy(r => Guid.NewGuid())
                        .FirstOrDefaultAsync();
                }
                else if (completedQuests >= 20) // Mid-level users  
                {
                    specialQuest = await _context.Quests
                        .Where(q => q.QuestType == QuestType.Special &&
                                   q.Difficulty == QuestDifficulty.Medium &&
                                   q.IsActive)
                        .OrderBy(r => Guid.NewGuid())
                        .FirstOrDefaultAsync();
                }
                else // Changed from >= 5 to >= 0 for testing
                {
                    specialQuest = await _context.Quests
                        .Where(q => q.QuestType == QuestType.Special &&
                                   q.Difficulty == QuestDifficulty.Easy &&
                                   q.IsActive)
                        .OrderBy(r => Guid.NewGuid())
                        .FirstOrDefaultAsync();
                }

                if (specialQuest == null)
                {
                    return Json(new { success = false, message = "No suitable special quest available" });
                }

                if (specialQuest == null)
                {
                    return Json(new { success = false, message = "No suitable special quest available" });
                }

                // Assign to user
                var userQuest = new UserQuest
                {
                    UserId = targetUserId,
                    QuestId = specialQuest.QuestId,
                    Status = QuestStatus.Active,
                    CurrentProgress = 0,  // Changed from Progress = 0
                    StartedAt = GetPhilippineTime(),
                    ExpiresAt = GetPhilippineTime().AddDays(7)
                };

                _context.UserQuests.Add(userQuest);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Special quest generated successfully",
                    quest = new
                    {
                        id = specialQuest.QuestId,
                        title = specialQuest.QuestTitle,
                        description = specialQuest.QuestDescription,
                        difficulty = specialQuest.Difficulty.ToString(),
                        tokenReward = specialQuest.TokenReward,
                        xpReward = specialQuest.XpReward
                    }
                });
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

                // Populate streak stats
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

        // Debug Methods
        [HttpGet]
        public async Task<IActionResult> DebugData()
        {
            try
            {
                var userId = GetUserId();

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
                var userStats = await GetOrCreateUserStats(userId);

                // Create a unified streak view using UserStats data
                var streakViewModels = new List<UserStreakViewModel>
        {
            new UserStreakViewModel
            {
                StreakName = "Daily Check-in",
                Description = "Check in to the app daily to maintain your streak",
                CurrentStreak = userStats.CurrentStreak,
                LongestStreak = userStats.LongestStreak,
                LastActivityDate = userStats.LastCheckIn,
                IsActive = userStats.CurrentStreak > 0,
                StreakUnit = "Days",
                NextMilestone = ((userStats.CurrentStreak / 7) + 1) * 7,
                DaysUntilMilestone = ((userStats.CurrentStreak / 7) + 1) * 7 - userStats.CurrentStreak,
                MilestoneReward = 10.0m
            }
        };

                var viewModel = new GamificationDashboardViewModel
                {
                    ActiveStreaks = streakViewModels,
                    Stats = new GamificationStatsViewModel
                    {
                        ActiveStreaksCount = streakViewModels.Count(s => s.IsActive)
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

        // Add these debug endpoints to QuestController.cs

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
    }
}