using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.Entities.AI
{
    [Table("vision_analysis")]
    public class VisionAnalysis
    {
        [Key]
        public int AnalysisId { get; set; }

        [Required]
        public int ItemId { get; set; }

        public string? AnalysisResult { get; set; }
        public double ConfidenceScore { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Status { get; set; } = "pending";

        // Navigation property
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; } = null!;
    }
}