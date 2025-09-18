using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels.Items;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IItemService _itemService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            IItemService itemService,
            ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _itemService = itemService;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        //[Authorize]
        //[Authorize]
        public async Task<IActionResult> Main()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // Handle null user case properly
                if (currentUser != null)
                {
                    _logger.LogInformation("User {UserId} accessed Main page successfully", currentUser.Id);
                }
                else
                {
                    _logger.LogInformation("Anonymous user accessed Main page");
                }

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

                // Set ViewBag data for the view - Handle null user case
                ViewBag.CurrentUserTokenBalance = currentUser?.TokenBalance ?? 0;
                ViewBag.CurrentUserAvatar = "/assets/default-avatar.svg";
                ViewBag.UserName = currentUser?.UserName ?? "Anonymous";
                ViewBag.FirstName = currentUser?.FirstName ?? "Guest";
                ViewBag.LastName = currentUser?.LastName ?? "";

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

                // Set fallback ViewBag data
                ViewBag.CurrentUserTokenBalance = 0;
                ViewBag.CurrentUserAvatar = "/assets/default-avatar.svg";
                ViewBag.UserName = "Anonymous";
                ViewBag.FirstName = "Guest";
                ViewBag.LastName = "";

                return View(fallbackViewModel);
            }
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