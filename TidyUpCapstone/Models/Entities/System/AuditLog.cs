using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.System
{
    public class AuditLog
    {
        [Key]
        public long AuditId { get; set; }

        public int? UserId { get; set; }

        public int? AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        public int EntityId { get; set; }

        [Column(TypeName = "json")]
        public string? OldValues { get; set; }

        [Column(TypeName = "json")]
        public string? NewValues { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("AdminId")]
        public virtual Admin? Admin { get; set; }
    }
}