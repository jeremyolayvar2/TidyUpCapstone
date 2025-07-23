using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Reporting;
using TidyUpCapstone.Models.Entities.System;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Core
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public AdminRole AdminRole { get; set; } = AdminRole.Moderator;

        // Removed [Column(TypeName = "json")] - will be configured in DbContext
        public string? AdminPermissions { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastAdminLogin { get; set; }

        [Required]
        public AdminStatus AdminStatus { get; set; } = AdminStatus.Active;

        public bool CanManageSso { get; set; } = false;

        public DateTime? LastPasswordChange { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
        public virtual ICollection<UserReport> ResolvedReports { get; set; } = new List<UserReport>();
        public virtual ICollection<AdminReport> GeneratedReports { get; set; } = new List<AdminReport>();
        public virtual ICollection<SystemSetting> SystemSettingsUpdated { get; set; } = new List<SystemSetting>();
        public virtual ICollection<AuditLog> AdminAuditLogs { get; set; } = new List<AuditLog>();
    }

    public enum AdminRole
    {
        Superadmin,
        Moderator
    }

    public enum AdminStatus
    {
        Active,
        Inactive
    }
}