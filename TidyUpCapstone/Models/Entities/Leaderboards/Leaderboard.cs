using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Leaderboards
{
    [Table("leaderboards")]
    public class Leaderboard
    {
        [Key]
        [Column("leaderboards_id")]
        public int LeaderboardsId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("metric")]
        public string Metric { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        [Column("reset_frequency")]
        public string ResetFrequency { get; set; } = "monthly"; // never, daily, weekly, monthly, yearly

        [Column("last_reset")]
        public DateTime? LastReset { get; set; }

        [Column("next_reset")]
        public DateTime? NextReset { get; set; }

        [Column("max_entries")]
        public int MaxEntries { get; set; } = 100;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
    }
}