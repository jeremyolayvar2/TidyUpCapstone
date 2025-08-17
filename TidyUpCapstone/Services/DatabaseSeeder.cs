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
                    TokenBalance = 100.00m, // Proper token balance
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
                    TokenBalance = 150.00m, // Proper token balance
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

            // Seed sample items
            await SeedSampleItemsAsync(context, testUser1!.Id, testUser2!.Id);

            // Seed sample transaction for chat
            await SeedSampleTransactionAsync(context, testUser1!.Id, testUser2!.Id);

            await context.SaveChangesAsync();
        }

        private static async Task SeedBasicDataAsync(ApplicationDbContext context)
        {
            // Check what properties actually exist by looking at the first category
            var existingCategory = await context.ItemCategories.FirstOrDefaultAsync();

            // Seed Categories - using property names that should exist based on your DTO
            if (!await context.ItemCategories.AnyAsync())
            {
                // Since I can't see the exact model, I'll try common property names
                // You may need to adjust these property names to match your actual ItemCategory model
                try
                {
                    var categories = new List<ItemCategory>();

                    // Try to create categories with the most likely property names
                    for (int i = 1; i <= 5; i++)
                    {
                        var category = new ItemCategory();

                        // Try setting properties that commonly exist
                        var categoryType = typeof(ItemCategory);
                        var nameProperty = categoryType.GetProperty("Name") ??
                                         categoryType.GetProperty("CategoryName") ??
                                         categoryType.GetProperty("Title");

                        var descProperty = categoryType.GetProperty("Description");

                        if (nameProperty != null)
                        {
                            string[] names = { "Electronics", "Furniture", "Books", "Clothing", "Other" };
                            nameProperty.SetValue(category, names[i - 1]);
                        }

                        if (descProperty != null)
                        {
                            string[] descriptions = {
                                "Electronic devices and gadgets",
                                "Home and office furniture",
                                "Books and educational materials",
                                "Clothes and accessories",
                                "Miscellaneous items"
                            };
                            descProperty.SetValue(category, descriptions[i - 1]);
                        }

                        categories.Add(category);
                    }

                    context.ItemCategories.AddRange(categories);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // If seeding fails, we'll handle it gracefully
                    Console.WriteLine($"Warning: Could not seed categories: {ex.Message}");
                }
            }

            // Similar approach for Conditions
            if (!await context.ItemConditions.AnyAsync())
            {
                try
                {
                    var conditions = new List<ItemCondition>();

                    for (int i = 1; i <= 5; i++)
                    {
                        var condition = new ItemCondition();
                        var conditionType = typeof(ItemCondition);
                        var nameProperty = conditionType.GetProperty("Name") ??
                                         conditionType.GetProperty("ConditionName") ??
                                         conditionType.GetProperty("Title");

                        var descProperty = conditionType.GetProperty("Description");

                        if (nameProperty != null)
                        {
                            string[] names = { "New", "Like New", "Good", "Fair", "Poor" };
                            nameProperty.SetValue(condition, names[i - 1]);
                        }

                        if (descProperty != null)
                        {
                            string[] descriptions = {
                                "Brand new, unused",
                                "Barely used, excellent condition",
                                "Used but in good condition",
                                "Shows signs of wear but functional",
                                "Heavily used, may need repairs"
                            };
                            descProperty.SetValue(condition, descriptions[i - 1]);
                        }

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

            // Similar approach for Locations
            if (!await context.ItemLocations.AnyAsync())
            {
                try
                {
                    var locations = new List<ItemLocation>();

                    for (int i = 1; i <= 5; i++)
                    {
                        var location = new ItemLocation();
                        var locationType = typeof(ItemLocation);
                        var nameProperty = locationType.GetProperty("Name") ??
                                         locationType.GetProperty("LocationName") ??
                                         locationType.GetProperty("Title");

                        var descProperty = locationType.GetProperty("Description");

                        if (nameProperty != null)
                        {
                            string[] names = { "Metro Manila", "Quezon City", "Manila", "Makati", "Other" };
                            nameProperty.SetValue(location, names[i - 1]);
                        }

                        if (descProperty != null)
                        {
                            string[] descriptions = {
                                "National Capital Region",
                                "Quezon City area",
                                "Manila City area",
                                "Makati City area",
                                "Other locations"
                            };
                            descProperty.SetValue(location, descriptions[i - 1]);
                        }

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

                    // If we don't have reference data, create minimal entries
                    if (category == null || condition == null || location == null)
                    {
                        Console.WriteLine("Warning: Missing reference data for items. Creating minimal entries...");
                        return;
                    }

                    var items = new[]
                    {
                        new Item
                        {
                            UserId = user1Id,
                            ItemTitle = "Sample Bookshelf",
                            Description = "A nice wooden bookshelf for testing chat feature",
                            CategoryId = category.CategoryId,
                            ConditionId = condition.ConditionId,
                            LocationId = location.LocationId,
                            AdjustedTokenPrice = 50.00m,
                            FinalTokenPrice = 45.00m,
                            Status = ItemStatus.Available,
                            DatePosted = DateTime.UtcNow,
                            ViewCount = 0
                        },
                        new Item
                        {
                            UserId = user2Id,
                            ItemTitle = "Test Laptop",
                            Description = "A laptop for testing purposes",
                            CategoryId = category.CategoryId,
                            ConditionId = condition.ConditionId,
                            LocationId = location.LocationId,
                            AdjustedTokenPrice = 200.00m,
                            FinalTokenPrice = 180.00m,
                            Status = ItemStatus.Available,
                            DatePosted = DateTime.UtcNow,
                            ViewCount = 0
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
                // Check if sample transaction already exists
                var existingTransaction = await context.Transactions.FirstOrDefaultAsync();
                if (existingTransaction != null) return;

                var sampleItem = await context.Items.FirstOrDefaultAsync();
                if (sampleItem == null)
                {
                    Console.WriteLine("Warning: No items available for creating sample transaction");
                    return;
                }

                var transaction = new Transaction
                {
                    BuyerId = user1Id,
                    SellerId = user2Id,
                    ItemId = sampleItem.ItemId,
                    TokenAmount = 45.00m, // Proper positive token amount
                    TransactionStatus = TransactionStatus.Pending,
                    DeliveryMethod = DeliveryMethod.Pickup,
                    CreatedAt = DateTime.UtcNow
                };

                context.Transactions.Add(transaction);
                await context.SaveChangesAsync();

                // Create chat for this transaction
                var chat = new Chat
                {
                    TransactionId = transaction.TransactionId,
                    StartTime = DateTime.UtcNow,
                    LastMessageTime = DateTime.UtcNow,
                    EscrowAmount = 45.00m,
                    EscrowStatus = EscrowStatus.Pending
                };

                context.Chats.Add(chat);
                await context.SaveChangesAsync();

                // Add some sample messages
                var messages = new[]
                {
                    new ChatMessage
                    {
                        ChatId = chat.ChatId,
                        SenderId = user2Id,
                        Message = "Hi! I saw you're interested in the Bookshelf I posted. It's still available!",
                        MessageType = MessageType.Text,
                        SentAt = DateTime.UtcNow.AddMinutes(-10)
                    },
                    new ChatMessage
                    {
                        ChatId = chat.ChatId,
                        SenderId = user1Id,
                        Message = "Hello! Yes, I'd love to claim it.",
                        MessageType = MessageType.Text,
                        SentAt = DateTime.UtcNow.AddMinutes(-8)
                    },
                    new ChatMessage
                    {
                        ChatId = chat.ChatId,
                        SenderId = user1Id,
                        Message = "Can we meet this Saturday afternoon around 3PM at SM Sta. Mesa?",
                        MessageType = MessageType.Text,
                        SentAt = DateTime.UtcNow.AddMinutes(-5)
                    },
                    new ChatMessage
                    {
                        ChatId = chat.ChatId,
                        SenderId = user2Id,
                        Message = "Sure! Saturday at 3PM works for me. Let's meet at the main entrance near Starbucks.",
                        MessageType = MessageType.Text,
                        SentAt = DateTime.UtcNow.AddMinutes(-2)
                    }
                };

                context.ChatMessages.AddRange(messages);
                await context.SaveChangesAsync();
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