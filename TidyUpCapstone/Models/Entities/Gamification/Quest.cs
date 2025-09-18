using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class Quest
    {
        [Key]
        public int QuestId { get; set; }

        [Required]
        [StringLength(100)]
        public string QuestTitle { get; set; } = string.Empty;

        [Required]
        public QuestType QuestType { get; set; }

        public string? QuestDescription { get; set; }

        [StringLength(200)]
        public string? QuestObjective { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TokenReward { get; set; } = 0.00m;

        public int XpReward { get; set; } = 0;

        [Required]
        public QuestDifficulty Difficulty { get; set; } = QuestDifficulty.Easy;

        public int TargetValue { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserQuest> UserQuests { get; set; } = new List<UserQuest>();
    }

    public enum QuestType
    {
        Daily,
        Weekly,
        Special,
        Milestone
    }

    public enum QuestDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum QuestStatus
    {
        Active,
        Completed,
        Expired,
        Abandoned
    }
}