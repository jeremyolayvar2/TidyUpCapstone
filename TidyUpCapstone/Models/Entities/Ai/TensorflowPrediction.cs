using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    [Table("tensorflow_prediction")]
    public class TensorflowPrediction
    {
        [Key]
        [Column("prediction_id")]
        public int PredictionId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [StringLength(100)]
        [Column("predicted_category")]
        public string? PredictedCategory { get; set; }

        [Column("condition_score", TypeName = "decimal(5,2)")]
        public decimal? ConditionScore { get; set; }

        [Column("estimated_token_value", TypeName = "decimal(10,2)")]
        public decimal? EstimatedTokenValue { get; set; }

        [Column("damage_detected", TypeName = "text")]
        public string? DamageDetected { get; set; }

        [StringLength(100)]
        [Column("model_name")]
        public string? ModelName { get; set; }

        [StringLength(50)]
        [Column("model_version")]
        public string? ModelVersion { get; set; }

        [Column("prediction_confidence", TypeName = "decimal(5,4)")]
        public decimal? PredictionConfidence { get; set; }

        [Column("is_training_data")]
        public bool IsTrainingData { get; set; } = false;

        [Column("processing_time_ms")]
        public int? ProcessingTimeMs { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        public virtual ICollection<AiProcessingPipeline> ProcessingPipelines { get; set; } = new List<AiProcessingPipeline>();
    }
}