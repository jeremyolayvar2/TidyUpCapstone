using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Gamification;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Services
{
    public interface IGamificationService
    {
        Task<List<QuestDto>> GetUserQuestsAsync(int userId);
        Task<List<AchievementDto>> GetUserAchievementsAsync(int userId);
        Task<LeaderboardDto> GetLeaderboardAsync(int leaderboardId, int userId);
        Task<bool> CompleteUserQuestAsync(int userId, int questId);
        Task<bool> ClaimQuestRewardAsync(int userId, int userQuestId);
    }

    public class GamificationService : IGamificationService
    {
        private readonly ApplicationDbContext _context;

        public GamificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuestDto>> GetUserQuestsAsync(int userId)
        {
            var activeQuests = await _context.Set<Quest>()
                .Where(q => q.IsActive)
                .Select(q => new
                {
                    Quest = q,
                    UserQuest = _context.Set<UserQuest>()
                        .FirstOrDefault(uq => uq.UserId == userId && uq.QuestId == q.QuestId)
                })
                .ToListAsync();

            var questDtos = activeQuests.Select(q => new QuestDto
            {
                QuestId = q.Quest.QuestId,
                QuestTitle = q.Quest.QuestTitle,
                QuestType = q.Quest.QuestType,
                QuestDescription = q.Quest.QuestDescription,
                QuestObjective = q.Quest.QuestObjective,
                TokenReward = q.Quest.TokenReward,
                XpReward = q.Quest.XpReward,
                Difficulty = q.Quest.Difficulty,
                TargetValue = q.Quest.TargetValue,
                IsActive = q.Quest.IsActive,
                StartDate = q.Quest.StartDate,
                EndDate = q.Quest.EndDate,
                IsCompleted = q.UserQuest?.IsCompleted ?? false,
                CurrentProgress = q.UserQuest?.CurrentProgress ?? 0,
                IsClaimed = q.UserQuest?.DateClaimed.HasValue ?? false,
                CompletedAt = q.UserQuest?.CompletedAt,
                ProgressPercentage = q.Quest.TargetValue > 0
                    ? Math.Min(100, (int)((q.UserQuest?.CurrentProgress ?? 0) * 100.0 / q.Quest.TargetValue))
                    : 0,
                IsAvailable = IsQuestAvailable(q.Quest),
                StatusMessage = GetQuestStatusMessage(q.Quest, q.UserQuest)
            }).ToList();

            return questDtos;
        }

        public async Task<List<AchievementDto>> GetUserAchievementsAsync(int userId)
        {
            var achievements = await _context.Set<Achievement>()
                .Where(a => a.IsActive)
                .Select(a => new
                {
                    Achievement = a,
                    UserAchievement = _context.Set<UserAchievement>()
                        .FirstOrDefault(ua => ua.UserId == userId && ua.AchievementId == a.AchievementId)
                })
                .ToListAsync();

            var achievementDtos = achievements.Select(a => new AchievementDto
            {
                AchievementId = a.Achievement.AchievementId,
                Name = a.Achievement.Name,
                Description = a.Achievement.Description,
                Category = a.Achievement.Category,
                CriteriaType = a.Achievement.CriteriaType,
                CriteriaValue = a.Achievement.CriteriaValue,
                TokenReward = a.Achievement.TokenReward,
                XpReward = a.Achievement.XpReward,
                BadgeImageUrl = a.Achievement.BadgeImageUrl,
                Rarity = a.Achievement.Rarity,
                IsSecret = a.Achievement.IsSecret,
                IsEarned = a.UserAchievement != null,
                EarnedDate = a.UserAchievement?.EarnedDate,
                Progress = a.UserAchievement?.Progress ?? 0,
                ProgressPercentage = a.Achievement.CriteriaValue > 0
                    ? Math.Min(100, (int)((a.UserAchievement?.Progress ?? 0) * 100.0 / a.Achievement.CriteriaValue))
                    : 0
            }).ToList();

            return achievementDtos;
        }

        public async Task<LeaderboardDto> GetLeaderboardAsync(int leaderboardId, int userId)
        {
            var leaderboard = await _context.Set<Leaderboard>()
                .Include(l => l.Entries)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(l => l.LeaderboardId == leaderboardId && l.IsActive);

            if (leaderboard == null)
                return new LeaderboardDto();

            var topEntries = leaderboard.Entries
                .OrderBy(e => e.RankPosition)
                .Take(10)
                .Select(e => new LeaderboardEntryDto
                {
                    EntryId = e.EntryId,
                    UserId = e.UserId,
                    Username = e.User.UserName ?? "Unknown",
                    RankPosition = e.RankPosition,
                    Score = e.Score,
                    PreviousRank = e.PreviousRank,
                    RankChange = e.RankChange,
                    LastUpdated = e.LastUpdated,
                    IsCurrentUser = e.UserId == userId
                })
                .ToList();

            var userEntry = leaderboard.Entries
                .Where(e => e.UserId == userId)
                .Select(e => new LeaderboardEntryDto
                {
                    EntryId = e.EntryId,
                    UserId = e.UserId,
                    Username = e.User.UserName ?? "Unknown",
                    RankPosition = e.RankPosition,
                    Score = e.Score,
                    PreviousRank = e.PreviousRank,
                    RankChange = e.RankChange,
                    LastUpdated = e.LastUpdated,
                    IsCurrentUser = true
                })
                .FirstOrDefault();

            return new LeaderboardDto
            {
                LeaderboardId = leaderboard.LeaderboardId,
                Name = leaderboard.Name,
                Type = leaderboard.Type,
                Metric = leaderboard.Metric,
                Description = leaderboard.Description,
                ResetFrequency = leaderboard.ResetFrequency,
                NextReset = leaderboard.NextReset,
                TopEntries = topEntries,
                UserEntry = userEntry
            };
        }

        public async Task<bool> CompleteUserQuestAsync(int userId, int questId)
        {
            var userQuest = await _context.Set<UserQuest>()
                .Include(uq => uq.Quest)
                .FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == questId);

            if (userQuest == null || userQuest.IsCompleted)
                return false;

            // Check if progress meets target
            if (userQuest.CurrentProgress >= userQuest.Quest.TargetValue)
            {
                userQuest.IsCompleted = true;
                userQuest.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ClaimQuestRewardAsync(int userId, int userQuestId)
        {
            var userQuest = await _context.Set<UserQuest>()
                .Include(uq => uq.Quest)
                .Include(uq => uq.User)
                .FirstOrDefaultAsync(uq => uq.UserQuestId == userQuestId && uq.UserId == userId);

            if (userQuest == null || !userQuest.IsCompleted || userQuest.DateClaimed.HasValue)
                return false;

            // Award rewards
            userQuest.User.TokenBalance += userQuest.Quest.TokenReward;

            // Award XP (you'll need to update user level logic here)
            var userLevel = await _context.Set<UserLevel>()
                .FirstOrDefaultAsync(ul => ul.UserId == userId);

            if (userLevel != null)
            {
                userLevel.CurrentXp += userQuest.Quest.XpReward;
                userLevel.TotalXp += userQuest.Quest.XpReward;
            }

            userQuest.DateClaimed = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private bool IsQuestAvailable(Quest quest)
        {
            var now = DateTime.UtcNow;

            if (quest.StartDate.HasValue && now < quest.StartDate.Value)
                return false;

            if (quest.EndDate.HasValue && now > quest.EndDate.Value)
                return false;

            return quest.IsActive;
        }

        private string GetQuestStatusMessage(Quest quest, UserQuest? userQuest)
        {
            if (!IsQuestAvailable(quest))
                return "Not Available";

            if (userQuest == null)
                return "Available";

            if (userQuest.DateClaimed.HasValue)
                return "Completed & Claimed";

            if (userQuest.IsCompleted)
                return "Ready to Claim";

            return $"In Progress ({userQuest.CurrentProgress}/{quest.TargetValue})";
        }
    }
}