using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class StreakType
    {
        [Key]
        public int StreakTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public StreakUnit StreakUnit { get; set; } = StreakUnit.Days;

        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseRewards { get; set; } = 0.00m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MilestoneRewards { get; set; } = 0.00m;

        public int MilestoneInterval { get; set; } = 7;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserStreak> UserStreaks { get; set; } = new List<UserStreak>();
    }

    public enum StreakUnit
    {
        Days,
        Weeks,
        Items,
        Transactions
    }
}