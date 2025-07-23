using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;      

namespace TidyUpCapstone.Models.Entities.Core
{
    public class EmailVerification
    {
        [Key]
        public int VerificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string VerificationCode { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;

        [Required]
        public DateTime Expiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public VerificationType VerificationType { get; set; } = VerificationType.EmailSignup;

        [StringLength(50)]
        public string? ProviderName { get; set; }

        [StringLength(500)]
        public string? RedirectUrl { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }

    public enum VerificationType
    {
        EmailSignup,
        EmailChange,
        SsoLink,
        PasswordReset
    }
}