using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Models.ViewModels.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IQuestService _questService;
        private readonly IAchievementService _achievementService;
        private readonly IStreakService _streakService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            IQuestService questService,
            IAchievementService achievementService,
            IStreakService streakService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _questService = questService;
            _achievementService = achievementService;
            _streakService = streakService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> QuestPage()
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    userId = 1; // Test user ID
                }

                // Generate quests
                await _questService.GenerateDailyQuestsAsync();
                await _questService.GenerateWeeklyQuestsAsync();

                var viewModel = new GamificationDashboardViewModel
                {
                    ActiveQuests = await _questService.GetActiveQuestsForUserAsync(userId),
                    RecentAchievements = await _achievementService.GetRecentAchievementsAsync(userId, 3),
                    ActiveStreaks = await _streakService.GetActiveStreaksAsync(userId),
                    Stats = new GamificationStatsViewModel(),
                    LevelProgress = await GetUserLevelProgressAsync(userId)
                };

                // Populate stats
                var questStats = await _questService.GetQuestStatsAsync(userId);
                var achievementStats = await _achievementService.GetAchievementStatsAsync(userId);
                var streakStats = await _streakService.GetStreakStatsAsync(userId);

                var userStats = await _context.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);

                viewModel.Stats.TotalQuestsCompleted = (int)(questStats.ContainsKey("CompletedQuests") ? questStats["CompletedQuests"] : 0);
                viewModel.Stats.AchievementsEarned = (int)(achievementStats.ContainsKey("TotalEarned") ? achievementStats["TotalEarned"] : 0);
                viewModel.Stats.TotalAchievements = (int)(achievementStats.ContainsKey("TotalAvailable") ? achievementStats["TotalAvailable"] : 0);
                viewModel.Stats.TokenBalance = userStats?.TotalTokens ?? 0m;   // ✅ Direct from UserStats
                viewModel.Stats.TotalXpEarned = userStats?.CurrentXp ?? 0;     // ✅ Optional: match XP from UserStats
                viewModel.Stats.ActiveStreaksCount = userStats?.CurrentStreak ?? 0;


                ViewData["Title"] = "Daily Quests";
                ViewData["PageType"] = "quest";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred while loading your quest dashboard: " + ex.Message;
                return View(new GamificationDashboardViewModel
                {
                    LevelProgress = new UserLevelProgressViewModel
                    {
                        CurrentLevel = 1,
                        CurrentLevelName = "Beginner",
                        CurrentXp = 0,
                        TotalXp = 0,
                        XpToNextLevel = 100,
                        XpProgress = 0
                    }
                });
            }
        }


        private async Task<UserLevelProgressViewModel> GetUserLevelProgressAsync(int userId)
        {
            var userLevel = await _context.UserLevels
                .Include(ul => ul.CurrentLevel)
                .FirstOrDefaultAsync(ul => ul.UserId == userId);

            if (userLevel == null)
            {
                // Create a default user level if none exists
                var firstLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == 1);
                if (firstLevel != null)
                {
                    var newUserLevel = new UserLevel
                    {
                        UserId = userId,
                        CurrentLevelId = firstLevel.LevelId,
                        CurrentXp = 0,
                        TotalXp = 0,
                        XpToNextLevel = firstLevel.XpToNext
                    };
                    _context.UserLevels.Add(newUserLevel);
                    await _context.SaveChangesAsync();

                    return new UserLevelProgressViewModel
                    {
                        CurrentLevel = 1,
                        CurrentLevelName = firstLevel.LevelName,
                        CurrentXp = 0,
                        TotalXp = 0,
                        XpToNextLevel = firstLevel.XpToNext,
                        XpProgress = 0,
                        NextLevelName = "Next Level"
                    };
                }

                return new UserLevelProgressViewModel
                {
                    CurrentLevel = 1,
                    CurrentLevelName = "Beginner",
                    CurrentXp = 0,
                    TotalXp = 0,
                    XpToNextLevel = 100,
                    XpProgress = 0
                };
            }

            var nextLevel = await _context.Levels
                .Where(l => l.LevelNumber > userLevel.CurrentLevel.LevelNumber)
                .OrderBy(l => l.LevelNumber)
                .FirstOrDefaultAsync();

            return new UserLevelProgressViewModel
            {
                CurrentLevel = userLevel.CurrentLevel.LevelNumber,
                CurrentLevelName = userLevel.CurrentLevel.LevelName,
                CurrentXp = userLevel.CurrentXp,
                TotalXp = userLevel.TotalXp,
                XpToNextLevel = userLevel.XpToNextLevel,
                XpProgress = userLevel.XpToNextLevel > 0 ?
                    (userLevel.CurrentXp * 100 / (userLevel.CurrentXp + userLevel.XpToNextLevel)) : 100,
                NextLevelName = nextLevel?.LevelName,
                TokenBonus = userLevel.CurrentLevel.TokenBonus
            };
        }

        // Helper method to get current user ID
        private int GetUserId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
            }
            return 0;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}