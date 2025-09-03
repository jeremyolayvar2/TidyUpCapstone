// Add this to your database initialization/seeding code
// This should go in your DbContext OnModelCreating method or a separate seeding class

using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Items;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Categories - matching your PricingController values
        if (!context.ItemCategories.Any())
        {
            var categories = new[]
            {
                new ItemCategory
                {
                    CategoryId = 1,
                    Name = "Books & Stationery",
                    Description = "Books, notebooks, pens, and office supplies",
                    IsActive = true,
                    SortOrder = 1
                },
                new ItemCategory
                {
                    CategoryId = 2,
                    Name = "Electronics & Gadgets",
                    Description = "Phones, computers, headphones, and electronic devices",
                    IsActive = true,
                    SortOrder = 2
                },
                new ItemCategory
                {
                    CategoryId = 3,
                    Name = "Toys & Games",
                    Description = "Board games, toys, puzzles, and gaming items",
                    IsActive = true,
                    SortOrder = 3
                },
                new ItemCategory
                {
                    CategoryId = 4,
                    Name = "Home & Kitchen",
                    Description = "Kitchen utensils, home decor, and household items",
                    IsActive = true,
                    SortOrder = 4
                },
                new ItemCategory
                {
                    CategoryId = 5,
                    Name = "Furniture",
                    Description = "Tables, chairs, beds, and other furniture",
                    IsActive = true,
                    SortOrder = 5
                },
                new ItemCategory
                {
                    CategoryId = 6,
                    Name = "Appliances",
                    Description = "Kitchen appliances, fans, and electrical appliances",
                    IsActive = true,
                    SortOrder = 6
                },
                new ItemCategory
                {
                    CategoryId = 7,
                    Name = "Health & Beauty",
                    Description = "Cosmetics, skincare, and health products",
                    IsActive = true,
                    SortOrder = 7
                },
                new ItemCategory
                {
                    CategoryId = 8,
                    Name = "Crafts & DIY",
                    Description = "Art supplies, craft materials, and DIY tools",
                    IsActive = true,
                    SortOrder = 8
                },
                new ItemCategory
                {
                    CategoryId = 9,
                    Name = "School & Office",
                    Description = "School supplies, office equipment, and educational materials",
                    IsActive = true,
                    SortOrder = 9
                },
                new ItemCategory
                {
                    CategoryId = 10,
                    Name = "Sentimental Items",
                    Description = "Personal items with emotional value",
                    IsActive = true,
                    SortOrder = 10
                },
                new ItemCategory
                {
                    CategoryId = 11,
                    Name = "Miscellaneous",
                    Description = "Other items not fitting specific categories",
                    IsActive = true,
                    SortOrder = 11
                }
            };

            context.ItemCategories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // Seed Conditions - matching your PricingController values
        if (!context.ItemConditions.Any())
        {
            var conditions = new[]
            {
                new ItemCondition
                {
                    ConditionId = 1,
                    Name = "Brand New",
                    Description = "Item is completely new and unused",
                    ConditionMultiplier = 1.25m, // +25% as per your PricingController
                    IsActive = true
                },
                new ItemCondition
                {
                    ConditionId = 2,
                    Name = "Like New",
                    Description = "Item is barely used and in excellent condition",
                    ConditionMultiplier = 1.15m, // +15% as per your PricingController
                    IsActive = true
                },
                new ItemCondition
                {
                    ConditionId = 3,
                    Name = "Gently Used",
                    Description = "Item shows minimal signs of use",
                    ConditionMultiplier = 1.05m, // +5% as per your PricingController
                    IsActive = true
                },
                new ItemCondition
                {
                    ConditionId = 4,
                    Name = "Visible Wear",
                    Description = "Item shows noticeable signs of use but still functional",
                    ConditionMultiplier = 0.90m, // -10% as per your PricingController
                    IsActive = true
                },
                new ItemCondition
                {
                    ConditionId = 5,
                    Name = "For Repair/Parts",
                    Description = "Item needs repair or is suitable for parts only",
                    ConditionMultiplier = 0.75m, // -25% as per your PricingController
                    IsActive = true
                }
            };

            context.ItemConditions.AddRange(conditions);
            await context.SaveChangesAsync();
        }
    }
}

