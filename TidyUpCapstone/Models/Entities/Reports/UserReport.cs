using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Reporting
{
    public class UserReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int ReporterId { get; set; }

        [Required]
        public int ReportedUserId { get; set; }

        [Required]
        public ReportedEntityType ReportedEntityType { get; set; } = ReportedEntityType.User;

        public int? ReportedEntityId { get; set; }

        [Required]
        public ReportReason Reason { get; set; }

        public string? Description { get; set; }

  
        public string? EvidenceUrls { get; set; }

        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

        [Required]
        public ReportStatus ReportStatus { get; set; } = ReportStatus.Pending;

        [Required]
        public ReportPriority Priority { get; set; } = ReportPriority.Medium;

        public string? AdminNotes { get; set; }

        public int? ResolvedByAdminId { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public ResolutionAction? ResolutionAction { get; set; }

        // Navigation properties
        [ForeignKey("ReporterId")]
        public virtual AppUser Reporter { get; set; } = null!;

        [ForeignKey("ReportedUserId")]
        public virtual AppUser ReportedUser { get; set; } = null!;

        [ForeignKey("ResolvedByAdminId")]
        public virtual Admin? ResolvedByAdmin { get; set; }
    }

    public enum ReportedEntityType
    {
        User,
        Item,
        Post,
        Comment,
        Chat
    }

    public enum ReportReason
    {
        Spam,
        Inappropriate,
        Scam,
        Harassment,
        FakeListing,
        Other
    }

    public enum ReportStatus
    {
        Pending,
        Investigating,
        Resolved,
        Rejected,
        Escalated
    }

    public enum ReportPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum ResolutionAction
    {
        NoAction,
        Warning,
        TemporaryBan,
        PermanentBan,
        ContentRemoved
    }
}