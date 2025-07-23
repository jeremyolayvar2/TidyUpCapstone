using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.User;


namespace TidyUpCapstone.Models.Entities.Notifications
{
    public class UserNotificationPreference
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int TypeId { get; set; }

        public bool IsEnabled { get; set; } = true;

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.InApp;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("TypeId")]
        public virtual NotificationType Type { get; set; } = null!;
    }

    public enum DeliveryMethod
    {
        InApp,
        Email,
        Both
    }
}