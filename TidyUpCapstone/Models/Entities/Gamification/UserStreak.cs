using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class UserStreak
    {
        [Key]
        public int StreakId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int StreakTypeId { get; set; }

        public int CurrentStreak { get; set; } = 0;

        public DateTime? LastActivityDate { get; set; }

        public int LongestStreak { get; set; } = 0;

        public int TotalMilestonesReached { get; set; } = 0;

        public DateTime? LastMilestoneDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("StreakTypeId")]
        public virtual StreakType StreakType { get; set; } = null!;
    }
}