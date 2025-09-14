using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Notifications;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers.Api
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        private readonly ApplicationDbContext _context;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger, ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _logger = logger;
            _context = context;
        }

        // GET: api/notification/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/notification/user/{userId}/unread
        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadNotifications(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/notification/user/{userId}/count
        [HttpGet("user/{userId}/count")]
        public async Task<ActionResult<int>> GetUnreadCount(int userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // PUT: api/notification/user/{userId}/mark-read/{notificationId}
        [HttpPut("user/{userId}/mark-read/{notificationId}")]
        public async Task<ActionResult> MarkNotificationAsRead(int userId, int notificationId)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(notificationId, userId);
                if (success)
                {
                    return Ok(new { success = true, message = "Notification marked as read" });
                }
                return NotFound(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        // PUT: api/notification/user/{userId}/mark-all-read
        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead(int userId)
        {
            try
            {
                var success = await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { success = true, message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        // DELETE: api/notification/{notificationId}/user/{userId}
        [HttpDelete("{notificationId}/user/{userId}")]
        public async Task<ActionResult> DeleteNotification(int notificationId, int userId)
        {
            try
            {
                var success = await _notificationService.DeleteNotificationAsync(notificationId, userId);
                if (success)
                {
                    return Ok(new { success = true, message = "Notification deleted" });
                }
                return NotFound(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", notificationId, userId);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        // TESTING ENDPOINTS

        // POST: api/notification/test/transaction
        [HttpPost("test/transaction")]
        public async Task<ActionResult> CreateTestTransactionNotification([FromBody] TestNotificationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating test transaction notification: {@Request}", request);

                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                if (request.Count <= 0) request.Count = 1;

                var userId = await GetFirstAvailableUserIdAsync();

                if (request.Count == 1)
                {
                    var notification = await _notificationService.CreateTestTransactionNotificationAsync(
                        userId,
                        request.SubType ?? "success",
                        request.ItemName ?? "Test Item"
                    );
                    return Ok(notification);
                }
                else
                {
                    var notifications = new List<NotificationDto>();
                    for (int i = 0; i < request.Count; i++)
                    {
                        var notification = await _notificationService.CreateTestTransactionNotificationAsync(
                            userId,
                            request.SubType ?? "success",
                            $"{request.ItemName ?? "Test Item"} #{i + 1}"
                        );
                        notifications.Add(notification);
                    }
                    return Ok(notifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test transaction notification: {Error}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/notification/test/social
        [HttpPost("test/social")]
        public async Task<ActionResult> CreateTestSocialNotification([FromBody] TestNotificationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating test social notification: {@Request}", request);

                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                if (request.Count <= 0) request.Count = 1;

                var userId = await GetFirstAvailableUserIdAsync();

                if (request.Count == 1)
                {
                    var notification = await _notificationService.CreateTestSocialNotificationAsync(
                        userId,
                        request.SubType ?? "like",
                        request.Username ?? "Test User",
                        request.PostTitle ?? "Test Post"
                    );
                    return Ok(notification);
                }
                else
                {
                    var notifications = new List<NotificationDto>();
                    var usernames = new[] { "Ana Santos", "Juan Dela Cruz", "Maria Rodriguez", "Carlos Miguel", "Sofia Chen" };
                    var posts = new[] { "Vintage Camera Collection", "Gaming Setup", "Coffee Corner", "Book Organization" };
                    var random = new Random();

                    for (int i = 0; i < request.Count; i++)
                    {
                        var notification = await _notificationService.CreateTestSocialNotificationAsync(
                            userId,
                            request.SubType ?? "like",
                            usernames[random.Next(usernames.Length)],
                            posts[random.Next(posts.Length)]
                        );
                        notifications.Add(notification);
                    }
                    return Ok(notifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test social notification: {Error}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/notification/test/gamification
        [HttpPost("test/gamification")]
        public async Task<ActionResult> CreateTestGamificationNotification([FromBody] TestNotificationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating test gamification notification: {@Request}", request);

                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                if (request.Count <= 0) request.Count = 1;

                var userId = await GetFirstAvailableUserIdAsync();

                if (request.Count == 1)
                {
                    var notification = await _notificationService.CreateTestGamificationNotificationAsync(
                        userId,
                        request.SubType ?? "quest",
                        request.Details ?? "Test Achievement"
                    );
                    return Ok(notification);
                }
                else
                {
                    var notifications = new List<NotificationDto>();
                    var achievements = new[] { "First Sale", "Community Helper", "Photo Master", "Daily Streak", "Top Seller" };
                    var quests = new[] { "Daily Login", "Upload 5 Photos", "Complete Transaction", "Help Community Member" };
                    var random = new Random();

                    for (int i = 0; i < request.Count; i++)
                    {
                        string detail = (request.SubType ?? "quest").ToLower() switch
                        {
                            "quest" => quests[random.Next(quests.Length)],
                            "achievement" => achievements[random.Next(achievements.Length)],
                            "levelup" or "level" => $"Level {random.Next(2, 20)}",
                            "leaderboard" => $"You moved up to #{random.Next(1, 50)} in Weekly Sales",
                            _ => "Test Detail"
                        };

                        var notification = await _notificationService.CreateTestGamificationNotificationAsync(
                            userId,
                            request.SubType ?? "quest",
                            detail
                        );
                        notifications.Add(notification);
                    }
                    return Ok(notifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test gamification notification: {Error}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/notification/test/communication
        [HttpPost("test/communication")]
        public async Task<ActionResult> CreateTestCommunicationNotification([FromBody] TestNotificationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating test communication notification: {@Request}", request);

                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                if (request.Count <= 0) request.Count = 1;

                var userId = await GetFirstAvailableUserIdAsync();

                if (request.Count == 1)
                {
                    var notification = await _notificationService.CreateTestCommunicationNotificationAsync(
                        userId,
                        request.SubType ?? "message",
                        request.Username ?? "Test User"
                    );
                    return Ok(notification);
                }
                else
                {
                    var notifications = new List<NotificationDto>();
                    var usernames = new[] { "Ana Santos", "Juan Dela Cruz", "Maria Rodriguez", "Carlos Miguel", "Sofia Chen", "Miguel Torres" };
                    var random = new Random();

                    for (int i = 0; i < request.Count; i++)
                    {
                        var notification = await _notificationService.CreateTestCommunicationNotificationAsync(
                            userId,
                            request.SubType ?? "message",
                            usernames[random.Next(usernames.Length)]
                        );
                        notifications.Add(notification);
                    }
                    return Ok(notifications);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test communication notification: {Error}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/notification/test/bulk
        [HttpPost("test/bulk")]
        public async Task<ActionResult> CreateBulkTestNotifications([FromBody] TestNotificationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating bulk test notifications: {@Request}", request);

                if (request == null)
                {
                    return BadRequest(new { error = "Request body is required" });
                }

                var count = Math.Max(1, Math.Min(request.Count, 50));
                var userId = await GetFirstAvailableUserIdAsync();
                var notifications = await _notificationService.CreateBulkTestNotificationsAsync(userId, count);

                return Ok(new
                {
                    message = $"Created {notifications.Count} test notifications",
                    notifications = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk test notifications: {Error}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/notification/test/summary
        [HttpGet("test/summary")]
        public async Task<ActionResult<NotificationSummaryDto>> GetNotificationSummary()
        {
            try
            {
                var userId = await GetFirstAvailableUserIdAsync();
                var allNotifications = await _notificationService.GetUserNotificationsAsync(userId, 1, 1000);
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

                var today = DateTime.UtcNow.Date;
                var thisWeek = DateTime.UtcNow.AddDays(-7);

                var summary = new NotificationSummaryDto
                {
                    TotalNotifications = allNotifications.Count(),
                    UnreadCount = unreadCount,
                    TodayCount = allNotifications.Count(n => n.CreatedAt.Date == today),
                    ThisWeekCount = allNotifications.Count(n => n.CreatedAt >= thisWeek),
                    TypeStats = allNotifications
                        .GroupBy(n => n.TypeName)
                        .Select(g => new NotificationTypeStatsDto
                        {
                            TypeName = g.Key,
                            Count = g.Count(),
                            UnreadCount = g.Count(n => !n.IsRead)
                        })
                        .OrderByDescending(s => s.Count)
                        .ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification summary: {Error}", ex.Message);
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // DELETE: api/notification/user/{userId}/clear-all
        [HttpDelete("user/{userId}/clear-all")]
        public async Task<ActionResult> ClearAllNotifications(int userId)
        {
            try
            {
                var success = await _notificationService.ClearAllTestNotificationsAsync(userId);
                return Ok(new { success = true, message = $"All notifications cleared for user {userId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all notifications for user {UserId}: {Error}", userId, ex.Message);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        // GET: api/notification/debug/users
        [HttpGet("debug/users")]
        public async Task<ActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users.Select(u => new {
                    id = u.Id,
                    username = u.UserName,
                    email = u.Email
                }).ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/notification/test/ping
        [HttpGet("test/ping")]
        public ActionResult Ping()
        {
            return Ok(new { message = "Notification API is working!", timestamp = DateTime.UtcNow });
        }

        // Helper method to get first available user ID
        private async Task<int> GetFirstAvailableUserIdAsync()
        {
            var user = await _context.Users.FirstAsync();
            return user.Id;
        }
    }
}