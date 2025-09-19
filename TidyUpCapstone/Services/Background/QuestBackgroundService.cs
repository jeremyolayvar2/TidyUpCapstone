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
        private readonly Timer _specialQuestTimer;

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

            // Generate special quests bi-weekly (every 15 days) instead of monthly
            //var timeUntilNextSpecial = TimeSpan.FromDays(15);
            //_specialQuestTimer = new Timer(GenerateSpecialQuests, null, timeUntilNextSpecial, TimeSpan.FromDays(15));
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
            var userInitService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUserInitializationService>();

            foreach (var user in users)
            {
                await userInitService.InitializeUserAsync(user.Id);
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


        private async void GenerateSpecialQuests(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();

            try
            {
                await questService.GenerateSpecialQuestAsync();
                _logger.LogInformation("Special quest generation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating special quest");
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
            _specialQuestTimer?.Dispose();
            base.Dispose();
        }
    }
}