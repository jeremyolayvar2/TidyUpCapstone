using TidyUpCapstone.Models.Entities.Gamification;

namespace TidyUpCapstone.Models.Templates
{
    public static class KonMariAchievementTemplates
    {
        // Main method to get all KonMari achievements
        public static List<AchievementTemplate> GetAllKonMariAchievements()
        {
            var allAchievements = new List<AchievementTemplate>();

            allAchievements.AddRange(GetCategoryMasteryAchievements());
            allAchievements.AddRange(GetProgressiveAchievements());
            allAchievements.AddRange(GetMethodologyAchievements());
            allAchievements.AddRange(GetCommunityAchievements());
            allAchievements.AddRange(GetSpecialAchievements());
            allAchievements.AddRange(GetCrossCategoryAchievements());

            return allAchievements;
        }

        // Helper method to get all valid categories
        public static List<string> GetValidCategories()
        {
            return new List<string>
            {
                "books_stationery",
                "clothing_accessories",
                "electronics_gadgets",
                "toys_games",
                "home_kitchen",
                "furniture",
                "appliances",
                "health_beauty",
                "crafts_diy",
                "school_office",
                "sentimental"
            };
        }

        // Category Mastery Achievements (11 achievements)
        public static List<AchievementTemplate> GetCategoryMasteryAchievements()
        {
            var achievements = new List<AchievementTemplate>();

            var categories = new[]
            {
                new { Category = "books_stationery", Title = "Literary Curator", Items = 20, Tokens = 50m, Xp = 100 },
                new { Category = "clothing_accessories", Title = "Style Curator", Items = 25, Tokens = 55m, Xp = 110 },
                new { Category = "electronics_gadgets", Title = "Tech Minimalist", Items = 15, Tokens = 60m, Xp = 120 },
                new { Category = "toys_games", Title = "Joy Keeper", Items = 20, Tokens = 45m, Xp = 90 },
                new { Category = "home_kitchen", Title = "Domestic Zen Master", Items = 30, Tokens = 65m, Xp = 130 },
                new { Category = "furniture", Title = "Space Designer", Items = 10, Tokens = 70m, Xp = 140 },
                new { Category = "appliances", Title = "Functional Living Master", Items = 8, Tokens = 60m, Xp = 120 },
                new { Category = "health_beauty", Title = "Wellness Curator", Items = 25, Tokens = 50m, Xp = 100 },
                new { Category = "crafts_diy", Title = "Creative Organizer", Items = 20, Tokens = 45m, Xp = 90 },
                new { Category = "school_office", Title = "Productivity Pro", Items = 15, Tokens = 40m, Xp = 80 },
                new { Category = "sentimental", Title = "Memory Keeper", Items = 10, Tokens = 100m, Xp = 200 } // Highest reward
            };

            foreach (var category in categories)
            {
                achievements.Add(new AchievementTemplate
                {
                    Name = category.Title,
                    Description = $"Master the art of {category.Category.Replace('_', ' ')} organization by completing category quests and listing {category.Items} items",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = $"category_mastery_{category.Category}",
                    CriteriaValue = 1, // Will be validated by complex logic
                    TokenReward = category.Tokens,
                    XpReward = category.Xp,
                    Rarity = category.Category == "sentimental" ? AchievementRarity.Platinum : AchievementRarity.Gold,
                    IsSecret = false,
                    IsActive = true
                });
            }

            return achievements;
        }

