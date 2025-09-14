using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Notifications
{
    [Table("notification_settings")]
    public class NotificationSettings
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("email_new_messages")]
        public bool EmailNewMessages { get; set; } = true;

        [Column("email_item_updates")]
        public bool EmailItemUpdates { get; set; } = true;

        [Column("email_weekly_summary")]
        public bool EmailWeeklySummary { get; set; } = false;

        [Column("desktop_notifications")]
        public bool DesktopNotifications { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}