namespace TidyUpCapstone.Models.DTOs.Gamification
{
    public class UserStatisticsDto
    {
        public int UserId { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentXp { get; set; }
        public int XpToNextLevel { get; set; }
        public decimal TokenBalance { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
    }
}
