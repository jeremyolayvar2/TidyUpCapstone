using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Authentication
{
    [Table("email_verifications")]
    public class EmailVerification
    {
        [Key]
        [Column("verification_id")]
        public int VerificationId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("verification_code")]
        public string VerificationCode { get; set; } = string.Empty;

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Required]
        [Column("expiry")]
        public DateTime Expiry { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        [Column("verification_type")]
        public string VerificationType { get; set; } = "email_signup"; // email_signup, email_change, sso_link, password_reset

        [StringLength(50)]
        [Column("provider_name")]
        public string? ProviderName { get; set; }

        [StringLength(500)]
        [Column("redirect_url")]
        public string? RedirectUrl { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}