using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class Achievement
    {
        [Key]
        public int AchievementId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public AchievementCategory Category { get; set; } = AchievementCategory.Decluttering;

        [Required]
        [StringLength(100)]
        public string CriteriaType { get; set; } = string.Empty;

        [Required]
        public int CriteriaValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TokenReward { get; set; } = 0.00m;

        public int? XpReward { get; set; } = 0;

        [StringLength(255)]
        public string? BadgeImageUrl { get; set; }

        [Required]
        public AchievementRarity Rarity { get; set; } = AchievementRarity.Bronze;

        public bool IsActive { get; set; } = true;

        public bool IsSecret { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }

    public enum AchievementCategory
    {
        Decluttering,
        Community,
        Trading,
        Exploration,
        Special
    }

    public enum AchievementRarity
    {
        Bronze,
        Silver,
        Gold,
        Platinum
    }
}