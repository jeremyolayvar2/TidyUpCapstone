using TidyUpCapstone.Models.DTOs.Transactions;

namespace TidyUpCapstone.Models.ViewModels.Transactions
{
    public class ClaimedItemsViewModel
    {
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public ClaimedItemsSearchDto SearchCriteria { get; set; } = new ClaimedItemsSearchDto();
        public ClaimedItemsStatsDto Statistics { get; set; } = new ClaimedItemsStatsDto();
        public Dictionary<int, string> Categories { get; set; } = new Dictionary<int, string>();

        // Pagination
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        // Helper properties
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        // Filter options
        public Dictionary<string, string> StatusOptions => new Dictionary<string, string>
{
    { "", "All Status" },
    { "pending", "Pending" },
    { "escrowed", "In Progress" },
    { "confirmed", "Completed" },
    { "cancelled", "Cancelled" }
};

        public Dictionary<string, string> SortOptions => new Dictionary<string, string>
        {
            { "newest", "Newest First" },
            { "oldest", "Oldest First" },
            { "price-high", "Price: High to Low" },
            { "price-low", "Price: Low to High" },
            { "title", "Title A-Z" }
        };
    }
}