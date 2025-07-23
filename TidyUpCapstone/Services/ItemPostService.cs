//using Microsoft.EntityFrameworkCore;
//using TidyUpCapstone.Models.DTOs;
//using TidyUpCapstone.Models.Entities.Items;
//using TidyUpCapstone.Services.Interfaces;
//using TidyUpCapstone.Data;

//namespace TidyUpCapstone.Services
//{
//    public class ItemPostService : IItemPostService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IPricingService _pricingService;
//        private readonly IFileService _fileService;
//        private readonly ILogger<ItemPostService> _logger;

//        public ItemPostService(
//            ApplicationDbContext context,
//            IPricingService pricingService,
//            IFileService fileService,
//            ILogger<ItemPostService> logger)
//        {
//            _context = context;
//            _pricingService = pricingService;
//            _fileService = fileService;
//            _logger = logger;
//        }

//        //public async Task<Item> CreateItemPostAsync(ItemDto dto, string userId, int locationId)
//        //{
//        //    try
//        //    {
//        //        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
//        //        if (!userExists)
//        //        {
//        //            throw new ArgumentException($"User with ID {userId} not found");
//        //        }

//        //        var adjustedPrice = _pricingService.CalculateAdjustedPrice(dto.CategoryId, dto.ConditionId);
//        //        var finalPrice = _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

//        //        var item = new ItemPost
//        //        {
//        //            ItemTitle = dto.ItemTitle,
//        //            CategoryId = dto.CategoryId,
//        //            ConditionId = dto.ConditionId,
//        //            LocationId = locationId, // <-- from controller
//        //            Description = dto.Description,
//        //            AdjustedTokenPrice = adjustedPrice,
//        //            FinalTokenPrice = finalPrice,
//        //            Latitude = dto.Latitude,
//        //            Longitude = dto.Longitude,
//        //            CreatedAt = DateTime.UtcNow,
//        //            IsActive = true,
//        //            UserId = userId
//        //        };

//        //        if (dto.ImageFile != null)
//        //        {
//        //            item.ImageFileName = await _fileService.SaveFileAsync(dto.ImageFile, "ItemPosts");
//        //        }

//        //        _context.ItemPosts.Add(item);
//        //        await _context.SaveChangesAsync();

//        //        _logger.LogInformation("Item created successfully: {Title}, ID: {Id}, UserId: {UserId}",
//        //            item.ItemTitle, item.Id, item.UserId);

//        //        return item;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error creating item post for user {UserId}", userId);
//        //        throw;
//        //    }
//        //}

//        //public async Task<ItemPost?> GetItemPostByIdAsync(int id)
//        //{
//        //    return await _context.ItemPosts
//        //        .Include(p => p.Category)
//        //        .Include(p => p.Condition)
//        //        .Include(p => p.Location)
//        //        .Include(p => p.User)
//        //        .FirstOrDefaultAsync(p => p.Id == id);
//        //}

//        //public async Task<List<ItemPost>> GetAllItemPostsAsync()
//        //{
//        //    try
//        //    {
//        //        var posts = await _context.ItemPosts
//        //            .Where(p => p.IsActive)
//        //            .Include(p => p.Category)
//        //            .Include(p => p.Condition)
//        //            .Include(p => p.Location)
//        //            .Include(p => p.User)
//        //            .OrderByDescending(p => p.CreatedAt)
//        //            .ToListAsync();

//        //        _logger.LogInformation("Retrieved {Count} active item posts", posts.Count);
//        //        return posts;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error retrieving all item posts");
//        //        throw;
//        //    }
//        //}

//        //public async Task<ItemPost> UpdateItemPostAsync(int id, ItemPostDto dto)
//        //{
//        //    var item = await GetItemPostByIdAsync(id);
//        //    if (item == null)
//        //        throw new ArgumentException($"Item with ID {id} not found");

//        //    try
//        //    {
//        //        var adjustedPrice = _pricingService.CalculateAdjustedPrice(dto.CategoryId, dto.ConditionId);
//        //        var finalPrice = _pricingService.CalculateFinalPriceAfterTax(adjustedPrice);

//        //        // ✅ Handle image update
//        //        if (dto.ImageFile != null)
//        //        {
//        //            if (!string.IsNullOrEmpty(item.ImageFileName))
//        //            {
//        //                var oldImagePath = Path.Combine("wwwroot", "ItemPosts", item.ImageFileName);
//        //                await _fileService.DeleteFileAsync(oldImagePath);
//        //            }

//        //            item.ImageFileName = await _fileService.SaveFileAsync(dto.ImageFile, "ItemPosts");
//        //        }

//        //        // ✅ Resolve or create location and update LocationId
//        //        var location = await ResolveOrCreateLocationAsync(dto.LocationName);
//        //        item.LocationId = location.Id;

//        //        // ✅ Update other fields
//        //        item.ItemTitle = dto.ItemTitle;
//        //        item.CategoryId = dto.CategoryId;
//        //        item.ConditionId = dto.ConditionId;
//        //        item.Description = dto.Description;
//        //        item.Latitude = dto.Latitude;
//        //        item.Longitude = dto.Longitude;
//        //        item.AdjustedTokenPrice = adjustedPrice;
//        //        item.FinalTokenPrice = finalPrice;
//        //        item.UpdatedAt = DateTime.UtcNow;

//        //        await _context.SaveChangesAsync();

//        //        _logger.LogInformation("Item updated successfully: {Title}, ID: {Id}", item.ItemTitle, item.Id);
//        //        return item;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error updating item post with ID {ItemId}", id);
//        //        throw;
//        //    }
//        //}


//        //public async Task<bool> DeleteItemPostAsync(int id)
//        //{
//        //    var item = await GetItemPostByIdAsync(id);
//        //    if (item == null)
//        //        return false;

//        //    try
//        //    {
//        //        if (!string.IsNullOrEmpty(item.ImageFileName))
//        //        {
//        //            var imagePath = Path.Combine("wwwroot", "ItemPosts", item.ImageFileName);
//        //            await _fileService.DeleteFileAsync(imagePath);
//        //        }

//        //        _context.ItemPosts.Remove(item);
//        //        await _context.SaveChangesAsync();

//        //        _logger.LogInformation("Item deleted successfully: ID {Id}", id);
//        //        return true;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error deleting item post with ID {ItemId}", id);
//        //        throw;
//        //    }
//        //}

//        //public async Task<List<ItemPost>> GetItemPostsByUserIdAsync(string userId)
//        //{
//        //    try
//        //    {
//        //        return await _context.ItemPosts
//        //            .Where(p => p.UserId == userId && p.IsActive)
//        //            .Include(p => p.Category)
//        //            .Include(p => p.Condition)
//        //            .Include(p => p.Location)
//        //            .OrderByDescending(p => p.CreatedAt)
//        //            .ToListAsync();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error retrieving item posts for user {UserId}", userId);
//        //        throw;
//        //    }
//        //}

//        //public async Task<ItemLocation> ResolveOrCreateLocationAsync(string locationName)
//        //{
//        //    var existing = await _context.ItemLocations
//        //        .FirstOrDefaultAsync(l => l.Name.ToLower() == locationName.ToLower());

//        //    if (existing != null)
//        //        return existing;

//        //    var newLocation = new ItemLocation { Name = locationName };
//        //    _context.ItemLocations.Add(newLocation);
//        //    await _context.SaveChangesAsync();

//        //    _logger.LogInformation("Created new location: {Name}", locationName);
//        //    return newLocation;
//        //}
//    }
//}
