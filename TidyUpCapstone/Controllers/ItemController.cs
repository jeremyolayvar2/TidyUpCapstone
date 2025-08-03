using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.ViewModels.Items;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ItemController> _logger;

        public ItemController(
            IItemService itemService,
            ApplicationDbContext context,
            ILogger<ItemController> logger)
        {
            _itemService = itemService;
            _context = context;
            _logger = logger;
        }

        // GET: Item/Index
        [AllowAnonymous]
        public async Task<IActionResult> Index(ItemSearchDto? searchCriteria = null)
        {
            try
            {
                searchCriteria ??= new ItemSearchDto();

                var items = await _itemService.SearchItemsAsync(searchCriteria);
                var categories = await GetCategoriesAsync();
                var locations = await GetLocationsAsync();
                var conditions = await GetConditionsAsync();

                var itemDtos = items.Select(MapToItemDto).ToList();

                var viewModel = new ItemListViewModel
                {
                    Items = itemDtos,
                    SearchCriteria = searchCriteria,
                    Categories = categories,
                    Locations = locations,
                    Conditions = conditions,
                    TotalItems = itemDtos.Count,
                    PageTitle = "Browse Items",
                    ShowFilters = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading items index");
                return View(new ItemListViewModel());
            }
        }

        // GET: Item/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                // Increment view count
                await _itemService.IncrementViewCountAsync(id);

                var currentUserId = GetCurrentUserId();
                var itemDto = MapToItemDto(item);

                var relatedItems = await GetRelatedItemsAsync(item.CategoryId, id);
                var sellerOtherItems = await GetSellerOtherItemsAsync(item.UserId, id);

                var viewModel = new ItemDetailsViewModel
                {
                    Item = itemDto,
                    RelatedItems = relatedItems.Take(6).ToList(),
                    SellerOtherItems = sellerOtherItems.Take(4).ToList(),
                    CanEdit = currentUserId == item.UserId,
                    CanPurchase = currentUserId != item.UserId && currentUserId > 0,
                    IsOwner = currentUserId == item.UserId,
                    ShowContactSeller = currentUserId != item.UserId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item details for ID {ItemId}", id);
                return NotFound();
            }
        }

        // GET: Item/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await GetCategoriesAsync();
                var conditions = await GetConditionsAsync();
                var locations = await GetLocationsAsync();

                var viewModel = new CreateItemViewModel
                {
                    Categories = categories,
                    Conditions = conditions,
                    Locations = locations,
                    ShowPriceGuidance = true,
                    PricingTips = GetPricingTips()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create item page");
                return RedirectToAction("Index");
            }
        }

        // POST: Item/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateItemDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please fill in all required fields correctly." });
                }

                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Json(new { success = false, message = "User authentication required." });
                }

                var item = await _itemService.CreateItemAsync(dto, userId);

                _logger.LogInformation("Item created successfully by user {UserId}: {ItemTitle}", userId, item.ItemTitle);

                return Json(new { success = true, message = "Item posted successfully!", itemId = item.ItemId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item for user {UserId}", GetCurrentUserId());
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Item/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                var userId = GetCurrentUserId();
                if (item.UserId != userId)
                {
                    return Forbid();
                }

                // Return JSON for AJAX requests
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        itemTitle = item.ItemTitle,
                        description = item.Description,
                        itemCategory = item.Category?.Name,
                        itemCondition = item.Condition?.Name,
                        itemLocation = item.Location?.Name,
                        latitude = item.Latitude,
                        longitude = item.Longitude,
                        finalTokenPrice = item.FinalTokenPrice
                    });
                }

                // Return view for normal requests
                var categories = await GetCategoriesAsync();
                var conditions = await GetConditionsAsync();
                var locations = await GetLocationsAsync();

                var updateDto = new UpdateItemDto
                {
                    ItemTitle = item.ItemTitle,
                    Description = item.Description,
                    CategoryId = item.CategoryId,
                    ConditionId = item.ConditionId,
                    LocationId = item.LocationId,
                    FinalTokenPrice = item.FinalTokenPrice,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    ExpiresAt = item.ExpiresAt,
                    Status = item.Status
                };

                var viewModel = new EditItemViewModel
                {
                    Item = updateDto,
                    ItemId = id,
                    Categories = categories,
                    Conditions = conditions,
                    Locations = locations,
                    CurrentImageUrl = item.ImageFileName,
                    IsActive = item.Status == Models.Entities.Items.ItemStatus.Available,
                    ViewCount = item.ViewCount,
                    DatePosted = item.DatePosted
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit page for item {ItemId}", id);
                return NotFound();
            }
        }

        // POST: Item/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateItemDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Please fill in all required fields correctly." });
                }

                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Json(new { success = false, message = "User authentication required." });
                }

                // Create a temporary DTO with location name for the service
                var serviceDto = new UpdateItemDto
                {
                    ItemTitle = dto.ItemTitle,
                    Description = dto.Description,
                    CategoryId = dto.CategoryId,
                    ConditionId = dto.ConditionId,
                    LocationId = dto.LocationId,
                    FinalTokenPrice = dto.FinalTokenPrice,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    ExpiresAt = dto.ExpiresAt,
                    Status = dto.Status,
                    LocationName = Request.Form["LocationName"].ToString()
                };

                var updatedItem = await _itemService.UpdateItemAsync(id, serviceDto, userId);

                _logger.LogInformation("Item updated successfully: {ItemTitle} (ID: {ItemId})", updatedItem.ItemTitle, id);

                return Json(new { success = true, message = "Item updated successfully!" });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "You don't have permission to edit this item." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Item/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Json(new { success = false, message = "User authentication required." });
                }

                var success = await _itemService.DeleteItemAsync(id, userId);
                if (!success)
                {
                    return Json(new { success = false, message = "Item not found." });
                }

                _logger.LogInformation("Item deleted successfully: ID {ItemId} by user {UserId}", id, userId);

                return Json(new { success = true, message = "Item deleted successfully!" });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "You don't have permission to delete this item." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the item." });
            }
        }

        // GET: Item/MyItems
        public async Task<IActionResult> MyItems()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return RedirectToAction("Login", "Account");
                }

                var items = await _itemService.GetItemsByUserIdAsync(userId);
                var itemDtos = items.Select(MapToItemDto).ToList();

                var viewModel = new ItemListViewModel
                {
                    Items = itemDtos,
                    PageTitle = "My Items",
                    ShowFilters = false
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user items for user {UserId}", GetCurrentUserId());
                return View("Index", new ItemListViewModel());
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdString, out var userId) ? userId : 0;
        }

        private async Task<List<ItemCategoryDto>> GetCategoriesAsync()
        {
            return await _context.ItemCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new ItemCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ItemCount = c.Items.Count(i => i.Status == Models.Entities.Items.ItemStatus.Available)
                })
                .ToListAsync();
        }

        private async Task<List<ItemConditionDto>> GetConditionsAsync()
        {
            return await _context.ItemConditions
                .Where(c => c.IsActive)
                .OrderBy(c => c.ConditionId)
                .Select(c => new ItemConditionDto
                {
                    ConditionId = c.ConditionId,
                    Name = c.Name,
                    Description = c.Description,
                    ConditionMultiplier = c.ConditionMultiplier,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        private async Task<List<ItemLocationDto>> GetLocationsAsync()
        {
            return await _context.ItemLocations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new ItemLocationDto
                {
                    LocationId = l.LocationId,
                    Name = l.Name,
                    Region = l.Region,
                    IsActive = l.IsActive,
                    ItemCount = l.Items.Count(i => i.Status == Models.Entities.Items.ItemStatus.Available)
                })
                .ToListAsync();
        }

        private async Task<List<ItemDto>> GetRelatedItemsAsync(int categoryId, int excludeItemId)
        {
            var items = await _context.Items
                .Where(i => i.CategoryId == categoryId &&
                           i.ItemId != excludeItemId &&
                           i.Status == Models.Entities.Items.ItemStatus.Available)
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Condition)
                .Include(i => i.Location)
                .OrderByDescending(i => i.DatePosted)
                .Take(6)
                .ToListAsync();

            return items.Select(MapToItemDto).ToList();
        }

        private async Task<List<ItemDto>> GetSellerOtherItemsAsync(int sellerId, int excludeItemId)
        {
            var items = await _context.Items
                .Where(i => i.UserId == sellerId &&
                           i.ItemId != excludeItemId &&
                           i.Status == Models.Entities.Items.ItemStatus.Available)
                .Include(i => i.User)
                .Include(i => i.Category)
                .Include(i => i.Condition)
                .Include(i => i.Location)
                .OrderByDescending(i => i.DatePosted)
                .Take(4)
                .ToListAsync();

            return items.Select(MapToItemDto).ToList();
        }

        private ItemDto MapToItemDto(Models.Entities.Items.Item item)
        {
            return new ItemDto
            {
                ItemId = item.ItemId,
                UserId = item.UserId,
                Username = item.User?.UserName ?? "Unknown User",
                //UserAvatarUrl = item.User?.ProfilePictureUrl ?? "/assets/person-image.svg",
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name ?? "Unknown Category",
                ConditionId = item.ConditionId,
                ConditionName = item.Condition?.Name ?? "Unknown Condition",
                LocationId = item.LocationId,
                LocationName = item.Location?.Name ?? "Unknown Location",
                ItemTitle = item.ItemTitle,
                Description = item.Description,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                FinalTokenPrice = item.FinalTokenPrice,
                ImageFileName = item.ImageFileName,
                Status = item.Status,
                DatePosted = item.DatePosted,
                ExpiresAt = item.ExpiresAt,
                ViewCount = item.ViewCount,
                IsAiProcessed = item.AiProcessingStatus == Models.Entities.Items.AiProcessingStatus.Completed,
                AiConfidenceLevel = item.AiConfidenceLevel
            };
        }

        private List<string> GetPricingTips()
        {
            return new List<string>
            {
                "Items in better condition receive higher pricing automatically",
                "Electronics and furniture typically have higher base values",
                "Clear, well-lit photos can improve AI price suggestions",
                "Accurate category selection ensures proper pricing calculation",
                "Location can affect final pricing due to demand variations"
            };
        }

        #endregion
    }
}