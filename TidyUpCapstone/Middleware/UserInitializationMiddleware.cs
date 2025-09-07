using TidyUpCapstone.Data;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Models.Entities.Gamification;
using TidyUpCapstone.Services.Interfaces;
namespace TidyUpCapstone.Middleware
{
    // Create Middleware/UserInitializationMiddleware.cs
    public class UserInitializationMiddleware
    {
        private readonly RequestDelegate _next;

        public UserInitializationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Get user ID from your session logic
            if (context.Session.Keys.Contains("CurrentUserId"))
            {
                var userId = context.Session.GetInt32("CurrentUserId") ?? 1;

                // Check if user needs initialization
                var hasAchievements = await dbContext.UserAchievements.AnyAsync(ua => ua.UserId == userId);

                if (!hasAchievements)
                {
                    // Initialize user in background
                    _ = Task.Run(async () =>
                    {
                        using var scope = context.RequestServices.CreateScope();
                        var initService = scope.ServiceProvider.GetRequiredService<IUserInitializationService>();
                        await initService.InitializeUserAsync(userId);
                    });
                }
            }

            await _next(context);
        }
    }
}
