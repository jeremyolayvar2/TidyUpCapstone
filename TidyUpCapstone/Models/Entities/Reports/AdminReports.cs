using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;

namespace TidyUpCapstone.Models.Entities.Reports
{
    [Table("admin_reports")]
    public class AdminReport
    {
        [Key]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Required]
        [Column("generated_by_admin_id")]
        public int GeneratedByAdminId { get; set; }

        [Column("target_user_id")]
        public int? TargetUserId { get; set; }

        [Column("reporter_user_id")]
        public int? ReporterUserId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("report_name")]
        public string ReportName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("report_type")]
        public string ReportType { get; set; } = string.Empty; // user_activity, content_moderation, system_metrics, financial, performance

        [Column("report_description", TypeName = "text")]
        public string? ReportDescription { get; set; }

        [Column("report_data", TypeName = "json")]
        public string? ReportData { get; set; }

        [Column("report_parameters", TypeName = "json")]
        public string? ReportParameters { get; set; }

        [Required]
        [StringLength(50)]
        [Column("report_status")]
        public string ReportStatus { get; set; } = "queued"; // queued, generating, completed, failed, cancelled

        [Column("generated_date")]
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        [Column("file_path")]
        public string? FilePath { get; set; }

        [StringLength(50)]
        [Column("file_format")]
        public string? FileFormat { get; set; } // pdf, csv, xlsx, json

        [Column("file_size_bytes")]
        public long? FileSizeBytes { get; set; }

        [Column("download_count")]
        public int DownloadCount { get; set; } = 0;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("is_scheduled")]
        public bool IsScheduled { get; set; } = false;

        [StringLength(50)]
        [Column("scheduled_frequency")]
        public string? ScheduledFrequency { get; set; } // daily, weekly, monthly, quarterly

        [Column("next_run_date")]
        public DateTime? NextRunDate { get; set; }

        [Column("last_run_date")]
        public DateTime? LastRunDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("GeneratedByAdminId")]
        public virtual Admin GeneratedByAdmin { get; set; } = null!;

        [ForeignKey("TargetUserId")]
        public virtual AppUser? TargetUser { get; set; }

        [ForeignKey("ReporterUserId")]
        public virtual AppUser? ReporterUser { get; set; }
    }
}