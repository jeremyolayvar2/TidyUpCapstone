using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Models.DTOs.User
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public UserRole Role { get; set; }
        public decimal TokenBalance { get; set; }
        public DateTime DateCreated { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? AvatarUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public RegistrationMethod RegistrationMethod { get; set; }
        public bool MarketingEmailsEnabled { get; set; }
        public List<string> ConnectedProviders { get; set; } = new List<string>();
    }

    public class UpdateUserProfileDto
    {
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool MarketingEmailsEnabled { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalItemsPosted { get; set; }
        public int TotalItemsSold { get; set; }
        public int TotalItemsBought { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalSpent { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentXp { get; set; }
        public int XpToNextLevel { get; set; }
        public int AchievementsEarned { get; set; }
        public int ActiveStreaks { get; set; }
        public int CompletedQuests { get; set; }
    }
}