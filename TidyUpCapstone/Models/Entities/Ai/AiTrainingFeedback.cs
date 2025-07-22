using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.Entities.AI
{
    public class AiTrainingFeedback
    {
        [Key]
        public int AiFeedbackId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(100)]
        public string? AiPredictedCategory { get; set; }

        [StringLength(100)]
        public string? UserCorrectedCategory { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? AiPredictedPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? UserSetPrice { get; set; }

        [Required]
        public FeedbackType FeedbackType { get; set; }

        public int? ConfidenceRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }

    public enum FeedbackType
    {
        Price,
        Category,
        Both
    }
}