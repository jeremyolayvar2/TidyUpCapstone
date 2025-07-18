using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.SSO;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.Search;
using TidyUpCapstone.Models.Entities.Reports;
using TidyUpCapstone.Models.Entities.Customization;
using TidyUpCapstone.Models.Entities.Leaderboards;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.System;

namespace YourApp.Models.Entities.Authentication
{
    [Table("app_user")]
    public class AppUser
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Column("is_verified")]
        public bool IsVerified { get; set; } = false;

        [Required]
        [StringLength(50)]
        [Column("role")]
        public string Role { get; set; } = "user"; // user, moderator, admin

        [Column("token_balance", TypeName = "decimal(10,2)")]
        public decimal TokenBalance { get; set; } = 0.00m;

        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column("managed_by_admin_id")]
        public int? ManagedByAdminId { get; set; }

        [Column("admin_notes", TypeName = "text")]
        public string? AdminNotes { get; set; }

        [Required]
        [StringLength(50)]
        [Column("status")]
        public string Status { get; set; } = "active"; // active, inactive, banned

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        // SSO-related fields
        [StringLength(50)]
        [Column("external_provider")]
        public string? ExternalProvider { get; set; }

        [StringLength(255)]
        [Column("external_user_id")]
        public string? ExternalUserId { get; set; }

        [StringLength(255)]
        [Column("provider_display_name")]
        public string? ProviderDisplayName { get; set; }

        [StringLength(500)]
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        // Enhanced authentication fields
        [Column("email_confirmed")]
        public bool EmailConfirmed { get; set; } = false;

        [StringLength(20)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("phone_number_confirmed")]
        public bool PhoneNumberConfirmed { get; set; } = false;

        [Column("two_factor_enabled")]
        public bool TwoFactorEnabled { get; set; } = false;

        [Column("lockout_end")]
        public DateTime? LockoutEnd { get; set; }

        [Column("lockout_enabled")]
        public bool LockoutEnabled { get; set; } = true;

        [Column("access_failed_count")]
        public int AccessFailedCount { get; set; } = 0;

        // User preferences
        [Required]
        [StringLength(50)]
        [Column("registration_method")]
        public string RegistrationMethod { get; set; } = "email"; // email, google, microsoft, facebook

        [Column("terms_accepted_at")]
        public DateTime? TermsAcceptedAt { get; set; }

        [Column("privacy_accepted_at")]
        public DateTime? PrivacyAcceptedAt { get; set; }

        [Column("marketing_emails_enabled")]
        public bool MarketingEmailsEnabled { get; set; } = false;

        // Navigation properties
        [ForeignKey("ManagedByAdminId")]
        public virtual AppUser? ManagedByAdmin { get; set; }

        public virtual ICollection<AppUser> ManagedUsers { get; set; } = new List<AppUser>();
        public virtual Admin? Admin { get; set; }
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
        public virtual ICollection<Transaction> BuyerTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transaction> SellerTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<UserSsoLink> SsoLinks { get; set; } = new List<UserSsoLink>();
        public virtual ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
        public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
        public virtual UserLocationPreference? LocationPreference { get; set; }
        public virtual UserLevel? UserLevel { get; set; }
        public virtual ICollection<UserQuest> UserQuests { get; set; } = new List<UserQuest>();
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
        public virtual ICollection<UserStreak> UserStreaks { get; set; } = new List<UserStreak>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
        public virtual ICollection<UserReport> ReporterReports { get; set; } = new List<UserReport>();
        public virtual ICollection<UserReport> ReportedUserReports { get; set; } = new List<UserReport>();
        public virtual ICollection<AiTrainingFeedback> AiTrainingFeedbacks { get; set; } = new List<AiTrainingFeedback>();
        public virtual ICollection<UserVisualsPurchase> VisualsPurchases { get; set; } = new List<UserVisualsPurchase>();
        public virtual ICollection<LeaderboardEntry> LeaderboardEntries { get; set; } = new List<LeaderboardEntry>();
        public virtual ICollection<UserNotificationPreference> NotificationPreferences { get; set; } = new List<UserNotificationPreference>();
    }
}