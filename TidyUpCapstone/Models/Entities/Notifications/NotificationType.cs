using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Notifications;

namespace TidyUpCapstone.Models.Entities.Notifications
{
    [Table("notification_types")]
    public class NotificationType
    {
        [Key]
        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("type_name")]
        public string TypeName { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [StringLength(50)]
        [Column("icon")]
        public string? Icon { get; set; }

        [StringLength(10)]
        [Column("color")]
        public string? Color { get; set; }

        [Column("default_enabled")]
        public bool DefaultEnabled { get; set; } = true;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<UserNotificationPreference> UserPreferences { get; set; } = new List<UserNotificationPreference>();
    }
}