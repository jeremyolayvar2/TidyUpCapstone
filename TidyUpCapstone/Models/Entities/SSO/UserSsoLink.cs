using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;

namespace TidyUpCapstone.Models.Entities.SSO
{
    [Table("user_sso_links")]
    public class UserSsoLink
    {
        [Key]
        [Column("link_id")]
        public int LinkId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("provider_name")]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("provider_user_id")]
        public string ProviderUserId { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("provider_email")]
        public string? ProviderEmail { get; set; }

        [StringLength(255)]
        [Column("provider_display_name")]
        public string? ProviderDisplayName { get; set; }

        [StringLength(500)]
        [Column("provider_avatar_url")]
        public string? ProviderAvatarUrl { get; set; }

        [Column("access_token", TypeName = "text")]
        public string? AccessToken { get; set; }

        [Column("refresh_token", TypeName = "text")]
        public string? RefreshToken { get; set; }

        [Column("id_token", TypeName = "text")]
        public string? IdToken { get; set; }

        [Column("token_expires_at")]
        public DateTime? TokenExpiresAt { get; set; }

        [StringLength(255)]
        [Column("scope")]
        public string? Scope { get; set; }

        [Column("linked_at")]
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        [Column("last_used")]
        public DateTime? LastUsed { get; set; }

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;

        [Column("is_verified")]
        public bool IsVerified { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("ProviderName")]
        public virtual SsoProvider Provider { get; set; } = null!;
    }
}