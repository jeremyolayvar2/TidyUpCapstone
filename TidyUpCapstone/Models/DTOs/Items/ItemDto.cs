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
        public decimal FinalTokenPrice { get; set; }
        public string? ImageFileName { get; set; }
        public ItemStatus Status { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ViewCount { get; set; }
        public bool IsAiProcessed { get; set; }
        public decimal? AiConfidenceLevel { get; set; }
        public double? DistanceKm { get; set; }
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
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Condition is required")]
        public int ConditionId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public int LocationId { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99")]
        public decimal? UserSetPrice { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public IFormFile? ImageFile { get; set; }
    }

    public class UpdateItemDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ItemTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int ConditionId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Range(0, 999999.99)]
        public decimal FinalTokenPrice { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public ItemStatus Status { get; set; }
    }

    public class ItemSearchDto
    {
        public string? SearchQuery { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusKm { get; set; }
        public ItemStatus? Status { get; set; }
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
}