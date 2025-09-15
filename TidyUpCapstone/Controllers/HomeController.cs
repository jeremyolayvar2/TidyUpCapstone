using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.DTOs.Transactions;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Services;


namespace TidyUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            IServiceProvider serviceProvider,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

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

        // Add this authentication method
        private int GetCurrentAuthenticatedUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            }
            return 0;
        }


    }

}