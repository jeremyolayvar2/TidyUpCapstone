namespace TidyUpCapstone.Models.Configuration
{
    public class GoogleCloudSetting
    {
        public string ProjectId { get; set; } = string.Empty;
        public string CredentialsPath { get; set; } = string.Empty;
    }

    public class VisionSettings
    {
        public int MaxImageSize { get; set; } = 10485760; // 10MB
        public string[] AllowedFormats { get; set; } = { "jpg", "jpeg", "png", "webp" };
        public decimal MinConfidenceThreshold { get; set; } = 0.6m;
    }
}