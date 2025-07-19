using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;


namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("user_levels")]
    public class UserLevel
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("current_level_id")]
        public int CurrentLevelId { get; set; }

        [Column("current_xp")]
        public int CurrentXp { get; set; } = 0;

        [Column("total_xp")]
        public int TotalXp { get; set; } = 0;

        [Required]
        [Column("xp_to_next_level")]
        public int XpToNextLevel { get; set; }

        [Column("level_up_date")]
        public DateTime? LevelUpDate { get; set; }

        [Column("total_level_ups")]
        public int TotalLevelUps { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("CurrentLevelId")]
        public virtual Level CurrentLevel { get; set; } = null!;
    }
}