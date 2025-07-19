using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Notifications;

namespace TidyUpCapstone.Models.Entities.Notifications
{
    [Table("user_notification_preferences")]
    public class UserNotificationPreference
    {
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("type_id")]
        public int TypeId { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [Required]
        [StringLength(50)]
        [Column("delivery_method")]
        public string DeliveryMethod { get; set; } = "in_app"; // in_app, email, both

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("TypeId")]
        public virtual NotificationType Type { get; set; } = null!;
    }
}