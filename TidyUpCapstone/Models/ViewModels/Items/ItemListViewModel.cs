using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.ViewModels.Items;

namespace TidyUpCapstone.Models.ViewModels.Items
{
    public class ItemListViewModel
    {
        public List<ItemSummaryViewModel> Items { get; set; } = new List<ItemSummaryViewModel>();
        public ItemSearchFilterViewModel Filter { get; set; } = new ItemSearchFilterViewModel();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();

        // Summary statistics
        public int TotalItems { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? AveragePrice { get; set; }
    }

    public class ItemSummaryViewModel
    {
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal FinalTokenPrice { get; set; }
        public string? ImageFileName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        public int ViewCount { get; set; }

        // Category and Location
        public string CategoryName { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;

        // User info
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }

        // Distance (if location search is used)
        public double? DistanceKm { get; set; }

        // AI confidence
        public decimal? AiConfidenceLevel { get; set; }
    }

    public class ItemSearchFilterViewModel
    {
        [Display(Name = "Search")]
        public string? SearchQuery { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }

        [Display(Name = "Condition")]
        public int? ConditionId { get; set; }

        [Display(Name = "Min Price")]
        [Range(0, 999999.99)]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Max Price")]
        [Range(0, 999999.99)]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "DatePosted"; // DatePosted, Price, Distance, Popularity

        [Display(Name = "Sort Order")]
        public string SortOrder { get; set; } = "Desc"; // Asc, Desc

        [Display(Name = "Status")]
        public string? Status { get; set; } = "available";

        // Location-based search
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [Display(Name = "Radius (km)")]
        [Range(1, 100)]
        public int? RadiusKm { get; set; }

        // Available options for filters
        public List<CategoryOption> Categories { get; set; } = new List<CategoryOption>();
        public List<LocationOption> Locations { get; set; } = new List<LocationOption>();
        public List<ConditionOption> Conditions { get; set; } = new List<ConditionOption>();
    }

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }
}