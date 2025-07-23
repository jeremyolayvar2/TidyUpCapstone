using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class UserAchievement
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int AchievementId { get; set; }

        public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

        public int Progress { get; set; } = 0;

        public bool IsNotified { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("AchievementId")]
        public virtual Achievement Achievement { get; set; } = null!;
    }
}