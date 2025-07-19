using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    [Table("ai_processing_pipeline")]
    public class AiProcessingPipeline
    {
        [Key]
        [Column("processing_id")]
        public int ProcessingId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("analysis_id")]
        public int? AnalysisId { get; set; }

        [Column("prediction_id")]
        public int? PredictionId { get; set; }

        [StringLength(20)]
        [Column("azure_cv_status")]
        public string AzureCvStatus { get; set; } = "pending";

        [StringLength(20)]
        [Column("tensorflow_status")]
        public string TensorflowStatus { get; set; } = "pending";

        [StringLength(100)]
        [Column("final_category")]
        public string? FinalCategory { get; set; }

        [Column("final_token_value", TypeName = "decimal(10,2)")]
        public decimal? FinalTokenValue { get; set; }

        [Column("confidence_level", TypeName = "decimal(5,4)")]
        public decimal? ConfidenceLevel { get; set; }

        [Column("error_message", TypeName = "text")]
        public string? ErrorMessage { get; set; }

        [Column("retry_count")]
        public int RetryCount { get; set; } = 0;

        [Column("started_at")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        [ForeignKey("AnalysisId")]
        public virtual AzureCvAnalysis? Analysis { get; set; }

        [ForeignKey("PredictionId")]
        public virtual TensorflowPrediction? Prediction { get; set; }
    }
}