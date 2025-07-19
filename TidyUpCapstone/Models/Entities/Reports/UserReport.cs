using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;


namespace TidyUpCapstone.Models.Entities.Reports
{
    [Table("user_reports")]
    public class UserReport
    {
        [Key]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Required]
        [Column("reporter_id")]
        public int ReporterId { get; set; }

        [Required]
        [Column("reported_user_id")]
        public int ReportedUserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("reported_entity_type")]
        public string ReportedEntityType { get; set; } = "user"; // user, item, post, comment, chat

        [Column("reported_entity_id")]
        public int? ReportedEntityId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("reason")]
        public string Reason { get; set; } = string.Empty; // spam, inappropriate, scam, harassment, fake_listing, other

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("evidence_urls", TypeName = "json")]
        public string? EvidenceUrls { get; set; }

        [Column("date_submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        [Column("report_status")]
        public string ReportStatus { get; set; } = "pending"; // pending, investigating, resolved, rejected, escalated

        [Required]
        [StringLength(50)]
        [Column("priority")]
        public string Priority { get; set; } = "medium"; // low, medium, high, urgent

        [Column("admin_notes", TypeName = "text")]
        public string? AdminNotes { get; set; }

        [Column("resolved_by_admin_id")]
        public int? ResolvedByAdminId { get; set; }

        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        [StringLength(50)]
        [Column("resolution_action")]
        public string? ResolutionAction { get; set; } // no_action, warning, temporary_ban, permanent_ban, content_removed

        // Navigation properties
        [ForeignKey("ReporterId")]
        public virtual AppUser Reporter { get; set; } = null!;

        [ForeignKey("ReportedUserId")]
        public virtual AppUser ReportedUser { get; set; } = null!;

        [ForeignKey("ResolvedByAdminId")]
        public virtual Admin? ResolvedByAdmin { get; set; }
    }
}