using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("streak_types")]
    public class StreakType
    {
        [Key]
        [Column("streak_type_id")]
        public int StreakTypeId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        [Column("streak_unit")]
        public string StreakUnit { get; set; } = "days"; // days, weeks, items, transactions

        [Column("base_rewards", TypeName = "decimal(10,2)")]
        public decimal BaseRewards { get; set; } = 0.00m;

        [Column("milestone_rewards", TypeName = "decimal(10,2)")]
        public decimal MilestoneRewards { get; set; } = 0.00m;

        [Column("milestone_interval")]
        public int MilestoneInterval { get; set; } = 7;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserStreak> UserStreaks { get; set; } = new List<UserStreak>();
    }
}