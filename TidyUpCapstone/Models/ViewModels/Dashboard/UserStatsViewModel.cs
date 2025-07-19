namespace TidyUpCapstone.Models.ViewModels.Dashboard
{
    public class UserStatsViewModel
    {
        // Basic user info
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLogin { get; set; }

        // Token and transaction stats
        public decimal TokenBalance { get; set; }
        public decimal TotalTokensEarned { get; set; }
        public decimal TotalTokensSpent { get; set; }
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }

        // Item stats
        public int TotalItemsPosted { get; set; }
        public int ActiveItems { get; set; }
        public int CompletedItems { get; set; }
        public int TotalViews { get; set; }
        public decimal AverageItemPrice { get; set; }

        // Gamification stats
        public int CurrentLevel { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public int CurrentXp { get; set; }
        public int XpToNextLevel { get; set; }
        public int TotalXp { get; set; }
        public int CompletedQuests { get; set; }
        public int TotalAchievements { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

        // Leaderboard position
        public int? GlobalRank { get; set; }
        public int? MonthlyRank { get; set; }
        public decimal? GlobalScore { get; set; }

        // Activity summary (last 30 days)
        public int RecentItemsPosted { get; set; }
        public int RecentTransactions { get; set; }
        public decimal RecentTokensEarned { get; set; }
        public int RecentQuestsCompleted { get; set; }

        // Community stats
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalReactions { get; set; }
        public int PostLikes { get; set; }

        // Progress indicators
        public double LevelProgressPercentage => XpToNextLevel > 0 ? (double)CurrentXp / (CurrentXp + XpToNextLevel) * 100 : 100;
        public string AccountStatus { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int LinkedSsoProviders { get; set; }

        // Recent activity
        public List<ActivityLogViewModel> RecentActivity { get; set; } = new List<ActivityLogViewModel>();
    }

    public class ActivityLogViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}