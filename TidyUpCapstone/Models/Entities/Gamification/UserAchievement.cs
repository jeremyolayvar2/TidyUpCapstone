using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Gamification;


namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("user_achievements")]
    public class UserAchievement
    {
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("achievement_id")]
        public int AchievementId { get; set; }

        [Column("earned_date")]
        public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

        [Column("progress")]
        public int Progress { get; set; } = 0;

        [Column("is_notified")]
        public bool IsNotified { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("AchievementId")]
        public virtual Achievement Achievement { get; set; } = null!;
    }
}