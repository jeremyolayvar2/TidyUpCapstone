using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    [Table("userquests")]
    public class UserQuest
    {
        [Key]
        [Column("user_quest_id")]
        public int UserQuestId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("quest_id")]
        public int QuestId { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("current_progress")]
        public int CurrentProgress { get; set; } = 0;

        [Column("date_claimed")]
        public DateTime? DateClaimed { get; set; }

        [Column("started_at")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("QuestId")]
        public virtual Quest Quest { get; set; } = null!;

        public virtual ICollection<QuestProgress> QuestProgresses { get; set; } = new List<QuestProgress>();
    }
}