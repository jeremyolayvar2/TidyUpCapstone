using TidyUpCapstone.Models.DTOs.Notifications;

namespace TidyUpCapstone.Models.ViewModels.Notifications
{
    public class NotificationPageViewModel
    {
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
        public int UnreadCount { get; set; }
        public string CurrentFilter { get; set; } = "all";
        public int CurrentPage { get; set; } = 1;
        public bool HasNotifications { get; set; }
        public bool HasUnread { get; set; }
        public NotificationTestingViewModel? TestingPanel { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class NotificationTestingViewModel
    {
        public bool IsEnabled { get; set; }
        public int TestUserId { get; set; }
        public List<TestUserDto> AvailableUsers { get; set; } = new List<TestUserDto>();
        public List<NotificationTypeOptionDto> NotificationTypes { get; set; } = new List<NotificationTypeOptionDto>();
        public List<string> RecentTestActions { get; set; } = new List<string>();
        public NotificationSummaryDto? Summary { get; set; }
    }

    public class TestUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class NotificationTypeOptionDto
    {
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresItemName { get; set; } = false;
        public bool RequiresUsername { get; set; } = false;
        public bool RequiresPostTitle { get; set; } = false;
        public bool RequiresDetails { get; set; } = false;
    }

    public class TestNotificationFormModel
    {
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string? Username { get; set; }
        public string? PostTitle { get; set; }
        public string? Details { get; set; }
        public int Count { get; set; } = 1;
        public int UserId { get; set; } = 1;
    }
}