namespace TidyUpCapstone.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public SystemOverviewViewModel SystemOverview { get; set; } = new SystemOverviewViewModel();
        public List<RecentUserActivityViewModel> RecentActivity { get; set; } = new List<RecentUserActivityViewModel>();
        public List<PendingReportViewModel> PendingReports { get; set; } = new List<PendingReportViewModel>();
        public List<SystemAlertViewModel> SystemAlerts { get; set; } = new List<SystemAlertViewModel>();
        public RevenueStatsViewModel RevenueStats { get; set; } = new RevenueStatsViewModel();
        public List<PopularItemCategoryViewModel> PopularCategories { get; set; } = new List<PopularItemCategoryViewModel>();
    }

    public class SystemOverviewViewModel
    {
        // User statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int BannedUsers { get; set; }
        public int VerifiedUsers { get; set; }

        // Item statistics
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int ItemsPostedToday { get; set; }
        public int ItemsPostedThisWeek { get; set; }
        public int CompletedItems { get; set; }
        public int RemovedItems { get; set; }

        // Transaction statistics
        public int TotalTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public int DisputedTransactions { get; set; }
        public decimal TotalTokensInCirculation { get; set; }
        public decimal TokensTransferredToday { get; set; }

        // System health
        public int PendingReports { get; set; }
        public int HighPriorityReports { get; set; }
        public int ActiveChats { get; set; }
        public int UnresolvedDisputes { get; set; }
        public double SystemUptime { get; set; }
        public int AiProcessingQueue { get; set; }

        // Performance metrics
        public double AverageResponseTime { get; set; }
        public int ErrorRate { get; set; }
        public long DatabaseSize { get; set; }
        public int ActiveSessions { get; set; }
    }

    public class RecentUserActivityViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string Priority { get; set; } = string.Empty; // low, medium, high
    }

    public class PendingReportViewModel
    {
        public int ReportId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string ReportedUsername { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime DateSubmitted { get; set; }
        public string? Description { get; set; }
    }

    public class SystemAlertViewModel
    {
        public string Type { get; set; } = string.Empty; // error, warning, info, success
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsResolved { get; set; }
        public string? ActionUrl { get; set; }
    }

    public class RevenueStatsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public int TransactionCount { get; set; }
        public decimal TokenUtilizationRate { get; set; }
        public List<DailyRevenueViewModel> DailyRevenue { get; set; } = new List<DailyRevenueViewModel>();
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }

    public class PopularItemCategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal AveragePrice { get; set; }
        public int CompletionRate { get; set; }
        public decimal TotalValue { get; set; }
    }
}