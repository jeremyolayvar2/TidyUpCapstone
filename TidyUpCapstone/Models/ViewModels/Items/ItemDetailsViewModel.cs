namespace TidyUpCapstone.Models.ViewModels.Items
{
    public class ItemDetailsViewModel
    {
        public int ItemId { get; set; }
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

        // Location info
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string? LocationRegion { get; set; }

        // Category and Condition info
        public string CategoryName { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;
        public decimal ConditionMultiplier { get; set; }

        // User info
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public DateTime UserJoinDate { get; set; }
        public int UserItemCount { get; set; }

        // AI Analysis info
        public string AiProcessingStatus { get; set; } = string.Empty;
        public DateTime? AiProcessedAt { get; set; }
        public string? AiDetectedCategory { get; set; }
        public decimal? AiConditionScore { get; set; }
        public decimal? AiConfidenceLevel { get; set; }

        // Current user context
        public bool IsOwner { get; set; }
        public bool CanClaim { get; set; }
        public bool HasSufficientTokens { get; set; }
        public decimal UserTokenBalance { get; set; }

        // Related items
        public List<RelatedItemViewModel> RelatedItems { get; set; } = new List<RelatedItemViewModel>();
    }

    public class RelatedItemViewModel
    {
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public decimal FinalTokenPrice { get; set; }
        public string? ImageFileName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
    }
}