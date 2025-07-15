using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.Authentication
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmNewPassword", ErrorMessage = "Password Does not Match.")]
        [Display(Name = "New Password")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public required string ConfirmNewPassword { get; set; }
    }
}