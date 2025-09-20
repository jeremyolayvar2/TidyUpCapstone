using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.DTOs.Leaderboard
{
    // Custom DTO for the frontend leaderboard display
    public class LeaderboardDisplayDto
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Items { get; set; }
        public int Streak { get; set; }
        public int Level { get; set; }
        public string Position { get; set; } = string.Empty; // "first", "second", "third"
        public int UserId { get; set; }
    }

    public class LeaderboardResponseDto
    {
        public List<LeaderboardDisplayDto> TopThree { get; set; } = new();
        public List<LeaderboardDisplayDto> TableEntries { get; set; } = new();
        public string FilterType { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class UserStatsDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemsDecluttered { get; set; }
        public int CurrentStreak { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentXp { get; set; }
        public decimal TokenBalance { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class SessionUserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public enum LeaderboardFilterType
    {
        AllTime,
        Weekly,
        Daily
    }
}