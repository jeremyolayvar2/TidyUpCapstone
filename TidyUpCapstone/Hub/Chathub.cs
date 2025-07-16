using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities;

namespace TidyUpCapstone.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task Send(string message, string senderId, string buyerId, string sellerId, int itemPostId)
        {
            try
            {
                // Create and save the message to database
                var messageEntity = new Messages
                {
                    Text = message,
                    SenderId = senderId,
                    BuyerId = buyerId,
                    SellerId = sellerId,
                    ItemPostId = itemPostId,
                    Timestamp = DateTime.UtcNow
                };

                _context.Message.Add(messageEntity);
                await _context.SaveChangesAsync();

                // Create a group name for this conversation
                string groupName = GetGroupName(buyerId, sellerId, itemPostId);

                // Send message to all users in this conversation group
                await Clients.Group(groupName).SendAsync("ReceiveMessage", message, messageEntity.Timestamp, senderId);
            }
            catch (Exception ex)
            {
                // Log the error (you should inject ILogger for proper logging)
                Console.WriteLine($"Error saving message: {ex.Message}");
                throw;
            }
        }

        public async Task StartChat(string buyerId, string sellerId, int itemPostId)
        {
            try
            {
                string groupName = GetGroupName(buyerId, sellerId, itemPostId);

                // Add both users to the conversation group
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Notify the seller that a new chat has started
                await Clients.User(sellerId).SendAsync("NewChatStarted", buyerId, itemPostId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting chat: {ex.Message}");
                throw;
            }
        }

        public async Task JoinConversation(string buyerId, string sellerId, int itemPostId)
        {
            string groupName = GetGroupName(buyerId, sellerId, itemPostId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveConversation(string buyerId, string sellerId, int itemPostId)
        {
            string groupName = GetGroupName(buyerId, sellerId, itemPostId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private static string GetGroupName(string buyerId, string sellerId, int itemPostId)
        {
            // Create a consistent group name regardless of who creates it
            var ids = new[] { buyerId, sellerId }.OrderBy(x => x).ToArray();
            return $"chat_{ids[0]}_{ids[1]}_{itemPostId}";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                // Load user's existing conversations and add them to appropriate groups
                var userConversations = await _context.Message
                    .Where(m => m.BuyerId == userId || m.SellerId == userId)
                    .Select(m => new { m.BuyerId, m.SellerId, m.ItemPostId })
                    .Distinct()
                    .ToListAsync();

                foreach (var conv in userConversations)
                {
                    string groupName = GetGroupName(conv.BuyerId, conv.SellerId, conv.ItemPostId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Groups are automatically cleaned up when user disconnects
            await base.OnDisconnectedAsync(exception);
        }
    }
}