using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Notifications;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            var notifications = await _context.Notifications
                .Include(n => n.Type)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    TypeId = n.TypeId,
                    TypeName = n.Type.TypeName,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    ActionUrl = n.ActionUrl,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId,
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    Icon = n.Type.Icon,
                    Color = n.Type.Color,
                    TimeAgo = GetTimeAgo(n.CreatedAt),
                    NotificationCategory = GetNotificationCategory(n.Type.TypeName)
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Include(n => n.Type)
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    TypeId = n.TypeId,
                    TypeName = n.Type.TypeName,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    ActionUrl = n.ActionUrl,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId,
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    Icon = n.Type.Icon,
                    Color = n.Type.Color,
                    TimeAgo = GetTimeAgo(n.CreatedAt),
                    NotificationCategory = GetNotificationCategory(n.Type.TypeName)
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .Include(n => n.Type)
                .Where(n => n.NotificationId == notificationId && n.UserId == userId)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    TypeId = n.TypeId,
                    TypeName = n.Type.TypeName,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    ActionUrl = n.ActionUrl,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId,
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    Icon = n.Type.Icon,
                    Color = n.Type.Color,
                    TimeAgo = GetTimeAgo(n.CreatedAt),
                    NotificationCategory = GetNotificationCategory(n.Type.TypeName)
                })
                .FirstOrDefaultAsync();

            return notification;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating notification for user {UserId} with type {TypeId}", createDto.UserId, createDto.TypeId);

                var notification = new Notification
                {
                    UserId = createDto.UserId,
                    TypeId = createDto.TypeId,
                    Title = createDto.Title,
                    Message = createDto.Message,
                    ActionUrl = createDto.ActionUrl,
                    RelatedEntityType = createDto.RelatedEntityType,
                    RelatedEntityId = createDto.RelatedEntityId,
                    ExpiresAt = createDto.ExpiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification created successfully with ID {NotificationId}", notification.NotificationId);

                // Return the created notification with type information
                var createdNotification = await GetNotificationByIdAsync(notification.NotificationId, notification.UserId);
                return createdNotification!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", createDto.UserId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        // Test-specific methods
        public async Task<NotificationDto> CreateTestTransactionNotificationAsync(int userId, string transactionType, string itemName)
        {
            try
            {
                _logger.LogInformation("Creating test transaction notification: userId={UserId}, type={TransactionType}, item={ItemName}", userId, transactionType, itemName);

                var typeId = transactionType.ToLower() switch
                {
                    "success" or "completed" => await GetTypeIdByNameAsync("Transaction Completed"),
                    "cancel" or "cancelled" => await GetTypeIdByNameAsync("Transaction Cancelled"),
                    "confirmation" or "confirm" => await GetTypeIdByNameAsync("Transaction Confirmation"),
                    _ => await GetTypeIdByNameAsync("Transaction Completed")
                };

                if (typeId == 0)
                {
                    throw new InvalidOperationException("Could not find matching notification type");
                }

                var (title, message) = transactionType.ToLower() switch
                {
                    "success" or "completed" => (
                        "Transaction Completed Successfully",
                        $"Your transaction for \"{itemName}\" has been completed successfully. Tokens have been transferred."
                    ),
                    "cancel" or "cancelled" => (
                        "Transaction Cancelled",
                        $"The transaction for \"{itemName}\" has been cancelled. Your tokens have been refunded."
                    ),
                    "confirmation" or "confirm" => (
                        "Confirmation Meet-up Transaction",
                        $"You have selected Meet-up as the transaction method for the item: \"{itemName}\". Please confirm completion."
                    ),
                    _ => (
                        "Transaction Update",
                        $"There's an update regarding your transaction for \"{itemName}\"."
                    )
                };

                var createDto = new CreateNotificationDto
                {
                    UserId = userId,
                    TypeId = typeId,
                    Title = title,
                    Message = message,
                    RelatedEntityType = RelatedEntityType.Transaction,
                    RelatedEntityId = new Random().Next(1, 1000)
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTestTransactionNotificationAsync");
                throw;
            }
        }

        public async Task<NotificationDto> CreateTestSocialNotificationAsync(int userId, string socialType, string username, string postTitle = "")
        {
            try
            {
                var typeId = socialType.ToLower() switch
                {
                    "like" or "love" or "reaction" => await GetTypeIdByNameAsync("Post Reaction"),
                    "comment" => await GetTypeIdByNameAsync("Post Comment"),
                    "reply" => await GetTypeIdByNameAsync("Comment Reply"),
                    _ => await GetTypeIdByNameAsync("Post Reaction")
                };

                if (typeId == 0)
                {
                    throw new InvalidOperationException("Could not find matching notification type");
                }

                var (title, message) = socialType.ToLower() switch
                {
                    "like" => (
                        "New Reaction on Your Post",
                        $"{username} liked your post" + (!string.IsNullOrEmpty(postTitle) ? $" about \"{postTitle}\"" : "")
                    ),
                    "love" => (
                        "New Reaction on Your Post",
                        $"{username} loved your post" + (!string.IsNullOrEmpty(postTitle) ? $" about \"{postTitle}\"" : "")
                    ),
                    "reaction" => (
                        "New Reaction on Your Post",
                        $"{username} reacted to your post" + (!string.IsNullOrEmpty(postTitle) ? $" about \"{postTitle}\"" : "")
                    ),
                    "comment" => (
                        "New Comment on Your Post",
                        $"{username} commented on your post" + (!string.IsNullOrEmpty(postTitle) ? $" about \"{postTitle}\"" : "") + ": \"This looks amazing! Is it still available?\""
                    ),
                    "reply" => (
                        "New Reply to Your Comment",
                        $"{username} replied to your comment" + (!string.IsNullOrEmpty(postTitle) ? $" on \"{postTitle}\"" : "")
                    ),
                    _ => (
                        "Social Activity",
                        $"{username} interacted with your content"
                    )
                };

                var createDto = new CreateNotificationDto
                {
                    UserId = userId,
                    TypeId = typeId,
                    Title = title,
                    Message = message,
                    RelatedEntityType = RelatedEntityType.Item,
                    RelatedEntityId = new Random().Next(1, 1000)
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTestSocialNotificationAsync");
                throw;
            }
        }

        public async Task<NotificationDto> CreateTestGamificationNotificationAsync(int userId, string gamificationType, string details)
        {
            try
            {
                var typeId = gamificationType.ToLower() switch
                {
                    "quest" => await GetTypeIdByNameAsync("Quest Completed"),
                    "achievement" => await GetTypeIdByNameAsync("Achievement Unlocked"),
                    "levelup" or "level" => await GetTypeIdByNameAsync("Level Up"),
                    "leaderboard" => await GetTypeIdByNameAsync("Leaderboard Update"),
                    _ => await GetTypeIdByNameAsync("Quest Completed")
                };

                if (typeId == 0)
                {
                    throw new InvalidOperationException("Could not find matching notification type");
                }

                var (title, message) = gamificationType.ToLower() switch
                {
                    "quest" => (
                        "Quest Completed! 🏆",
                        $"Congratulations! You've completed the quest: \"{details}\". Rewards have been added to your account."
                    ),
                    "achievement" => (
                        "Achievement Unlocked! 🥇",
                        $"You've unlocked the achievement: \"{details}\". Great job!"
                    ),
                    "levelup" or "level" => (
                        "Level Up! ⬆️",
                        $"Congratulations! You've reached {details}. New features and rewards are now available!"
                    ),
                    "leaderboard" => (
                        "Leaderboard Update 📊",
                        $"Your ranking has changed! {details}"
                    ),
                    _ => (
                        "Gamification Update",
                        $"You have a new gamification update: {details}"
                    )
                };

                var createDto = new CreateNotificationDto
                {
                    UserId = userId,
                    TypeId = typeId,
                    Title = title,
                    Message = message,
                    RelatedEntityType = RelatedEntityType.Quest,
                    RelatedEntityId = new Random().Next(1, 100)
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTestGamificationNotificationAsync");
                throw;
            }
        }

        public async Task<NotificationDto> CreateTestCommunicationNotificationAsync(int userId, string communicationType, string senderName)
        {
            try
            {
                var typeId = communicationType.ToLower() switch
                {
                    "message" => await GetTypeIdByNameAsync("New Message"),
                    "interest" => await GetTypeIdByNameAsync("Interest Expressed"),
                    _ => await GetTypeIdByNameAsync("New Message")
                };

                if (typeId == 0)
                {
                    throw new InvalidOperationException("Could not find matching notification type");
                }

                var (title, message) = communicationType.ToLower() switch
                {
                    "message" => (
                        "New Message Received",
                        $"You have a new message from {senderName}. Tap to view the conversation."
                    ),
                    "interest" => (
                        "Someone is Interested!",
                        $"{senderName} has expressed interest in your item. Start a conversation to discuss details."
                    ),
                    _ => (
                        "New Communication",
                        $"{senderName} is trying to reach you."
                    )
                };

                var createDto = new CreateNotificationDto
                {
                    UserId = userId,
                    TypeId = typeId,
                    Title = title,
                    Message = message,
                    RelatedEntityType = RelatedEntityType.User,
                    RelatedEntityId = new Random().Next(1, 1000)
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTestCommunicationNotificationAsync");
                throw;
            }
        }

        public async Task<List<NotificationDto>> CreateBulkTestNotificationsAsync(int userId, int count)
        {
            var notifications = new List<NotificationDto>();
            var random = new Random();

            var notificationTypes = new[]
            {
                ("transaction", "success"),
                ("transaction", "cancel"),
                ("transaction", "confirmation"),
                ("social", "like"),
                ("social", "comment"),
                ("social", "reply"),
                ("gamification", "quest"),
                ("gamification", "achievement"),
                ("gamification", "levelup"),
                ("communication", "message"),
                ("communication", "interest")
            };

            var sampleUsernames = new[] { "Ana Santos", "Juan Dela Cruz", "Maria Rodriguez", "Carlos Miguel", "Sofia Chen", "Miguel Torres" };
            var sampleItems = new[] { "Wireless Headphones", "Vintage Camera", "Gaming Mouse", "Bluetooth Speaker", "Laptop Stand", "Coffee Maker" };
            var samplePosts = new[] { "Vintage Camera Collection", "Gaming Setup Showcase", "Coffee Corner Design", "Book Organization Tips" };
            var sampleQuests = new[] { "Daily Login Streak", "First Sale Achievement", "Community Helper", "Photo Upload Master" };

            for (int i = 0; i < count; i++)
            {
                try
                {
                    var (category, type) = notificationTypes[random.Next(notificationTypes.Length)];
                    var username = sampleUsernames[random.Next(sampleUsernames.Length)];
                    var item = sampleItems[random.Next(sampleItems.Length)];
                    var post = samplePosts[random.Next(samplePosts.Length)];
                    var quest = sampleQuests[random.Next(sampleQuests.Length)];

                    NotificationDto notification = category switch
                    {
                        "transaction" => await CreateTestTransactionNotificationAsync(userId, type, item),
                        "social" => await CreateTestSocialNotificationAsync(userId, type, username, post),
                        "gamification" => await CreateTestGamificationNotificationAsync(userId, type, type == "quest" ? quest : $"Level {random.Next(2, 20)}"),
                        "communication" => await CreateTestCommunicationNotificationAsync(userId, type, username),
                        _ => await CreateTestTransactionNotificationAsync(userId, "success", item)
                    };

                    notifications.Add(notification);

                    // Add some delay to create different timestamps
                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating bulk notification {Index}", i);
                    // Continue with the next notification
                }
            }

            return notifications;
        }

        public async Task<bool> ClearAllTestNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper methods
        private async Task<int> GetTypeIdByNameAsync(string typeName)
        {
            try
            {
                var type = await _context.NotificationTypes
                    .FirstOrDefaultAsync(nt => nt.TypeName == typeName);

                if (type == null)
                {
                    _logger.LogWarning("Notification type '{TypeName}' not found", typeName);
                    return 0;
                }

                return type.TypeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification type ID for '{TypeName}'", typeName);
                return 0;
            }
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            return timeSpan.TotalSeconds switch
            {
                < 60 => "Just now",
                < 3600 => $"{(int)(timeSpan.TotalMinutes)} minutes ago",
                < 86400 => $"{(int)(timeSpan.TotalHours)} hours ago",
                < 2592000 => $"{(int)(timeSpan.TotalDays)} days ago",
                _ => dateTime.ToString("MMM dd, yyyy")
            };
        }

        private static string GetNotificationCategory(string typeName)
        {
            return typeName.ToLower() switch
            {
                var t when t.Contains("transaction") => "transaction",
                var t when t.Contains("post") || t.Contains("comment") => "social",
                var t when t.Contains("quest") || t.Contains("achievement") || t.Contains("level") || t.Contains("leaderboard") => "gamification",
                var t when t.Contains("message") || t.Contains("interest") => "communication",
                _ => "system"
            };
        }
    }
}