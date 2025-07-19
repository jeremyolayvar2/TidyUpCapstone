using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;


namespace TidyUpCapstone.Models.Entities.Notifications
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("message", TypeName = "text")]
        public string Message { get; set; } = string.Empty;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [StringLength(500)]
        [Column("action_url")]
        public string? ActionUrl { get; set; }

        [StringLength(50)]
        [Column("related_entity_type")]
        public string? RelatedEntityType { get; set; } // item, transaction, quest, achievement, user

        [Column("related_entity_id")]
        public int? RelatedEntityId { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("TypeId")]
        public virtual NotificationType Type { get; set; } = null!;
    }
}