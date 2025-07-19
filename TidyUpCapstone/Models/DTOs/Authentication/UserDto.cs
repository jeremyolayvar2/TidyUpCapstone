namespace TidyUpCapstone.Models.DTOs.Authentication
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool EmailConfirmed { get; set; }
        public decimal TokenBalance { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? AvatarUrl { get; set; }
        public string RegistrationMethod { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool MarketingEmailsEnabled { get; set; }
        public int? ManagedByAdminId { get; set; }
        public List<string> LinkedSsoProviders { get; set; } = new List<string>();
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "user";
        public string RegistrationMethod { get; set; } = "email";
        public bool AcceptTerms { get; set; }
        public bool AcceptPrivacy { get; set; }
        public bool MarketingEmailsEnabled { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? MarketingEmailsEnabled { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AdminUpdateUserDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public bool? IsVerified { get; set; }
        public bool? EmailConfirmed { get; set; }
        public decimal? TokenBalance { get; set; }
        public bool? LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? AdminNotes { get; set; }
        public int? ManagedByAdminId { get; set; }
    }

    public class UserStatsDto
    {
        public int UserId { get; set; }
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int CompletedItems { get; set; }
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public decimal TotalTokensEarned { get; set; }
        public decimal TotalTokensSpent { get; set; }
        public int CurrentLevel { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public int CurrentXp { get; set; }
        public int TotalXp { get; set; }
        public int CompletedQuests { get; set; }
        public int TotalAchievements { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int? GlobalRank { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int PostLikes { get; set; }
    }
}