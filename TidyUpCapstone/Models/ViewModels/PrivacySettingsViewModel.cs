using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.ViewModels
{
    public class PrivacySettingsViewModel
    {
        [Required]
        [RegularExpression("^(public|private)$", ErrorMessage = "Profile visibility must be public or private")]
        public string ProfileVisibility { get; set; } = "public";

        [Required]
        [RegularExpression("^(show|hide)$", ErrorMessage = "Location visibility must be show or hide")]
        public string LocationVisibility { get; set; } = "show";

        [Required]
        [RegularExpression("^(show|hide)$", ErrorMessage = "Activity streaks visibility must be show or hide")]
        public string ActivityStreaksVisibility { get; set; } = "show";

        [Required]
        [RegularExpression("^(show|hide)$", ErrorMessage = "Online status must be show or hide")]
        public string OnlineStatus { get; set; } = "show";

        [Required]
        [RegularExpression("^(allow|block)$", ErrorMessage = "Search indexing must be allow or block")]
        public string SearchIndexing { get; set; } = "allow";

        [Required]
        [RegularExpression("^(public|private)$", ErrorMessage = "Contact visibility must be public or private")]
        public string ContactVisibility { get; set; } = "public";

        [Required]
        [RegularExpression("^(show|hide)$", ErrorMessage = "Activity history must be show or hide")]
        public string ActivityHistory { get; set; } = "show";
    }
}