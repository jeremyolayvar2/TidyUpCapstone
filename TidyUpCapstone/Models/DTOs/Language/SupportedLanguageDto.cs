namespace TidyUpCapstone.Models.DTOs.Language
{
    public class SupportedLanguageDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public bool IsRTL { get; set; }
    }
}