using TidyUpCapstone.Models.DTOs.Community;
using TidyUpCapstone.Models.DTOs.Items;
using TidyUpCapstone.Models.DTOs.Gamification;

namespace TidyUpCapstone.Models.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<ItemDto> FeaturedItems { get; set; } = new List<ItemDto>();
        public List<ItemDto> RecentItems { get; set; } = new List<ItemDto>();
        public List<ItemDto> NearbyItems { get; set; } = new List<ItemDto>();
        public List<PostDto> RecentPosts { get; set; } = new List<PostDto>();
        public List<ItemCategoryDto> PopularCategories { get; set; } = new List<ItemCategoryDto>();
        public UserDashboardStatsViewModel? UserStats { get; set; }
        public List<QuestDto> ActiveQuests { get; set; } = new List<QuestDto>();
        public List<AchievementDto> RecentAchievements { get; set; } = new List<AchievementDto>();
        public bool IsAuthenticated { get; set; }
        public string? WelcomeMessage { get; set; }
    }

    public class UserDashboardStatsViewModel
    {
        public decimal TokenBalance { get; set; }
        public int ActiveListings { get; set; }
        public int PendingTransactions { get; set; }
        public int UnreadNotifications { get; set; }
        public int UnreadMessages { get; set; }
        public int CurrentLevel { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public int CurrentXp { get; set; }
        public int XpToNextLevel { get; set; }
        public int XpProgress { get; set; }
        public List<QuestDto> DailyQuests { get; set; } = new List<QuestDto>();
        public List<string> ActiveStreaks { get; set; } = new List<string>();
    }
}
