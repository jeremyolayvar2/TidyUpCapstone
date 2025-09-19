using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUpCapstone.Services.Background
{
    /// <summary>
    /// Background service that handles user-specific special quest generation
    /// Generates next special quest 15 days after a user completes their current one
    /// </summary>
    public class UserSpecialQuestService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserSpecialQuestService> _logger;

        public UserSpecialQuestService(IServiceProvider serviceProvider, ILogger<UserSpecialQuestService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("User Special Quest Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndGenerateSpecialQuestsAsync();

                    // Check every 4 hours
                    await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in User Special Quest Service");

                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("User Special Quest Service stopped");
        }

        /// <summary>
        /// Check for users who completed their special quest 15+ days ago and need a new one
        /// </summary>
        private async Task CheckAndGenerateSpecialQuestsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();

            try
            {
                var fifteenDaysAgo = DateTime.UtcNow.AddDays(-15);

                // Find users who:
                // 1. Completed a special quest 15+ days ago
                // 2. Don't currently have an active special quest
                var usersNeedingSpecialQuest = await context.UserQuests
                    .Include(uq => uq.Quest)
                    .Include(uq => uq.User)
                    .Where(uq => uq.Quest.QuestType == QuestType.Special &&
                                uq.IsCompleted &&
                                uq.CompletedAt.HasValue &&
                                uq.CompletedAt.Value <= fifteenDaysAgo)
                    .GroupBy(uq => uq.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        LastCompletedSpecialQuest = g.OrderByDescending(uq => uq.CompletedAt).First(),
                        User = g.First().User
                    })
                    .ToListAsync();

                foreach (var userInfo in usersNeedingSpecialQuest)
                {
                    // Check if user already has an active special quest
                    var hasActiveSpecialQuest = await context.UserQuests
                        .Include(uq => uq.Quest)
                        .AnyAsync(uq => uq.UserId == userInfo.UserId &&
                                       uq.Quest.QuestType == QuestType.Special &&
                                       uq.Quest.IsActive &&
                                       !uq.IsCompleted);

                    if (!hasActiveSpecialQuest)
                    {
                        _logger.LogInformation($"Generating next special quest for user {userInfo.UserId} (last completed: {userInfo.LastCompletedSpecialQuest.CompletedAt})");

                        await questService.GenerateNextSpecialQuestForUserAsync(userInfo.UserId);

                        // Small delay between generations to avoid overwhelming the system
                        await Task.Delay(1000);
                    }
                }

                if (usersNeedingSpecialQuest.Any())
                {
                    _logger.LogInformation($"Processed {usersNeedingSpecialQuest.Count} users for special quest generation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and generating user special quests");
            }
        }
    }
}