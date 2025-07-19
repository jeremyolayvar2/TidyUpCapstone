using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;

namespace TidyUpCapstone.Models.Entities.SSO
{
    [Table("sso_audit_logs")]
    public class SsoAuditLog
    {
        [Key]
        [Column("audit_id")]
        public long AuditId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("provider_name")]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("result")]
        public string Result { get; set; } = string.Empty;

        [StringLength(45)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent", TypeName = "text")]
        public string? UserAgent { get; set; }

        [StringLength(255)]
        [Column("session_id")]
        public string? SessionId { get; set; }

        [StringLength(50)]
        [Column("error_code")]
        public string? ErrorCode { get; set; }

        [Column("error_message", TypeName = "text")]
        public string? ErrorMessage { get; set; }

        [StringLength(255)]
        [Column("request_id")]
        public string? RequestId { get; set; }

        [Column("additional_data", TypeName = "json")]
        public string? AdditionalData { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("ProviderName")]
        public virtual SsoProvider Provider { get; set; } = null!;
    }
}