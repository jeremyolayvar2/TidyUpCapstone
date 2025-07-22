using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    public class TensorflowPrediction
    {
        [Key]
        public int PredictionId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [StringLength(100)]
        public string? PredictedCategory { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ConditionScore { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedTokenValue { get; set; }

        public string? DamageDetected { get; set; }

        [StringLength(100)]
        public string? ModelName { get; set; }

        [StringLength(50)]
        public string? ModelVersion { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? PredictionConfidence { get; set; }

        public bool IsTrainingData { get; set; } = false;

        public int? ProcessingTimeMs { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        public virtual ICollection<AiProcessingPipeline> ProcessingPipelines { get; set; } = new List<AiProcessingPipeline>();
    }
}
