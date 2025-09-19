using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Filters;
using TidyUpCapstone.Helpers;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.DTOs.Transactions;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Models.ViewModels.Gamification;
using TidyUpCapstone.Models.ViewModels.Items;
using TidyUpCapstone.Services;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IItemService _itemService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IServiceProvider _serviceProvider;

        // Gamification services - optional dependencies to prevent breaking changes
        private readonly IQuestService? _questService;
        private readonly IAchievementService? _achievementService;
        private readonly IStreakService? _streakService;

        public HomeController(
            ILogger<HomeController> logger,
            IItemService itemService,
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IServiceProvider serviceProvider,
            IQuestService? questService = null,
            IAchievementService? achievementService = null,
            IStreakService? streakService = null)
            : base(userManager)
        {
            _logger = logger;
            _itemService = itemService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _serviceProvider = serviceProvider;
            _questService = questService;
            _achievementService = achievementService;
            _streakService = streakService;
        }

        public IActionResult Index(string? error = null, string? success = null)
        {
            // Pass any error or success messages to the view
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.ErrorMessage = error;
            }

            if (!string.IsNullOrEmpty(success))
            {
                ViewBag.SuccessMessage = success;
            }

            return View();
        }

        public async Task<IActionResult> Main()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("User not found in Main action, redirecting to Index");

                    // Sign out if user claims to be authenticated but doesn't exist
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        await _signInManager.SignOutAsync();
                    }

                    return RedirectToAction("Index", "Home", new { showLogin = true });
                }

                _logger.LogInformation("User {UserId} accessed Main page successfully", currentUser.Id);
                _logger.LogInformation("Loading main page data...");

                // Load categories from database
                var categories = await _context.ItemCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new ItemCategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        SortOrder = c.SortOrder
                    })
                    .ToListAsync();

                // Load conditions from database
                var conditions = await _context.ItemConditions
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.ConditionId)
                    .Select(c => new ItemConditionDto
                    {
                        ConditionId = c.ConditionId,
                        Name = c.Name,
                        Description = c.Description,
                        ConditionMultiplier = c.ConditionMultiplier,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                _logger.LogInformation("Loaded {CategoryCount} categories and {ConditionCount} conditions",
                    categories.Count, conditions.Count);

                // If no data found, use fallback
                if (!categories.Any())
                {
                    _logger.LogWarning("No categories found in database, using fallback data");
                    categories = GetFallbackCategories();
                }

                if (!conditions.Any())
                {
                    _logger.LogWarning("No conditions found in database, using fallback data");
                    conditions = GetFallbackConditions();
                }

                // Load items
                var items = await _itemService.GetAllActiveItemsAsync();

                var viewModel = new ItemListViewModel
                {
                    Items = items.Select(i => new ItemDto
                    {
                        ItemId = i.ItemId,
                        UserId = i.UserId,
                        Username = i.User?.UserName ?? "Unknown",
                        UserAvatarUrl = "/assets/default-avatar.svg",
                        CategoryId = i.CategoryId,
                        CategoryName = i.Category?.Name ?? "Unknown",
                        ConditionId = i.ConditionId,
                        ConditionName = i.Condition?.Name ?? "Unknown",
                        LocationId = i.LocationId,
                        LocationName = i.Location?.Name ?? "Unknown",
                        ItemTitle = i.ItemTitle,
                        Description = i.Description,
                        ImageFileName = i.ImageFileName,
                        FinalTokenPrice = i.FinalTokenPrice,
                        AdjustedTokenPrice = i.AdjustedTokenPrice,
                        Status = i.Status,
                        DatePosted = i.DatePosted,
                        ExpiresAt = i.ExpiresAt,
                        AiProcessingStatus = i.AiProcessingStatus,
                        AiConfidenceLevel = i.AiConfidenceLevel,
                        ViewCount = i.ViewCount,
                        Latitude = i.Latitude,
                        Longitude = i.Longitude
                    }).ToList(),
                    Categories = categories,
                    Conditions = conditions,
                    TotalItems = items.Count,
                };

                decimal userTokenBalance = 0m;

                try
                {
                    // First try to get from UserStats (gamification system)
                    var userStats = await _context.UserStats
                        .Where(us => us.UserId == currentUser.Id)
                        .FirstOrDefaultAsync();

                    if (userStats != null)
                    {
                        userTokenBalance = userStats.TotalTokens;

                        // Sync with AppUser.TokenBalance if they differ
                        if (currentUser.TokenBalance != userTokenBalance)
                        {
                            currentUser.TokenBalance = userTokenBalance;
                            await _userManager.UpdateAsync(currentUser);
                        }
                    }
                    else
                    {
                        // Fallback to AppUser.TokenBalance if UserStats doesn't exist
                        userTokenBalance = currentUser.TokenBalance;

                        // Create UserStats if it doesn't exist for this user
                        var newUserStats = new UserStats
                        {
                            UserId = currentUser.Id,
                            TotalTokens = userTokenBalance,
                            CurrentXp = 0,
                            CurrentStreak = 0,
                            LastCheckIn = null,
                        };

                        _context.UserStats.Add(newUserStats);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception tokenEx)
                {
                    _logger.LogError(tokenEx, "Error fetching user token balance for user {UserId}", currentUser.Id);
                    // Fallback to AppUser.TokenBalance
                    userTokenBalance = currentUser.TokenBalance;
                }

                // Set ViewBag data for the view
                ViewBag.CurrentUserTokenBalance = userTokenBalance;
                ViewBag.CurrentUserAvatar = UserHelper.GetUserAvatarUrl(currentUser);
                ViewBag.UserName = currentUser.UserName;
                ViewBag.FirstName = currentUser.FirstName;
                ViewBag.LastName = currentUser.LastName;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading main page");

                // Return view model with fallback data
                var fallbackViewModel = new ItemListViewModel
                {
                    Items = new List<ItemDto>(),
                    Categories = GetFallbackCategories(),
                    Conditions = GetFallbackConditions()
                };

                return View(fallbackViewModel);
            }
        }

        // MessagePage action from chat-page branch
        public async Task<IActionResult> MessagePage(int? otherUserId, int? itemId)
        {
            // Replace test user session logic with real authentication
            var currentUserId = GetCurrentAuthenticatedUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account"); // Redirect to your login page
            }

            var currentUser = await _context.Users.FindAsync(currentUserId);
            var otherUser = otherUserId.HasValue ? await _context.Users.FindAsync(otherUserId.Value) : null;

            var viewModel = new MessagePageViewModel
            {
                CurrentUser = currentUser,
                OtherUser = otherUser
            };

            // Item-based escrow logic (replaces hardcoded test user logic)
            if (currentUser != null && itemId.HasValue)
            {
                var item = await _context.Items
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.ItemId == itemId.Value);

                if (item != null && currentUser.Id != item.UserId) // Only if user is not the item owner
                {
                    try
                    {
                        var escrowService = HttpContext.RequestServices.GetRequiredService<IEscrowService>();
                        await escrowService.AutoEscrowOnChatAsync(currentUser.Id, item.UserId, item.FinalTokenPrice);

                        // Set the other user as the item owner (seller)
                        otherUser = item.User;
                        viewModel.OtherUser = otherUser;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during auto-escrow");
                    }
                }
            }

            // Load existing transaction (keep this logic unchanged)
            if (currentUser != null && otherUser != null)
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Item)
                    .Include(t => t.Escrows)
                    .FirstOrDefaultAsync(t =>
                        (t.BuyerId == currentUser.Id && t.SellerId == otherUser.Id) ||
                        (t.BuyerId == otherUser.Id && t.SellerId == currentUser.Id));

                if (transaction != null)
                {
                    viewModel.CurrentTransactionId = transaction.TransactionId;
                    viewModel.HasActiveTransaction = true;
                    viewModel.TransactionItemTitle = transaction.Item?.ItemTitle ?? "Item";
                    viewModel.TransactionAmount = transaction.TokenAmount;

                    viewModel.TransactionStatus = new TransactionStatusDto
                    {
                        TransactionId = transaction.TransactionId,
                        Status = transaction.TransactionStatus,
                        BuyerConfirmed = transaction.BuyerConfirmed,
                        SellerConfirmed = transaction.SellerConfirmed,
                        CanConfirm = transaction.TransactionStatus == TransactionStatus.Escrowed,
                        CanCancel = transaction.TransactionStatus == TransactionStatus.Escrowed,
                        UserRole = currentUser.Id == transaction.BuyerId ? "buyer" : "seller"
                    };
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Quest page for gamification features
        /// </summary>
        [Authorize]
        public async Task<IActionResult> QuestPage()
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    // For development/testing - you may want to remove this in production
                    userId = 1; // Test user ID
                }

                // Check if gamification services are available
                if (_questService == null || _achievementService == null || _streakService == null)
                {
                    _logger.LogWarning("Gamification services not available");
                    ViewData["ErrorMessage"] = "Quest system is currently unavailable.";
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
                _logger.LogError(ex, "Error loading quest dashboard");
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserTokenBalance()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                decimal tokenBalance = 0m;

                // Try to get from UserStats first (gamification system)
                var userStats = await _context.UserStats
                    .Where(us => us.UserId == currentUser.Id)
                    .FirstOrDefaultAsync();

                if (userStats != null)
                {
                    tokenBalance = userStats.TotalTokens;
                }
                else
                {
                    // Fallback to AppUser.TokenBalance
                    tokenBalance = currentUser.TokenBalance;
                }

                return Json(new
                {
                    success = true,
                    tokenBalance = tokenBalance,
                    formattedBalance = tokenBalance.ToString("N0") // Format with commas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user token balance");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [NoCache]
        public async Task<IActionResult> SettingsPage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Load notification settings
            var settings = await _context.NotificationSettings
                .FirstOrDefaultAsync(ns => ns.UserId == user.Id);

            // Pass settings to view
            ViewBag.EmailNewMessages = settings?.EmailNewMessages ?? true;
            ViewBag.EmailItemUpdates = settings?.EmailItemUpdates ?? true;
            ViewBag.EmailWeeklySummary = settings?.EmailWeeklySummary ?? false;
            ViewBag.DesktopNotifications = settings?.DesktopNotifications ?? true;

            return View();
        }

        /// <summary>
        /// Gets user level progress for gamification features
        /// </summary>
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

        // Add this authentication method from chat-page branch
        private int GetCurrentAuthenticatedUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            }
            return 0;
        }

        private List<ItemCategoryDto> GetFallbackCategories()
        {
            return new List<ItemCategoryDto>
            {
                new() { CategoryId = 1, Name = "Books & Stationery", IsActive = true, SortOrder = 1 },
                new() { CategoryId = 2, Name = "Electronics & Gadgets", IsActive = true, SortOrder = 2 },
                new() { CategoryId = 3, Name = "Toys & Games", IsActive = true, SortOrder = 3 },
                new() { CategoryId = 4, Name = "Home & Kitchen", IsActive = true, SortOrder = 4 },
                new() { CategoryId = 5, Name = "Furniture", IsActive = true, SortOrder = 5 },
                new() { CategoryId = 6, Name = "Appliances", IsActive = true, SortOrder = 6 },
                new() { CategoryId = 7, Name = "Health & Beauty", IsActive = true, SortOrder = 7 },
                new() { CategoryId = 8, Name = "Crafts & DIY", IsActive = true, SortOrder = 8 },
                new() { CategoryId = 9, Name = "School & Office", IsActive = true, SortOrder = 9 },
                new() { CategoryId = 10, Name = "Sentimental Items", IsActive = true, SortOrder = 10 },
                new() { CategoryId = 11, Name = "Miscellaneous", IsActive = true, SortOrder = 11 },
                new() { CategoryId = 12, Name = "Clothing", IsActive = true, SortOrder = 12 }
            };
        }

        private List<ItemConditionDto> GetFallbackConditions()
        {
            return new List<ItemConditionDto>
            {
                new() { ConditionId = 1, Name = "Excellent", ConditionMultiplier = 1.25m, IsActive = true },
                new() { ConditionId = 3, Name = "Good", ConditionMultiplier = 1.05m, IsActive = true },
                new() { ConditionId = 4, Name = "Fair", ConditionMultiplier = 0.90m, IsActive = true }
            };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}