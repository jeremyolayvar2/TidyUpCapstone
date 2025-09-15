using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Join transaction group for real-time updates
        public async Task JoinTransactionGroup(string transactionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Transaction_{transactionId}");
            _logger.LogInformation($"User {Context.ConnectionId} joined transaction {transactionId}");
        }
        // Leave transaction group
        public async Task LeaveTransactionGroup(string transactionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Transaction_{transactionId}");
            _logger.LogInformation($"User {Context.ConnectionId} left transaction {transactionId}");
        }
        // Send transaction method selection
        public async Task SendTransactionMethodSelected(int transactionId, int userId, string method, object status)
        {
            await Clients.Group($"Transaction_{transactionId}").SendAsync("TransactionMethodSelected", new
            {
                transactionId,
                userId,
                method,
                status,
                timestamp = DateTime.UtcNow
            });
        }

        // Send transaction confirmation
        public async Task SendTransactionConfirmed(int transactionId, int userId, object status)
        {
            await Clients.Group($"Transaction_{transactionId}").SendAsync("TransactionConfirmed", new
            {
                transactionId,
                userId,
                status,
                timestamp = DateTime.UtcNow
            });
        }

        // Send transaction cancellation
        public async Task SendTransactionCancelled(int transactionId, int userId, string reason, object status)
        {
            await Clients.Group($"Transaction_{transactionId}").SendAsync("TransactionCancelled", new
            {
                transactionId,
                userId,
                reason,
                status,
                timestamp = DateTime.UtcNow
            });
        }

        // Send delivery order created
        public async Task SendDeliveryOrderCreated(int transactionId, object orderData)
        {
            await Clients.Group($"Transaction_{transactionId}").SendAsync("DeliveryOrderCreated", new
            {
                transactionId,
                order = orderData,
                timestamp = DateTime.UtcNow
            });
        }
        // Join a specific chat room
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
            _logger.LogInformation($"User {Context.ConnectionId} joined chat {chatId}");
        }

        // Leave a specific chat room
        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
            _logger.LogInformation($"User {Context.ConnectionId} left chat {chatId}");
        }

        // Send a message to a specific chat
        public async Task SendMessage(int chatId, int senderId, string message)
        {
            try
            {
                // Validate the message
                if (string.IsNullOrWhiteSpace(message))
                {
                    await Clients.Caller.SendAsync("Error", "Message cannot be empty");
                    return;
                }

                // Get sender information
                var sender = await _context.Users.FindAsync(senderId);
                if (sender == null)
                {
                    await Clients.Caller.SendAsync("Error", "Sender not found");
                    return;
                }

                // Verify chat exists
                var chat = await _context.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    await Clients.Caller.SendAsync("Error", "Chat not found");
                    return;
                }

                // Create and save the message
                var chatMessage = new ChatMessage
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    Message = message.Trim(),
                    MessageType = MessageType.Text,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.ChatMessages.Add(chatMessage);

                // Update chat's last message time
                chat.LastMessageTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Prepare message data for broadcast
                var messageData = new
                {
                    messageId = chatMessage.MessageId,
                    chatId = chatMessage.ChatId,
                    senderId = chatMessage.SenderId,
                    senderName = sender.UserName,
                    message = chatMessage.Message,
                    messageType = chatMessage.MessageType.ToString(),
                    sentAt = chatMessage.SentAt,
                    isRead = chatMessage.IsRead
                };

                // Broadcast to all users in this chat room
                await Clients.Group($"Chat_{chatId}").SendAsync("ReceiveMessage", messageData);

                _logger.LogInformation($"Message sent in chat {chatId} by user {senderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message in SignalR");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        // Mark messages as read
        public async Task MarkMessagesAsRead(int chatId, int userId)
        {
            try
            {
                var unreadMessages = await _context.ChatMessages
                    .Where(m => m.ChatId == chatId && m.SenderId != userId && !m.IsRead)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // Notify other users that messages have been read
                await Clients.Group($"Chat_{chatId}").SendAsync("MessagesRead", new { chatId, userId, messageIds = unreadMessages.Select(m => m.MessageId).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
            }
        }

        // User typing indicator
        public async Task UserTyping(int chatId, int userId, string userName, bool isTyping)
        {
            try
            {
                // Notify other users in the chat that someone is typing
                await Clients.OthersInGroup($"Chat_{chatId}").SendAsync("UserTyping", new { chatId, userId, userName, isTyping });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator");
            }
        }

        // User online status
        public async Task UpdateOnlineStatus(int userId, bool isOnline)
        {
            try
            {
                // You can implement user online status tracking here
                // For now, we'll just broadcast the status
                await Clients.All.SendAsync("UserStatusChanged", new { userId, isOnline });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating online status");
            }
        }

        // Connection events
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Test method for development
        public async Task TestConnection()
        {
            await Clients.Caller.SendAsync("TestResponse", "SignalR connection working!");
        }
    }
}