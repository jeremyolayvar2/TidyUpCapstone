using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.SSO
{
    [Table("sso_providers")]
    public class SsoProvider
    {
        [Key]
        [Column("provider_id")]
        public int ProviderId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("provider_name")]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("client_id")]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        [StringLength(500)]
        [Column("authority_url")]
        public string? AuthorityUrl { get; set; }

        [StringLength(500)]
        [Column("scopes")]
        public string? Scopes { get; set; }

        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [Column("configuration_json", TypeName = "json")]
        public string? ConfigurationJson { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserSsoLink> UserSsoLinks { get; set; } = new List<UserSsoLink>();
        public virtual ICollection<SsoAuditLog> AuditLogs { get; set; } = new List<SsoAuditLog>();
    }
}