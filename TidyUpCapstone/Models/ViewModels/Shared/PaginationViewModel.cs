namespace TidyUpCapstone.Models.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
        public string? BaseUrl { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();

        public List<int> GetPageNumbers()
        {
            var pages = new List<int>();
            var start = Math.Max(1, CurrentPage - 2);
            var end = Math.Min(TotalPages, CurrentPage + 2);

            for (int i = start; i <= end; i++)
            {
                pages.Add(i);
            }

            return pages;
        }

        public bool ShowFirstPage => CurrentPage > 3;
        public bool ShowLastPage => CurrentPage < TotalPages - 2;
        public bool ShowPreviousEllipsis => CurrentPage > 4;
        public bool ShowNextEllipsis => CurrentPage < TotalPages - 3;
    }

    public class BreadcrumbViewModel
    {
        public List<BreadcrumbItem> Items { get; set; } = new List<BreadcrumbItem>();

        public void AddItem(string text, string? url = null, bool isActive = false)
        {
            Items.Add(new BreadcrumbItem { Text = text, Url = url, IsActive = isActive });
        }
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }

    public class AlertViewModel
    {
        public string Message { get; set; } = string.Empty;
        public AlertType Type { get; set; } = AlertType.Info;
        public bool Dismissible { get; set; } = true;
        public string? Title { get; set; }
        public string? Icon { get; set; }
    }

    public enum AlertType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public class ModalViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? Footer { get; set; }
        public ModalSize Size { get; set; } = ModalSize.Default;
        public bool ShowCloseButton { get; set; } = true;
        public bool Backdrop { get; set; } = true;
        public bool Keyboard { get; set; } = true;
    }

    public enum ModalSize
    {
        Small,
        Default,
        Large,
        ExtraLarge
    }
}