using System.ComponentModel.DataAnnotations;

namespace TidyUp.Models.Authentication
{
    public class RegisterView
    {
        [Required(ErrorMessage = "Name is Required.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password Does not Match.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public required string ConfirmPassword { get; set; }
    }
}