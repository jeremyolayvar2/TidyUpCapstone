using Microsoft.EntityFrameworkCore;
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


namespace TidyUpCapstone.Models.Configurations
{
    public static class ModelBuilderExtensions
    {
        public static void ConfigureCustomRelationships(this ModelBuilder modelBuilder)
        {
            // Configure composite primary keys
            modelBuilder.Entity<UserAchievement>()
                .HasKey(ua => new { ua.UserId, ua.AchievementId });

            modelBuilder.Entity<UserNotificationPreference>()
                .HasKey(unp => new { unp.UserId, unp.TypeId });

            // Configure unique constraints
            modelBuilder.Entity<SsoProvider>()
                .HasIndex(sp => sp.ProviderName)
                .IsUnique();

            modelBuilder.Entity<UserSsoLink>()
                .HasIndex(usl => new { usl.ProviderName, usl.ProviderUserId })
                .IsUnique();

            modelBuilder.Entity<UserSsoLink>()
                .HasIndex(usl => new { usl.UserId, usl.ProviderName })
                .IsUnique();

            modelBuilder.Entity<UserQuest>()
                .HasIndex(uq => new { uq.UserId, uq.QuestId })
                .IsUnique();

            modelBuilder.Entity<UserStreak>()
                .HasIndex(us => new { us.UserId, us.StreakTypeId })
                .IsUnique();

            modelBuilder.Entity<UserVisualsPurchase>()
                .HasIndex(uvp => new { uvp.UserId, uvp.VisualId })
                .IsUnique();

            modelBuilder.Entity<LeaderboardEntry>()
                .HasIndex(le => new { le.UserId, le.LeaderboardId })
                .IsUnique();

            modelBuilder.Entity<Reaction>()
                .HasIndex(r => new { r.PostId, r.UserId })
                .IsUnique();
        }

