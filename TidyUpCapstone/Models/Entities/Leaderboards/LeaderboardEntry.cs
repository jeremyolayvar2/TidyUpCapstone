using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class LeaderboardEntry
    {
        [Key]
        public int EntryId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LeaderboardId { get; set; }

        [Required]
        public int RankPosition { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal Score { get; set; } = 0.00m;

        public int? PreviousRank { get; set; }

        [Required]
        public RankChange RankChange { get; set; } = RankChange.New;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("LeaderboardId")]
        public virtual Leaderboard Leaderboard { get; set; } = null!;
    }

    public enum RankChange
    {
        Up,
        Down,
        Same,
        New
    }
}