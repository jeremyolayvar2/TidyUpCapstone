using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.SSO;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.SSO
{
    public class UserSsoLink
    {
        [Key]
        public int LinkId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ProviderUserId { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ProviderEmail { get; set; }

        [StringLength(255)]
        public string? ProviderDisplayName { get; set; }

        [StringLength(500)]
        public string? ProviderAvatarUrl { get; set; }

        // Token management (these should be encrypted in application)
        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public string? IdToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }

        [StringLength(255)]
        public string? Scope { get; set; }

        // Link metadata
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsed { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsVerified { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("ProviderName")]
        public virtual SsoProvider Provider { get; set; } = null!;
    }
}