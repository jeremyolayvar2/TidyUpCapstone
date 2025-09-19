using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Models.DTOs.Transactions
{
    public class ClaimedItemsSearchDto
    {
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class ClaimedItemsStatsDto
    {
        public int TotalClaimed { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }
}