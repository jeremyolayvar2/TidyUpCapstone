using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.ViewModels
{
    public class MessagePageViewModel
    {
        public AppUser? CurrentUser { get; set; }
        public AppUser? OtherUser { get; set; }
        public List<AppUser> TestUsers { get; set; } = new List<AppUser>();
        public bool IsTestMode { get; set; } = true;
    }
}