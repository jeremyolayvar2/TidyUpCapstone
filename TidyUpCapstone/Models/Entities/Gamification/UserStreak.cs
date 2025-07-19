using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("user_streaks")]
    public class UserStreak
    {
        [Key]
        [Column("streak_id")]
        public int StreakId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("streak_type_id")]
        public int StreakTypeId { get; set; }

        [Column("current_streak")]
        public int CurrentStreak { get; set; } = 0;

        [Column("last_activity_date")]
        public DateTime? LastActivityDate { get; set; }

        [Column("longest_streak")]
        public int LongestStreak { get; set; } = 0;

        [Column("total_milestones_reached")]
        public int TotalMilestonesReached { get; set; } = 0;

        [Column("last_milestone_date")]
        public DateTime? LastMilestoneDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("StreakTypeId")]
        public virtual StreakType StreakType { get; set; } = null!;
    }
}