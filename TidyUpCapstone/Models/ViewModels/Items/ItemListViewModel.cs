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
    }
}
