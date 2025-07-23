using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    public class AzureCvAnalysis
    {
        [Key]
        public int AnalysisId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public string? DetectedObjects { get; set; }

        public string? DetectedCategories { get; set; }

        public string? ImageDescription { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? ConfidenceScore { get; set; }

        [StringLength(255)]
        public string? ApiRequestId { get; set; }

        public int? ProcessingTimeMs { get; set; }

        [StringLength(50)]
        public string? ApiVersion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;

        public virtual ICollection<AiProcessingPipeline> ProcessingPipelines { get; set; } = new List<AiProcessingPipeline>();
    }
}