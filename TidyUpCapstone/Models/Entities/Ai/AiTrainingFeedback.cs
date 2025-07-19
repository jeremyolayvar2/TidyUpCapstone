using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    [Table("ai_training_feedback")]
    public class AiTrainingFeedback
    {
        [Key]
        [Column("ai_feedback_id")]
        public int AiFeedbackId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [StringLength(100)]
        [Column("ai_predicted_category")]
        public string? AiPredictedCategory { get; set; }

        [StringLength(100)]
        [Column("user_corrected_category")]
        public string? UserCorrectedCategory { get; set; }

        [Column("ai_predicted_price", TypeName = "decimal(10,2)")]
        public decimal? AiPredictedPrice { get; set; }

        [Column("user_set_price", TypeName = "decimal(10,2)")]
        public decimal? UserSetPrice { get; set; }

        [Required]
        [StringLength(50)]
        [Column("feedback_type")]
        public string FeedbackType { get; set; } = string.Empty; // price, category, both

        [Column("confidence_rating")]
        public int? ConfidenceRating { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}