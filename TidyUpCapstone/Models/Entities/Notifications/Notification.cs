using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.User;


namespace TidyUpCapstone.Models.Entities.Notifications
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        public RelatedEntityType? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("TypeId")]
        public virtual NotificationType Type { get; set; } = null!;
    }

    public enum RelatedEntityType
    {
        Item,
        Transaction,
        Quest,
        Achievement,
        User
    }
}