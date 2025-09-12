using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Models.AI; // Add this for VisionAnalysisResult

namespace TidyUpCapstone.Services
{
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly IFileService _fileService;
        private readonly IVisionService _visionService; // Add Vision service
        private readonly ILogger<ItemService> _logger;

        public ItemService(
            ApplicationDbContext context,
            IPricingService pricingService,
            IFileService fileService,
            IVisionService visionService, // Add Vision service injection
            ILogger<ItemService> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _fileService = fileService;
            _visionService = visionService; // Initialize Vision service
            _logger = logger;
        }

        public async Task<Item> CreateItemAsync(CreateItemDto dto, int userId)
        {
            try
            {
                _logger.LogInformation("Creating item with Vision AI analysis for user {UserId}", userId);

                // Validate user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    throw new ArgumentException($"User with ID {userId} not found");
                }

                // Step 1: Analyze image with Google Vision API
                VisionAnalysisResult? visionResult = null;
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    try
                    {
                        visionResult = await _visionService.AnalyzeImageAsync(dto.ImageFile);
                        _logger.LogInformation("Vision analysis completed. Suggested category: {CategoryId}, Confidence: {Confidence:P2}",
                            visionResult.SuggestedCategoryId, visionResult.ConfidenceScore);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Vision analysis failed, continuing with user-provided category");
                    }
                }

                // Step 2: Determine final category
                int finalCategoryId = dto.CategoryId;
                bool categoryOverridden = false;

                // If vision suggests a different category with high confidence, log suggestion
                if (visionResult?.Success == true &&
                    visionResult.SuggestedCategoryId != dto.CategoryId &&
                    visionResult.ConfidenceScore > 0.75m)
                {
                    _logger.LogInformation("Vision API suggests different category: {SuggestedId} vs user selected: {UserSelected}",
                        visionResult.SuggestedCategoryId, dto.CategoryId);

                    // For now, we'll use the user's selection but store the AI suggestion
                    // You could implement a confirmation dialog in the frontend later
                    categoryOverridden = true;
                }

                // Resolve or create location
                var location = await ResolveOrCreateLocationAsync(dto.LocationName);

                // Calculate pricing
                var adjustedPrice = _pricingService.CalculateAdjustedPrice(finalCategoryId, dto.ConditionId);
                var finalPrice = dto.UserSetPrice ?? _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

                // Create the item
                var item = new Item
                {
                    UserId = userId,
                    CategoryId = finalCategoryId,
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
                    ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(30),

                    // AI-related properties
                    AiProcessingStatus = visionResult?.Success == true ? AiProcessingStatus.Completed : AiProcessingStatus.Failed,
                    AiProcessedAt = visionResult?.Success == true ? DateTime.UtcNow : null,
                    AiDetectedCategory = visionResult?.Success == true ? GetCategoryName(visionResult.SuggestedCategoryId) : null,
                    AiConfidenceLevel = visionResult?.ConfidenceScore,
                    AiSuggestedPrice = null // You can implement price suggestion later
                };

