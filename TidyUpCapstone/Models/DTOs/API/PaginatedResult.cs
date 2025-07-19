namespace TidyUpCapstone.Models.DTOs.API
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
        public SortInfo? Sort { get; set; }
        public FilterInfo? Filter { get; set; }

        public static PaginatedResult<T> Create(
            List<T> data,
            int totalItems,
            int currentPage,
            int pageSize,
            string? sortBy = null,
            string? sortOrder = null)
        {
            return new PaginatedResult<T>
            {
                Data = data,
                Pagination = new PaginationInfo
                {
                    CurrentPage = currentPage,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                },
                Sort = !string.IsNullOrEmpty(sortBy) ? new SortInfo
                {
                    SortBy = sortBy,
                    SortOrder = sortOrder ?? "Asc"
                } : null
            };
        }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
        public int? NextPage => HasNextPage ? CurrentPage + 1 : null;
        public int? PreviousPage => HasPreviousPage ? CurrentPage - 1 : null;
    }

    public class SortInfo
    {
        public string SortBy { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "Asc"; // Asc, Desc
    }

    public class FilterInfo
    {
        public Dictionary<string, object> AppliedFilters { get; set; } = new Dictionary<string, object>();
        public int FilteredItemCount { get; set; }
        public int UnfilteredItemCount { get; set; }
    }

    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "Asc";

        public void Normalize()
        {
            if (Page < 1) Page = 1;
            if (PageSize < 1) PageSize = 20;
            if (PageSize > 100) PageSize = 100;

            if (string.IsNullOrEmpty(SortOrder))
                SortOrder = "Asc";
            else
                SortOrder = SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "Desc" : "Asc";
        }
    }

    public class SearchRequest : PagedRequest
    {
        public string? SearchQuery { get; set; }
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}