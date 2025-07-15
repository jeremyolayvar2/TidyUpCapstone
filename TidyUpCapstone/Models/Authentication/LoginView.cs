using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.Authentication
{
    public class LoginView
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}