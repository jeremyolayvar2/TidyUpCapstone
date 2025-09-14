using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Data
{
    public static class NotificationSeeder
    {
        public static async Task SeedNotificationTypesAsync(ApplicationDbContext context)
        {
            if (!await context.NotificationTypes.AnyAsync())
            {
                var notificationTypes = new List<NotificationType>
                {
                    // Transaction Notifications
                    new NotificationType
                    {
                        TypeName = "Transaction Completed",
                        Description = "Notification when a transaction is successfully completed",
                        Icon = "✅",
                        Color = "#10B981",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Transaction Cancelled",
                        Description = "Notification when a transaction is cancelled",
                        Icon = "❌",
                        Color = "#EF4444",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Transaction Confirmation",
                        Description = "Notification requiring transaction confirmation",
                        Icon = "⏳",
                        Color = "#F59E0B",
                        DefaultEnabled = true,
                        IsActive = true
                    },

                    // Social Notifications
                    new NotificationType
                    {
                        TypeName = "Post Reaction",
                        Description = "Notification when someone reacts to your post",
                        Icon = "❤️",
                        Color = "#EF4444",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Post Comment",
                        Description = "Notification when someone comments on your post",
                        Icon = "💬",
                        Color = "#3B82F6",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Comment Reply",
                        Description = "Notification when someone replies to your comment",
                        Icon = "↩️",
                        Color = "#8B5CF6",
                        DefaultEnabled = true,
                        IsActive = true
                    },

                    // Communication Notifications
                    new NotificationType
                    {
                        TypeName = "New Message",
                        Description = "Notification for new chat messages",
                        Icon = "📨",
                        Color = "#06B6D4",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Interest Expressed",
                        Description = "Notification when someone shows interest in your item",
                        Icon = "👀",
                        Color = "#84CC16",
                        DefaultEnabled = true,
                        IsActive = true
                    },

                    // Gamification Notifications
                    new NotificationType
                    {
                        TypeName = "Quest Completed",
                        Description = "Notification when a quest is completed",
                        Icon = "🏆",
                        Color = "#F59E0B",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Achievement Unlocked",
                        Description = "Notification when an achievement is unlocked",
                        Icon = "🥇",
                        Color = "#EAB308",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Level Up",
                        Description = "Notification when user levels up",
                        Icon = "⬆️",
                        Color = "#22C55E",
                        DefaultEnabled = true,
                        IsActive = true
                    },
                    new NotificationType
                    {
                        TypeName = "Leaderboard Update",
                        Description = "Notification for leaderboard rank changes",
                        Icon = "📊",
                        Color = "#6366F1",
                        DefaultEnabled = true,
                        IsActive = true
                    },

                    // System Notifications
                    new NotificationType
                    {
                        TypeName = "System Update",
                        Description = "System maintenance and update notifications",
                        Icon = "⚙️",
                        Color = "#6B7280",
                        DefaultEnabled = false,
                        IsActive = true
                    }
                };

                await context.NotificationTypes.AddRangeAsync(notificationTypes);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedTestUsersAsync(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            // Check if test users already exist
            var existingUser = await userManager.FindByEmailAsync("testuser1@example.com");
            if (existingUser != null)
            {
                return; // Users already exist
            }

            var testUsers = new[]
            {
                new { Email = "testuser1@example.com", Username = "testuser1@example.com" },
                new { Email = "testuser2@example.com", Username = "testuser2@example.com" },
                new { Email = "testuser3@example.com", Username = "testuser3@example.com" }
            };

            foreach (var userData in testUsers)
            {
                var user = new AppUser
                {
                    UserName = userData.Username,
                    Email = userData.Email,
                    EmailConfirmed = true,
                    TokenBalance = 1000.00m,
                    DateCreated = DateTime.UtcNow,
                    Status = "active"
                };

                var result = await userManager.CreateAsync(user, "TestPass123!"); // Temporary password

                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create test user {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}