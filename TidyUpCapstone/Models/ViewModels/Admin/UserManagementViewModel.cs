using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.ViewModels.Items;

namespace TidyUpCapstone.Models.ViewModels.Admin
{
    public class UserManagementViewModel
    {
        public List<UserSummaryViewModel> Users { get; set; } = new List<UserSummaryViewModel>();
        public UserSearchFilterViewModel Filter { get; set; } = new UserSearchFilterViewModel();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int BannedUsers { get; set; }
        public int VerifiedUsers { get; set; }
    }

    public class UserSummaryViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool EmailConfirmed { get; set; }
        public decimal TokenBalance { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLogin { get; set; }
        public int TotalItems { get; set; }
        public int TotalTransactions { get; set; }
        public int ReportCount { get; set; }
        public string RegistrationMethod { get; set; } = string.Empty;
        public List<string> LinkedProviders { get; set; } = new List<string>();
        public bool TwoFactorEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

    public class UserSearchFilterViewModel
    {
        [Display(Name = "Search")]
        public string? SearchQuery { get; set; }

        [Display(Name = "Role")]
        public string? Role { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Verified")]
        public bool? IsVerified { get; set; }

        [Display(Name = "Registration Method")]
        public string? RegistrationMethod { get; set; }

        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "DateCreated";

        [Display(Name = "Sort Order")]
        public string SortOrder { get; set; } = "Desc";

        [Display(Name = "Has Reports")]
        public bool? HasReports { get; set; }

        [Display(Name = "Min Token Balance")]
        public decimal? MinTokenBalance { get; set; }

        [Display(Name = "Max Token Balance")]
        public decimal? MaxTokenBalance { get; set; }
    }

    public class UserDetailViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public decimal TokenBalance { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string RegistrationMethod { get; set; } = string.Empty;
        public DateTime? TermsAcceptedAt { get; set; }
        public DateTime? PrivacyAcceptedAt { get; set; }
        public bool MarketingEmailsEnabled { get; set; }

        // Lock out info
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        // Admin management
        public int? ManagedByAdminId { get; set; }
        public string? ManagedByAdminName { get; set; }
        public string? AdminNotes { get; set; }

        // Statistics
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int CompletedItems { get; set; }
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public decimal TotalTokensEarned { get; set; }
        public decimal TotalTokensSpent { get; set; }
        public int ReportCount { get; set; }
        public int ReportedCount { get; set; }

        // SSO Links
        public List<UserSsoLinkViewModel> SsoLinks { get; set; } = new List<UserSsoLinkViewModel>();

        // Recent activity
        public List<UserActivityViewModel> RecentActivity { get; set; } = new List<UserActivityViewModel>();

        // Recent reports
        public List<UserReportSummaryViewModel> RecentReports { get; set; } = new List<UserReportSummaryViewModel>();
    }

    public class UserSsoLinkViewModel
    {
        public string ProviderName { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public string? ProviderDisplayName { get; set; }
        public DateTime LinkedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class UserActivityViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
    }

    public class UserReportSummaryViewModel
    {
        public int ReportId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateSubmitted { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string? Resolution { get; set; }
    }

    public class EditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public bool IsVerified { get; set; }
        public bool EmailConfirmed { get; set; }

        [Range(0, 999999.99)]
        public decimal TokenBalance { get; set; }

        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        public int? ManagedByAdminId { get; set; }

        // Available options
        public List<string> AvailableRoles { get; set; } = new List<string> { "user", "moderator", "admin" };
        public List<string> AvailableStatuses { get; set; } = new List<string> { "active", "inactive", "banned" };
        public List<AdminOption> AvailableAdmins { get; set; } = new List<AdminOption>();
    }

    public class AdminOption
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}