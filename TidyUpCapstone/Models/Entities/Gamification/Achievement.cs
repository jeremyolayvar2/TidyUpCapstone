using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("achievements")]
    public class Achievement
    {
        [Key]
        [Column("achievements_id")]
        public int AchievementsId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        [Column("category")]
        public string Category { get; set; } = "decluttering"; // decluttering, community, trading, exploration, special

        [Required]
        [StringLength(100)]
        [Column("criteria_type")]
        public string CriteriaType { get; set; } = string.Empty;

        [Required]
        [Column("criteria_value")]
        public int CriteriaValue { get; set; }

        [Column("token_reward", TypeName = "decimal(10,2)")]
        public decimal TokenReward { get; set; } = 0.00m;

        [Column("xp_reward")]
        public int? XpReward { get; set; } = 0;

        [StringLength(255)]
        [Column("badge_image_url")]
        public string? BadgeImageUrl { get; set; }

        [Required]
        [StringLength(50)]
        [Column("rarity")]
        public string Rarity { get; set; } = "bronze"; // bronze, silver, gold, platinum

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_secret")]
        public bool IsSecret { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}