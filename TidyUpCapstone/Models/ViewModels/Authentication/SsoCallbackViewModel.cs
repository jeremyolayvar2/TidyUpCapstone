using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.ViewModels.Authentication
{
    public class SsoCallbackViewModel
    {
        public string? Code { get; set; }
        public string? State { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }

    public class LinkSsoAccountViewModel
    {
        [Required]
        public string Provider { get; set; } = string.Empty;

        [Required]
        public string ProviderUserId { get; set; } = string.Empty;

        [EmailAddress]
        public string? ProviderEmail { get; set; }

        public string? ProviderDisplayName { get; set; }

        public string? ProviderAvatarUrl { get; set; }

        [Display(Name = "Set as primary login method")]
        public bool SetAsPrimary { get; set; }
    }

    public class SsoLoginResultViewModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public bool RequiresAccountLink { get; set; }
        public string? Provider { get; set; }
        public string? ProviderEmail { get; set; }
        public string? ReturnUrl { get; set; }
    }
}