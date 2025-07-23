using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.ViewModels.Gamification
{
    public class GamificationDashboardViewModel
    {
        public UserLevelProgressViewModel LevelProgress { get; set; } = new UserLevelProgressViewModel();
        public List<QuestDto> ActiveQuests { get; set; } = new List<QuestDto>();
        public List<QuestDto> CompletedQuests { get; set; } = new List<QuestDto>();
        public List<AchievementDto> RecentAchievements { get; set; } = new List<AchievementDto>();
        public List<AchievementDto> AllAchievements { get; set; } = new List<AchievementDto>();
        public List<UserStreakViewModel> ActiveStreaks { get; set; } = new List<UserStreakViewModel>();
        public List<LeaderboardDto> Leaderboards { get; set; } = new List<LeaderboardDto>();
        public GamificationStatsViewModel Stats { get; set; } = new GamificationStatsViewModel();
    }

    public class UserLevelProgressViewModel
    {
        public int CurrentLevel { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public int CurrentXp { get; set; }
        public int TotalXp { get; set; }
        public int XpToNextLevel { get; set; }
        public int XpProgress { get; set; }
        public string? NextLevelName { get; set; }
        public decimal TokenBonus { get; set; }
        public string? SpecialPrivilege { get; set; }
        public List<string> UnlockedTitles { get; set; } = new List<string>();
    }

    public class UserStreakViewModel
    {
        public int StreakId { get; set; }
        public string StreakName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public string StreakUnit { get; set; } = string.Empty;
        public DateTime? LastActivityDate { get; set; }
        public int NextMilestone { get; set; }
        public int DaysUntilMilestone { get; set; }
        public decimal MilestoneReward { get; set; }
        public bool IsActive { get; set; }
    }

    public class GamificationStatsViewModel
    {
        public int TotalQuestsCompleted { get; set; }
        public int AchievementsEarned { get; set; }
        public int TotalAchievements { get; set; }
        public decimal TotalTokensEarned { get; set; }
        public int TotalXpEarned { get; set; }
        public int ActiveStreaksCount { get; set; }
        public int HighestRank { get; set; }
        public string? FavoriteQuestType { get; set; }
        public List<string> RecentRewards { get; set; } = new List<string>();
    }

    public class VisualCustomizationViewModel
    {
        public List<VisualItemDto> OwnedVisuals { get; set; } = new List<VisualItemDto>();
        public List<VisualItemDto> AvailableVisuals { get; set; } = new List<VisualItemDto>();
        public VisualItemDto? EquippedBadge { get; set; }
        public VisualItemDto? EquippedTheme { get; set; }
        public VisualItemDto? EquippedIcon { get; set; }
        public VisualItemDto? EquippedBorder { get; set; }
        public VisualItemDto? EquippedBackground { get; set; }
        public decimal UserTokenBalance { get; set; }
        public VisualFilterDto Filter { get; set; } = new VisualFilterDto();
    }

    public class VisualFilterDto
    {
        public VisualType? Type { get; set; }
        public VisualRarity? Rarity { get; set; }
        public bool ShowOwnedOnly { get; set; } = false;
        public bool ShowAvailableOnly { get; set; } = false;
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } = "Price";
        public string? SortOrder { get; set; } = "asc";
    }
}