        public static void ConfigureComplexRelationships(this ModelBuilder modelBuilder)
        {
            // SSO relationships
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

            // Chat relationships
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Transaction)
                .WithOne(t => t.Chat)
                .HasForeignKey<Chat>(c => c.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            // User preference relationships
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

            // Admin relationships
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public static void ConfigureReportingRelationships(this ModelBuilder modelBuilder)
        {
            // User report relationships - prevent cascade delete conflicts
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

            // Audit log relationships
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

        public static void ConfigureBusinessConstraints(this ModelBuilder modelBuilder)
        {
            // Quest constraints
            modelBuilder.Entity<Quest>()
                .HasCheckConstraint("chk_positive_quest_reward", "token_reward >= 0");

            modelBuilder.Entity<Quest>()
                .HasCheckConstraint("chk_positive_target", "target_value > 0");

            modelBuilder.Entity<Quest>()
                .HasCheckConstraint("chk_quest_dates",
                    "(start_date IS NULL AND end_date IS NULL) OR " +
                    "(start_date IS NULL OR end_date IS NULL OR end_date > start_date)");

            // Achievement constraints
            modelBuilder.Entity<Achievement>()
                .HasCheckConstraint("chk_positive_achievement_rewards",
                    "token_reward >= 0 AND (xp_reward IS NULL OR xp_reward >= 0)");

            modelBuilder.Entity<Achievement>()
                .HasCheckConstraint("chk_positive_criteria", "criteria_value > 0");

            // Visual item constraints
            modelBuilder.Entity<VisualItem>()
                .HasCheckConstraint("chk_positive_visual_price", "visual_price >= 0");

            // User level constraints
            modelBuilder.Entity<Level>()
                .HasCheckConstraint("chk_positive_xp", "xp_required >= 0 AND xp_to_next >= 0");

            modelBuilder.Entity<Level>()
                .HasCheckConstraint("chk_positive_level", "level_number > 0");

            // User location preference constraints
            modelBuilder.Entity<UserLocationPreference>()
                .HasCheckConstraint("chk_valid_radius", "radius_km > 0 AND radius_km <= 1000");

            modelBuilder.Entity<UserLocationPreference>()
                .HasCheckConstraint("chk_valid_location_coordinates",
                    "(preferred_lat IS NULL AND preferred_lng IS NULL) OR " +
                    "(preferred_lat BETWEEN -90 AND 90 AND preferred_lng BETWEEN -180 AND 180)");

            // Notification constraints
            modelBuilder.Entity<Notification>()
                .HasCheckConstraint("chk_notification_expiry",
                    "expires_at IS NULL OR expires_at > created_at");

            // Email verification constraints
            modelBuilder.Entity<EmailVerification>()
                .HasCheckConstraint("chk_expiry_future", "expiry > created_at");

            // Chat constraints
            modelBuilder.Entity<Chat>()
                .HasCheckConstraint("chk_positive_escrow", "escrow_amount >= 0");

            modelBuilder.Entity<Chat>()
                .HasCheckConstraint("chk_completion_after_start",
                    "completion_date IS NULL OR completion_date >= start_time");

            // Streak constraints
            modelBuilder.Entity<UserStreak>()
                .HasCheckConstraint("chk_non_negative_streaks",
                    "current_streak >= 0 AND longest_streak >= 0 AND total_milestones_reached >= 0");

            // User achievement constraints
            modelBuilder.Entity<UserAchievement>()
                .HasCheckConstraint("chk_non_negative_progress", "progress >= 0");

            // Quest progress constraints
            modelBuilder.Entity<QuestProgress>()
                .HasCheckConstraint("chk_positive_goal", "goal_value > 0");

            modelBuilder.Entity<QuestProgress>()
                .HasCheckConstraint("chk_progress_within_goal", "progress_value <= goal_value");

            // User quest constraints
            modelBuilder.Entity<UserQuest>()
                .HasCheckConstraint("chk_progress_non_negative", "current_progress >= 0");

            modelBuilder.Entity<UserQuest>()
                .HasCheckConstraint("chk_completion_after_start",
                    "completed_at IS NULL OR completed_at >= started_at");

            // Leaderboard entry constraints
            modelBuilder.Entity<LeaderboardEntry>()
                .HasCheckConstraint("chk_positive_rank", "rank_position > 0");

            // User level constraints
            modelBuilder.Entity<UserLevel>()
                .HasCheckConstraint("chk_non_negative_xp",
                    "current_xp >= 0 AND total_xp >= 0 AND xp_to_next_level >= 0");

            modelBuilder.Entity<UserLevel>()
                .HasCheckConstraint("chk_total_xp_consistency", "total_xp >= current_xp");

            // AI constraints
            modelBuilder.Entity<AzureCvAnalysis>()
                .HasCheckConstraint("chk_valid_confidence",
                    "confidence_score IS NULL OR confidence_score BETWEEN 0 AND 1");

            modelBuilder.Entity<TensorflowPrediction>()
                .HasCheckConstraint("chk_valid_prediction_confidence",
                    "prediction_confidence IS NULL OR prediction_confidence BETWEEN 0 AND 1");

            modelBuilder.Entity<TensorflowPrediction>()
                .HasCheckConstraint("chk_positive_estimated_value",
                    "estimated_token_value IS NULL OR estimated_token_value >= 0");

            modelBuilder.Entity<AiProcessingPipeline>()
                .HasCheckConstraint("chk_valid_pipeline_confidence",
                    "confidence_level IS NULL OR confidence_level BETWEEN 0 AND 1");

            modelBuilder.Entity<AiProcessingPipeline>()
                .HasCheckConstraint("chk_pipeline_completion",
                    "(completed_at IS NULL) OR (completed_at >= started_at)");

            // AI training feedback constraints
            modelBuilder.Entity<AiTrainingFeedback>()
                .HasCheckConstraint("chk_valid_confidence_rating",
                    "confidence_rating IS NULL OR confidence_rating BETWEEN 1 AND 5");

            // Report constraints
            modelBuilder.Entity<UserReport>()
                .HasCheckConstraint("chk_not_self_report", "reporter_id != reported_user_id");

            modelBuilder.Entity<UserReport>()
                .HasCheckConstraint("chk_resolution_consistency",
                    "(report_status IN ('resolved', 'rejected') AND resolved_at IS NOT NULL) OR " +
                    "(report_status NOT IN ('resolved', 'rejected') AND resolved_at IS NULL)");

            // Admin report constraints
            modelBuilder.Entity<AdminReport>()
                .HasCheckConstraint("chk_positive_file_size",
                    "file_size_bytes IS NULL OR file_size_bytes >= 0");

            modelBuilder.Entity<AdminReport>()
                .HasCheckConstraint("chk_completion_consistency",
                    "(report_status = 'completed' AND completed_date IS NOT NULL) OR " +
                    "(report_status != 'completed')");
        }

        public static void SeedInitialData(this ModelBuilder modelBuilder)
        {
            // Seed default notification types
            modelBuilder.Entity<NotificationType>().HasData(
                new NotificationType { TypeId = 1, TypeName = "item_claimed", Description = "When your item is claimed", Icon = "shopping-cart", Color = "#28a745" },
                new NotificationType { TypeId = 2, TypeName = "transaction_completed", Description = "When a transaction is completed", Icon = "check-circle", Color = "#28a745" },
                new NotificationType { TypeId = 3, TypeName = "quest_completed", Description = "When you complete a quest", Icon = "trophy", Color = "#ffc107" },
                new NotificationType { TypeId = 4, TypeName = "achievement_earned", Description = "When you earn an achievement", Icon = "award", Color = "#ffc107" },
                new NotificationType { TypeId = 5, TypeName = "level_up", Description = "When you level up", Icon = "arrow-up", Color = "#17a2b8" },
                new NotificationType { TypeId = 6, TypeName = "message_received", Description = "When you receive a message", Icon = "message-circle", Color = "#007bff" },
                new NotificationType { TypeId = 7, TypeName = "item_expiring", Description = "When your item is about to expire", Icon = "clock", Color = "#dc3545" },
                new NotificationType { TypeId = 8, TypeName = "system_maintenance", Description = "System maintenance notifications", Icon = "settings", Color = "#6c757d" }
            );

            // Seed default levels
            modelBuilder.Entity<Level>().HasData(
                new Level { LevelId = 1, LevelNumber = 1, LevelName = "Novice Declutterer", XpRequired = 0, XpToNext = 100, TokenBonus = 0.00m },
                new Level { LevelId = 2, LevelNumber = 2, LevelName = "Casual Trader", XpRequired = 100, XpToNext = 150, TokenBonus = 1.00m },
                new Level { LevelId = 3, LevelNumber = 3, LevelName = "Active Member", XpRequired = 250, XpToNext = 200, TokenBonus = 2.50m },
                new Level { LevelId = 4, LevelNumber = 4, LevelName = "Experienced Seller", XpRequired = 450, XpToNext = 300, TokenBonus = 5.00m },
                new Level { LevelId = 5, LevelNumber = 5, LevelName = "Marketplace Pro", XpRequired = 750, XpToNext = 500, TokenBonus = 10.00m },
                new Level { LevelId = 6, LevelNumber = 6, LevelName = "Community Leader", XpRequired = 1250, XpToNext = 750, TokenBonus = 20.00m },
                new Level { LevelId = 7, LevelNumber = 7, LevelName = "Declutter Master", XpRequired = 2000, XpToNext = 1000, TokenBonus = 35.00m },
                new Level { LevelId = 8, LevelNumber = 8, LevelName = "Trading Legend", XpRequired = 3000, XpToNext = 1500, TokenBonus = 50.00m },
                new Level { LevelId = 9, LevelNumber = 9, LevelName = "Marketplace Guru", XpRequired = 4500, XpToNext = 2000, TokenBonus = 75.00m },
                new Level { LevelId = 10, LevelNumber = 10, LevelName = "Ultimate Declutterer", XpRequired = 6500, XpToNext = 0, TokenBonus = 100.00m }
            );

            // Seed default item conditions
            modelBuilder.Entity<ItemCondition>().HasData(
                new ItemCondition { ConditionId = 1, Name = "New", Description = "Brand new, never used", ConditionMultiplier = 1.00m, IsActive = true },
                new ItemCondition { ConditionId = 2, Name = "Like New", Description = "Excellent condition, barely used", ConditionMultiplier = 0.90m, IsActive = true },
                new ItemCondition { ConditionId = 3, Name = "Very Good", Description = "Minor signs of use, well maintained", ConditionMultiplier = 0.80m, IsActive = true },
                new ItemCondition { ConditionId = 4, Name = "Good", Description = "Used with normal wear, fully functional", ConditionMultiplier = 0.70m, IsActive = true },
                new ItemCondition { ConditionId = 5, Name = "Fair", Description = "Used with noticeable wear, still functional", ConditionMultiplier = 0.50m, IsActive = true },
                new ItemCondition { ConditionId = 6, Name = "Poor", Description = "Heavy wear, may need repair", ConditionMultiplier = 0.30m, IsActive = true }
            );

            // Seed default streak types
            modelBuilder.Entity<StreakType>().HasData(
                new StreakType { StreakTypeId = 1, Name = "Daily Login", Description = "Login to the platform daily", StreakUnit = "days", BaseRewards = 1.00m, MilestoneRewards = 5.00m, MilestoneInterval = 7, IsActive = true },
                new StreakType { StreakTypeId = 2, Name = "Item Posting", Description = "Post items regularly", StreakUnit = "items", BaseRewards = 2.00m, MilestoneRewards = 10.00m, MilestoneInterval = 5, IsActive = true },
                new StreakType { StreakTypeId = 3, Name = "Transaction Completion", Description = "Complete transactions successfully", StreakUnit = "transactions", BaseRewards = 5.00m, MilestoneRewards = 25.00m, MilestoneInterval = 3, IsActive = true },
                new StreakType { StreakTypeId = 4, Name = "Community Engagement", Description = "Participate in community discussions", StreakUnit = "days", BaseRewards = 1.50m, MilestoneRewards = 7.50m, MilestoneInterval = 5, IsActive = true }
            );
        }
    }
}