using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Entities.AI;
using TidyUpCapstone.Models.Entities.Community;
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
        public DbSet<Escrow> Escrows { get; set; }


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
                entity.Property(e => e.TokenBalance).HasColumnName("token_balance");
                entity.Property(e => e.DateCreated).HasColumnName("date_created");
                entity.Property(e => e.ManagedByAdminId).HasColumnName("managed_by_admin_id");
                entity.Property(e => e.AdminNotes).HasColumnName("admin_notes");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
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
            builder.Entity<Escrow>().ToTable("escrows");


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

            // Configure JSON columns as nvarchar(max) for compatibility
            builder.Entity<Admin>()
                .Property(a => a.AdminPermissions)
                .HasColumnType("nvarchar(max)");

            builder.Entity<SsoAuditLog>()
                .Property(sal => sal.AdditionalData)
                .HasColumnType("nvarchar(max)");

            builder.Entity<AdminReport>()
                .Property(ar => ar.ReportData)
                .HasColumnType("nvarchar(max)");

            builder.Entity<AdminReport>()
                .Property(ar => ar.ReportParameters)
                .HasColumnType("nvarchar(max)");

            builder.Entity<UserReport>()
                .Property(ur => ur.EvidenceUrls)
                .HasColumnType("nvarchar(max)");

            builder.Entity<AuditLog>()
                .Property(al => al.OldValues)
                .HasColumnType("nvarchar(max)");

            builder.Entity<AuditLog>()
                .Property(al => al.NewValues)
                .HasColumnType("nvarchar(max)");
        }

        private void ConfigureRelationships(ModelBuilder builder)
        {
            // AppUser self-referencing relationship
            builder.Entity<AppUser>()
                .HasOne(u => u.ManagedByAdmin)
                .WithMany(u => u.ManagedUsers)
                .HasForeignKey(u => u.ManagedByAdminId)
                .OnDelete(DeleteBehavior.NoAction);

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

            // TransactionMethodHistory relationships

            builder.Entity<Escrow>()
    .HasOne(e => e.Transaction)
    .WithMany(t => t.Escrows)
    .HasForeignKey(e => e.TransactionId)
    .OnDelete(DeleteBehavior.Cascade);

            // Comment relationships - ALL set to NoAction to prevent cascade cycles
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Post relationships
            builder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Reaction relationships
            builder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Reaction>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            // UserSsoLink relationships
            builder.Entity<UserSsoLink>()
                .HasOne(usl => usl.User)
                .WithMany(u => u.SsoHistory)
                .HasForeignKey(usl => usl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserSsoLink>()
                .HasOne(usl => usl.Provider)
                .WithMany(sp => sp.UserSsoLinks)
                .HasForeignKey(usl => usl.ProviderName)
                .HasPrincipalKey(sp => sp.ProviderName)
                .OnDelete(DeleteBehavior.Cascade);

            // SsoAuditLog relationship with SsoProvider
            builder.Entity<SsoAuditLog>()
                .HasOne(sal => sal.Provider)
                .WithMany(sp => sp.SsoAuditLogs)
                .HasForeignKey(sal => sal.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SsoAuditLog>()
                .HasOne(sal => sal.User)
                .WithMany()
                .HasForeignKey(sal => sal.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Item relationships
            builder.Entity<Item>()
                .HasOne(i => i.User)
                .WithMany(u => u.Items)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // AI Analysis and Prediction relationships
            builder.Entity<AzureCvAnalysis>()
                .HasOne(aca => aca.Item)
                .WithMany(i => i.AzureCvAnalyses)
                .HasForeignKey(aca => aca.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TensorflowPrediction>()
                .HasOne(tp => tp.Item)
                .WithMany(i => i.TensorflowPredictions)
                .HasForeignKey(tp => tp.ItemId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<AiProcessingPipeline>()
                .HasOne(app => app.Analysis)
                .WithMany(aca => aca.ProcessingPipelines)
                .HasForeignKey(app => app.AnalysisId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AiProcessingPipeline>()
                .HasOne(app => app.Prediction)
                .WithMany(tp => tp.ProcessingPipelines)
                .HasForeignKey(app => app.PredictionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AiProcessingPipeline>()
                .HasOne(app => app.Item)
                .WithMany(i => i.AiProcessingPipelines)
                .HasForeignKey(app => app.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            // AI Training Feedback relationships
            builder.Entity<AiTrainingFeedback>()
                .HasOne(atf => atf.User)
                .WithMany(u => u.AiTrainingFeedbacks)
                .HasForeignKey(atf => atf.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AiTrainingFeedback>()
                .HasOne(atf => atf.Item)
                .WithMany(i => i.AiTrainingFeedbacks)
                .HasForeignKey(atf => atf.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            // Notification relationships
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage relationships
            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Gamification relationships
            builder.Entity<UserQuest>()
                .HasOne(uq => uq.User)
                .WithMany(u => u.UserQuests)
                .HasForeignKey(uq => uq.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserStreak>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserStreaks)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserVisualsPurchase>()
                .HasOne(uvp => uvp.User)
                .WithMany(u => u.UserVisualsPurchases)
                .HasForeignKey(uvp => uvp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserReport>()
                .HasOne(ur => ur.ReportedUser)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(ur => ur.ReportedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // System Setting relationship
            builder.Entity<SystemSetting>()
                .HasOne(ss => ss.UpdatedByAdmin)
                .WithMany(a => a.SystemSettingsUpdated)
                .HasForeignKey(ss => ss.UpdatedByAdminId)
                .OnDelete(DeleteBehavior.NoAction);

            // Audit Log relationships
            builder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AuditLog>()
                .HasOne(al => al.Admin)
                .WithMany(a => a.AdminAuditLogs)
                .HasForeignKey(al => al.AdminId)
                .OnDelete(DeleteBehavior.NoAction);

            // Search History relationships
            builder.Entity<SearchHistory>()
                .HasOne(sh => sh.User)
                .WithMany()
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Login Log relationships
            builder.Entity<LoginLog>()
                .HasOne(ll => ll.User)
                .WithMany(u => u.LoginLogs)
                .HasForeignKey(ll => ll.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Email Verification relationships
            builder.Entity<EmailVerification>()
                .HasOne(ev => ev.User)
                .WithMany(u => u.EmailVerifications)
                .HasForeignKey(ev => ev.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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

            // SsoProvider ProviderName must be unique since it's used as principal key
            builder.Entity<SsoProvider>()
                .HasIndex(sp => sp.ProviderName)
                .IsUnique()
                .HasDatabaseName("idx_sso_provider_name");

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

            // Item price constraints - Fix column names to match actual properties
            builder.Entity<Item>()
                .HasCheckConstraint("chk_positive_prices",
                    "[AdjustedTokenPrice] >= 0 AND [FinalTokenPrice] >= 0 AND ([AiSuggestedPrice] IS NULL OR [AiSuggestedPrice] >= 0)");

            // Coordinate constraints
            builder.Entity<Item>()
                .HasCheckConstraint("chk_valid_coordinates",
                    "([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)");

            // Transaction constraints
            builder.Entity<Transaction>()
                .HasCheckConstraint("chk_buyer_not_seller", "[BuyerId] != [SellerId]");

            builder.Entity<Transaction>()
                .HasCheckConstraint("chk_positive_token_amount", "[TokenAmount] > 0");

            // Location preference constraints - Fix column name to match actual property
            builder.Entity<UserLocationPreference>()
                .HasCheckConstraint("chk_valid_radius", "[RadiusKm] > 0 AND [RadiusKm] <= 1000");

            // User Report constraint to prevent self-reporting
            builder.Entity<UserReport>()
                .HasCheckConstraint("chk_no_self_report", "[ReporterId] != [ReportedUserId]");
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