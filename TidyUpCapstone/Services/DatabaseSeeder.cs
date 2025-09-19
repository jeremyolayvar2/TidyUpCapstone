using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.Items;
using TidyUpCapstone.Models.Entities.Transactions;

namespace TidyUpCapstone.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedTestUsersAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Categories, Conditions, and Locations first
            await SeedBasicDataAsync(context);

            // Check if test users already exist
            var testUser1 = await userManager.FindByEmailAsync("testuser1@example.com");
            var testUser2 = await userManager.FindByEmailAsync("testuser2@example.com");

            if (testUser1 == null)
            {
                testUser1 = new AppUser
                {
                    UserName = "TestUser1",
                    Email = "testuser1@example.com",
                    EmailConfirmed = true,
                    TokenBalance = 1000.00m,
                    DateCreated = DateTime.UtcNow,
                    Status = "active"
                };

                var result1 = await userManager.CreateAsync(testUser1, "TestPassword123!");
                if (!result1.Succeeded)
                {
                    throw new Exception($"Failed to create TestUser1: {string.Join(", ", result1.Errors.Select(e => e.Description))}");
                }
            }

            if (testUser2 == null)
            {
                testUser2 = new AppUser
                {
                    UserName = "TestUser2",
                    Email = "testuser2@example.com",
                    EmailConfirmed = true,
                    TokenBalance = 1500.00m,
                    DateCreated = DateTime.UtcNow,
                    Status = "active"
                };

                var result2 = await userManager.CreateAsync(testUser2, "TestPassword123!");
                if (!result2.Succeeded)
                {
                    throw new Exception($"Failed to create TestUser2: {string.Join(", ", result2.Errors.Select(e => e.Description))}");
                }
            }

            // Refresh users from database to get their IDs
            testUser1 = await userManager.FindByEmailAsync("testuser1@example.com");
            testUser2 = await userManager.FindByEmailAsync("testuser2@example.com");

            // Seed sample items for both users
            await SeedSampleItemsAsync(context, testUser1!.Id, testUser2!.Id);

            // Seed sample transaction and chat between the two users
            await SeedSampleTransactionAsync(context, testUser1!.Id, testUser2!.Id);

            await context.SaveChangesAsync();
        }

        private static async Task SeedBasicDataAsync(ApplicationDbContext context)
        {
            // Seed Categories
            if (!await context.ItemCategories.AnyAsync())
            {
                try
                {
                    var categories = new List<ItemCategory>();

                    var categoryData = new[]
                    {
                        new { Name = "Electronics", Description = "Electronic devices and gadgets" },
                        new { Name = "Furniture", Description = "Home and office furniture" },
                        new { Name = "Books", Description = "Books and educational materials" },
                        new { Name = "Clothing", Description = "Clothes and accessories" },
                        new { Name = "Other", Description = "Miscellaneous items" }
                    };

                    foreach (var data in categoryData)
                    {
                        var category = new ItemCategory();
                        var categoryType = typeof(ItemCategory);

                        var nameProperty = categoryType.GetProperty("Name") ??
                                         categoryType.GetProperty("CategoryName") ??
                                         categoryType.GetProperty("Title");
                        var descProperty = categoryType.GetProperty("Description");

                        nameProperty?.SetValue(category, data.Name);
                        descProperty?.SetValue(category, data.Description);

                        categories.Add(category);
                    }

                    context.ItemCategories.AddRange(categories);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not seed categories: {ex.Message}");
                }
            }

            // Seed Conditions
            if (!await context.ItemConditions.AnyAsync())
            {
                try
                {
                    var conditionData = new[]
                    {
                        new { Name = "New", Description = "Brand new, unused" },
                        new { Name = "Like New", Description = "Barely used, excellent condition" },
                        new { Name = "Good", Description = "Used but in good condition" },
                        new { Name = "Fair", Description = "Shows signs of wear but functional" },
                        new { Name = "Poor", Description = "Heavily used, may need repairs" }
                    };

                    var conditions = new List<ItemCondition>();

                    foreach (var data in conditionData)
                    {
                        var condition = new ItemCondition();
                        var conditionType = typeof(ItemCondition);

                        var nameProperty = conditionType.GetProperty("Name") ??
                                         conditionType.GetProperty("ConditionName") ??
                                         conditionType.GetProperty("Title");
                        var descProperty = conditionType.GetProperty("Description");

                        nameProperty?.SetValue(condition, data.Name);
                        descProperty?.SetValue(condition, data.Description);

                        conditions.Add(condition);
                    }

                    context.ItemConditions.AddRange(conditions);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not seed conditions: {ex.Message}");
                }
            }

            // Seed Locations
            if (!await context.ItemLocations.AnyAsync())
            {
                try
                {
                    var locationData = new[]
                    {
                        new { Name = "Metro Manila", Description = "National Capital Region" },
                        new { Name = "Quezon City", Description = "Quezon City area" },
                        new { Name = "Manila", Description = "Manila City area" },
                        new { Name = "Makati", Description = "Makati City area" },
                        new { Name = "Other", Description = "Other locations" }
                    };

                    var locations = new List<ItemLocation>();

                    foreach (var data in locationData)
                    {
                        var location = new ItemLocation();
                        var locationType = typeof(ItemLocation);

                        var nameProperty = locationType.GetProperty("Name") ??
                                         locationType.GetProperty("LocationName") ??
                                         locationType.GetProperty("Title");
                        var descProperty = locationType.GetProperty("Description");

                        nameProperty?.SetValue(location, data.Name);
                        descProperty?.SetValue(location, data.Description);

                        locations.Add(location);
                    }

                    context.ItemLocations.AddRange(locations);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not seed locations: {ex.Message}");
                }
            }
        }

        private static async Task SeedSampleItemsAsync(ApplicationDbContext context, int user1Id, int user2Id)
        {
            if (!await context.Items.AnyAsync())
            {
                try
                {
                    // Get the first available category, condition, and location
                    var category = await context.ItemCategories.FirstOrDefaultAsync();
                    var condition = await context.ItemConditions.FirstOrDefaultAsync();
                    var location = await context.ItemLocations.FirstOrDefaultAsync();

                    if (category == null || condition == null || location == null)
                    {
                        Console.WriteLine("Warning: Missing reference data for items.");
                        return;
                    }

                    // Create items for both users so they have something to chat about
                    var items = new[]
                    {
                        new Item
                        {
                            UserId = user1Id,
                            ItemTitle = "Wooden Bookshelf",
                            Description = "Beautiful oak bookshelf, perfect condition. Great for organizing books and displaying items.",
                            CategoryId = category.CategoryId,
                            ConditionId = condition.ConditionId,
                            LocationId = location.LocationId,
                            AdjustedTokenPrice = 50.00m,
                            FinalTokenPrice = 45.00m,
                            Status = ItemStatus.Available,
                            DatePosted = DateTime.UtcNow.AddDays(-2),
                            ViewCount = 5
                        },
                        new Item
                        {
                            UserId = user2Id,
                            ItemTitle = "Gaming Laptop",
                            Description = "High-performance gaming laptop. Perfect for work and gaming. Includes charger and mouse.",
                            CategoryId = category.CategoryId,
                            ConditionId = condition.ConditionId,
                            LocationId = location.LocationId,
                            AdjustedTokenPrice = 200.00m,
                            FinalTokenPrice = 180.00m,
                            Status = ItemStatus.Available,
                            DatePosted = DateTime.UtcNow.AddDays(-1),
                            ViewCount = 12
                        },
                        new Item
                        {
                            UserId = user1Id,
                            ItemTitle = "Office Chair",
                            Description = "Ergonomic office chair with lumbar support. Black leather, adjustable height.",
                            CategoryId = category.CategoryId,
                            ConditionId = condition.ConditionId,
                            LocationId = location.LocationId,
                            AdjustedTokenPrice = 75.00m,
                            FinalTokenPrice = 70.00m,
                            Status = ItemStatus.Available,
                            DatePosted = DateTime.UtcNow.AddHours(-6),
                            ViewCount = 3
                        }
                    };

                    context.Items.AddRange(items);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not seed items: {ex.Message}");
                }
            }
        }

        private static async Task SeedSampleTransactionAsync(ApplicationDbContext context, int user1Id, int user2Id)
        {
            try
            {
                // Don't create any transactions - let the auto-escrow handle it
                Console.WriteLine("Transaction seeding skipped - will be handled by auto-escrow");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not seed sample transaction: {ex.Message}");
            }
        }

        public static async Task<List<AppUser>> GetTestUsersAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var users = new List<AppUser>();

            var testUser1 = await userManager.FindByEmailAsync("testuser1@example.com");
            var testUser2 = await userManager.FindByEmailAsync("testuser2@example.com");

            if (testUser1 != null) users.Add(testUser1);
            if (testUser2 != null) users.Add(testUser2);

            return users;
        }
    }
}