using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.Entities.Reporting;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Controllers
{
    [Route("[controller]")]
    [Authorize] // Require authentication for all actions
    public class ItemController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly ILogger<ItemController> _logger;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _context;

        public ItemController(
        IItemService itemService,
        ILogger<ItemController> logger,
        IFileService fileService,
        UserManager<AppUser> userManager,
        ApplicationDbContext context)
            : base(userManager)
        {
            _itemService = itemService;
            _logger = logger;
            _fileService = fileService;
            _context = context;
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

                // Create DTO with LocationName, let the service handle location resolution
                var createDto = new CreateItemDto
                {
                    ItemTitle = itemTitle,
                    Description = description,
                    CategoryId = categoryId,
                    ConditionId = conditionId,
                    LocationName = locationName,
                    LocationId = 0, // The service will resolve this
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageFile = imageFile
                };

                // Let the service handle location resolution internally
                var createdItem = await _itemService.CreateItemAsync(createDto, currentUserId);

                _logger.LogInformation("Item created successfully: {ItemId}", createdItem.ItemId);

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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in Create");
                return Json(new { success = false, message = "Authentication required" });
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in Edit: {ItemId}", id);
                return Json(new { success = false, message = "Authentication required" });
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
                _logger.LogInformation("=== EDIT ITEM REQUEST ===");
                _logger.LogInformation("Item ID: {ItemId}", id);

                var currentUserId = GetCurrentUserId();

                // Extract form values
                var itemTitle = form["ItemTitle"].ToString()?.Trim();
                var description = form["Description"].ToString()?.Trim();
                var locationName = form["LocationName"].ToString()?.Trim();
                var latitude = decimal.TryParse(form["Latitude"], out decimal lat) ? lat : (decimal?)null;
                var longitude = decimal.TryParse(form["Longitude"], out decimal lng) ? lng : (decimal?)null;
                var imageFile = form.Files.FirstOrDefault();
                var removeImage = form["RemoveImage"].ToString() == "true";

                // Log what we received
                _logger.LogInformation("Form data - Title: {Title}, Description: {Description}, LocationName: {LocationName}, Lat: {Lat}, Lng: {Lng}",
                    itemTitle, description, locationName, latitude, longitude);

                // Only validate fields that users can actually edit
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(itemTitle)) validationErrors.Add("Item title is required");
                if (string.IsNullOrWhiteSpace(description)) validationErrors.Add("Description is required");
                if (string.IsNullOrWhiteSpace(locationName)) validationErrors.Add("Location is required");
                if (!latitude.HasValue || !longitude.HasValue) validationErrors.Add("Location coordinates are required");

                if (validationErrors.Any())
                {
                    _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", validationErrors));
                    return Json(new { success = false, message = string.Join(", ", validationErrors) });
                }

                // Get the existing item to preserve category and condition
                var existingItem = await _itemService.GetItemByIdAsync(id);
                if (existingItem == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                if (existingItem.UserId != currentUserId)
                {
                    return Json(new { success = false, message = "You don't have permission to edit this item" });
                }

                // Create update DTO - preserve existing category and condition
                var updateDto = new UpdateItemDto
                {
                    ItemTitle = itemTitle,
                    Description = description,
                    CategoryId = existingItem.CategoryId, // Keep existing
                    ConditionId = existingItem.ConditionId, // Keep existing
                    LocationName = locationName,
                    Latitude = latitude,
                    Longitude = longitude,
                    ImageFile = imageFile,
                    Status = existingItem.Status // Keep existing status
                };

                _logger.LogInformation("Calling UpdateItemAsync with DTO: {@UpdateDto}", updateDto);

                var updatedItem = await _itemService.UpdateItemAsync(id, updateDto, currentUserId);

                _logger.LogInformation("Item updated successfully: {ItemId}", updatedItem.ItemId);

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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access for item {ItemId}", id);
                return Json(new { success = false, message = "You don't have permission to edit this item" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument exception for item {ItemId}", id);
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in Delete: {ItemId}", id);
                return Json(new { success = false, message = "Authentication required" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {ItemId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the item" });
            }
        }

        [HttpPost("Report")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(IFormCollection form)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var itemIdStr = form["itemId"].ToString();
                var reason = form["reason"].ToString();

                if (!int.TryParse(itemIdStr, out int itemId) || itemId <= 0)
                {
                    return Json(new { success = false, message = "Invalid item ID" });
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Json(new { success = false, message = "Report reason is required" });
                }

                // Get the item to find its owner
                var item = await _itemService.GetItemByIdAsync(itemId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                // Prevent self-reporting
                if (item.UserId == currentUserId)
                {
                    return Json(new { success = false, message = "You cannot report your own item" });
                }

                // Create the report using your existing model
                var report = new UserReport
                {
                    ReporterId = currentUserId,
                    ReportedUserId = item.UserId,
                    ReportedEntityType = ReportedEntityType.Item,
                    ReportedEntityId = itemId,
                    Reason = MapReasonToEnum(reason),
                    Description = $"Reported item: {item.ItemTitle}",
                    DateSubmitted = DateTime.UtcNow,
                    ReportStatus = ReportStatus.Pending,
                    Priority = ReportPriority.Medium
                };

                // Save to database
                _context.UserReports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item {ItemId} reported by user {UserId} for reason: {Reason}",
                    itemId, currentUserId, reason);

                return Json(new { success = true, message = "Report submitted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in Report");
                return Json(new { success = false, message = "Authentication required" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report");
                return Json(new { success = false, message = "An error occurred while submitting the report" });
            }
        }

        private ReportReason MapReasonToEnum(string reason)
        {
            return reason?.ToLower() switch
            {
                "inappropriate" => ReportReason.Inappropriate,
                "spam" => ReportReason.Spam,
                "fraud" => ReportReason.Scam,
                "other" => ReportReason.Other,
                _ => ReportReason.Other
            };
        }

        /// <summary>
        /// Gets the current authenticated user ID
        /// </summary>
        /// <returns>User ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        private int GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User must be authenticated");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID claim");
            }

            return userId;
        }
    }
}