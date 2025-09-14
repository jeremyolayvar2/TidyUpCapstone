using TidyUpCapstone.Models.DTOs.Notifications;
using TidyUpCapstone.Models.Entities.Notifications;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(int userId);
        Task<NotificationDto?> GetNotificationByIdAsync(int notificationId, int userId);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
        Task<int> GetUnreadCountAsync(int userId);

        // Test-specific methods
        Task<NotificationDto> CreateTestTransactionNotificationAsync(int userId, string transactionType, string itemName);
        Task<NotificationDto> CreateTestSocialNotificationAsync(int userId, string socialType, string username, string postTitle = "");
        Task<NotificationDto> CreateTestGamificationNotificationAsync(int userId, string gamificationType, string details);
        Task<NotificationDto> CreateTestCommunicationNotificationAsync(int userId, string communicationType, string senderName);
        Task<List<NotificationDto>> CreateBulkTestNotificationsAsync(int userId, int count);
        Task<bool> ClearAllTestNotificationsAsync(int userId);
    }
}