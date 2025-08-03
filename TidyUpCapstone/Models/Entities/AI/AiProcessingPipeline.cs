using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Items;


namespace TidyUpCapstone.Models.Entities.AI
{
    public class AiProcessingPipeline
    {
        [Key]
        public int ProcessingId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public int? AnalysisId { get; set; }

        public int? PredictionId { get; set; }

        [StringLength(20)]
        public string AzureCvStatus { get; set; } = "pending";

        [StringLength(20)]
        public string TensorflowStatus { get; set; } = "pending";

        [StringLength(100)]
        public string? FinalCategory { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FinalTokenValue { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? ConfidenceLevel { get; set; }

        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

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