using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.ViewModels.Authentication
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        // SSO options
        public bool GoogleEnabled { get; set; }
        public bool MicrosoftEnabled { get; set; }
        public bool FacebookEnabled { get; set; }
    }
}