using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Core
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

        [Required]
        [StringLength(45)]
        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Required]
        [StringLength(20)]
        [Column("login_status")]
        public string LoginStatus { get; set; } = "Success"; // Success, Failed, Blocked

        [StringLength(255)]
        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("login_timestamp")]
        public DateTime LoginTimestamp { get; set; } = DateTime.UtcNow;

        [Column("logout_timestamp")]
        public DateTime? LogoutTimestamp { get; set; }

        [StringLength(100)]
        [Column("failure_reason")]
        public string? FailureReason { get; set; }

        [StringLength(50)]
        [Column("device_type")]
        public string? DeviceType { get; set; }

        [StringLength(100)]
        [Column("browser")]
        public string? Browser { get; set; }

        [StringLength(50)]
        [Column("os")]
        public string? OperatingSystem { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}