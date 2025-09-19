using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Models.Entities.Items;

namespace TidyUpCapstone.Services
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task SeedAsync()
        {
            try
            {
                // Only seed if database is empty
                if (await _context.Users.AnyAsync())
                {
                    Console.WriteLine("Database already contains data, skipping seeding.");
                    return;
                }

                Console.WriteLine("Starting database seeding...");

                await SeedUsersAsync();
                await SeedLevelsAsync();
                await SeedUserLevelsAsync();
                await SeedStreakTypesAsync();
                await SeedUserStreaksAsync();
                await SeedCategoriesAsync();
                await SeedConditionsAsync();
                await SeedLocationsAsync();
                await SeedItemsAsync();

                // Gamification data
                await SeedQuestsAsync();
                await SeedUserQuestsAsync();
                await SeedAchievementsAsync();
                await SeedUserAchievementsAsync();
                await SeedLeaderboardsAsync();
                await SeedLeaderboardEntriesAsync();

                Console.WriteLine("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task SeedUsersAsync()
        {
                    var users = new List<AppUser>
            {
                new AppUser
                {
                    // REMOVED: Id = 1,  // Let SQL Server auto-generate this
                    UserName = "Kate Gonzales",
                    Email = "kate.gonzales@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 1500.00m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-6),
                    LastLogin = DateTime.UtcNow.AddHours(-2)
                },
                new AppUser
                {
                    // REMOVED: Id = 2,
                    UserName = "Deither Arias",
                    Email = "deither.arias@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 980.50m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-4),
                    LastLogin = DateTime.UtcNow.AddHours(-5)
                },
                new AppUser
                {
                    // REMOVED: Id = 3,
                    UserName = "Jiro Llaguno",
                    Email = "jiro.llaguno@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 750.25m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-3),
                    LastLogin = DateTime.UtcNow.AddDays(-1)
                },
                new AppUser
                {
                    // REMOVED: Id = 4,
                    UserName = "Russel Saldivar",
                    Email = "russel.saldivar@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 650.00m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-2),
                    LastLogin = DateTime.UtcNow.AddHours(-8)
                },
                new AppUser
                {
                    // REMOVED: Id = 5,
                    UserName = "Joaquin Bordado",
                    Email = "joaquin.bordado@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 580.75m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-2),
                    LastLogin = DateTime.UtcNow.AddDays(-2)
                },
                new AppUser
                {
                    // REMOVED: Id = 6,
                    UserName = "Nardong Putik",
                    Email = "nardong.putik@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 450.00m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-1),
                    LastLogin = DateTime.UtcNow.AddDays(-3)
                },
                new AppUser
                {
                    // REMOVED: Id = 7,
                    UserName = "Tonyong Bayawak",
                    Email = "tonyong.bayawak@demo.com",
                    EmailConfirmed = true,
                    TokenBalance = 320.50m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddDays(-21),
                    LastLogin = DateTime.UtcNow.AddDays(-1)
                }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            Console.WriteLine("Users seeded successfully");
        }

        private async Task SeedLevelsAsync()
        {
            var levels = new List<Level>();
            for (int i = 1; i <= 50; i++)
            {
                levels.Add(new Level
                {
                    // REMOVED: LevelId = i,  // Let SQL Server auto-generate
                    LevelNumber = i,
                    LevelName = $"Level {i}",
                    XpRequired = i * 100,
                    XpToNext = (i + 1) * 100,
                    TokenBonus = i * 10.00m,
                    TitleUnlock = i % 10 == 0 ? $"Master of Level {i}" : null,
                    SpecialPrivilege = i >= 25 ? "Advanced Trading Access" : null
                });
            }

            _context.Levels.AddRange(levels);
            await _context.SaveChangesAsync();
            Console.WriteLine("Levels seeded successfully");
        }

        private async Task SeedUserLevelsAsync()
        {
            var users = await _context.Users.OrderBy(u => u.DateCreated).ToListAsync();
            var levels = await _context.Levels.OrderBy(l => l.LevelNumber).ToListAsync();

            if (users.Count < 7 || levels.Count < 15)
            {
                Console.WriteLine("Not enough users or levels to seed user levels");
                return;
            }

            var userLevels = new List<UserLevel>
    {
        new UserLevel { UserId = users[0].Id, CurrentLevelId = levels[14].LevelId, CurrentXp = 1450, TotalXp = 2800, XpToNextLevel = 150, TotalLevelUps = 15 },
        new UserLevel { UserId = users[1].Id, CurrentLevelId = levels[11].LevelId, CurrentXp = 1150, TotalXp = 2100, XpToNextLevel = 250, TotalLevelUps = 12 },
        new UserLevel { UserId = users[2].Id, CurrentLevelId = levels[9].LevelId, CurrentXp = 950, TotalXp = 1800, XpToNextLevel = 150, TotalLevelUps = 10 },
        new UserLevel { UserId = users[3].Id, CurrentLevelId = levels[8].LevelId, CurrentXp = 820, TotalXp = 1650, XpToNextLevel = 180, TotalLevelUps = 9 },
        new UserLevel { UserId = users[4].Id, CurrentLevelId = levels[7].LevelId, CurrentXp = 750, TotalXp = 1500, XpToNextLevel = 150, TotalLevelUps = 8 },
        new UserLevel { UserId = users[5].Id, CurrentLevelId = levels[6].LevelId, CurrentXp = 650, TotalXp = 1200, XpToNextLevel = 150, TotalLevelUps = 7 },
        new UserLevel { UserId = users[6].Id, CurrentLevelId = levels[5].LevelId, CurrentXp = 550, TotalXp = 1000, XpToNextLevel = 150, TotalLevelUps = 6 }
    };

            _context.UserLevels.AddRange(userLevels);
            await _context.SaveChangesAsync();
            Console.WriteLine("User levels seeded successfully");
        }


        private async Task SeedStreakTypesAsync()
        {
            var streakTypes = new List<StreakType>
            {
                new StreakType
                {
                    // REMOVED: StreakTypeId = 1,  // Let SQL auto-generate
                    Name = "Daily Decluttering",
                    Description = "Consecutive days of decluttering items",
                    StreakUnit = StreakUnit.Days,
                    BaseRewards = 10.00m,
                    MilestoneRewards = 50.00m,
                    MilestoneInterval = 7,
                    IsActive = true
                },
                new StreakType
                {
                    // REMOVED: StreakTypeId = 2,
                    Name = "Weekly Warrior",
                    Description = "Consecutive weeks of active participation",
                    StreakUnit = StreakUnit.Weeks,
                    BaseRewards = 25.00m,
                    MilestoneRewards = 100.00m,
                    MilestoneInterval = 4,
                    IsActive = true
                }
            };

            _context.StreakTypes.AddRange(streakTypes);
            await _context.SaveChangesAsync();
            Console.WriteLine("Streak types seeded successfully");
        }


        private async Task SeedUserStreaksAsync()
        {
            var users = await _context.Users.OrderBy(u => u.DateCreated).ToListAsync();
            var streakTypes = await _context.StreakTypes.OrderBy(st => st.Name).ToListAsync();

            if (users.Count < 7 || streakTypes.Count == 0)
            {
                Console.WriteLine("Not enough users or streak types to seed user streaks");
                return;
            }

            var userStreaks = new List<UserStreak>
            {
                new UserStreak { UserId = users[0].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 24, LongestStreak = 30, LastActivityDate = DateTime.UtcNow, TotalMilestonesReached = 3 },
                new UserStreak { UserId = users[1].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 19, LongestStreak = 25, LastActivityDate = DateTime.UtcNow.AddDays(-1), TotalMilestonesReached = 2 },
                new UserStreak { UserId = users[2].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 11, LongestStreak = 15, LastActivityDate = DateTime.UtcNow.AddDays(-2), TotalMilestonesReached = 1 },
                new UserStreak { UserId = users[3].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 10, LongestStreak = 12, LastActivityDate = DateTime.UtcNow.AddDays(-1), TotalMilestonesReached = 1 },
                new UserStreak { UserId = users[4].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 11, LongestStreak = 14, LastActivityDate = DateTime.UtcNow.AddDays(-3), TotalMilestonesReached = 1 },
                new UserStreak { UserId = users[5].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 8, LongestStreak = 10, LastActivityDate = DateTime.UtcNow.AddDays(-2), TotalMilestonesReached = 1 },
                new UserStreak { UserId = users[6].Id, StreakTypeId = streakTypes[0].StreakTypeId, CurrentStreak = 8, LongestStreak = 9, LastActivityDate = DateTime.UtcNow.AddDays(-1), TotalMilestonesReached = 1 }
            };

            _context.UserStreaks.AddRange(userStreaks);
            await _context.SaveChangesAsync();
            Console.WriteLine("User streaks seeded successfully");
        }

        private async Task SeedCategoriesAsync()
        {
            var categories = new List<ItemCategory>
            {
                new ItemCategory { CategoryId = 1, Name = "Electronics", IsActive = true, SortOrder = 1 },
                new ItemCategory { CategoryId = 2, Name = "Clothing", IsActive = true, SortOrder = 2 },
                new ItemCategory { CategoryId = 3, Name = "Books", IsActive = true, SortOrder = 3 },
                new ItemCategory { CategoryId = 4, Name = "Furniture", IsActive = true, SortOrder = 4 },
                new ItemCategory { CategoryId = 5, Name = "Kitchen", IsActive = true, SortOrder = 5 }
            };

            _context.ItemCategories.AddRange(categories);
            await _context.SaveChangesAsync();
            Console.WriteLine("Categories seeded successfully");
        }

        private async Task SeedConditionsAsync()
        {
            var conditions = new List<ItemCondition>
            {
                new ItemCondition { ConditionId = 1, Name = "Like New", ConditionMultiplier = 1.0m, IsActive = true },
                new ItemCondition { ConditionId = 2, Name = "Good", ConditionMultiplier = 0.8m, IsActive = true },
                new ItemCondition { ConditionId = 3, Name = "Fair", ConditionMultiplier = 0.6m, IsActive = true },
                new ItemCondition { ConditionId = 4, Name = "Poor", ConditionMultiplier = 0.4m, IsActive = true }
            };

            _context.ItemConditions.AddRange(conditions);
            await _context.SaveChangesAsync();
            Console.WriteLine("Conditions seeded successfully");
        }

        private async Task SeedLocationsAsync()
        {
            var locations = new List<ItemLocation>
            {
                new ItemLocation { LocationId = 1, Name = "Metro Manila", Region = "NCR", IsActive = true },
                new ItemLocation { LocationId = 2, Name = "Cebu City", Region = "Central Visayas", IsActive = true },
                new ItemLocation { LocationId = 3, Name = "Davao City", Region = "Davao", IsActive = true }
            };

            _context.ItemLocations.AddRange(locations);
            await _context.SaveChangesAsync();
            Console.WriteLine("Locations seeded successfully");
        }

        private async Task SeedItemsAsync()
        {
            var random = new Random();
            var items = new List<Item>();

            // Create items for each user with the specified counts
            var userItemCounts = new Dictionary<int, int>
            {
                { 1, 251 }, { 2, 178 }, { 3, 142 }, { 4, 98 },
                { 5, 80 }, { 6, 78 }, { 7, 58 }
            };

            foreach (var userItemCount in userItemCounts)
            {
                for (int i = 0; i < userItemCount.Value; i++)
                {
                    items.Add(CreateRandomItem(userItemCount.Key, i + 1, random));
                }
            }

            _context.Items.AddRange(items);
            await _context.SaveChangesAsync();
            Console.WriteLine("Items seeded successfully");
        }

        private Item CreateRandomItem(int userId, int itemNumber, Random random)
        {
            var categories = new[] { "Phone", "Laptop", "Book", "Shirt", "Table", "Chair", "Plate" };
            var category = categories[random.Next(categories.Length)];

            return new Item
            {
                UserId = userId,
                CategoryId = random.Next(1, 6),
                ConditionId = random.Next(1, 5),
                LocationId = random.Next(1, 4),
                ItemTitle = $"{category} #{itemNumber}",
                Description = $"Used {category.ToLower()} in good condition",
                AdjustedTokenPrice = random.Next(10, 200),
                FinalTokenPrice = random.Next(10, 200),
                Status = ItemStatus.Completed, // Using your actual enum value
                DatePosted = DateTime.UtcNow.AddDays(-random.Next(1, 180)),
                AiProcessingStatus = AiProcessingStatus.Completed, // Using your actual enum value
                AiSuggestedPrice = random.Next(10, 200),
                AiConfidenceLevel = (decimal)(random.NextDouble() * 0.5 + 0.5), // 0.5 to 1.0
                PriceOverriddenByUser = random.Next(0, 2) == 1,
                ViewCount = random.Next(0, 50)
            };
        }

        private async Task SeedQuestsAsync()
        {
            var quests = new List<Quest>
            {
                new Quest
                {
                    QuestId = 1,
                    QuestTitle = "Daily Declutterer",
                    QuestType = QuestType.Daily,
                    QuestDescription = "Declutter at least 3 items today",
                    QuestObjective = "Post 3 items for decluttering",
                    TokenReward = 50.00m,
                    XpReward = 100,
                    Difficulty = QuestDifficulty.Easy,
                    TargetValue = 3,
                    IsActive = true
                },
                new Quest
                {
                    QuestId = 2,
                    QuestTitle = "Weekly Warrior",
                    QuestType = QuestType.Weekly,
                    QuestDescription = "Complete 20 items this week",
                    QuestObjective = "Declutter 20 items in 7 days",
                    TokenReward = 200.00m,
                    XpReward = 500,
                    Difficulty = QuestDifficulty.Medium,
                    TargetValue = 20,
                    IsActive = true
                },
                new Quest
                {
                    QuestId = 3,
                    QuestTitle = "Streak Master",
                    QuestType = QuestType.Special,
                    QuestDescription = "Maintain a 7-day decluttering streak",
                    QuestObjective = "Complete items for 7 consecutive days",
                    TokenReward = 300.00m,
                    XpReward = 750,
                    Difficulty = QuestDifficulty.Hard,
                    TargetValue = 7,
                    IsActive = true
                }
            };

            _context.Quests.AddRange(quests);
            await _context.SaveChangesAsync();
            Console.WriteLine("Quests seeded successfully");
        }

        private async Task SeedUserQuestsAsync()
        {
            var userQuests = new List<UserQuest>
            {
                new UserQuest { UserId = 1, QuestId = 1, CurrentProgress = 3, IsCompleted = true, CompletedAt = DateTime.UtcNow.AddHours(-2) },
                new UserQuest { UserId = 1, QuestId = 2, CurrentProgress = 15, IsCompleted = false },
                new UserQuest { UserId = 2, QuestId = 1, CurrentProgress = 2, IsCompleted = false },
                new UserQuest { UserId = 3, QuestId = 1, CurrentProgress = 3, IsCompleted = true, CompletedAt = DateTime.UtcNow.AddHours(-5) }
            };

            _context.UserQuests.AddRange(userQuests);
            await _context.SaveChangesAsync();
            Console.WriteLine("User quests seeded successfully");
        }

        private async Task SeedAchievementsAsync()
        {
            var achievements = new List<Achievement>
            {
                new Achievement
                {
                    AchievementId = 1,
                    Name = "First Steps",
                    Description = "Complete your first decluttering",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_completed",
                    CriteriaValue = 1,
                    TokenReward = 25.00m,
                    XpReward = 50,
                    Rarity = AchievementRarity.Bronze,
                    IsActive = true
                },
                new Achievement
                {
                    AchievementId = 2,
                    Name = "Declutter Champion",
                    Description = "Complete 100 decluttering items",
                    Category = AchievementCategory.Decluttering,
                    CriteriaType = "items_completed",
                    CriteriaValue = 100,
                    TokenReward = 500.00m,
                    XpReward = 1000,
                    Rarity = AchievementRarity.Gold,
                    IsActive = true
                },
                new Achievement
                {
                    AchievementId = 3,
                    Name = "Community Helper",
                    Description = "Help 10 community members",
                    Category = AchievementCategory.Community,
                    CriteriaType = "community_helps",
                    CriteriaValue = 10,
                    TokenReward = 200.00m,
                    XpReward = 400,
                    Rarity = AchievementRarity.Silver,
                    IsActive = true
                }
            };

            _context.Achievements.AddRange(achievements);
            await _context.SaveChangesAsync();
            Console.WriteLine("Achievements seeded successfully");
        }

        private async Task SeedUserAchievementsAsync()
        {
            var userAchievements = new List<UserAchievement>
            {
                new UserAchievement { UserId = 1, AchievementId = 1, Progress = 1, EarnedDate = DateTime.UtcNow.AddMonths(-5) },
                new UserAchievement { UserId = 1, AchievementId = 2, Progress = 100, EarnedDate = DateTime.UtcNow.AddDays(-10) },
                new UserAchievement { UserId = 2, AchievementId = 1, Progress = 1, EarnedDate = DateTime.UtcNow.AddMonths(-3) },
                new UserAchievement { UserId = 3, AchievementId = 1, Progress = 1, EarnedDate = DateTime.UtcNow.AddMonths(-2) }
            };

            _context.UserAchievements.AddRange(userAchievements);
            await _context.SaveChangesAsync();
            Console.WriteLine("User achievements seeded successfully");
        }

        private async Task SeedLeaderboardsAsync()
        {
            var leaderboards = new List<Leaderboard>
            {
                new Leaderboard
                {
                    LeaderboardId = 1,
                    Name = "Top Declutterers",
                    Type = "decluttering",
                    Metric = "items_completed",
                    Description = "Users with the most completed decluttering items",
                    ResetFrequency = ResetFrequency.Monthly,
                    MaxEntries = 100,
                    IsActive = true
                },
                new Leaderboard
                {
                    LeaderboardId = 2,
                    Name = "Streak Leaders",
                    Type = "streaks",
                    Metric = "current_streak",
                    Description = "Users with the longest current streaks",
                    ResetFrequency = ResetFrequency.Never,
                    MaxEntries = 50,
                    IsActive = true
                }
            };

            _context.Leaderboards.AddRange(leaderboards);
            await _context.SaveChangesAsync();
            Console.WriteLine("Leaderboards seeded successfully");
        }

        private async Task SeedLeaderboardEntriesAsync()
        {
            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { UserId = 1, LeaderboardId = 1, RankPosition = 1, Score = 251, RankChange = RankChange.Same },
                new LeaderboardEntry { UserId = 2, LeaderboardId = 1, RankPosition = 2, Score = 178, RankChange = RankChange.Up },
                new LeaderboardEntry { UserId = 3, LeaderboardId = 1, RankPosition = 3, Score = 142, RankChange = RankChange.Down },
                new LeaderboardEntry { UserId = 4, LeaderboardId = 1, RankPosition = 4, Score = 98, RankChange = RankChange.Same },
                new LeaderboardEntry { UserId = 5, LeaderboardId = 1, RankPosition = 5, Score = 80, RankChange = RankChange.Down },
                new LeaderboardEntry { UserId = 6, LeaderboardId = 1, RankPosition = 6, Score = 78, RankChange = RankChange.Up },
                new LeaderboardEntry { UserId = 7, LeaderboardId = 1, RankPosition = 7, Score = 58, RankChange = RankChange.New },

                new LeaderboardEntry { UserId = 1, LeaderboardId = 2, RankPosition = 1, Score = 24, RankChange = RankChange.Same },
                new LeaderboardEntry { UserId = 2, LeaderboardId = 2, RankPosition = 2, Score = 19, RankChange = RankChange.Same },
                new LeaderboardEntry { UserId = 5, LeaderboardId = 2, RankPosition = 3, Score = 11, RankChange = RankChange.Up },
                new LeaderboardEntry { UserId = 3, LeaderboardId = 2, RankPosition = 4, Score = 11, RankChange = RankChange.Down },
                new LeaderboardEntry { UserId = 4, LeaderboardId = 2, RankPosition = 5, Score = 10, RankChange = RankChange.Same }
            };

            _context.LeaderboardEntries.AddRange(entries);
            await _context.SaveChangesAsync();
            Console.WriteLine("Leaderboard entries seeded successfully");
        }
    }
}