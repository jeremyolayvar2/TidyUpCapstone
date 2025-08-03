using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.DTOs.Transactions;
using TidyUpCapstone.Models.ViewModels.Shared;

namespace TidyUpCapstone.Models.ViewModels.Items
{
    public class ItemListViewModel
    {
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();
        public ItemSearchDto SearchCriteria { get; set; } = new ItemSearchDto();
        public List<ItemCategoryDto> Categories { get; set; } = new List<ItemCategoryDto>();
        public List<ItemLocationDto> Locations { get; set; } = new List<ItemLocationDto>();
        public List<ItemConditionDto> Conditions { get; set; } = new List<ItemConditionDto>();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
        public int TotalItems { get; set; }
        public string? PageTitle { get; set; }
        public string? PageDescription { get; set; }
        public bool ShowFilters { get; set; } = true;
        public bool ShowMap { get; set; } = false;
        public decimal? UserLatitude { get; set; }
        public decimal? UserLongitude { get; set; }
        public ItemStatisticsDto? Statistics { get; set; }

        // UI state properties
        public string CurrentView { get; set; } = "grid"; // grid, list, map
        public bool ShowOnlyMyItems { get; set; } = false;
        public bool ShowExpiredItems { get; set; } = false;
        public string? SelectedCategoryName { get; set; }
        public string? SelectedLocationName { get; set; }

        // Computed properties for UI
        public bool HasItems => Items?.Any() == true;
        public bool HasActiveFilters => !string.IsNullOrEmpty(SearchCriteria?.SearchQuery) ||
                                       SearchCriteria?.CategoryId.HasValue == true ||
                                       SearchCriteria?.LocationId.HasValue == true ||
                                       SearchCriteria?.MinPrice.HasValue == true ||
                                       SearchCriteria?.MaxPrice.HasValue == true;
    }

    public class ItemDetailsViewModel
    {
        public ItemDto Item { get; set; } = null!;
        public List<ItemDto> RelatedItems { get; set; } = new List<ItemDto>();
        public List<ItemDto> SellerOtherItems { get; set; } = new List<ItemDto>();
        public bool CanEdit { get; set; }
        public bool CanPurchase { get; set; }
        public bool IsOwner { get; set; }
        public bool HasActiveTransaction { get; set; }
        public string? ActiveTransactionStatus { get; set; }
        public CreateTransactionDto TransactionRequest { get; set; } = new CreateTransactionDto();
        public List<string> ImageGallery { get; set; } = new List<string>();
        public bool ShowContactSeller { get; set; }
        public decimal? DistanceFromUser { get; set; }

        // AI-related display properties
        public bool ShowAiInsights { get; set; }
        public string? AiProcessingStatusDisplay { get; set; }
        public List<AiInsightDto> AiInsights { get; set; } = new List<AiInsightDto>();

        // Computed properties
        public bool IsExpired => Item?.IsExpired == true;
        public bool IsAvailable => Item?.Status == TidyUpCapstone.Models.Entities.Items.ItemStatus.Available && !IsExpired;
        public string StatusBadgeClass => Item?.Status switch
        {
            TidyUpCapstone.Models.Entities.Items.ItemStatus.Available => "badge-success",
            TidyUpCapstone.Models.Entities.Items.ItemStatus.Claimed => "badge-warning",
            TidyUpCapstone.Models.Entities.Items.ItemStatus.Completed => "badge-info",
            TidyUpCapstone.Models.Entities.Items.ItemStatus.Removed => "badge-danger",
            _ => "badge-secondary"
        };
    }

    public class CreateItemViewModel
    {
        public CreateItemDto Item { get; set; } = new CreateItemDto();
        public List<ItemCategoryDto> Categories { get; set; } = new List<ItemCategoryDto>();
        public List<ItemConditionDto> Conditions { get; set; } = new List<ItemConditionDto>();
        public List<ItemLocationDto> Locations { get; set; } = new List<ItemLocationDto>();
        public decimal? SuggestedPrice { get; set; }
        public bool ShowPriceGuidance { get; set; } = true;
        public string? AiProcessingStatus { get; set; }
        public List<string> PricingTips { get; set; } = new List<string>();
        public ItemValidationResult? ValidationResult { get; set; }

