using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Community;
using TidyUpCapstone.Models.Entities.Customization;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.Reporting;
using TidyUpCapstone.Models.Entities.SSO;
using TidyUpCapstone.Models.Entities.System;
using TidyUpCapstone.Models.Entities.Transactions;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.Core;


namespace TidyUpCapstone.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Core entities
        public DbSet<Admin> Admins { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }

        // SSO entities
        public DbSet<SsoProvider> SsoProviders { get; set; }
        public DbSet<UserSsoLink> UserSsoLinks { get; set; }
        public DbSet<SsoAuditLog> SsoAuditLogs { get; set; }

        // Item entities
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<ItemCondition> ItemConditions { get; set; }
        public DbSet<ItemLocation> ItemLocations { get; set; }
        public DbSet<Item> Items { get; set; }

        // AI entities
        public DbSet<AzureCvAnalysis> AzureCvAnalyses { get; set; }
        public DbSet<TensorflowPrediction> TensorflowPredictions { get; set; }
        public DbSet<AiProcessingPipeline> AiProcessingPipelines { get; set; }
        public DbSet<AiTrainingFeedback> AiTrainingFeedbacks { get; set; }

        // Transaction entities
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        // Community entities
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }

        // Gamification entities
        public DbSet<Quest> Quests { get; set; }
        public DbSet<UserQuest> UserQuests { get; set; }
        public DbSet<QuestProgress> QuestProgresses { get; set; }
        public DbSet<VisualItem> VisualItems { get; set; }
        public DbSet<UserVisualsPurchase> UserVisualsPurchases { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<StreakType> StreakTypes { get; set; }
        public DbSet<UserStreak> UserStreaks { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<LeaderboardEntry> LeaderboardEntries { get; set; }

        // Notification entities
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }

        // System entities
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<UserLocationPreference> UserLocationPreferences { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Reporting entities
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<AdminReport> AdminReports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables with custom names
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("app_user");
                entity.Property(e => e.Id).HasColumnName("user_id");
                entity.Property(e => e.UserName).HasColumnName("username");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            });

            builder.Entity<IdentityRole<int>>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");

            // Configure table names to match SQL schema
            ConfigureTableNames(builder);

            // Configure relationships and constraints
            ConfigureRelationships(builder);

            // Configure indexes
            ConfigureIndexes(builder);

            // Configure check constraints
            ConfigureCheckConstraints(builder);

            // Configure composite keys
            ConfigureCompositeKeys(builder);
        }

        private void ConfigureTableNames(ModelBuilder builder)
        {
            // Core entities
            builder.Entity<Admin>().ToTable("admin");
            builder.Entity<EmailVerification>().ToTable("email_verifications");
            builder.Entity<LoginLog>().ToTable("login_logs");

            // SSO entities
            builder.Entity<SsoProvider>().ToTable("sso_providers");
            builder.Entity<UserSsoLink>().ToTable("user_sso_links");
            builder.Entity<SsoAuditLog>().ToTable("sso_audit_logs");

            // Item entities
            builder.Entity<ItemCategory>().ToTable("item_category");
            builder.Entity<ItemCondition>().ToTable("itemcondition");
            builder.Entity<ItemLocation>().ToTable("itemlocation");
            builder.Entity<Item>().ToTable("items");

            // AI entities
            builder.Entity<AzureCvAnalysis>().ToTable("azure_cv_analysis");
            builder.Entity<TensorflowPrediction>().ToTable("tensorflow_prediction");
            builder.Entity<AiProcessingPipeline>().ToTable("ai_processing_pipeline");
            builder.Entity<AiTrainingFeedback>().ToTable("ai_training_feedback");

            // Transaction entities
            builder.Entity<Transaction>().ToTable("transactions");
            builder.Entity<Chat>().ToTable("chats");
            builder.Entity<ChatMessage>().ToTable("chat_messages");

            // Community entities
            builder.Entity<Post>().ToTable("posts");
            builder.Entity<Comment>().ToTable("comments");
            builder.Entity<Reaction>().ToTable("reactions");

            // Gamification entities
            builder.Entity<Quest>().ToTable("quests");
            builder.Entity<UserQuest>().ToTable("userquests");
            builder.Entity<QuestProgress>().ToTable("quest_progress");
            builder.Entity<VisualItem>().ToTable("visual_items");
            builder.Entity<UserVisualsPurchase>().ToTable("user_visuals_purchases");
            builder.Entity<Achievement>().ToTable("achievements");
            builder.Entity<UserAchievement>().ToTable("user_achievements");
            builder.Entity<StreakType>().ToTable("streak_types");
            builder.Entity<UserStreak>().ToTable("user_streaks");
            builder.Entity<Level>().ToTable("levels");
            builder.Entity<UserLevel>().ToTable("user_levels");
            builder.Entity<Leaderboard>().ToTable("leaderboards");
            builder.Entity<LeaderboardEntry>().ToTable("leaderboard_entries");

            // Notification entities
            builder.Entity<NotificationType>().ToTable("notification_types");
            builder.Entity<Notification>().ToTable("notifications");
            builder.Entity<UserNotificationPreference>().ToTable("user_notification_preferences");

            // System entities
            builder.Entity<SearchHistory>().ToTable("search_history");
            builder.Entity<UserLocationPreference>().ToTable("user_location_preferences");
            builder.Entity<SystemSetting>().ToTable("system_settings");
            builder.Entity<AuditLog>().ToTable("audit_logs");

            // Reporting entities
            builder.Entity<UserReport>().ToTable("user_reports");
            builder.Entity<AdminReport>().ToTable("admin_reports");
        }

        private void ConfigureRelationships(ModelBuilder builder)
        {
            // AppUser self-referencing relationship
            builder.Entity<AppUser>()
                .HasOne(u => u.ManagedByAdmin)
                .WithMany(u => u.ManagedUsers)
                .HasForeignKey(u => u.ManagedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // Admin relationship
            builder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction relationships
            builder.Entity<Transaction>()
                .HasOne(t => t.Buyer)
                .WithMany(u => u.BuyerTransactions)
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.Seller)
                .WithMany(u => u.SellerTransactions)
                .HasForeignKey(t => t.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chat relationship (one-to-one with Transaction)
            builder.Entity<Chat>()
                .HasOne(c => c.Transaction)
                .WithOne(t => t.Chat)
                .HasForeignKey<Chat>(c => c.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment self-referencing relationship
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSsoLink relationships
            builder.Entity<UserSsoLink>()
                .HasOne(usl => usl.Provider)
                .WithMany(sp => sp.UserSsoLinks)
                .HasForeignKey(usl => usl.ProviderName)
                .HasPrincipalKey(sp => sp.ProviderName)
                .OnDelete(DeleteBehavior.Cascade);

            // AI Processing Pipeline relationships
            builder.Entity<AiProcessingPipeline>()
                .HasOne(app => app.Analysis)
                .WithMany(aca => aca.ProcessingPipelines)
                .HasForeignKey(app => app.AnalysisId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AiProcessingPipeline>()
                .HasOne(app => app.Prediction)
                .WithMany(tp => tp.ProcessingPipelines)
                .HasForeignKey(app => app.PredictionId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserLevel relationship (one-to-one)
            builder.Entity<UserLevel>()
                .HasOne(ul => ul.User)
                .WithOne(u => u.UserLevel)
                .HasForeignKey<UserLevel>(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserLocationPreference relationship (one-to-one)
            builder.Entity<UserLocationPreference>()
                .HasOne(ulp => ulp.User)
                .WithOne(u => u.LocationPreference)
                .HasForeignKey<UserLocationPreference>(ulp => ulp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User Report relationships (prevent self-reporting)
            builder.Entity<UserReport>()
                .HasOne(ur => ur.Reporter)
                .WithMany(u => u.ReportsMade)
                .HasForeignKey(ur => ur.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserReport>()
                .HasOne(ur => ur.ReportedUser)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(ur => ur.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureIndexes(ModelBuilder builder)
        {
            // User indexes
            builder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .HasDatabaseName("idx_user_email");

            builder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .HasDatabaseName("idx_user_username");

            // Item indexes
            builder.Entity<Item>()
                .HasIndex(i => new { i.UserId, i.Status })
                .HasDatabaseName("idx_item_user_status");

            builder.Entity<Item>()
                .HasIndex(i => new { i.CategoryId, i.Status })
                .HasDatabaseName("idx_item_category_status");

            builder.Entity<Item>()
                .HasIndex(i => new { i.Latitude, i.Longitude })
                .HasDatabaseName("idx_item_coordinates");

            // Transaction indexes
            builder.Entity<Transaction>()
                .HasIndex(t => new { t.BuyerId, t.TransactionStatus })
                .HasDatabaseName("idx_transaction_buyer_status");

            builder.Entity<Transaction>()
                .HasIndex(t => new { t.SellerId, t.TransactionStatus })
                .HasDatabaseName("idx_transaction_seller_status");

            // SSO indexes
            builder.Entity<UserSsoLink>()
                .HasIndex(usl => new { usl.ProviderName, usl.ProviderUserId })
                .IsUnique()
                .HasDatabaseName("idx_user_sso_links_provider");

            // Notification indexes
            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("idx_notification_user_read");
        }

        private void ConfigureCheckConstraints(ModelBuilder builder)
        {
            // Token balance constraints
            builder.Entity<AppUser>()
                .HasCheckConstraint("chk_positive_token_balance", "token_balance >= 0");

            // Item price constraints
            builder.Entity<Item>()
                .HasCheckConstraint("chk_positive_prices",
                    "adjusted_token_price >= 0 AND final_token_price >= 0 AND (ai_suggested_price IS NULL OR ai_suggested_price >= 0)");

            // Coordinate constraints
            builder.Entity<Item>()
                .HasCheckConstraint("chk_valid_coordinates",
                    "(latitude IS NULL AND longitude IS NULL) OR (latitude BETWEEN -90 AND 90 AND longitude BETWEEN -180 AND 180)");

            // Transaction constraints
            builder.Entity<Transaction>()
                .HasCheckConstraint("chk_buyer_not_seller", "buyer_id != seller_id");

            builder.Entity<Transaction>()
                .HasCheckConstraint("chk_positive_token_amount", "token_amount > 0");

            // Location preference constraints
            builder.Entity<UserLocationPreference>()
                .HasCheckConstraint("chk_valid_radius", "radius_km > 0 AND radius_km <= 1000");
        }

        private void ConfigureCompositeKeys(ModelBuilder builder)
        {
            // UserAchievement composite key
            builder.Entity<UserAchievement>()
                .HasKey(ua => new { ua.UserId, ua.AchievementId });

            // UserNotificationPreference composite key
            builder.Entity<UserNotificationPreference>()
                .HasKey(unp => new { unp.UserId, unp.TypeId });

            // Unique constraints
            builder.Entity<UserSsoLink>()
                .HasIndex(usl => new { usl.UserId, usl.ProviderName })
                .IsUnique();

            builder.Entity<UserQuest>()
                .HasIndex(uq => new { uq.UserId, uq.QuestId })
                .IsUnique();

            builder.Entity<UserStreak>()
                .HasIndex(us => new { us.UserId, us.StreakTypeId })
                .IsUnique();

            builder.Entity<UserVisualsPurchase>()
                .HasIndex(uvp => new { uvp.UserId, uvp.VisualId })
                .IsUnique();

            builder.Entity<Reaction>()
                .HasIndex(r => new { r.PostId, r.UserId })
                .IsUnique();

            builder.Entity<LeaderboardEntry>()
                .HasIndex(le => new { le.UserId, le.LeaderboardId })
                .IsUnique();
        }
    }
}