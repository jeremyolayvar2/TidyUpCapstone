using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.X86;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.Reporting;
using TidyUpCapstone.Models.Entities.System;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Models.Entities.User
{
    public class AppUser : IdentityUser<int>
    {
        [Column(TypeName = "decimal(10,2)")]
        public decimal TokenBalance { get; set; } = 0.00m;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public string? ManagedByAdminId { get; set; }

        public string? AdminNotes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "active";

        public DateTime? LastLogin { get; set; }

        // Navigation Properties
        public virtual AppUser? ManagedByAdmin { get; set; }
        public virtual ICollection<AppUser> ManagedUsers { get; set; } = new List<AppUser>();
        public virtual ICollection<Items.Item> Items { get; set; } = new List<Items.Item>();
        public virtual ICollection<Transactions.Transaction> BuyerTransactions { get; set; } = new List<Transactions.Transaction>();
        public virtual ICollection<Transactions.Transaction> SellerTransactions { get; set; } = new List<Transactions.Transaction>();
        public virtual ICollection<Community.Post> Posts { get; set; } = new List<Community.Post>();
        public virtual ICollection<Community.Comment> Comments { get; set; } = new List<Community.Comment>();
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public virtual ICollection<Notifications.Notification> Notifications { get; set; } = new List<Notifications.Notification>();
        public virtual ICollection<Gamification.UserQuest> UserQuests { get; set; } = new List<Gamification.UserQuest>();
        public virtual Admin? Admin { get; set; }
        public virtual ICollection<SSO.UserSsoLink> SsoHistory { get; set; } = new List<SSO.UserSsoLink>();



        public virtual UserLevel UserLevel { get; set; }

        public virtual UserLocationPreference LocationPreference { get; set; }
        public virtual ICollection<UserReport> ReportsMade { get; set; } = new List<UserReport>();
        public virtual ICollection<UserReport> ReportsReceived { get; set; }


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