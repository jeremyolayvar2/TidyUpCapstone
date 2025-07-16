using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Entities;
using TidyUpCapstone.Data;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _messageContext;

        public ChatController(
            ILogger<ChatController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext messageContext)
        {
            _logger = logger;
            _userManager = userManager;
            _messageContext = messageContext;
        }

        public IActionResult Index()
        {
            var messages = _messageContext.Message.OrderBy(m => m.Timestamp).ToList();
            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string buyerId, string sellerId, int postId)
        {
            try
            {
                var messages = await _messageContext.Message
                    .Include(m => m.Sender)
                    .Where(m =>
                        m.ItemPostId == postId &&
                        ((m.BuyerId == buyerId && m.SellerId == sellerId) ||
                         (m.BuyerId == sellerId && m.SellerId == buyerId))
                    )
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new {
                        m.Text,
                        m.SenderId,
                        SenderName = m.Sender.UserName,
                        Timestamp = m.Timestamp.ToLocalTime().ToString("t")
                    })
                    .ToListAsync();

                return Json(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages");
                return StatusCode(500, new { message = "Error loading messages" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatPreview(string buyerId, int postId)
        {
            try
            {
                var buyer = await _userManager.FindByIdAsync(buyerId);
                var item = await _messageContext.ItemPosts
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == postId);

                if (buyer == null || item == null)
                    return NotFound();

                return Json(new
                {
                    buyerId = buyer.Id,
                    buyerName = buyer.UserName,
                    sellerId = item.UserId,
                    sellerName = item.User?.UserName,
                    postId = item.Id,
                    itemTitle = item.ItemTitle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat preview");
                return StatusCode(500, new { message = "Error loading chat preview" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            try
            {
                var currentUserId = _userManager.GetUserId(User);

                var chats = await _messageContext.Message
                    .Include(m => m.Buyer)
                    .Include(m => m.Seller)
                    .Include(m => m.ItemPost)
                    .Where(m => m.BuyerId == currentUserId || m.SellerId == currentUserId)
                    .GroupBy(m => new { m.BuyerId, m.SellerId, m.ItemPostId })
                    .Select(g => new
                    {
                        BuyerId = g.Key.BuyerId,
                        SellerId = g.Key.SellerId,
                        PostId = g.Key.ItemPostId,
                        LastMessage = g.OrderByDescending(m => m.Timestamp).First(),
                        ItemTitle = g.First().ItemPost.ItemTitle,
                        BuyerName = g.First().Buyer.UserName,
                        SellerName = g.First().Seller.UserName,
                        LastMessageTime = g.OrderByDescending(m => m.Timestamp).First().Timestamp
                    })
                    .OrderByDescending(c => c.LastMessageTime)
                    .ToListAsync();

                return Json(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user chats");
                return StatusCode(500, new { message = "Error loading chats" });
            }
        }
    }
}