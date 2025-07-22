using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.SSO;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.SSO
{
    public class SsoAuditLog
    {
        [Key]
        public long AuditId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Result { get; set; } = string.Empty;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        [StringLength(255)]
        public string? SessionId { get; set; }

        [StringLength(50)]
        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        [StringLength(255)]
        public string? RequestId { get; set; }

        [Column(TypeName = "json")]
        public string? AdditionalData { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("ProviderName")]
        public virtual SsoProvider Provider { get; set; } = null!;
    }
}