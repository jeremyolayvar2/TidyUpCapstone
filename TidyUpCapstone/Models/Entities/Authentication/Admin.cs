using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Reports;
using TidyUpCapstone.Models.Entities.System;


namespace TidyUpCapstone.Models.Entities.Authentication
{
    [Table("admin")]
    public class Admin
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("admin_role")]
        public string AdminRole { get; set; } = "moderator"; // superadmin, moderator

        [Column("admin_permissions", TypeName = "json")]
        public string? AdminPermissions { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("last_admin_login")]
        public DateTime? LastAdminLogin { get; set; }

        [Required]
        [StringLength(50)]
        [Column("admin_status")]
        public string AdminStatus { get; set; } = "active"; // active, inactive

        [Column("can_manage_sso")]
        public bool CanManageSso { get; set; } = false;

        [Column("last_password_change")]
        public DateTime? LastPasswordChange { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        public virtual ICollection<UserReport> ResolvedReports { get; set; } = new List<UserReport>();
        public virtual ICollection<AdminReport> GeneratedReports { get; set; } = new List<AdminReport>();
        public virtual ICollection<SystemSetting> UpdatedSettings { get; set; } = new List<SystemSetting>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}