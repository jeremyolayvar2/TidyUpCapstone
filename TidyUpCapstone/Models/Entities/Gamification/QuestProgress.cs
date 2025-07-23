using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class QuestProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public int UserQuestId { get; set; }

        public int ProgressValue { get; set; } = 0;

        [Required]
        public int GoalValue { get; set; }

        [StringLength(50)]
        public string? ActionType { get; set; }

        public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserQuestId")]
        public virtual UserQuest UserQuest { get; set; } = null!;
    }
}