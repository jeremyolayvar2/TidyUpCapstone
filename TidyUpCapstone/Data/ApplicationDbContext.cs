using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Configurations;
using TidyUpCapstone.Models.Entities;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Authentication;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.Customization;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Leaderboards;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.Reports;
using TidyUpCapstone.Models.Entities.Search;
using TidyUpCapstone.Models.Entities.SSO;
using TidyUpCapstone.Models.Entities.System;
using TidyUpCapstone.Models.Entities.Transactions;
using YourApp.Models.Configurations;


namespace TidyUpCapstone.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        //// Legacy entities (keep for backward compatibility)
        //public DbSet<Item> ItemPosts { get; set; }
        //public DbSet<ItemCategory> ItemCategories { get; set; }
        //public DbSet<ItemCondition> ItemConditions { get; set; }
        //public DbSet<ItemLocation> ItemLocations { get; set; }
        //public DbSet<Chat> Message { get; set; }

        // New comprehensive entities
        // Authentication & User Management
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }

        // SSO
        public DbSet<SsoProvider> SsoProviders { get; set; }
        public DbSet<UserSsoLink> UserSsoLinks { get; set; }
        public DbSet<SsoAuditLog> SsoAuditLogs { get; set; }

        // Items (New enhanced version)
        public DbSet<Item> Items { get; set; }
        public DbSet<TidyUpCapstone.Models.Entities.Items.ItemCategory> NewItemCategories { get; set; }
        public DbSet<TidyUpCapstone.Models.Entities.Items.ItemCondition> NewItemConditions { get; set; }
        public DbSet<TidyUpCapstone.Models.Entities.Items.ItemLocation> NewItemLocations { get; set; }

        // AI
        public DbSet<AzureCvAnalysis> AzureCvAnalyses { get; set; }
        public DbSet<TensorflowPrediction> TensorflowPredictions { get; set; }
        public DbSet<AiProcessingPipeline> AiProcessingPipelines { get; set; }
        public DbSet<AiTrainingFeedback> AiTrainingFeedbacks { get; set; }

        // Transactions
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        // Community
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }

        // Gamification
        public DbSet<Quest> Quests { get; set; }
        public DbSet<UserQuest> UserQuests { get; set; }
        public DbSet<QuestProgress> QuestProgresses { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<StreakType> StreakTypes { get; set; }
        public DbSet<UserStreak> UserStreaks { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }

        // Notifications
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

        // Search & Preferences
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<UserLocationPreference> UserLocationPreferences { get; set; }

        // Customization
        public DbSet<VisualItem> VisualItems { get; set; }
        public DbSet<UserVisualsPurchase> UserVisualsPurchases { get; set; }

        // Leaderboards
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }

        // Reports
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<AdminReport> AdminReports { get; set; }

        // System
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Legacy seed data (keep existing)
            modelBuilder.Entity<ItemCategory>().HasData(
                new ItemCategory { Id = 1, Name = "Books & Stationery" },
                new ItemCategory { Id = 2, Name = "Electronics & Gadgets" },
                new ItemCategory { Id = 3, Name = "Toys & Games" },
                new ItemCategory { Id = 4, Name = "Home & Kitchen" },
                new ItemCategory { Id = 5, Name = "Furniture" },
                new ItemCategory { Id = 6, Name = "Appliances" },
                new ItemCategory { Id = 7, Name = "Health & Beauty" },
                new ItemCategory { Id = 8, Name = "Crafts & DIY Supplies" },
                new ItemCategory { Id = 9, Name = "School & Office Supplies" }
            );

            modelBuilder.Entity<ItemCondition>().HasData(
                new ItemCondition { Id = 1, Name = "Brand New" },
                new ItemCondition { Id = 2, Name = "Like New" },
                new ItemCondition { Id = 3, Name = "Gently Used" },
                new ItemCondition { Id = 4, Name = "Visible Wear" },
                new ItemCondition { Id = 5, Name = "For Repair/Parts" }
            );

            // Legacy configurations (keep existing)
            modelBuilder.Entity<AppUser>()
                .Property(u => u.TokenBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Chat>().ToTable("Message");
            modelBuilder.Entity<Chat>()
                .HasOne(m => m.Buyer)
                .WithMany()
                .HasForeignKey(m => m.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(m => m.Seller)
                .WithMany()
                .HasForeignKey(m => m.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Messages>()
                .HasOne(m => m.ItemPost)
                .WithMany()
                .HasForeignKey(m => m.ItemPostId)
                .OnDelete(DeleteBehavior.Cascade);

            // New comprehensive configurations
            ConfigureNewEntities(modelBuilder);

            // Apply all entity configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure new relationships and constraints
            modelBuilder.ConfigureCustomRelationships();
            modelBuilder.ConfigureComplexRelationships();
            modelBuilder.ConfigureReportingRelationships();
            modelBuilder.ConfigureBusinessConstraints();

            // Seed initial data for new entities
            modelBuilder.SeedInitialData();
        }

        private void ConfigureNewEntities(ModelBuilder modelBuilder)
        {
            // Configure composite primary keys
            modelBuilder.Entity<UserAchievement>()
                .HasKey(ua => new { ua.UserId, ua.AchievementId });

            modelBuilder.Entity<UserNotificationPreference>()
                .HasKey(unp => new { unp.UserId, unp.TypeId });

            // Configure unique constraints for new entities
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<SsoProvider>()
                .HasIndex(sp => sp.ProviderName)
                .IsUnique();

            modelBuilder.Entity<UserSsoLink>()
                .HasIndex(usl => new { usl.ProviderName, usl.ProviderUserId })
                .IsUnique();

            modelBuilder.Entity<UserSsoLink>()
                .HasIndex(usl => new { usl.UserId, usl.ProviderName })
                .IsUnique();

            // Configure check constraints for new entities
            modelBuilder.Entity<AppUser>()
                .HasCheckConstraint("chk_positive_token_balance", "token_balance >= 0");

            modelBuilder.Entity<Item>()
                .HasCheckConstraint("chk_positive_prices",
                    "adjusted_token_price >= 0 AND final_token_price >= 0 AND (ai_suggested_price IS NULL OR ai_suggested_price >= 0)");

            modelBuilder.Entity<Item>()
                .HasCheckConstraint("chk_valid_coordinates",
                    "(latitude IS NULL AND longitude IS NULL) OR (latitude BETWEEN -90 AND 90 AND longitude BETWEEN -180 AND 180)");

            modelBuilder.Entity<Transaction>()
                .HasCheckConstraint("chk_buyer_not_seller", "buyer_id != seller_id");

            modelBuilder.Entity<Transaction>()
                .HasCheckConstraint("chk_positive_token_amount", "token_amount > 0");

            // Configure foreign key relationships for new entities
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.ManagedByAdmin)
                .WithMany(u => u.ManagedUsers)
                .HasForeignKey(u => u.ManagedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Buyer)
                .WithMany(u => u.BuyerTransactions)
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Seller)
                .WithMany(u => u.SellerTransactions)
                .HasForeignKey(t => t.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.Reporter)
                .WithMany(u => u.ReporterReports)
                .HasForeignKey(ur => ur.ReporterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.ReportedUser)
                .WithMany(u => u.ReportedUserReports)
                .HasForeignKey(ur => ur.ReportedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Transaction)
                .WithOne(t => t.Chat)
                .HasForeignKey<Chat>(c => c.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLocationPreference>()
                .HasOne(ulp => ulp.User)
                .WithOne(u => u.LocationPreference)
                .HasForeignKey<UserLocationPreference>(ulp => ulp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLevel>()
                .HasOne(ul => ul.User)
                .WithOne(u => u.UserLevel)
                .HasForeignKey<UserLevel>(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SSO relationships
            modelBuilder.Entity<UserSsoLink>()
                .HasOne(usl => usl.Provider)
                .WithMany(sp => sp.UserSsoLinks)
                .HasForeignKey(usl => usl.ProviderName)
                .HasPrincipalKey(sp => sp.ProviderName)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SsoAuditLog>()
                .HasOne(sal => sal.Provider)
                .WithMany(sp => sp.AuditLogs)
                .HasForeignKey(sal => sal.ProviderName)
                .HasPrincipalKey(sp => sp.ProviderName)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure audit logs
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.Admin)
                .WithMany(a => a.AuditLogs)
                .HasForeignKey(al => al.AdminId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}