using Google.Cloud.Vision.V1;

namespace TidyUpCapstone.Models.AI
{
    public class VisionAnalysisResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int SuggestedCategoryId { get; set; }
        public decimal ConfidenceScore { get; set; }
        public List<EntityAnnotation> Labels { get; set; } = new();
        public List<LocalizedObjectAnnotation> Objects { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }
}