        // UI state
        public int CurrentStep { get; set; } = 1;
        public bool EnableAiPricing { get; set; } = true;
        public bool ShowAdvancedOptions { get; set; } = false;

        // Form validation helpers
        public bool IsStep1Valid => !string.IsNullOrEmpty(Item.ItemTitle) &&
                                   Item.CategoryId > 0 &&
                                   Item.ConditionId > 0;

        public bool IsStep2Valid => !string.IsNullOrEmpty(Item.Description) &&
                                   !string.IsNullOrEmpty(Item.LocationName);

        public bool IsStep3Valid => Item.ImageFile != null;
    }

    public class EditItemViewModel
    {
        public UpdateItemDto Item { get; set; } = new UpdateItemDto();
        public int ItemId { get; set; }
        public List<ItemCategoryDto> Categories { get; set; } = new List<ItemCategoryDto>();
        public List<ItemConditionDto> Conditions { get; set; } = new List<ItemConditionDto>();
        public List<ItemLocationDto> Locations { get; set; } = new List<ItemLocationDto>();
        public string? CurrentImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int ViewCount { get; set; }
        public DateTime DatePosted { get; set; }
        public bool HasActiveTransactions { get; set; }
        public decimal? OriginalAiSuggestedPrice { get; set; }
        public bool WasPriceOverridden { get; set; }

        // AI insights for editing
        public List<AiInsightDto> AiInsights { get; set; } = new List<AiInsightDto>();
        public bool ShowAiSuggestions { get; set; } = true;

        // Computed properties
        public bool CanChangeStatus => !HasActiveTransactions;
        public bool ShowPriceWarning => WasPriceOverridden && OriginalAiSuggestedPrice.HasValue;
        public string ItemAge
        {
            get
            {
                var age = DateTime.UtcNow - DatePosted;
                return age.TotalDays switch
                {
                    < 1 => "Today",
                    < 7 => $"{(int)age.TotalDays} day{(age.TotalDays < 2 ? "" : "s")} ago",
                    < 30 => $"{(int)(age.TotalDays / 7)} week{(age.TotalDays / 7 < 2 ? "" : "s")} ago",
                    _ => $"{(int)(age.TotalDays / 30)} month{(age.TotalDays / 30 < 2 ? "" : "s")} ago"
                };
            }
        }
    }

    public class ItemManagementViewModel
    {
        public List<ItemDto> MyItems { get; set; } = new List<ItemDto>();
        public ItemStatisticsDto Statistics { get; set; } = new ItemStatisticsDto();
        public List<ItemDto> RecentlyViewed { get; set; } = new List<ItemDto>();
        public List<ItemDto> ExpiringItems { get; set; } = new List<ItemDto>();
        public List<ItemDto> PendingAiProcessing { get; set; } = new List<ItemDto>();

        // Filter options for my items
        public string? StatusFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public DateTime? DateFromFilter { get; set; }
        public DateTime? DateToFilter { get; set; }

        // Quick actions
        public bool BulkEditMode { get; set; } = false;
        public List<int> SelectedItemIds { get; set; } = new List<int>();

        // Computed properties
        public int ActiveItemsCount => MyItems.Count(i => i.IsActive);
        public int ExpiredItemsCount => MyItems.Count(i => i.IsExpired);
        public int CompletedItemsCount => MyItems.Count(i => i.Status == TidyUpCapstone.Models.Entities.Items.ItemStatus.Completed);
        public decimal TotalEarnings => MyItems.Where(i => i.Status == TidyUpCapstone.Models.Entities.Items.ItemStatus.Completed)
                                              .Sum(i => i.FinalTokenPrice);
    }

    // Supporting DTOs for ViewModels
    public class AiInsightDto
    {
        public string Type { get; set; } = string.Empty; // price, category, condition, market
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? SuggestedValue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public string? ActionRecommendation { get; set; }
        public string IconClass { get; set; } = "fas fa-info-circle";
        public string BadgeClass { get; set; } = "badge-info";
    }

    public class ItemFormStepDto
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public List<string> RequiredFields { get; set; } = new List<string>();
    }
}