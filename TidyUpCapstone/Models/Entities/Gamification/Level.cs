using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class Level
    {
        [Key]
        public int LevelId { get; set; }

        [Required]
        public int LevelNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string LevelName { get; set; } = string.Empty;

        [Required]
        public int XpRequired { get; set; }

        [Required]
        public int XpToNext { get; set; }

        [StringLength(100)]
        public string? TitleUnlock { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TokenBonus { get; set; } = 0.00m;

        [StringLength(200)]
        public string? SpecialPrivilege { get; set; }

        // Navigation properties
        public virtual ICollection<UserLevel> UserLevels { get; set; } = new List<UserLevel>();
    }
}