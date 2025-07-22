using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("levels")]
    public class Level
    {
        [Key]
        [Column("level_id")]
        public int LevelId { get; set; }

        [Required]
        [Column("level_number")]
        public int LevelNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Column("level_name")]
        public string LevelName { get; set; } = string.Empty;

        [Required]
        [Column("xp_required")]
        public int XpRequired { get; set; }

        [Required]
        [Column("xp_to_next")]
        public int XpToNext { get; set; }

        [StringLength(100)]
        [Column("title_unlock")]
        public string? TitleUnlock { get; set; }

        [Column("token_bonus", TypeName = "decimal(10,2)")]
        public decimal TokenBonus { get; set; } = 0.00m;

        [StringLength(200)]
        [Column("special_privilege")]
        public string? SpecialPrivilege { get; set; }

        // Navigation properties
        public virtual ICollection<UserLevel> UserLevels { get; set; } = new List<UserLevel>();
    }
}