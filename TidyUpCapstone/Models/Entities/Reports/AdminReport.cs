using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Reporting
{
    public class AdminReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int GeneratedByAdminId { get; set; }

        public int? TargetUserId { get; set; }

        public int? ReporterUserId { get; set; }

        [Required]
        [StringLength(255)]
        public string ReportName { get; set; } = string.Empty;

        [Required]
        public AdminReportType ReportType { get; set; }

        public string? ReportDescription { get; set; }

     
        public string? ReportData { get; set; }

 
        public string? ReportParameters { get; set; }

        [Required]
        public AdminReportStatus ReportStatus { get; set; } = AdminReportStatus.Queued;

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        public string? FilePath { get; set; }

        public FileFormat? FileFormat { get; set; }

        public long? FileSizeBytes { get; set; }

        public int DownloadCount { get; set; } = 0;

        public DateTime? ExpiresAt { get; set; }

        public bool IsScheduled { get; set; } = false;

        public ScheduledFrequency? ScheduledFrequency { get; set; }

        public DateTime? NextRunDate { get; set; }

        public DateTime? LastRunDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("GeneratedByAdminId")]
        public virtual Admin GeneratedByAdmin { get; set; } = null!;

        [ForeignKey("TargetUserId")]
        public virtual AppUser? TargetUser { get; set; }

        [ForeignKey("ReporterUserId")]
        public virtual AppUser? ReporterUser { get; set; }
    }

    public enum AdminReportType
    {
        UserActivity,
        ContentModeration,
        SystemMetrics,
        Financial,
        Performance
    }

    public enum AdminReportStatus
    {
        Queued,
        Generating,
        Completed,
        Failed,
        Cancelled
    }

    public enum FileFormat
    {
        Pdf,
        Csv,
        Xlsx,
        Json
    }

    public enum ScheduledFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly
    }
}