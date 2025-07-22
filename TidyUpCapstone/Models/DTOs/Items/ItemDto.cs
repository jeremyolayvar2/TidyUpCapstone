namespace TidyUpCapstone.Models.DTOs.Items
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal FinalTokenPrice { get; set; }
        public decimal? AiSuggestedPrice { get; set; }
        public bool PriceOverriddenByUser { get; set; }
        public string? ImageFileName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ViewCount { get; set; }

        // Location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? LocationRegion { get; set; }

        // Category and Condition
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ConditionId { get; set; }
        public string ConditionName { get; set; } = string.Empty;
        public decimal ConditionMultiplier { get; set; }

        // AI Analysis
        public string AiProcessingStatus { get; set; } = string.Empty;
        public DateTime? AiProcessedAt { get; set; }
        public string? AiDetectedCategory { get; set; }
        public decimal? AiConditionScore { get; set; }
        public decimal? AiConfidenceLevel { get; set; }

        // Distance (for location-based searches)
        public double? DistanceKm { get; set; }
    }

    public class CreateItemDto
    {
        public string ItemTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int ConditionId { get; set; }
        public int LocationId { get; set; }
        public decimal? UserSetPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? ImageFileName { get; set; }
    }

    public class UpdateItemDto
    {
        public string? ItemTitle { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? ConditionId { get; set; }
        public int? LocationId { get; set; }
        public decimal? FinalTokenPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Status { get; set; }
    }

    public class ItemSearchDto
    {
        public string? SearchQuery { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public int? ConditionId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Status { get; set; } = "available";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusKm { get; set; }
        public string SortBy { get; set; } = "DatePosted";
        public string SortOrder { get; set; } = "Desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ItemStatsDto
    {
        public int ItemId { get; set; }
        public int ViewCount { get; set; }
        public int InterestedUsers { get; set; }
        public int TransactionAttempts { get; set; }
        public DateTime? FirstViewed { get; set; }
        public DateTime? LastViewed { get; set; }
        public decimal? AverageViewDuration { get; set; }
        public List<string> ViewCountries { get; set; } = new List<string>();
        public List<string> ViewCities { get; set; } = new List<string>();
    }
}