using Microsoft.AspNetCore.Identity;

namespace TidyUpCapstone.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public decimal TokenBalance { get; set; }
        public bool WelcomeTokenGranted { get; set; } = false;
        public ICollection<ItemPost> ItemPosts { get; set; } = new List<ItemPost>();
    }
}