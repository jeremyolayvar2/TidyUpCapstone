using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class UserStats
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int CurrentLevel { get; set; } = 1;
        public int CurrentXp { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalTokens { get; set; } = 0;

        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public DateTime? LastCheckIn { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}