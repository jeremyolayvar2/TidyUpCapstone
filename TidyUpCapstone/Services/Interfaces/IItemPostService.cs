using TidyUpCapstone.Models.DTOs;
using TidyUpCapstone.Models.Entities;


namespace TidyUpCapstone.Services.Interfaces
{
    public interface IItemPostService
    {
        Task<ItemPost> CreateItemPostAsync(ItemPostDto dto, string userId, int locationId);
        Task<ItemPost?> GetItemPostByIdAsync(int id);
        Task<List<ItemPost>> GetAllItemPostsAsync();
        Task<List<ItemPost>> GetItemPostsByUserIdAsync(string userId);
        Task<ItemPost> UpdateItemPostAsync(int id, ItemPostDto dto);
        Task<bool> DeleteItemPostAsync(int id);

        Task<ItemLocation> ResolveOrCreateLocationAsync(string locationName);
    }
}