        // Progressive Activity Achievements (20 achievements)
        // Progressive Activity Achievements - REFACTORED for quest-focused gameplay
        public static List<AchievementTemplate> GetProgressiveAchievements()
        {
            var achievements = new List<AchievementTemplate>();

            // Quest Completion Progression (Primary focus)
            var questTiers = new[]
            {
                new { Level = "Bronze", Quests = 3, Tokens = 25m, Xp = 50, Rarity = AchievementRarity.Bronze },
                new { Level = "Silver", Quests = 7, Tokens = 50m, Xp = 100, Rarity = AchievementRarity.Silver },
                new { Level = "Gold", Quests = 15, Tokens = 100m, Xp = 200, Rarity = AchievementRarity.Gold },
                new { Level = "Platinum", Quests = 30, Tokens = 200m, Xp = 400, Rarity = AchievementRarity.Platinum }
            };

            foreach (var tier in questTiers)
            {
                achievements.Add(new AchievementTemplate
                {
                    Name = $"Quest Champion {tier.Level}",
                    Description = $"Complete {tier.Quests} quests",
                    Category = AchievementCategory.Exploration,
                    CriteriaType = "total_quests_completed",
                    CriteriaValue = tier.Quests,
                    TokenReward = tier.Tokens,
                    XpReward = tier.Xp,
                    Rarity = tier.Rarity,
                    IsSecret = false,
                    IsActive = true
                });
            }

            // Daily Quest Focus
            var dailyTiers = new[]
            {
                new { Level = "Bronze", Quests = 5, Tokens = 20m, Xp = 40, Rarity = AchievementRarity.Bronze },
                new { Level = "Silver", Quests = 15, Tokens = 40m, Xp = 80, Rarity = AchievementRarity.Silver },
                new { Level = "Gold", Quests = 30, Tokens = 80m, Xp = 160, Rarity = AchievementRarity.Gold },
                new { Level = "Platinum", Quests = 60, Tokens = 160m, Xp = 320, Rarity = AchievementRarity.Platinum }
            };

            foreach (var tier in dailyTiers)
            {
                achievements.Add(new AchievementTemplate
                {
                    Name = $"Daily Warrior {tier.Level}",
                    Description = $"Complete {tier.Quests} daily quests",
                    Category = AchievementCategory.Exploration,
                    CriteriaType = "daily_quests_completed",
                    CriteriaValue = tier.Quests,
                    TokenReward = tier.Tokens,
                    XpReward = tier.Xp,
                    Rarity = tier.Rarity,
                    IsSecret = false,
                    IsActive = true
                });
            }

            // Weekly Quest Focus
            var weeklyTiers = new[]
            {
                new { Level = "Bronze", Quests = 2, Tokens = 30m, Xp = 60, Rarity = AchievementRarity.Bronze },
                new { Level = "Silver", Quests = 5, Tokens = 60m, Xp = 120, Rarity = AchievementRarity.Silver },
                new { Level = "Gold", Quests = 10, Tokens = 120m, Xp = 240, Rarity = AchievementRarity.Gold },
                new { Level = "Platinum", Quests = 20, Tokens = 240m, Xp = 480, Rarity = AchievementRarity.Platinum }
            };

            foreach (var tier in weeklyTiers)
            {
                achievements.Add(new AchievementTemplate
                {
                    Name = $"Weekly Champion {tier.Level}",
                    Description = $"Complete {tier.Quests} weekly quests",
                    Category = AchievementCategory.Exploration,
                    CriteriaType = "weekly_quests_completed",
                    CriteriaValue = tier.Quests,
                    TokenReward = tier.Tokens,
                    XpReward = tier.Xp,
                    Rarity = tier.Rarity,
                    IsSecret = false,
                    IsActive = true
                });
            }

            // Check-in Streak Progression
            var checkinTiers = new[]
            {
                new { Level = "Bronze", Days = 3, Tokens = 15m, Xp = 30, Rarity = AchievementRarity.Bronze },
                new { Level = "Silver", Days = 7, Tokens = 35m, Xp = 70, Rarity = AchievementRarity.Silver },
                new { Level = "Gold", Days = 14, Tokens = 70m, Xp = 140, Rarity = AchievementRarity.Gold },
                new { Level = "Platinum", Days = 30, Tokens = 150m, Xp = 300, Rarity = AchievementRarity.Platinum }
            };

            foreach (var tier in checkinTiers)
            {
                achievements.Add(new AchievementTemplate
                {
                    Name = $"Dedication Streak {tier.Level}",
                    Description = $"Check in for {tier.Days} consecutive days",
                    Category = AchievementCategory.Special,
                    CriteriaType = "daily_checkin_streak",
                    CriteriaValue = tier.Days,
                    TokenReward = tier.Tokens,
                    XpReward = tier.Xp,
                    Rarity = tier.Rarity,
                    IsSecret = false,
                    IsActive = true
                });
            }

            // Simple Item Listing (aligned with quest targets)
            var itemTiers = new[]
            {
                new { Level = "Bronze", Items = 10, Tokens = 15m, Xp = 30, Rarity = AchievementRarity.Bronze },
                new { Level = "Silver", Items = 25, Tokens = 35m, Xp = 70, Rarity = AchievementRarity.Silver },
                new { Level = "Gold", Items = 50, Tokens = 75m, Xp = 150, Rarity = AchievementRarity.Gold },
                new { Level = "Platinum", Items = 100, Tokens = 150m, Xp = 300, Rarity = AchievementRarity.Platinum }
            };

            foreach (var tier in itemTiers)
            {
                achievements.Add(new AchievementTemplate
                {
                    Name = $"Item Organizer {tier.Level}",
                    Description = $"List {tier.Items} items through quest completion",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "total_items_listed",
                    CriteriaValue = tier.Items,
                    TokenReward = tier.Tokens,
                    XpReward = tier.Xp,
                    Rarity = tier.Rarity,
                    IsSecret = false,
                    IsActive = true
                });
            }

            return achievements;
        }

