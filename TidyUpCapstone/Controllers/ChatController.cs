using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.Reporting;
using TidyUpCapstone.Hubs;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUp.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ChatController> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            ILogger<ChatController> logger,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId") ?? 1;

                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    return Json(new { success = false, message = "Test user not found" });

                // Find existing chat between users
                var chat = await _context.Chats
                    .Include(c => c.Transaction)
                    .FirstOrDefaultAsync(c =>
                        (c.Transaction.BuyerId == currentUserId && c.Transaction.SellerId == otherUserId) ||
                        (c.Transaction.BuyerId == otherUserId && c.Transaction.SellerId == currentUserId));

                if (chat == null)
                {
                    // Return empty messages if no chat exists yet
                    return Json(new { success = true, messages = new List<object>(), chatId = (int?)null });
                }

                var messages = await _context.ChatMessages
                    .Where(m => m.ChatId == chat.ChatId)
                    .Include(m => m.Sender)
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        messageId = m.MessageId,
                        message = m.Message,
                        senderId = m.SenderId,
                        senderName = m.Sender.UserName,
                        sentAt = m.SentAt,
                        isCurrentUser = m.SenderId == currentUserId,
                        isRead = m.IsRead
                    })
                    .ToListAsync();

                return Json(new { success = true, messages, chatId = chat.ChatId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages");
                return Json(new { success = false, message = "Error loading messages: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId") ?? 1;

                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser == null)
                    return Json(new { success = false, message = "Test user not found" });

                if (string.IsNullOrWhiteSpace(request.Message))
                    return Json(new { success = false, message = "Message cannot be empty" });

                // Find existing chat or use the seeded one
                var chat = await _context.Chats
                    .Include(c => c.Transaction)
                    .FirstOrDefaultAsync(c =>
                        (c.Transaction.BuyerId == currentUserId && c.Transaction.SellerId == request.RecipientId) ||
                        (c.Transaction.BuyerId == request.RecipientId && c.Transaction.SellerId == currentUserId));

                if (chat == null)
                {
                    // Create a new chat/transaction for testing
                    chat = await CreateNewChatAsync(currentUserId, request.RecipientId);
                    if (chat == null)
                    {
                        return Json(new { success = false, message = "Could not create chat" });
                    }
                }

                var chatMessage = new ChatMessage
                {
                    ChatId = chat.ChatId,
                    SenderId = currentUserId,
                    Message = request.Message.Trim(),
                    MessageType = MessageType.Text,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.ChatMessages.Add(chatMessage);

                // Update chat's last message time
                chat.LastMessageTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Prepare message data for SignalR broadcast
                var messageData = new
                {
                    messageId = chatMessage.MessageId,
                    chatId = chatMessage.ChatId,
                    senderId = chatMessage.SenderId,
                    senderName = currentUser.UserName,
                    message = chatMessage.Message,
                    messageType = chatMessage.MessageType.ToString(),
                    sentAt = chatMessage.SentAt,
                    isRead = chatMessage.IsRead
                };

                // Broadcast via SignalR to all users in this chat
                await _hubContext.Clients.Group($"Chat_{chat.ChatId}").SendAsync("ReceiveMessage", messageData);

                return Json(new
                {
                    success = true,
                    messageId = chatMessage.MessageId,
                    message = chatMessage.Message,
                    senderId = chatMessage.SenderId,
                    senderName = currentUser.UserName,
                    sentAt = chatMessage.SentAt,
                    chatId = chat.ChatId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return Json(new { success = false, message = "Error sending message: " + ex.Message });
            }
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ReportMessage([FromBody] ReportMessageRequest request)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId") ?? 1;
                var currentUser = await _context.Users.FindAsync(currentUserId);

                if (currentUser == null)
                    return Json(new { success = false, message = "Test user not found" });

                var message = await _context.ChatMessages
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.MessageId == request.MessageId);

                if (message == null)
                    return Json(new { success = false, message = "Message not found" });

                if (message.SenderId == currentUserId)
                    return Json(new { success = false, message = "Cannot report your own message" });

                var existingReport = await _context.UserReports
                    .FirstOrDefaultAsync(r => r.ReporterId == currentUserId
                                            && r.ReportedEntityType == ReportedEntityType.Chat
                                            && r.ReportedEntityId == request.MessageId);

                if (existingReport != null)
                    return Json(new { success = false, message = "You have already reported this message" });

                // PARSE THE ENUM SAFELY
                if (!Enum.TryParse<ReportReason>(request.Reason, true, out var parsedReason))
                {
                    return Json(new { success = false, message = "Invalid report reason" });
                }

                var report = new UserReport
                {
                    ReporterId = currentUserId,
                    ReportedUserId = message.SenderId,
                    ReportedEntityType = ReportedEntityType.Chat,
                    ReportedEntityId = request.MessageId,
                    Reason = parsedReason, // Use parsed enum
                    Description = request.Description ?? string.Empty,
                    DateSubmitted = DateTime.UtcNow,
                    ReportStatus = ReportStatus.Pending,
                    Priority = ReportPriority.Medium
                };

                _context.UserReports.Add(report);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Message reported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting message");
                return Json(new { success = false, message = "Error reporting message: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SwitchTestUser([FromBody] SwitchUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                HttpContext.Session.SetInt32("CurrentTestUserId", request.UserId);

                return Json(new
                {
                    success = true,
                    userId = user.Id,
                    userName = user.UserName,
                    message = $"Test user switched to {user.UserName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching test user");
                return Json(new { success = false, message = "Error switching user: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkAsReadRequest request)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId") ?? 1;

                var unreadMessages = await _context.ChatMessages
                    .Where(m => m.ChatId == request.ChatId && m.SenderId != currentUserId && !m.IsRead)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // Notify via SignalR
                await _hubContext.Clients.Group($"Chat_{request.ChatId}").SendAsync("MessagesRead",
                    new { chatId = request.ChatId, userId = currentUserId, messageIds = unreadMessages.Select(m => m.MessageId).ToList() });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<Chat?> CreateNewChatAsync(int userId1, int userId2)
        {
            try
            {
                // Get any available item for the transaction
                var sampleItem = await _context.Items.FirstOrDefaultAsync();
                if (sampleItem == null)
                {
                    // Create a minimal item for testing
                    sampleItem = new Item
                    {
                        UserId = userId2,
                        ItemTitle = "Test Item for Chat",
                        Description = "Created for testing chat functionality",
                        CategoryId = 1,
                        ConditionId = 1,
                        LocationId = 1,
                        AdjustedTokenPrice = 10.00m,
                        FinalTokenPrice = 10.00m,
                        Status = ItemStatus.Available,
                        DatePosted = DateTime.UtcNow,
                        ViewCount = 0
                    };

                    _context.Items.Add(sampleItem);
                    await _context.SaveChangesAsync();
                }

                var transaction = new Transaction
                {
                    BuyerId = userId1,
                    SellerId = userId2,
                    ItemId = sampleItem.ItemId,
                    TokenAmount = 10.00m,
                    TransactionStatus = TransactionStatus.Pending,
                    DeliveryMethod = DeliveryMethod.Pickup,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var chat = new Chat
                {
                    TransactionId = transaction.TransactionId,
                    StartTime = DateTime.UtcNow,
                    LastMessageTime = DateTime.UtcNow,
                    EscrowAmount = 10.00m,
                    EscrowStatus = EscrowStatus.Held
                };

                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();

                return chat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new chat");
                return null;
            }
        }
    }

    // Request models
    public class SendMessageRequest
    {
        public int RecipientId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ReportMessageRequest
    {
        public int MessageId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class SwitchUserRequest
    {
        public int UserId { get; set; }
    }

    public class MarkAsReadRequest
    {
        public int ChatId { get; set; }
    }
}