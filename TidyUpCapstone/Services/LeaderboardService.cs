using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Leaderboard;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Services
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResponseDto> GetLeaderboardAsync(LeaderboardFilterType filterType);
        Task<UserStatsDto> GetUserStatsAsync(int userId);
        Task<List<UserStatsDto>> GetTopUsersAsync(int count = 10);
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LeaderboardResponseDto> GetLeaderboardAsync(LeaderboardFilterType filterType)
        {
            var dateFilter = GetDateFilter(filterType);
            var userStats = await CalculateLeaderboardDataAsync(dateFilter);

            // Sort by items, then by streak, then by level
            var rankedUsers = userStats
                .OrderByDescending(u => u.Items)
                .ThenByDescending(u => u.Streak)
                .ThenByDescending(u => u.Level)
                .ToList();

            // Assign ranks
            for (int i = 0; i < rankedUsers.Count; i++)
            {
                rankedUsers[i].Rank = i + 1;
            }

            var topThree = rankedUsers.Take(3).ToList();
            var tableEntries = rankedUsers.Skip(3).Take(7).ToList();

            // Assign positions for top three
            if (topThree.Count > 0) topThree[0].Position = "first";
            if (topThree.Count > 1) topThree[1].Position = "second";
            if (topThree.Count > 2) topThree[2].Position = "third";

            return new LeaderboardResponseDto
            {
                TopThree = topThree,
                TableEntries = tableEntries,
                FilterType = filterType.ToString(),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<UserStatsDto> GetUserStatsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Items)
                .Include(u => u.UserLevel)
                .ThenInclude(ul => ul.CurrentLevel)
                .Include(u => u.UserStreaks)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new UserStatsDto();

            var itemCount = user.Items?.Count(i => i.Status == Models.Entities.Items.ItemStatus.Completed) ?? 0;
            var currentStreak = user.UserStreaks?.FirstOrDefault()?.CurrentStreak ?? 0;
            var currentLevel = user.UserLevel?.CurrentLevel?.LevelNumber ?? 1;
            var currentXp = user.UserLevel?.CurrentXp ?? 0;

            return new UserStatsDto
            {
                UserId = user.Id,
                Name = user.UserName ?? "Unknown User",
                ItemsDecluttered = itemCount,
                CurrentStreak = currentStreak,
                CurrentLevel = currentLevel,
                CurrentXp = currentXp,
                TokenBalance = user.TokenBalance,
                LastActivity = user.LastLogin ?? user.DateCreated
            };
        }

        public async Task<List<UserStatsDto>> GetTopUsersAsync(int count = 10)
        {
            var users = await _context.Users
                .Include(u => u.Items)
                .Include(u => u.UserLevel)
                .ThenInclude(ul => ul.CurrentLevel)
                .Include(u => u.UserStreaks)
                .Where(u => u.Status == "active")
                .ToListAsync();

            var userStats = users.Select(user => new UserStatsDto
            {
                UserId = user.Id,
                Name = user.UserName ?? "Unknown User",
                ItemsDecluttered = user.Items?.Count(i => i.Status == Models.Entities.Items.ItemStatus.Completed) ?? 0,
                CurrentStreak = user.UserStreaks?.FirstOrDefault()?.CurrentStreak ?? 0,
                CurrentLevel = user.UserLevel?.CurrentLevel?.LevelNumber ?? 1,
                CurrentXp = user.UserLevel?.CurrentXp ?? 0,
                TokenBalance = user.TokenBalance,
                LastActivity = user.LastLogin ?? user.DateCreated
            }).ToList();

            return userStats
                .OrderByDescending(u => u.ItemsDecluttered)
                .ThenByDescending(u => u.CurrentStreak)
                .ThenByDescending(u => u.CurrentLevel)
                .Take(count)
                .ToList();
        }

        private async Task<List<LeaderboardDisplayDto>> CalculateLeaderboardDataAsync(DateTime? dateFilter)
        {
            var query = _context.Users
                .Include(u => u.Items)
                .Include(u => u.UserLevel)
                .ThenInclude(ul => ul.CurrentLevel)
                .Include(u => u.UserStreaks)
                .Where(u => u.Status == "active");

            var users = await query.ToListAsync();
            var leaderboardEntries = new List<LeaderboardDisplayDto>();

            foreach (var user in users)
            {
                var itemQuery = user.Items.AsQueryable();

                if (dateFilter.HasValue)
                {
                    itemQuery = itemQuery.Where(i => i.DatePosted >= dateFilter.Value);
                }

                var itemCount = itemQuery.Count(i => i.Status == Models.Entities.Items.ItemStatus.Completed);
                var currentStreak = CalculateStreak(user, dateFilter);
                var currentLevel = user.UserLevel?.CurrentLevel?.LevelNumber ?? 1;

                leaderboardEntries.Add(new LeaderboardDisplayDto
                {
                    UserId = user.Id,
                    Name = user.UserName ?? "Unknown User",
                    Items = itemCount,
                    Streak = currentStreak,
                    Level = currentLevel
                });
            }

            return leaderboardEntries;
        }

        private int CalculateStreak(AppUser user, DateTime? dateFilter)
        {
            var streak = user.UserStreaks?.FirstOrDefault();
            if (streak == null) return 0;

            if (dateFilter.HasValue)
            {
                var days = (DateTime.UtcNow - dateFilter.Value).Days;
                return Math.Min(streak.CurrentStreak, days);
            }

            return streak.CurrentStreak;
        }

        private DateTime? GetDateFilter(LeaderboardFilterType filterType)
        {
            return filterType switch
            {
                LeaderboardFilterType.Daily => DateTime.UtcNow.Date,
                LeaderboardFilterType.Weekly => DateTime.UtcNow.AddDays(-7),
                LeaderboardFilterType.AllTime => null,
                _ => null
            };
        }
    }
}