        // KonMari Methodology Achievements (12 achievements)
        // Expanded Methodology Achievements (10 achievements)
        public static List<AchievementTemplate> GetMethodologyAchievements()
        {
            return new List<AchievementTemplate>
        {
            new AchievementTemplate
            {
                Name = "First Post",
                Description = "Create your first community post",
                Category = AchievementCategory.Community,
                CriteriaType = "posts_created",
                CriteriaValue = 1,
                TokenReward = 15m,
                XpReward = 30,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Social Butterfly",
                Description = "Create 5 community posts",
                Category = AchievementCategory.Community,
                CriteriaType = "posts_created",
                CriteriaValue = 5,
                TokenReward = 35m,
                XpReward = 70,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Content Creator",
                Description = "Create 10 community posts",
                Category = AchievementCategory.Community,
                CriteriaType = "posts_created",
                CriteriaValue = 10,
                TokenReward = 60m,
                XpReward = 120,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Helpful Friend",
                Description = "Leave 10 helpful comments",
                Category = AchievementCategory.Community,
                CriteriaType = "helpful_comments",
                CriteriaValue = 10,
                TokenReward = 25m,
                XpReward = 50,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Support Network",
                Description = "Leave 25 helpful comments",
                Category = AchievementCategory.Community,
                CriteriaType = "helpful_comments",
                CriteriaValue = 25,
                TokenReward = 50m,
                XpReward = 100,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Wise Advisor",
                Description = "Leave 50 helpful comments",
                Category = AchievementCategory.Community,
                CriteriaType = "helpful_comments",
                CriteriaValue = 50,
                TokenReward = 85m,
                XpReward = 170,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Habit Master",
                Description = "Check in for 14 consecutive days",
                Category = AchievementCategory.Special,
                CriteriaType = "daily_checkin_streak",
                CriteriaValue = 14,
                TokenReward = 70m,
                XpReward = 140,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Dedication Master",
                Description = "Check in for 30 consecutive days",
                Category = AchievementCategory.Special,
                CriteriaType = "daily_checkin_streak",
                CriteriaValue = 30,
                TokenReward = 120m,
                XpReward = 240,
                Rarity = AchievementRarity.Platinum,
                IsSecret = false,
                IsActive = true
            }
        };
        }

