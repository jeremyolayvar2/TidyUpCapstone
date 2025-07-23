

namespace TidyUpCapstone.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public AdminStatsViewModel Stats { get; set; } = new AdminStatsViewModel();
        public List<UserReportDto> PendingReports { get; set; } = new List<UserReportDto>();
        public List<UserActivityDto> RecentUserActivity { get; set; } = new List<UserActivityDto>();
        public List<SystemAlertDto> SystemAlerts { get; set; } = new List<SystemAlertDto>();
        public List<AdminReportDto> RecentReports { get; set; } = new List<AdminReportDto>();
        public bool CanManageUsers { get; set; }
        public bool CanManageSystem { get; set; }
        public bool CanGenerateReports { get; set; }
        public bool CanManageSso { get; set; }
    }

    public class AdminStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int NewItemsToday { get; set; }
        public int TotalTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public decimal TotalTransactionValue { get; set; }
        public int PendingReports { get; set; }
        public int SystemIssues { get; set; }
        public double SystemUptime { get; set; }
        public List<ChartDataPoint> UserGrowthData { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> TransactionData { get; set; } = new List<ChartDataPoint>();
    }

    public class UserReportDto
    {
        public int ReportId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string ReportedUsername { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class UserActivityDto
    {
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? Location { get; set; }
    }

    public class SystemAlertDto
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsResolved { get; set; }
    }

    public class AdminReportDto
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? FilePath { get; set; }
        public long? FileSizeBytes { get; set; }
        public int DownloadCount { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }
}