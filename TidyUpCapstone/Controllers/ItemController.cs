using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Services.Interfaces;
using System.Security.Claims;

namespace TidyUpCapstone.Controllers
{
    [Route("[controller]")]
    public class ItemController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemController> _logger;
        private readonly IFileService _fileService;

        public ItemController(
            IItemService itemService,
            ILogger<ItemController> logger,
            IFileService fileService)
        {
            _itemService = itemService;
            _logger = logger;
            _fileService = fileService;
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            try
            {
                _logger.LogInformation("=== CREATE ITEM REQUEST ===");

                // Extract form values
                var itemTitle = form["ItemTitle"].ToString()?.Trim();
                var description = form["Description"].ToString()?.Trim();
                var categoryId = int.TryParse(form["CategoryId"], out int catId) ? catId : 0;
                var conditionId = int.TryParse(form["ConditionId"], out int condId) ? condId : 0;
                var locationName = form["LocationName"].ToString()?.Trim();
                var latitude = decimal.TryParse(form["Latitude"], out decimal lat) ? lat : (decimal?)null;
                var longitude = decimal.TryParse(form["Longitude"], out decimal lng) ? lng : (decimal?)null;
                var imageFile = form.Files.FirstOrDefault();

                _logger.LogInformation("Form data - Title: {Title}, CategoryId: {CategoryId}, ConditionId: {ConditionId}, LocationName: {LocationName}",
                    itemTitle, categoryId, conditionId, locationName);

                // Basic validation
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(itemTitle)) validationErrors.Add("Item title is required");
                if (string.IsNullOrWhiteSpace(description)) validationErrors.Add("Description is required");
                if (categoryId <= 0) validationErrors.Add("Please select a valid category");
                if (conditionId <= 0) validationErrors.Add("Please select a valid condition");
                if (string.IsNullOrWhiteSpace(locationName)) validationErrors.Add("Location is required");
                if (imageFile == null || imageFile.Length == 0) validationErrors.Add("Image is required");
                if (!latitude.HasValue || !longitude.HasValue) validationErrors.Add("Location coordinates are required");

                if (validationErrors.Any())
                {
                    return Json(new { success = false, message = string.Join(", ", validationErrors) });
                }

                // Get current user ID
                int currentUserId = GetCurrentUserId();
                if (currentUserId <= 0)
                {
                    return Json(new { success = false, message = "User authentication required" });
                }

                // FIXED: Create DTO with LocationName, let the service handle location resolution
                var createDto = new CreateItemDto
                {
                    ItemTitle = itemTitle,
                    Description = description,
                    CategoryId = categoryId,
                    ConditionId = conditionId,
                    LocationName = locationName, // FIXED: Set the LocationName in the DTO
                    LocationId = 0, // The service will resolve this
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageFile = imageFile
                };

                // FIXED: Let the service handle location resolution internally
                var createdItem = await _itemService.CreateItemAsync(createDto, currentUserId);

                _logger.LogInformation("Item created successfully: {ItemId}", createdItem.ItemId);

                // Return success response (rest of the method stays the same)
                return Json(new
                {
                    success = true,
                    message = "Item created successfully!",
                    item = new
                    {
                        itemId = createdItem.ItemId,
                        userId = createdItem.UserId,
                        username = createdItem.User?.UserName ?? "Unknown",
                        userAvatarUrl = "/assets/default-avatar.svg",
                        categoryId = createdItem.CategoryId,
                        categoryName = createdItem.Category?.Name ?? "Unknown",
                        conditionId = createdItem.ConditionId,
                        conditionName = createdItem.Condition?.Name ?? "Unknown",
                        locationName = createdItem.Location?.Name ?? "Unknown",
                        itemTitle = createdItem.ItemTitle,
                        description = createdItem.Description,
                        imageUrl = !string.IsNullOrEmpty(createdItem.ImageFileName)
                            ? $"/ItemPosts/{createdItem.ImageFileName}"
                            : "/assets/no-image-placeholder.svg",
                        finalTokenPrice = createdItem.FinalTokenPrice,
                        adjustedTokenPrice = createdItem.AdjustedTokenPrice,
                        status = createdItem.Status.ToString(),
                        statusDisplayName = createdItem.Status.ToString(),
                        datePosted = createdItem.DatePosted,
                        isExpired = createdItem.ExpiresAt.HasValue && createdItem.ExpiresAt.Value <= DateTime.UtcNow,
                        isAiProcessed = createdItem.AiProcessingStatus == Models.Entities.Items.AiProcessingStatus.Completed,
                        aiConfidenceLevel = createdItem.AiConfidenceLevel
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                return Json(new { success = false, message = "An error occurred while creating the item" });
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                var currentUserId = GetCurrentUserId();
                if (item.UserId != currentUserId)
                {
                    return Forbid();
                }

                return Json(new
                {
                    itemTitle = item.ItemTitle,
                    itemCategory = item.Category?.Name,
                    itemCondition = item.Condition?.Name,
                    itemLocation = item.Location?.Name,
                    description = item.Description,
                    latitude = item.Latitude,
                    longitude = item.Longitude,
                    imageUrl = !string.IsNullOrEmpty(item.ImageFileName)
                        ? $"/ItemPosts/{item.ImageFileName}"
                        : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item for edit: {ItemId}", id);
                return Json(new { success = false, message = "Error loading item" });
            }
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0)
                {
                    return Json(new { success = false, message = "User authentication required" });
                }

                // Extract form values
                var itemTitle = form["ItemTitle"].ToString()?.Trim();
                var description = form["Description"].ToString()?.Trim();
                var categoryId = int.TryParse(form["CategoryId"], out int catId) ? catId : 0;
                var conditionId = int.TryParse(form["ConditionId"], out int condId) ? condId : 0;
                var locationName = form["LocationName"].ToString()?.Trim();
                var latitude = decimal.TryParse(form["Latitude"], out decimal lat) ? lat : (decimal?)null;
                var longitude = decimal.TryParse(form["Longitude"], out decimal lng) ? lng : (decimal?)null;
                var imageFile = form.Files.FirstOrDefault();
                var removeImage = form["RemoveImage"].ToString() == "true";

                // Basic validation
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(itemTitle)) validationErrors.Add("Item title is required");
                if (string.IsNullOrWhiteSpace(description)) validationErrors.Add("Description is required");
                if (categoryId <= 0) validationErrors.Add("Please select a valid category");
                if (conditionId <= 0) validationErrors.Add("Please select a valid condition");
                if (string.IsNullOrWhiteSpace(locationName)) validationErrors.Add("Location is required");

                if (validationErrors.Any())
                {
                    return Json(new { success = false, message = string.Join(", ", validationErrors) });
                }

                // Create update DTO with proper structure to match your existing DTO
                var updateDto = new UpdateItemDto
                {
                    ItemTitle = itemTitle,
                    Description = description,
                    CategoryId = categoryId,
                    ConditionId = conditionId,
                    LocationName = locationName, // Your existing DTO expects this
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageFile = imageFile,
                    Status = Models.Entities.Items.ItemStatus.Available
                };

                var updatedItem = await _itemService.UpdateItemAsync(id, updateDto, currentUserId);

                return Json(new
                {
                    success = true,
                    message = "Item updated successfully!",
                    newImageUrl = !string.IsNullOrEmpty(updatedItem.ImageFileName)
                        ? $"/ItemPosts/{updatedItem.ImageFileName}"
                        : null,
                    imageRemoved = removeImage
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "You don't have permission to edit this item" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item: {ItemId}", id);
                return Json(new { success = false, message = "An error occurred while updating the item" });
            }
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId <= 0)
                {
                    return Json(new { success = false, message = "User authentication required" });
                }

                var success = await _itemService.DeleteItemAsync(id, currentUserId);
                if (success)
                {
                    return Json(new { success = true, message = "Item deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Item not found or you don't have permission to delete it" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {ItemId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the item" });
            }
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            // For authenticated users
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }

            // For test mode (development only)
            var currentTestUser = HttpContext.Session.GetString("CurrentTestUser") ?? "Alice";
            return currentTestUser switch
            {
                "Alice" => 1,
                "Bob" => 2,
                "Charlie" => 3,
                _ => 1
            };
        }
    }
}