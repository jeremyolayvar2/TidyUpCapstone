using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.DTOs.Transactions;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Models.ViewModels.Transactions;
using TidyUpCapstone.Data;

namespace TidyUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        private int GetCurrentAuthenticatedUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            return 0;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> ItemsClaimedPage(string? status = null, int? categoryId = null,
            string? search = null, string? sortBy = "newest", int page = 1, int pageSize = 12)
        {
           

            
            var currentUserId = GetCurrentAuthenticatedUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
           

            var query = _context.Transactions
                .Include(t => t.Item)
                    .ThenInclude(i => i.Category)
                .Include(t => t.Item)
                    .ThenInclude(i => i.Condition)
                .Include(t => t.Item)
                    .ThenInclude(i => i.Location)
                .Include(t => t.Seller)
                .Include(t => t.Chat)
                .Where(t => t.BuyerId == currentUserId);

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                // Map the view model status strings to actual enum values
                var statusEnum = status.ToLower() switch
                {
                    "pending" => TransactionStatus.Pending,
                    "escrowed" => TransactionStatus.Disputed, // Map "escrowed" to Disputed
                    "confirmed" => TransactionStatus.Completed, // Map "confirmed" to Completed  
                    "cancelled" => TransactionStatus.Cancelled,
                    _ => (TransactionStatus?)null
                };

                if (statusEnum.HasValue)
                {
                    query = query.Where(t => t.TransactionStatus == statusEnum.Value);
                }
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.Item.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Item.ItemTitle.Contains(search) ||
                                        t.Item.Description.Contains(search));
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(t => t.CreatedAt),
                "price-high" => query.OrderByDescending(t => t.TokenAmount),
                "price-low" => query.OrderBy(t => t.TokenAmount),
                "title" => query.OrderBy(t => t.Item.ItemTitle),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            // Calculate stats - using actual enum values
            var totalClaimed = await query.CountAsync();
            var pendingCount = await query.CountAsync(t => t.TransactionStatus == TransactionStatus.Pending);
            var completedCount = await query.CountAsync(t => t.TransactionStatus == TransactionStatus.Completed);
            var cancelledCount = await query.CountAsync(t => t.TransactionStatus == TransactionStatus.Cancelled);
            var disputedCount = await query.CountAsync(t => t.TransactionStatus == TransactionStatus.Disputed);

            // Apply pagination
            var totalItems = await query.CountAsync();
            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    TransactionId = t.TransactionId,
                    BuyerId = t.BuyerId,
                    BuyerUsername = t.Buyer.UserName ?? "",
                    SellerId = t.SellerId,
                    SellerUsername = t.Seller.UserName ?? "",
                    ItemId = t.ItemId,
                    ItemTitle = t.Item.ItemTitle,
                    ItemImageUrl = !string.IsNullOrEmpty(t.Item.ImageFileName)
                        ? $"/uploads/items/{t.Item.ImageFileName}"
                        : "/assets/image-uploaded.jpg",
                    TokenAmount = t.TokenAmount,
                    TransactionStatus = t.TransactionStatus,
                    DeliveryMethod = t.DeliveryMethod,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    CancelledAt = t.CancelledAt,
                    CancellationReason = t.CancellationReason
                })
                .ToListAsync();

            // Get categories for filter
            var categories = await _context.ItemCategories
                .Where(c => c.IsActive)
                .ToDictionaryAsync(c => c.CategoryId, c => c.Name);

            var viewModel = new ClaimedItemsViewModel
            {
                Transactions = transactions,
                SearchCriteria = new ClaimedItemsSearchDto
                {
                    Status = status,
                    CategoryId = categoryId,
                    Search = search,
                    SortBy = sortBy,
                    Page = page,
                    PageSize = pageSize
                },
                Categories = categories,
                Statistics = new ClaimedItemsStatsDto
                {
                    TotalClaimed = totalClaimed,
                    PendingCount = pendingCount,
                    CompletedCount = completedCount,
                    CancelledCount = cancelledCount,
                    InProgressCount = disputedCount // Using disputed as in-progress for now
                },
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}