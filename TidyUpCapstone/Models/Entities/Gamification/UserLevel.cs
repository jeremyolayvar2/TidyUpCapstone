using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class UserLevel
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int CurrentLevelId { get; set; }

        public int CurrentXp { get; set; } = 0;

        public int TotalXp { get; set; } = 0;

        [Required]
        public int XpToNextLevel { get; set; }

        public DateTime? LevelUpDate { get; set; }

        public int TotalLevelUps { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("CurrentLevelId")]
        public virtual Level CurrentLevel { get; set; } = null!;
    }
}
