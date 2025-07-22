using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("quest_progress")]
    public class QuestProgress
    {
        [Key]
        [Column("progress_id")]
        public int ProgressId { get; set; }

        [Required]
        [Column("user_quest_id")]
        public int UserQuestId { get; set; }

        [Column("progress_value")]
        public int ProgressValue { get; set; } = 0;

        [Required]
        [Column("goal_value")]
        public int GoalValue { get; set; }

        [StringLength(50)]
        [Column("action_type")]
        public string? ActionType { get; set; }

        [Column("action_timestamp")]
        public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserQuestId")]
        public virtual UserQuest UserQuest { get; set; } = null!;
    }
}