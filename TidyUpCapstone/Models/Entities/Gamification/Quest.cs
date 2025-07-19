using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("quests")]
    public class Quest
    {
        [Key]
        [Column("quest_id")]
        public int QuestId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("quest_title")]
        public string QuestTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("quest_type")]
        public string QuestType { get; set; } = string.Empty; // daily, weekly, special, milestone

        [Column("quest_description", TypeName = "text")]
        public string? QuestDescription { get; set; }

        [StringLength(200)]
        [Column("quest_objective")]
        public string? QuestObjective { get; set; }

        [Required]
        [Column("token_reward", TypeName = "decimal(10,2)")]
        public decimal TokenReward { get; set; } = 0.00m;

        [Column("xp_reward")]
        public int XpReward { get; set; } = 0;

        [Required]
        [StringLength(50)]
        [Column("difficulty")]
        public string Difficulty { get; set; } = "easy"; // easy, medium, hard

        [Column("target_value")]
        public int TargetValue { get; set; } = 1;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserQuest> UserQuests { get; set; } = new List<UserQuest>();
    }
}