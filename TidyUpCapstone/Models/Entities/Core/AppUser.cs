using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.Reporting;
using TidyUpCapstone.Models.Entities.System;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.Entities.User
{
    [Table("app_user")]
    public class AppUser : IdentityUser<int>
    {

        [StringLength(50)]
        [Column("first_name")]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Column("last_name")]
        public string? LastName { get; set; }

        [StringLength(100)]
        [Column("location")]
        public string? Location { get; set; }

        [Column("birthday")]
        public DateTime? Birthday { get; set; }

        [StringLength(20)]
        [Column("gender")]
        public string? Gender { get; set; }

        [StringLength(500)]
        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        // Phone verification fields
        [StringLength(6)]
        [Column("verification_code")]
        public string? VerificationCode { get; set; }

        [Column("verification_code_expiry")]
        public DateTime? VerificationCodeExpiry { get; set; }

        [Column("token_balance", TypeName = "decimal(10,2)")]
        public decimal TokenBalance { get; set; } = 0.00m;

        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column("managed_by_admin_id")]
        public int? ManagedByAdminId { get; set; } // Changed from string? to int?

        [Column("admin_notes")]
        public string? AdminNotes { get; set; }

        [StringLength(50)]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [StringLength(10)]
        [Column("language")]
        public string Language { get; set; } = "en";

        [StringLength(50)]
        [Column("timezone")]
        public string Timezone { get; set; } = "Asia/Manila";

        [Column("high_contrast")]
        public bool HighContrast { get; set; } = false;

        [Column("large_text")]
        public bool LargeText { get; set; } = false;

        [Column("reduce_motion")]
        public bool ReduceMotion { get; set; } = false;

        [Column("screen_reader")]
        public bool ScreenReader { get; set; } = false;

        // Navigation Properties
        [ForeignKey("ManagedByAdminId")]
        public virtual AppUser? ManagedByAdmin { get; set; }

        public virtual ICollection<AppUser> ManagedUsers { get; set; } = new List<AppUser>();

        public virtual ICollection<Items.Item> Items { get; set; } = new List<Items.Item>();

        public virtual ICollection<Transactions.Transaction> BuyerTransactions { get; set; } = new List<Transactions.Transaction>();
        public virtual ICollection<Transactions.Transaction> SellerTransactions { get; set; } = new List<Transactions.Transaction>();

        public virtual ICollection<Community.Post> Posts { get; set; } = new List<Community.Post>();
        public virtual ICollection<Community.Comment> Comments { get; set; } = new List<Community.Comment>();
        public virtual ICollection<Community.Reaction> Reactions { get; set; } = new List<Community.Reaction>();

        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

        public virtual ICollection<Notifications.Notification> Notifications { get; set; } = new List<Notifications.Notification>();

        public virtual ICollection<Gamification.UserQuest> UserQuests { get; set; } = new List<Gamification.UserQuest>();
        public virtual ICollection<Gamification.UserAchievement> UserAchievements { get; set; } = new List<Gamification.UserAchievement>();
        public virtual ICollection<Gamification.UserStreak> UserStreaks { get; set; } = new List<Gamification.UserStreak>();
        public virtual ICollection<Gamification.UserVisualsPurchase> UserVisualsPurchases { get; set; } = new List<Gamification.UserVisualsPurchase>();

        public virtual Admin? Admin { get; set; }

        public virtual ICollection<SSO.UserSsoLink> SsoHistory { get; set; } = new List<SSO.UserSsoLink>();

        public virtual UserLevel? UserLevel { get; set; }

        public virtual UserLocationPreference? LocationPreference { get; set; }

        public virtual ICollection<UserReport> ReportsMade { get; set; } = new List<UserReport>();
        public virtual ICollection<UserReport> ReportsReceived { get; set; } = new List<UserReport>();

        public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
        public virtual ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();

        // AI-related navigation properties
        public virtual ICollection<AI.AiTrainingFeedback> AiTrainingFeedbacks { get; set; } = new List<AI.AiTrainingFeedback>();

        public virtual UserPrivacySettings? PrivacySettings { get; set; }

    }

    public enum UserRole
    {
        User,
        Moderator,
        Admin
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Banned
    }

    public enum RegistrationMethod
    {
        Email,
        Google,
        Microsoft,
        Facebook
    }
}