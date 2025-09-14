using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Models.ViewModels.Notifications;
using TidyUpCapstone.Models.DTOs.Notifications;

namespace TidyUpCapstone.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: /Notification
        public async Task<IActionResult> Index(int page = 1, string filter = "all")
        {
            try
            {
                var userId = GetCurrentUserId(); // We'll use test user for now
                var pageSize = 20;

                var viewModel = new NotificationPageViewModel();

                if (filter == "unread")
                {
                    var unreadNotifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                    viewModel.Notifications = unreadNotifications.ToList();
                    viewModel.HasUnread = unreadNotifications.Any();
                }
                else
                {
                    var allNotifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                    viewModel.Notifications = allNotifications.ToList();
                    var unreadNotifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                    viewModel.HasUnread = unreadNotifications.Any();
                }

                viewModel.UnreadCount = await _notificationService.GetUnreadCountAsync(userId);
                viewModel.CurrentFilter = filter;
                viewModel.CurrentPage = page;
                viewModel.HasNotifications = viewModel.Notifications.Any();

                // Add testing data for development
                viewModel.TestingPanel = new NotificationTestingViewModel
                {
                    IsEnabled = true, // Enable for development
                    TestUserId = userId,
                    AvailableUsers = GetTestUsers(),
                    NotificationTypes = GetNotificationTypes(),
                    RecentTestActions = new List<string>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications page");
                TempData["Error"] = "Unable to load notifications. Please try again.";
                return View(new NotificationPageViewModel());
            }
        }

        // GET: /Notification/Test
        public IActionResult Test()
        {
            var viewModel = new NotificationTestingViewModel
            {
                IsEnabled = true,
                TestUserId = GetCurrentUserId(),
                AvailableUsers = GetTestUsers(),
                NotificationTypes = GetNotificationTypes(),
                RecentTestActions = new List<string>()
            };

            return View(viewModel);
        }

        // POST: /Notification/MarkAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.MarkAsReadAsync(notificationId, userId);

                if (success)
                {
                    return Json(new { success = true, message = "Notification marked as read" });
                }

                return Json(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // POST: /Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.MarkAllAsReadAsync(userId);

                return Json(new { success = success, message = success ? "All notifications marked as read" : "No notifications to mark" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // POST: /Notification/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.DeleteNotificationAsync(notificationId, userId);

                if (success)
                {
                    return Json(new { success = true, message = "Notification deleted" });
                }

                return Json(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification: {NotificationId}", notificationId);
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // GET: /Notification/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);

                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return Json(new { count = 0 });
            }
        }

        // GET: /Notification/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications(string filter = "all", int page = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pageSize = 20;

                IEnumerable<NotificationDto> notifications;

                if (filter == "unread")
                {
                    notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                }
                else
                {
                    notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                }

                var result = notifications.Select(n => new
                {
                    id = n.NotificationId,
                    title = n.Title,
                    message = n.Message,
                    timeAgo = n.TimeAgo,
                    isRead = n.IsRead,
                    category = n.NotificationCategory,
                    icon = n.Icon,
                    color = n.Color,
                    actionUrl = n.ActionUrl
                });

                return Json(new { success = true, notifications = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications via AJAX");
                return Json(new { success = false, message = "An error occurred" });
            }
        }




        // Helper Methods
        private int GetCurrentUserId()
        {
            // For testing purposes, return test user ID
            // In production, this would get the actual logged-in user ID
            return 1;
        }

        private List<TestUserDto> GetTestUsers()
        {
            return new List<TestUserDto>
            {
                new TestUserDto { Id = 1, Username = "testuser1@example.com", DisplayName = "Test User 1" },
                new TestUserDto { Id = 2, Username = "testuser2@example.com", DisplayName = "Test User 2" },
                new TestUserDto { Id = 3, Username = "testuser3@example.com", DisplayName = "Test User 3" }
            };
        }

        private List<NotificationTypeOptionDto> GetNotificationTypes()
        {
            return new List<NotificationTypeOptionDto>
            {
                // Transaction Types
                new NotificationTypeOptionDto
                {
                    Category = "Transaction",
                    Type = "success",
                    DisplayName = "Transaction Success",
                    Description = "Simulate a successful transaction completion"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Transaction",
                    Type = "cancel",
                    DisplayName = "Transaction Cancelled",
                    Description = "Simulate a cancelled transaction"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Transaction",
                    Type = "confirmation",
                    DisplayName = "Transaction Confirmation",
                    Description = "Simulate a transaction requiring confirmation"
                },

                // Social Types
                new NotificationTypeOptionDto
                {
                    Category = "Social",
                    Type = "like",
                    DisplayName = "Post Liked",
                    Description = "Someone liked your post"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Social",
                    Type = "comment",
                    DisplayName = "Post Comment",
                    Description = "Someone commented on your post"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Social",
                    Type = "reply",
                    DisplayName = "Comment Reply",
                    Description = "Someone replied to your comment"
                },

                // Gamification Types
                new NotificationTypeOptionDto
                {
                    Category = "Gamification",
                    Type = "quest",
                    DisplayName = "Quest Completed",
                    Description = "A quest has been completed"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Gamification",
                    Type = "achievement",
                    DisplayName = "Achievement Unlocked",
                    Description = "New achievement unlocked"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Gamification",
                    Type = "levelup",
                    DisplayName = "Level Up",
                    Description = "User leveled up"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Gamification",
                    Type = "leaderboard",
                    DisplayName = "Leaderboard Update",
                    Description = "Leaderboard ranking changed"
                },

                // Communication Types
                new NotificationTypeOptionDto
                {
                    Category = "Communication",
                    Type = "message",
                    DisplayName = "New Message",
                    Description = "New chat message received"
                },
                new NotificationTypeOptionDto
                {
                    Category = "Communication",
                    Type = "interest",
                    DisplayName = "Interest Expressed",
                    Description = "Someone expressed interest in your item"
                }
            };
        }
       
    }
}