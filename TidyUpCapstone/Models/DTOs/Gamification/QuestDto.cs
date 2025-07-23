using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.DTOs.Gamification
{
    public class QuestDto
    {
        public int QuestId { get; set; }
        public string QuestTitle { get; set; } = string.Empty;
        public QuestType QuestType { get; set; }
        public string? QuestDescription { get; set; }
        public string? QuestObjective { get; set; }
        public decimal TokenReward { get; set; }
        public int XpReward { get; set; }
        public QuestDifficulty Difficulty { get; set; }
        public int TargetValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public int CurrentProgress { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int ProgressPercentage { get; set; }
        public bool IsAvailable { get; set; }
        public string? StatusMessage { get; set; }
    }

    public class UserQuestProgressDto
    {
        public int UserQuestId { get; set; }
        public int QuestId { get; set; }
        public string QuestTitle { get; set; } = string.Empty;
        public int CurrentProgress { get; set; }
        public int TargetValue { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsClaimed { get; set; }
        public decimal TokenReward { get; set; }
        public int XpReward { get; set; }
    }

    public class AchievementDto
    {
        public int AchievementId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AchievementCategory Category { get; set; }
        public string CriteriaType { get; set; } = string.Empty;
        public int CriteriaValue { get; set; }
        public decimal TokenReward { get; set; }
        public int? XpReward { get; set; }
        public string? BadgeImageUrl { get; set; }
        public AchievementRarity Rarity { get; set; }
        public bool IsSecret { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedDate { get; set; }
        public int Progress { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class VisualItemDto
    {
        public int VisualId { get; set; }
        public string VisualName { get; set; } = string.Empty;
        public string? VisualDescription { get; set; }
        public decimal VisualPrice { get; set; }
        public string? VisualImgUrl { get; set; }
        public VisualType VisualType { get; set; }
        public VisualRarity Rarity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOwned { get; set; }
        public bool IsEquipped { get; set; }
        public DateTime? PurchasedDate { get; set; }
    }

    public class LeaderboardDto
    {
        public int LeaderboardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ResetFrequency ResetFrequency { get; set; }
        public DateTime? NextReset { get; set; }
        public List<LeaderboardEntryDto> TopEntries { get; set; } = new List<LeaderboardEntryDto>();
        public LeaderboardEntryDto? UserEntry { get; set; }
    }

    public class LeaderboardEntryDto
    {
        public int EntryId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public int RankPosition { get; set; }
        public decimal Score { get; set; }
        public int? PreviousRank { get; set; }
        public RankChange RankChange { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}