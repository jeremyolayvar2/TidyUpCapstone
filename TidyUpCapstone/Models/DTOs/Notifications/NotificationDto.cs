using TidyUpCapstone.Models.Entities.Notifications;

namespace TidyUpCapstone.Models.DTOs.Notifications
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
        public RelatedEntityType? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string NotificationCategory { get; set; } = string.Empty;
    }

    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public int TypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public RelatedEntityType? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadCount { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
        public List<NotificationTypeStatsDto> TypeStats { get; set; } = new();
    }

    public class NotificationTypeStatsDto
    {
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int UnreadCount { get; set; }
    }

    public class TestNotificationRequestDto
    {
        public string NotificationType { get; set; } = string.Empty; // transaction, social, gamification, communication
        public string SubType { get; set; } = string.Empty; // success, cancel, like, comment, etc.
        public string? ItemName { get; set; }
        public string? Username { get; set; }
        public string? PostTitle { get; set; }
        public string? Details { get; set; }
        public int Count { get; set; } = 1;
    }
}