using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.ViewModels.Authentication
{
    public class UserManagementViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number (Optional)")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Display(Name = "I accept the Terms and Conditions")]
        public bool AcceptTerms { get; set; }

        [Display(Name = "I accept the Privacy Policy")]
        [Required(ErrorMessage = "You must accept the privacy policy")]
        public bool AcceptPrivacy { get; set; }

        [Display(Name = "Subscribe to marketing emails")]
        public bool MarketingEmails { get; set; }

        public string? ReturnUrl { get; set; }
    }
}