                // Handle image upload
                if (dto.ImageFile != null)
                {
                    item.ImageFileName = await _fileService.SaveFileAsync(dto.ImageFile, "ItemPosts");
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                // Step 3: Save detailed Vision analysis results for future reference
                if (visionResult?.Success == true)
                {
                    await SaveVisionAnalysisAsync(item.ItemId, visionResult);
                }

                _logger.LogInformation("Item created successfully with AI analysis: {Title}, ID: {Id}, UserId: {UserId}",
                    item.ItemTitle, item.ItemId, item.UserId);

                // Load relationships for return
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
                _logger.LogError(ex, "Error creating item with Vision AI analysis for user {UserId}", userId);
                throw;
            }
        }

        private async Task SaveVisionAnalysisAsync(int itemId, VisionAnalysisResult visionResult)
        {
            try
            {
                // Create a record to store the detailed Vision API results
                var analysis = new Models.Entities.AI.AzureCvAnalysis // Use existing entity
                {
                    ItemId = itemId,
                    AnalysisResult = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Labels = visionResult.Labels.Take(10).Select(l => new { l.Description, l.Score }),
                        Objects = visionResult.Objects.Take(5).Select(o => new { o.Name, o.Score }),
                        SuggestedCategoryId = visionResult.SuggestedCategoryId,
                        ConfidenceScore = visionResult.ConfidenceScore
                    }),
                    ConfidenceScore = visionResult.ConfidenceScore, // FIXED: No cast needed - both are decimal
                    ProcessedAt = visionResult.ProcessedAt,
                    Status = "completed"
                };

                _context.AzureCvAnalyses.Add(analysis);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Vision analysis results saved for item {ItemId}", itemId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save vision analysis results for item {ItemId}", itemId);
                // Don't throw - this is not critical for item creation
            }
        }

        private string GetCategoryName(int categoryId)
        {
            return categoryId switch
            {
                1 => "Books & Stationery",
                2 => "Electronics & Gadgets",
                3 => "Toys & Games",
                4 => "Home & Kitchen",
                5 => "Furniture",
                6 => "Appliances",
                7 => "Health & Beauty",
                8 => "Crafts & DIY",
                9 => "School & Office",
                10 => "Sentimental Items",
                11 => "Miscellaneous",
                12 => "Clothing",
                _ => "Unknown"
            };
        }

        // Add method to get Vision analysis for an item
        public async Task<VisionAnalysisResult?> GetVisionAnalysisAsync(int itemId)
        {
            try
            {
                var analysis = await _context.AzureCvAnalyses
                    .Where(a => a.ItemId == itemId)
                    .OrderByDescending(a => a.ProcessedAt)
                    .FirstOrDefaultAsync();

                if (analysis == null || string.IsNullOrEmpty(analysis.AnalysisResult))
                    return null;

                // Deserialize the stored analysis
                var analysisData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(analysis.AnalysisResult);

                return new VisionAnalysisResult
                {
                    Success = analysis.Status == "completed",
                    SuggestedCategoryId = analysisData?.SuggestedCategoryId ?? 11,
                    ConfidenceScore = (decimal)(analysisData?.ConfidenceScore ?? 0),
                    ProcessedAt = analysis.ProcessedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vision analysis for item {ItemId}", itemId);
                return null;
            }
        }

        // Add method to re-analyze an item's image
        public async Task<VisionAnalysisResult?> ReanalyzeItemImageAsync(int itemId)
        {
            try
            {
                var item = await GetItemByIdAsync(itemId);
                if (item == null || string.IsNullOrEmpty(item.ImageFileName))
                    return null;

                // Read the image file
                var imagePath = Path.Combine("wwwroot", "ItemPosts", item.ImageFileName);
                if (!File.Exists(imagePath))
                    return null;

                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                var result = await _visionService.AnalyzeImageAsync(imageBytes);

                if (result.Success)
                {
                    // Update item with new analysis
                    item.AiDetectedCategory = GetCategoryName(result.SuggestedCategoryId);
                    item.AiConfidenceLevel = result.ConfidenceScore;
                    item.AiProcessedAt = DateTime.UtcNow;
                    item.AiProcessingStatus = AiProcessingStatus.Completed;

                    await _context.SaveChangesAsync();

                    // Save detailed results
                    await SaveVisionAnalysisAsync(itemId, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-analyzing item {ItemId}", itemId);
                return null;
            }
        }

        // FIXED: Add the missing ProcessItemWithAIAsync method implementation
        public async Task ProcessItemWithAIAsync(int itemId)
        {
            try
            {
                _logger.LogInformation("Processing item {ItemId} with AI", itemId);

                var item = await GetItemByIdAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning("Item {ItemId} not found for AI processing", itemId);
                    return;
                }

                // Set processing status to pending
                item.AiProcessingStatus = AiProcessingStatus.Processing;
                await _context.SaveChangesAsync();

                // Re-analyze the image if it exists
                var analysisResult = await ReanalyzeItemImageAsync(itemId);

                if (analysisResult?.Success == true)
                {
                    // Update item with AI results
                    item.AiProcessingStatus = AiProcessingStatus.Completed;
                    item.AiProcessedAt = DateTime.UtcNow;
                    item.AiDetectedCategory = GetCategoryName(analysisResult.SuggestedCategoryId);
                    item.AiConfidenceLevel = analysisResult.ConfidenceScore;

                    _logger.LogInformation("AI processing completed for item {ItemId}", itemId);
                }
                else
                {
                    // Mark as failed
                    item.AiProcessingStatus = AiProcessingStatus.Failed;
                    _logger.LogWarning("AI processing failed for item {ItemId}", itemId);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI processing for item {ItemId}", itemId);

                // Mark as failed in database
                try
                {
                    var item = await _context.Items.FindAsync(itemId);
                    if (item != null)
                    {
                        item.AiProcessingStatus = AiProcessingStatus.Failed;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to update AI processing status to failed for item {ItemId}", itemId);
                }
            }
        }

        // Rest of your existing methods remain the same...
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

        public async Task<decimal> GetAISuggestedPriceAsync(int categoryId, int conditionId, string? imageUrl = null)
        {
            // This could be enhanced to use Vision API for price estimation
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
                3=> 3.5m, // Gently Used
                4 => 2.5m, // Visible Wear
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