using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.ViewModels.Account
{
    public class LanguageAccessibilityViewModel
    {
        [Required]
        public string Language { get; set; } = "en";

        [Required]
        public string Timezone { get; set; } = "Asia/Manila";

        public bool HighContrast { get; set; } = false;
        public bool LargeText { get; set; } = false;
        public bool ReduceMotion { get; set; } = false;
        public bool ScreenReader { get; set; } = false;
    }
}