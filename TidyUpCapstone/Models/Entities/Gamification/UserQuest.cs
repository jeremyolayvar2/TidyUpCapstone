using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.Gamification
{
    public class UserQuest
    {
        [Key]
        public int UserQuestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int QuestId { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int CurrentProgress { get; set; } = 0;

        public DateTime? DateClaimed { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [ForeignKey("QuestId")]
        public virtual Quest Quest { get; set; } = null!;

        public virtual ICollection<QuestProgress> QuestProgresses { get; set; } = new List<QuestProgress>();
    }
}