        // Expanded Community Achievements (9 achievements)
        public static List<AchievementTemplate> GetCommunityAchievements()
        {
            return new List<AchievementTemplate>
        {
            new AchievementTemplate
            {
                Name = "First Reaction",
                Description = "Give your first positive reaction",
                Category = AchievementCategory.Community,
                CriteriaType = "positive_reactions_given",
                CriteriaValue = 1,
                TokenReward = 10m,
                XpReward = 20,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Reaction Giver",
                Description = "Give 20 positive reactions to others' posts",
                Category = AchievementCategory.Community,
                CriteriaType = "positive_reactions_given",
                CriteriaValue = 20,
                TokenReward = 30m,
                XpReward = 60,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Super Supporter",
                Description = "Give 50 positive reactions to others' posts",
                Category = AchievementCategory.Community,
                CriteriaType = "positive_reactions_given",
                CriteriaValue = 50,
                TokenReward = 50m,
                XpReward = 100,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Positivity Spreader",
                Description = "Give 100 positive reactions to others' posts",
                Category = AchievementCategory.Community,
                CriteriaType = "positive_reactions_given",
                CriteriaValue = 100,
                TokenReward = 80m,
                XpReward = 160,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Community Newcomer",
                Description = "Make 10 total community actions (posts + comments + reactions)",
                Category = AchievementCategory.Community,
                CriteriaType = "community_engagement_total",
                CriteriaValue = 10,
                TokenReward = 25m,
                XpReward = 50,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Active Member",
                Description = "Make 25 total community actions (posts + comments + reactions)",
                Category = AchievementCategory.Community,
                CriteriaType = "community_engagement_total",
                CriteriaValue = 25,
                TokenReward = 45m,
                XpReward = 90,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Community Star",
                Description = "Make 50 total community actions (posts + comments + reactions)",
                Category = AchievementCategory.Community,
                CriteriaType = "community_engagement_total",
                CriteriaValue = 50,
                TokenReward = 75m,
                XpReward = 150,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Community Leader",
                Description = "Make 100 total community actions (posts + comments + reactions)",
                Category = AchievementCategory.Community,
                CriteriaType = "community_engagement_total",
                CriteriaValue = 100,
                TokenReward = 120m,
                XpReward = 240,
                Rarity = AchievementRarity.Platinum,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Conversation Starter",
                Description = "Start 5 discussions by creating posts with multiple responses",
                Category = AchievementCategory.Community,
                CriteriaType = "engaging_posts_created",
                CriteriaValue = 5,
                TokenReward = 40m,
                XpReward = 80,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            }
        };
        }

        // Expanded Activity Achievements (8 achievements) 
        public static List<AchievementTemplate> GetCrossCategoryAchievements()
        {
            return new List<AchievementTemplate>
        {
            new AchievementTemplate
            {
                Name = "Category Explorer",
                Description = "Complete quests from 2 different categories",
                Category = AchievementCategory.Exploration,
                CriteriaType = "quest_categories_completed",
                CriteriaValue = 2,
                TokenReward = 20m,
                XpReward = 40,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Multi-Tasker",
                Description = "Complete quests from 3 different categories",
                Category = AchievementCategory.Exploration,
                CriteriaType = "quest_categories_completed",
                CriteriaValue = 3,
                TokenReward = 35m,
                XpReward = 70,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Well-Rounded",
                Description = "Complete quests from 5 different categories",
                Category = AchievementCategory.Exploration,
                CriteriaType = "quest_categories_completed",
                CriteriaValue = 5,
                TokenReward = 60m,
                XpReward = 120,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Quest Master",
                Description = "Complete quests from all 7 categories",
                Category = AchievementCategory.Exploration,
                CriteriaType = "quest_categories_completed",
                CriteriaValue = 7,
                TokenReward = 100m,
                XpReward = 200,
                Rarity = AchievementRarity.Platinum,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Cross-Category Organizer",
                Description = "List 10 items through quest completion",
                Category = AchievementCategory.Decluttering,
                CriteriaType = "total_items_listed",
                CriteriaValue = 10,
                TokenReward = 25m,
                XpReward = 50,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Multi-Category Pro",
                Description = "List 25 items through quest completion",
                Category = AchievementCategory.Decluttering,
                CriteriaType = "total_items_listed",
                CriteriaValue = 25,
                TokenReward = 45m,
                XpReward = 90,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Category Expert",
                Description = "List 50 items through quest completion",
                Category = AchievementCategory.Decluttering,
                CriteriaType = "total_items_listed",
                CriteriaValue = 50,
                TokenReward = 75m,
                XpReward = 150,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Ultimate Organizer",
                Description = "List 100 items through quest completion",
                Category = AchievementCategory.Decluttering,
                CriteriaType = "total_items_listed",
                CriteriaValue = 100,
                TokenReward = 120m,
                XpReward = 240,
                Rarity = AchievementRarity.Platinum,
                IsSecret = false,
                IsActive = true
            }
        };
        }

