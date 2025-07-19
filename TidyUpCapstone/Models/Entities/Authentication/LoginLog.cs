using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Authentication
{
    [Table("login_logs")]
    public class LoginLog
    {
        [Key]
        [Column("log_id")]
        public int LogId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("login_time")]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(45)]
        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;

        [Column("user_agent", TypeName = "text")]
        public string? UserAgent { get; set; }

        [Required]
        [StringLength(50)]
        [Column("login_status")]
        public string LoginStatus { get; set; } = "success"; // success, failed

        [Required]
        [StringLength(50)]
        [Column("login_method")]
        public string LoginMethod { get; set; } = "email_password"; // email_password, google, microsoft, facebook, admin

        [StringLength(255)]
        [Column("provider_user_id")]
        public string? ProviderUserId { get; set; }

        [StringLength(255)]
        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("device_info", TypeName = "json")]
        public string? DeviceInfo { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}