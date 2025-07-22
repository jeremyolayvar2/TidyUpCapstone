namespace TidyUpCapstone.Models.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public UserStatsViewModel UserStats { get; set; } = new UserStatsViewModel();
        public List<RecentItemViewModel> RecentItems { get; set; } = new List<RecentItemViewModel>();
        public List<ActiveTransactionViewModel> ActiveTransactions { get; set; } = new List<ActiveTransactionViewModel>();
        public List<NotificationSummaryViewModel> RecentNotifications { get; set; } = new List<NotificationSummaryViewModel>();
        public List<QuestProgressViewModel> ActiveQuests { get; set; } = new List<QuestProgressViewModel>();
        public List<AchievementViewModel> RecentAchievements { get; set; } = new List<AchievementViewModel>();

        // Quick actions
        public bool CanPostItem { get; set; }
        public int UnreadMessageCount { get; set; }
        public int PendingTransactionCount { get; set; }
    }

    public class RecentItemViewModel
    {
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public decimal FinalTokenPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        public int ViewCount { get; set; }
        public string? ImageFileName { get; set; }
    }

    public class ActiveTransactionViewModel
    {
        public int TransactionId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public decimal TokenAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "buyer" or "seller"
        public string OtherPartyUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool RequiresAction { get; set; }
        public string? ActionRequired { get; set; }
    }

    public class NotificationSummaryViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? ActionUrl { get; set; }
    }

    public class QuestProgressViewModel
    {
        public int QuestId { get; set; }
        public string QuestTitle { get; set; } = string.Empty;
        public string? QuestDescription { get; set; }
        public int CurrentProgress { get; set; }
        public int TargetValue { get; set; }
        public decimal TokenReward { get; set; }
        public int XpReward { get; set; }
        public DateTime? EndDate { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public double ProgressPercentage => TargetValue > 0 ? (double)CurrentProgress / TargetValue * 100 : 0;
    }

    public class AchievementViewModel
    {
        public int AchievementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Rarity { get; set; } = string.Empty;
        public string? BadgeImageUrl { get; set; }
        public DateTime EarnedDate { get; set; }
        public decimal TokenReward { get; set; }
        public int? XpReward { get; set; }
    }
}