using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services
{
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly IFileService _fileService;
        private readonly ILogger<ItemService> _logger;

        public ItemService(
            ApplicationDbContext context,
            IPricingService pricingService,
            IFileService fileService,
            ILogger<ItemService> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<Item> CreateItemAsync(CreateItemDto dto, int userId)
        {
            try
            {
                // Validate user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                // Resolve or create location
                var location = await ResolveOrCreateLocationAsync(dto.LocationName);

                // Calculate pricing
                var adjustedPrice = _pricingService.CalculateAdjustedPrice(dto.CategoryId, dto.ConditionId);
                var finalPrice = dto.UserSetPrice ?? _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

                // Create the item
                var item = new Item
                {
                    UserId = userId,
                    CategoryId = dto.CategoryId,
                    ConditionId = dto.ConditionId,
                    LocationId = location.LocationId,
                    ItemTitle = dto.ItemTitle,
                    Description = dto.Description,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    AdjustedTokenPrice = adjustedPrice,
                    FinalTokenPrice = finalPrice,
                    PriceOverriddenByUser = dto.UserSetPrice.HasValue,
                    Status = ItemStatus.Available,
                    DatePosted = DateTime.UtcNow,
                    ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(30), // Default 30 days
                    AiProcessingStatus = AiProcessingStatus.Pending
                };

                // Handle image upload
                if (dto.ImageFile != null)
                {
                    item.ImageFileName = await _fileService.SaveFileAsync(dto.ImageFile, "ItemPosts");
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                // Start AI processing in background (temporary implementation)
               // _ = Task.Run(async () => await ProcessItemWithAIAsync(item.ItemId));

                _logger.LogInformation("Item created successfully: {Title}, ID: {Id}, UserId: {UserId}",
                    item.ItemTitle, item.ItemId, item.UserId);

                await _context.Entry(item)
                    .Reference(i => i.User)
                    .LoadAsync();
                await _context.Entry(item)
                    .Reference(i => i.Category)
                    .LoadAsync();
                await _context.Entry(item)
                    .Reference(i => i.Condition)
                    .LoadAsync();
                await _context.Entry(item)
                    .Reference(i => i.Location)
                    .LoadAsync();

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Item?> GetItemByIdAsync(int itemId)
        {
            return await _context.Items
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Condition)
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.ItemId == itemId);
        }

        public async Task<List<Item>> GetAllActiveItemsAsync()
        {
            try
            {
                var items = await _context.Items
                    .Where(i => i.Status == ItemStatus.Available && i.ExpiresAt > DateTime.UtcNow)
                    .Include(i => i.User)
                    .Include(i => i.Category)
                    .Include(i => i.Condition)
                    .Include(i => i.Location)
                    .OrderByDescending(i => i.DatePosted)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} active items", items.Count);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active items");
                throw;
            }
        }

        public async Task<List<Item>> GetItemsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Items
                    .Where(i => i.UserId == userId)
                    .Include(i => i.Category)
                    .Include(i => i.Condition)
                    .Include(i => i.Location)
                    .OrderByDescending(i => i.DatePosted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Item> UpdateItemAsync(int itemId, UpdateItemDto dto, int userId)
        {
            var item = await GetItemByIdAsync(itemId);
            if (item == null)
                throw new ArgumentException($"Item with ID {itemId} not found");

            if (item.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to edit this item");

            try
            {
                // Resolve or create location if changed
                var location = await ResolveOrCreateLocationAsync(dto.LocationName);

                // Recalculate pricing if category or condition changed
                var adjustedPrice = _pricingService.CalculateAdjustedPrice(dto.CategoryId, dto.ConditionId);
                var finalPrice = dto.FinalTokenPrice > 0 ? dto.FinalTokenPrice :
                                _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

                // Update item properties
                item.ItemTitle = dto.ItemTitle;
                item.Description = dto.Description;
                item.CategoryId = dto.CategoryId;
                item.ConditionId = dto.ConditionId;
                item.LocationId = location.LocationId;
                item.Latitude = dto.Latitude;
                item.Longitude = dto.Longitude;
                item.AdjustedTokenPrice = adjustedPrice;
                item.FinalTokenPrice = finalPrice;
                item.Status = dto.Status;
                item.ExpiresAt = dto.ExpiresAt;
                item.PriceOverriddenByUser = dto.FinalTokenPrice > 0;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Item updated successfully: {Title}, ID: {Id}", item.ItemTitle, item.ItemId);
                return await GetItemByIdAsync(itemId) ?? item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId}", itemId);
                throw;
            }
        }

        public async Task<bool> DeleteItemAsync(int itemId, int userId)
        {
            var item = await GetItemByIdAsync(itemId);
            if (item == null)
                return false;

            if (item.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this item");

            try
            {
                // Delete associated image file
                if (!string.IsNullOrEmpty(item.ImageFileName))
                {
                    var imagePath = Path.Combine("wwwroot", "ItemPosts", item.ImageFileName);
                    await _fileService.DeleteFileAsync(imagePath);
                }

                // Soft delete by updating status
                item.Status = ItemStatus.Removed;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item deleted successfully: ID {Id}", itemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId}", itemId);
                throw;
            }
        }

        public async Task<List<Item>> SearchItemsAsync(ItemSearchDto searchDto)
        {
            var query = _context.Items
                .Where(i => i.Status == ItemStatus.Available && i.ExpiresAt > DateTime.UtcNow)
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Condition)
                .Include(i => i.Location)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.SearchQuery))
            {
                query = query.Where(i => i.ItemTitle.Contains(searchDto.SearchQuery) ||
                                        i.Description.Contains(searchDto.SearchQuery));
            }

            if (searchDto.CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == searchDto.CategoryId.Value);
            }

            if (searchDto.LocationId.HasValue)
            {
                query = query.Where(i => i.LocationId == searchDto.LocationId.Value);
            }

            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(i => i.FinalTokenPrice >= searchDto.MinPrice.Value);
            }

            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(i => i.FinalTokenPrice <= searchDto.MaxPrice.Value);
            }

            // Apply sorting
            query = searchDto.SortBy?.ToLower() switch
            {
                "price" => searchDto.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.FinalTokenPrice)
                    : query.OrderBy(i => i.FinalTokenPrice),
                "title" => searchDto.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.ItemTitle)
                    : query.OrderBy(i => i.ItemTitle),
                _ => query.OrderByDescending(i => i.DatePosted)
            };

            // Apply pagination
            var skip = (searchDto.Page - 1) * searchDto.PageSize;
            return await query.Skip(skip).Take(searchDto.PageSize).ToListAsync();
        }

        public async Task<ItemLocation> ResolveOrCreateLocationAsync(string locationName)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                throw new ArgumentException("Location name cannot be empty");

            var existing = await _context.ItemLocations
                .FirstOrDefaultAsync(l => l.Name.ToLower() == locationName.ToLower());

            if (existing != null)
                return existing;

            var newLocation = new ItemLocation { Name = locationName, IsActive = true };
            _context.ItemLocations.Add(newLocation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new location: {Name}", locationName);
            return newLocation;
        }

        public async Task<bool> ValidateItemOwnershipAsync(int itemId, int userId)
        {
            return await _context.Items
                .AnyAsync(i => i.ItemId == itemId && i.UserId == userId);
        }

        public async Task IncrementViewCountAsync(int itemId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item != null)
            {
                item.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        // Temporary AI implementation - to be replaced with actual Azure CV and TensorFlow
        public async Task ProcessItemWithAIAsync(int itemId)
        {
            try
            {
                var item = await _context.Items.FindAsync(itemId);
                if (item == null) return;

                // Update status to processing
                item.AiProcessingStatus = AiProcessingStatus.Processing;
                await _context.SaveChangesAsync();

                // Simulate AI processing delay
                await Task.Delay(2000);

                // Temporary AI suggestions (replace with actual AI logic)
                item.AiDetectedCategory = await GetTemporaryAICategory(item.CategoryId);
                item.AiConditionScore = await GetTemporaryConditionScore(item.ConditionId);
                item.AiSuggestedPrice = await GetAISuggestedPriceAsync(item.CategoryId, item.ConditionId, item.ImageFileName);
                item.AiConfidenceLevel = GetRandomConfidenceLevel();
                item.AiProcessedAt = DateTime.UtcNow;
                item.AiProcessingStatus = AiProcessingStatus.Completed;

                await _context.SaveChangesAsync();

                _logger.LogInformation("AI processing completed for item {ItemId}", itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI processing failed for item {ItemId}", itemId);

                var item = await _context.Items.FindAsync(itemId);
                if (item != null)
                {
                    item.AiProcessingStatus = AiProcessingStatus.Failed;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<decimal> GetAISuggestedPriceAsync(int categoryId, int conditionId, string? imageUrl = null)
        {
            // Temporary implementation - replace with actual AI price prediction
            await Task.Delay(100); // Simulate processing time

            var basePrice = _pricingService.CalculateAdjustedPrice(categoryId, conditionId);
            var variation = new Random().Next(-20, 21) / 100m; // ±20% variation
            return Math.Max(1, basePrice + (basePrice * variation));
        }

        private async Task<string> GetTemporaryAICategory(int categoryId)
        {
            var category = await _context.ItemCategories.FindAsync(categoryId);
            return category?.Name ?? "Unknown";
        }

        private async Task<decimal> GetTemporaryConditionScore(int conditionId)
        {
            // Return a score between 1-5 based on condition
            return conditionId switch
            {
                1 => 5.0m, // Brand New
                2 => 4.5m, // Like New
                3 => 3.5m, // Gently Used
                4 => 2.5m, // Visible Wear
                5 => 1.5m, // For Repair/Parts
                _ => 3.0m
            };
        }

        private decimal GetRandomConfidenceLevel()
        {
            var random = new Random();
            return Math.Round((decimal)(random.NextDouble() * 0.4 + 0.6), 2); // 60-100% confidence
        }
    }
}