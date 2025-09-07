using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Background
{
    public class QuestBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuestBackgroundService> _logger;
        private readonly Timer _dailyTimer;
        private readonly Timer _weeklyTimer;
        private readonly Timer _maintenanceTimer;
        

        public QuestBackgroundService(IServiceProvider serviceProvider, ILogger<QuestBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            

            // Schedule daily quest generation at midnight
            var now = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var timeUntilMidnight = nextMidnight - now;

            _dailyTimer = new Timer(GenerateDailyQuests, null, timeUntilMidnight, TimeSpan.FromDays(1));

            // Schedule weekly quest generation on Sunday at midnight
            var nextSunday = GetNextSunday();
            var timeUntilNextSunday = nextSunday - now;

            _weeklyTimer = new Timer(GenerateWeeklyQuests, null, timeUntilNextSunday, TimeSpan.FromDays(7));

            // Schedule maintenance tasks every hour
            _maintenanceTimer = new Timer(RunMaintenanceTasks, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Quest Background Service started");

            // Initial setup on service start
            await InitializeServicesAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Quest Background Service stopped");
        }

        private async Task InitializeServicesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();
            var achievementService = scope.ServiceProvider.GetRequiredService<IAchievementService>();
            var streakService = scope.ServiceProvider.GetRequiredService<IStreakService>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Seed achievements if not already seeded
                if (!await achievementService.AreAchievementsSeededAsync())
                {
                    await achievementService.SeedAchievementsAsync();
                    _logger.LogInformation("Achievements seeded successfully");
                }

                // Seed streak types if not already seeded
                if (!await streakService.AreStreakTypesSeededAsync())
                {
                    await streakService.SeedStreakTypesAsync();
                    _logger.LogInformation("Streak types seeded successfully");
                }

                // Generate initial daily quests if none exist for today
                var today = DateTime.UtcNow.Date;
                await questService.GenerateDailyQuestsAsync();

                // Generate initial weekly quests if none exist for this week
                await questService.GenerateWeeklyQuestsAsync();

                //Auto - initialize all existing users
                await InitializeAllUsersAsync(context);


                _logger.LogInformation("Quest system initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during quest system initialization");
            }
        }

        private async Task InitializeAllUsersAsync(ApplicationDbContext context)
        {
            var users = await context.Users.ToListAsync();

            foreach (var user in users)
            {
                await InitializeUserIfNeededAsync(context, user.Id);
            }
        }

        private async Task InitializeUserIfNeededAsync(ApplicationDbContext context, int userId)
        {
            var hasAchievements = await context.UserAchievements.AnyAsync(ua => ua.UserId == userId);

            if (!hasAchievements)
            {
                // Only seed achievements and stats - remove quest assignment
                var allAchievements = await context.Achievements.Where(a => a.IsActive).ToListAsync();

                var userAchievements = allAchievements.Select(a => new UserAchievement
                {
                    UserId = userId,
                    AchievementId = a.AchievementId,
                    IsUnlocked = false,
                    Progress = 0,
                    EarnedDate = null,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                context.UserAchievements.AddRange(userAchievements);

                // Create user stats if not exists
                var hasStats = await context.UserStats.AnyAsync(us => us.UserId == userId);
                if (!hasStats)
                {
                    var userStats = new UserStats
                    {
                        UserId = userId,
                        CurrentLevel = 1,
                        CurrentXp = 0,
                        TotalTokens = 0,
                        CurrentStreak = 0,
                        LongestStreak = 0
                    };
                    context.UserStats.Add(userStats);
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"Initialized user {userId} with achievements and stats");
            }
        }

        private async void GenerateDailyQuests(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();

            try
            {
                await questService.GenerateDailyQuestsAsync();
                _logger.LogInformation("Daily quests generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily quests");
            }
        }

        private async void GenerateWeeklyQuests(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();

            try
            {
                await questService.GenerateWeeklyQuestsAsync();
                _logger.LogInformation("Weekly quests generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly quests");
            }
        }

        private async void RunMaintenanceTasks(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();
            var streakService = scope.ServiceProvider.GetRequiredService<IStreakService>();

            try
            {
                // Expire old quests
                await questService.CheckAndExpireQuestsAsync();

                // Reset broken streaks
                await streakService.CheckAndResetExpiredStreaksAsync();

                _logger.LogDebug("Maintenance tasks completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during maintenance tasks");
            }
        }

        private static DateTime GetNextSunday()
        {
            var now = DateTime.UtcNow;
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilSunday == 0) daysUntilSunday = 7; // If today is Sunday, get next Sunday

            return now.Date.AddDays(daysUntilSunday);
        }

        public override void Dispose()
        {
            _dailyTimer?.Dispose();
            _weeklyTimer?.Dispose();
            _maintenanceTimer?.Dispose();
            base.Dispose();
        }
    }
}