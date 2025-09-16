using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Services.Interfaces
{
    public interface IItemService
    {
        Task<Item> CreateItemAsync(CreateItemDto dto, int userId);
        Task<Item?> GetItemByIdAsync(int itemId);
        Task<List<Item>> GetAllActiveItemsAsync();
        Task<List<Item>> GetItemsByUserIdAsync(int userId);
        Task<Item> UpdateItemAsync(int itemId, UpdateItemDto dto, int userId);
        Task<bool> DeleteItemAsync(int itemId, int userId);
        Task<List<Item>> SearchItemsAsync(ItemSearchDto searchDto);

        // Helper methods
        Task<ItemLocation> ResolveOrCreateLocationAsync(string locationName);
        Task<bool> ValidateItemOwnershipAsync(int itemId, int userId);
        Task IncrementViewCountAsync(int itemId);

        // Future AI integration methods (temporary implementation)
        Task ProcessItemWithAIAsync(int itemId);
        Task<decimal> GetAISuggestedPriceAsync(int categoryId, int conditionId, string? imageUrl = null);

        Task<VisionAnalysisResult?> GetVisionAnalysisAsync(int itemId);
        Task<VisionAnalysisResult?> ReanalyzeItemImageAsync(int itemId);
    }
}