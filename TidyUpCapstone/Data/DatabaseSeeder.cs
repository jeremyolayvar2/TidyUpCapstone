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
                   
                    Name = "Books & Stationery",
                    Description = "Books, notebooks, pens, and office supplies",
                    IsActive = true,
                    SortOrder = 1
                },
                new ItemCategory
                {
                    
                    Name = "Electronics & Gadgets",
                    Description = "Phones, computers, headphones, and electronic devices",
                    IsActive = true,
                    SortOrder = 2
                },
                new ItemCategory
                {
                  
                    Name = "Toys & Games",
                    Description = "Board games, toys, puzzles, and gaming items",
                    IsActive = true,
                    SortOrder = 3
                },
                new ItemCategory
                {
                  
                    Name = "Home & Kitchen",
                    Description = "Kitchen utensils, home decor, and household items",
                    IsActive = true,
                    SortOrder = 4
                },
                new ItemCategory
                {
                  
                    Name = "Furniture",
                    Description = "Tables, chairs, beds, and other furniture",
                    IsActive = true,
                    SortOrder = 5
                },
                new ItemCategory
                {
                 
                    Name = "Appliances",
                    Description = "Kitchen appliances, fans, and electrical appliances",
                    IsActive = true,
                    SortOrder = 6
                },
                new ItemCategory
                {
           
                    Name = "Health & Beauty",
                    Description = "Cosmetics, skincare, and health products",
                    IsActive = true,
                    SortOrder = 7
                },
                new ItemCategory
                {
                   
                    Name = "Crafts & DIY",
                    Description = "Art supplies, craft materials, and DIY tools",
                    IsActive = true,
                    SortOrder = 8
                },
                new ItemCategory
                {
             
                    Name = "School & Office",
                    Description = "School supplies, office equipment, and educational materials",
                    IsActive = true,
                    SortOrder = 9
                },
                new ItemCategory
                {
                    
                    Name = "Sentimental Items",
                    Description = "Personal items with emotional value",
                    IsActive = true,
                    SortOrder = 10
                },
                new ItemCategory
                {
                    
                    Name = "Miscellaneous",
                    Description = "Other items not fitting specific categories",
                    IsActive = true,
                    SortOrder = 11
                },
                new ItemCategory
                {
                    
                    Name = "Clothing",
                    Description = "Apparel, shoes, accessories, and fashion items",
                    IsActive = true,
                    SortOrder = 12
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
                    
                    Name = "Excellent",
                    Description = "Item is in perfect or near-perfect condition",
                    ConditionMultiplier = 1.25m, // +25% (same as Brand New)
                    IsActive = true
                },
                new ItemCondition
                {
                   
                    Name = "Very Good",
                    Description = "Item shows minimal signs of use but functions perfectly",
                    ConditionMultiplier = 1.15m, // +15%
                    IsActive = true
                },
                new ItemCondition
                {
                    
                    Name = "Good",
                    Description = "Item shows minimal signs of use but functions perfectly",
                    ConditionMultiplier = 1.05m, // +5% (same as Gently Used)
                    IsActive = true
                },
                new ItemCondition
                {
                    
                    Name = "Fair",
                    Description = "Item shows noticeable signs of use but still functional",
                    ConditionMultiplier = 0.90m, // -10% (same as Visible Wear)
                    IsActive = true
                },
                new ItemCondition
                {
               
                    Name = "Poor",
                    Description = "Item has significant wear but still usable",
                    ConditionMultiplier = 0.75m, // -25%
                    IsActive = true
                }
            };

            context.ItemConditions.AddRange(conditions);
            await context.SaveChangesAsync();
        }

       
    }
}