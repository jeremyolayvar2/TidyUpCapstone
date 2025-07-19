namespace TidyUpCapstone.Models.DTOs.Items
{
    public class ItemSummaryDto
    {
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal FinalTokenPrice { get; set; }
        public string? ImageFileName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        public int ViewCount { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public double? DistanceKm { get; set; }
        public decimal? AiConfidenceLevel { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int ItemCount { get; set; }
        public decimal? AveragePrice { get; set; }
    }

    public class ConditionDto
    {
        public int ConditionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal ConditionMultiplier { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
    }

    public class LocationDto
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Region { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }
        public decimal? AveragePrice { get; set; }
    }

    public class ItemCategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int CompletedItems { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal TotalValue { get; set; }
        public double CompletionRate { get; set; }
        public double AverageTimeToComplete { get; set; } // in hours
        public List<LocationStatsDto> TopLocations { get; set; } = new List<LocationStatsDto>();
    }

    public class LocationStatsDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class ItemTrendDto
    {
        public DateTime Date { get; set; }
        public int ItemsPosted { get; set; }
        public int ItemsCompleted { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal TotalValue { get; set; }
        public int UniqueUsers { get; set; }
    }
}