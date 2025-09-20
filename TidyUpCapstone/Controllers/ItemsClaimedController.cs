using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Transactions;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Controllers
{
    [Authorize] 
    public class ItemsClaimedController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ItemsClaimedController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ContactSeller(int transactionId)
        {

            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var userId = user.Id;
            

          

            var transaction = await _context.Transactions
                .Include(t => t.Seller)
                .Include(t => t.Item)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.BuyerId == userId);

            if (transaction == null) return NotFound();

            return RedirectToAction("MessagePage", "Home", new
            {
                otherUserId = transaction.SellerId,
                itemId = transaction.ItemId
            });
        }

        [HttpGet]
        public async Task<IActionResult> ViewDetails(int transactionId)
        {
           

            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var userId = user.Id;
            

            // ============================================================================
            // END TESTING SECTION
            // ============================================================================

            var transaction = await _context.Transactions
                .Include(t => t.Item)
                    .ThenInclude(i => i.Category)
                .Include(t => t.Item)
                    .ThenInclude(i => i.Condition)
                .Include(t => t.Item)
                    .ThenInclude(i => i.Location)
                .Include(t => t.Seller)
                .Include(t => t.Chat)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.BuyerId == userId);

            if (transaction == null) return NotFound();

            var transactionDto = new TransactionDto
            {
                TransactionId = transaction.TransactionId,
                BuyerId = transaction.BuyerId,
                BuyerUsername = transaction.Buyer?.UserName ?? "",
                SellerId = transaction.SellerId,
                SellerUsername = transaction.Seller.UserName ?? "",
                ItemId = transaction.ItemId,
                ItemTitle = transaction.Item.ItemTitle,
                ItemImageUrl = !string.IsNullOrEmpty(transaction.Item.ImageFileName)
                    ? $"/uploads/items/{transaction.Item.ImageFileName}"
                    : "/assets/image-uploaded.jpg",
                TokenAmount = transaction.TokenAmount,
                TransactionStatus = transaction.TransactionStatus,
                DeliveryMethod = transaction.DeliveryMethod,
                CreatedAt = transaction.CreatedAt,
                CompletedAt = transaction.CompletedAt,
                CancelledAt = transaction.CancelledAt,
                CancellationReason = transaction.CancellationReason
            };

            return View(transactionDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });
            var userId = user.Id;

            var stats = await _context.Transactions
                .Where(t => t.BuyerId == userId)
                .GroupBy(t => 1)
                .Select(g => new ClaimedItemsStatsDto
                {
                    TotalClaimed = g.Count(),
                    PendingCount = g.Count(t => t.TransactionStatus == TransactionStatus.Pending),
                    CompletedCount = g.Count(t => t.TransactionStatus == TransactionStatus.Confirmed), 
                    CancelledCount = g.Count(t => t.TransactionStatus == TransactionStatus.Cancelled),
                    InProgressCount = g.Count(t => t.TransactionStatus == TransactionStatus.Escrowed) 
                })
                .FirstOrDefaultAsync();

            return Json(new { success = true, stats = stats ?? new ClaimedItemsStatsDto() });
        }
    }
}