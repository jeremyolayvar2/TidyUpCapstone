using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    [Table("azure_cv_analysis")]
    public class AzureCvAnalysis
    {
        [Key]
        [Column("analysis_id")]
        public int AnalysisId { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("detected_objects", TypeName = "text")]
        public string? DetectedObjects { get; set; }

        [Column("detected_categories", TypeName = "text")]
        public string? DetectedCategories { get; set; }

        [Column("image_description", TypeName = "text")]
        public string? ImageDescription { get; set; }

        [Column("confidence_score", TypeName = "decimal(5,4)")]
        public decimal? ConfidenceScore { get; set; }

        [StringLength(255)]
        [Column("api_request_id")]
        public string? ApiRequestId { get; set; }

        [Column("processing_time_ms")]
        public int? ProcessingTimeMs { get; set; }

        [StringLength(50)]
        [Column("api_version")]
        public string? ApiVersion { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        public virtual ICollection<AiProcessingPipeline> ProcessingPipelines { get; set; } = new List<AiProcessingPipeline>();
    }
}