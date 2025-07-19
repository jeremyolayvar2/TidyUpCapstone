using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;


namespace TidyUpCapstone.Models.Entities.Leaderboards
{
    [Table("leaderboard_entries")]
    public class LeaderboardEntry
    {
        [Key]
        [Column("entry_id")]
        public int EntryId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("leaderboard_id")]
        public int LeaderboardId { get; set; }

        [Required]
        [Column("rank_position")]
        public int RankPosition { get; set; }

        [Required]
        [Column("score", TypeName = "decimal(15,2)")]
        public decimal Score { get; set; } = 0.00m;

        [Column("previous_rank")]
        public int? PreviousRank { get; set; }

        [Required]
        [StringLength(50)]
        [Column("rank_change")]
        public string RankChange { get; set; } = "new"; // up, down, same, new

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("LeaderboardId")]
        public virtual Leaderboard Leaderboard { get; set; } = null!;
    }
}