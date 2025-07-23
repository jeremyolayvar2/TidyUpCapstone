using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class Leaderboard
    {
        [Key]
        public int LeaderboardId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Metric { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public ResetFrequency ResetFrequency { get; set; } = ResetFrequency.Monthly;

        public DateTime? LastReset { get; set; }

        public DateTime? NextReset { get; set; }

        public int MaxEntries { get; set; } = 100;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
    }

    public enum ResetFrequency
    {
        Never,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}