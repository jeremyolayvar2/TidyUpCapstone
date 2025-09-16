using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Models.DTOs.Items
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string UserAvatarUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ConditionId { get; set; }
        public string ConditionName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string ItemTitle { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AdjustedTokenPrice { get; set; }
        public decimal FinalTokenPrice { get; set; }
        public decimal? AiSuggestedPrice { get; set; }
        public string? ImageFileName { get; set; }
        public ItemStatus Status { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsAiProcessed { get; set; }
        public AiProcessingStatus AiProcessingStatus { get; set; }
        public decimal? AiConfidenceLevel { get; set; }
        public string? AiDetectedCategory { get; set; }
        public decimal? AiConditionScore { get; set; }
        public bool PriceOverriddenByUser { get; set; }
        public double? DistanceKm { get; set; }

        // Computed properties for UI
        public string ImageUrl => !string.IsNullOrEmpty(ImageFileName)
            ? $"/ItemPosts/{ImageFileName}"
            : "/assets/no-image-placeholder.svg";

        public string StatusDisplayName => Status switch
        {
            ItemStatus.Available => "Available",
            ItemStatus.Claimed => "Claimed",
            ItemStatus.Completed => "Completed",
            ItemStatus.Removed => "Removed",
            _ => "Unknown"
        };

        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        public bool IsActive => Status == ItemStatus.Available && !IsExpired;
    }

    public class CreateItemDto
    {
        [Required(ErrorMessage = "Item title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Item title must be between 3 and 100 characters")]
        public string ItemTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid condition")]
        public int ConditionId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid location")]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Location name is required")]
        [StringLength(200, ErrorMessage = "Location name cannot exceed 200 characters")]
        public string LocationName { get; set; } = string.Empty;

        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99")]
        public decimal? UserSetPrice { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Required(ErrorMessage = "Please upload an image")]
        public IFormFile? ImageFile { get; set; }
    }

    public class UpdateItemDto
    {
        [Required(ErrorMessage = "Item title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Item title must be between 3 and 100 characters")]
        public string ItemTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid condition")]
        public int ConditionId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid location")]
        public int LocationId { get; set; }

        public string LocationName { get; set; } = string.Empty;

        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99")]
        public decimal FinalTokenPrice { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public ItemStatus Status { get; set; } = ItemStatus.Available;

        public IFormFile? ImageFile { get; set; }
    }

    public class ItemSearchDto
    {
        public string? SearchQuery { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public int? ConditionId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusKm { get; set; }
        public ItemStatus? Status { get; set; }
        public AiProcessingStatus? AiStatus { get; set; }
        public bool? ShowExpired { get; set; } = false;
        public string? SortBy { get; set; } = "DatePosted";
        public string? SortOrder { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ItemCategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int ItemCount { get; set; }
    }

    public class ItemConditionDto
    {
        public int ConditionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal ConditionMultiplier { get; set; }
        public bool IsActive { get; set; }
    }

    public class ItemLocationDto
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Region { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
    }

    public class ItemStatisticsDto
    {
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int CompletedTransactions { get; set; }
        public decimal AveragePrice { get; set; }
        public int TotalViews { get; set; }
        public Dictionary<string, int> ItemsByCategory { get; set; } = new();
        public Dictionary<string, int> ItemsByCondition { get; set; } = new();
        public Dictionary<string, decimal> AverageePriceByCategory { get; set; } = new();
    }

    public class ItemValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public decimal? SuggestedPrice { get; set; }
        public string? SuggestedCategory { get; set; }
    }
}