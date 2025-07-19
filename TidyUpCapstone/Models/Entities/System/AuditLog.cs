using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;

namespace TidyUpCapstone.Models.Entities.System
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("audit_id")]
        public long AuditId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("admin_id")]
        public int? AdminId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("entity_type")]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [Column("entity_id")]
        public int EntityId { get; set; }

        [Column("old_values", TypeName = "json")]
        public string? OldValues { get; set; }

        [Column("new_values", TypeName = "json")]
        public string? NewValues { get; set; }

        [StringLength(45)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent", TypeName = "text")]
        public string? UserAgent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("AdminId")]
        public virtual Admin? Admin { get; set; }
    }
}