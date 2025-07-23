using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.ViewModels.Shared;

namespace TidyUpCapstone.Models.ViewModels.Search
{
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();
        public List<SearchSuggestionDto> Suggestions { get; set; } = new List<SearchSuggestionDto>();
        public List<SearchFilterDto> AvailableFilters { get; set; } = new List<SearchFilterDto>();
        public List<SearchFilterDto> ActiveFilters { get; set; } = new List<SearchFilterDto>();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
        public SearchStatsDto Stats { get; set; } = new SearchStatsDto();
        public bool ShowMap { get; set; } = false;
        public bool HasResults { get; set; }
        public string? SearchTips { get; set; }
    }

    public class SearchSuggestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "category", "item", "location"
        public int Count { get; set; }
        public string? Url { get; set; }
    }

    public class SearchFilterDto
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "category", "location", "price", "condition"
        public bool IsActive { get; set; }
    }

    public class SearchStatsDto
    {
        public int TotalResults { get; set; }
        public string SearchTime { get; set; } = string.Empty;
        public List<string> RelatedQueries { get; set; } = new List<string>();
        public List<ItemCategoryDto> TopCategories { get; set; } = new List<ItemCategoryDto>();
        public decimal? AveragePrice { get; set; }
        public decimal? PriceRange { get; set; }
    }
}