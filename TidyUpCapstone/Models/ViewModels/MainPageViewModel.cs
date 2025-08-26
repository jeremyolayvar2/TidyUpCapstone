using TidyUpCapstone.Models.Entities;
//using TidyUpCapstone.Models.DTOs;

namespace TidyUpCapstone.Models.ViewModels
{
    public class MainPageViewModel
    {
        public List<ItemPost> ItemPosts { get; set; } = new();
        //public ItemPostDto NewItemPost { get; set; } = new();
        public decimal CurrentUserTokenBalance { get; set; }
        public List<Messages> Messages { get; set; } = new();
    }
}