using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.SSO;

namespace TidyUpCapstone.Models.Entities.SSO
{
    public class SsoProvider
    {
        [Key]
        public int ProviderId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ClientSecret { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AuthorityUrl { get; set; }

        [StringLength(500)]
        public string? Scopes { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string? ConfigurationJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserSsoLink> UserSsoLinks { get; set; } = new List<UserSsoLink>();
        public virtual ICollection<SsoAuditLog> SsoAuditLogs { get; set; } = new List<SsoAuditLog>();
    }
}