        // Expanded Special Achievements (10 achievements)
        public static List<AchievementTemplate> GetSpecialAchievements()
        {
            return new List<AchievementTemplate>
        {
            new AchievementTemplate
            {
                Name = "Early Starter",
                Description = "Complete your first quest within 24 hours of joining",
                Category = AchievementCategory.Special,
                CriteriaType = "quick_start",
                CriteriaValue = 1,
                TokenReward = 25m,
                XpReward = 50,
                Rarity = AchievementRarity.Bronze,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Weekend Organizer",
                Description = "Complete 5 quests on weekends",
                Category = AchievementCategory.Special,
                CriteriaType = "weekend_quests",
                CriteriaValue = 5,
                TokenReward = 35m,
                XpReward = 70,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Speed Runner",
                Description = "Complete 5 quests in a single day",
                Category = AchievementCategory.Special,
                CriteriaType = "daily_quest_burst",
                CriteriaValue = 5,
                TokenReward = 50m,
                XpReward = 100,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Comeback Kid",
                Description = "Return and complete a quest after 7+ days away",
                Category = AchievementCategory.Special,
                CriteriaType = "return_activity",
                CriteriaValue = 1,
                TokenReward = 30m,
                XpReward = 60,
                Rarity = AchievementRarity.Silver,
                IsSecret = true,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Milestone Maker",
                Description = "Reach your first level up",
                Category = AchievementCategory.Special,
                CriteriaType = "level_up",
                CriteriaValue = 1,
                TokenReward = 40m,
                XpReward = 0, // No XP since this IS the XP milestone
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Level Climber",
                Description = "Reach level 5",
                Category = AchievementCategory.Special,
                CriteriaType = "player_level",
                CriteriaValue = 5,
                TokenReward = 75m,
                XpReward = 0,
                Rarity = AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Token Collector",
                Description = "Earn 100 tokens total",
                Category = AchievementCategory.Special,
                CriteriaType = "tokens_earned",
                CriteriaValue = 100,
                TokenReward = 25m,
                XpReward = 50,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Token Hoarder",
                Description = "Earn 500 tokens total",
                Category = AchievementCategory.Special,
                CriteriaType = "tokens_earned",
                CriteriaValue = 500,
                TokenReward = 100m,
                XpReward = 200,
                Rarity = AchievementRarity.Platinum,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Night Owl",
                Description = "Complete 10 quests after 8 PM",
                Category = AchievementCategory.Special,
                CriteriaType = "evening_quests",
                CriteriaValue = 10,
                TokenReward = 30m,
                XpReward = 60,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            },
            new AchievementTemplate
            {
                Name = "Early Bird",
                Description = "Complete 10 quests before 10 AM",
                Category = AchievementCategory.Special,
                CriteriaType = "morning_quests",
                CriteriaValue = 10,
                TokenReward = 30m,
                XpReward = 60,
                Rarity = AchievementRarity.Silver,
                IsSecret = false,
                IsActive = true
            }
        };
        }
        // Helper method to create category mastery achievements
        public static AchievementTemplate CreateCategoryMastery(string category, string title, int requiredItems, decimal tokens, int xp)
        {
            return new AchievementTemplate
            {
                Name = title,
                Description = $"Master the art of {category.Replace('_', ' ')} organization by completing category quests and listing {requiredItems} items",
                Category = AchievementCategory.Decluttering,
                CriteriaType = $"category_mastery_{category}",
                CriteriaValue = 1, // Complex validation handled separately
                TokenReward = tokens,
                XpReward = xp,
                Rarity = category == "sentimental" ? AchievementRarity.Platinum : AchievementRarity.Gold,
                IsSecret = false,
                IsActive = true
            };
        }
    }

    // Achievement Template class for easier management
    public class AchievementTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AchievementCategory Category { get; set; }
        public string CriteriaType { get; set; } = string.Empty;
        public int CriteriaValue { get; set; }
        public decimal TokenReward { get; set; }
        public int XpReward { get; set; }
        public string? BadgeImageUrl { get; set; }
        public AchievementRarity Rarity { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsSecret { get; set; } = false;

        // Helper method to convert template to Achievement entity
        public Achievement ToAchievement()
        {
            return new Achievement
            {
                Name = this.Name,
                Description = this.Description,
                Category = this.Category,
                CriteriaType = this.CriteriaType,
                CriteriaValue = this.CriteriaValue,
                TokenReward = this.TokenReward,
                XpReward = this.XpReward,
                BadgeImageUrl = this.BadgeImageUrl,
                Rarity = this.Rarity,
                IsActive = this.IsActive,
                IsSecret = this.IsSecret,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Helper method to validate template data
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   !string.IsNullOrWhiteSpace(CriteriaType) &&
                   CriteriaValue > 0 &&
                   TokenReward >= 0 &&
                   XpReward >= 0;
        }
    }
}