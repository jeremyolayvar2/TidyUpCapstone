using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Leaderboard;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Services
{
    public interface IUserSessionService
    {
        Task<List<SessionUserDto>> GetAvailableUsersAsync();
        Task<SessionUserDto?> GetUserByIdAsync(int userId);
        int GetCurrentUserId(HttpContext httpContext);
        void SetCurrentUserId(HttpContext httpContext, int userId);
        Task InitializeDemoUsersAsync();
    }

    public class UserSessionService : IUserSessionService
    {
        private readonly ApplicationDbContext _context;
        private const string SESSION_USER_KEY = "CurrentUserId";

        public UserSessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SessionUserDto>> GetAvailableUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.Status == "active")
                .Select(u => new SessionUserDto
                {
                    UserId = u.Id,
                    Name = u.UserName ?? "Unknown",
                    Email = u.Email ?? "",
                    IsActive = u.Status == "active"
                })
                .Take(20) // Limit for demo purposes
                .ToListAsync();

            return users;
        }

        public async Task<SessionUserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new SessionUserDto
                {
                    UserId = u.Id,
                    Name = u.UserName ?? "Unknown",
                    Email = u.Email ?? "",
                    IsActive = u.Status == "active"
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public int GetCurrentUserId(HttpContext httpContext)
        {
            if (httpContext.Session.GetInt32(SESSION_USER_KEY) is int userId)
            {
                return userId;
            }

            // Default to first user if no session set
            var firstUser = _context.Users.OrderBy(u => u.Id).FirstOrDefault();
            if (firstUser != null)
            {
                SetCurrentUserId(httpContext, firstUser.Id);
                return firstUser.Id;
            }

            return 1; // Fallback
        }

        public void SetCurrentUserId(HttpContext httpContext, int userId)
        {
            httpContext.Session.SetInt32(SESSION_USER_KEY, userId);
        }

        public async Task InitializeDemoUsersAsync()
        {
            // Check if demo users already exist
            if (await _context.Users.AnyAsync())
                return;

            var demoUsers = new List<AppUser>
            {
                new AppUser
                {
                    UserName = "kate.gonzales",
                    Email = "kate@demo.com",
                    TokenBalance = 1500.00m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-6),
                    LastLogin = DateTime.UtcNow.AddHours(-2)
                },
                new AppUser
                {
                    UserName = "deither.arias",
                    Email = "deither@demo.com",
                    TokenBalance = 980.50m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-4),
                    LastLogin = DateTime.UtcNow.AddHours(-5)
                },
                new AppUser
                {
                    UserName = "jiro.llaguno",
                    Email = "jiro@demo.com",
                    TokenBalance = 750.25m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-3),
                    LastLogin = DateTime.UtcNow.AddDays(-1)
                },
                new AppUser
                {
                    UserName = "russel.saldivar",
                    Email = "russel@demo.com",
                    TokenBalance = 650.00m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-2),
                    LastLogin = DateTime.UtcNow.AddHours(-8)
                },
                new AppUser
                {
                    UserName = "joaquin.bordado",
                    Email = "joaquin@demo.com",
                    TokenBalance = 580.75m,
                    Status = "active",
                    DateCreated = DateTime.UtcNow.AddMonths(-2),
                    LastLogin = DateTime.UtcNow.AddDays(-2)
                }
            };

            _context.Users.AddRange(demoUsers);
            await _context.SaveChangesAsync();

            // Initialize related data for each user
            await InitializeDemoDataAsync();
        }

        private async Task InitializeDemoDataAsync()
        {
            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                // Create user levels
                var level = new Models.Entities.Gamification.UserLevel
                {
                    UserId = user.Id,
                    CurrentLevelId = Random.Shared.Next(1, 10),
                    CurrentXp = Random.Shared.Next(100, 1000),
                    TotalXp = Random.Shared.Next(1000, 5000),
                    XpToNextLevel = Random.Shared.Next(100, 500),
                    TotalLevelUps = Random.Shared.Next(5, 25)
                };

                // Create user streaks
                var streak = new Models.Entities.Gamification.UserStreak
                {
                    UserId = user.Id,
                    StreakTypeId = 1, // Assuming daily streak type
                    CurrentStreak = Random.Shared.Next(5, 30),
                    LongestStreak = Random.Shared.Next(10, 50),
                    LastActivityDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 3)),
                    TotalMilestonesReached = Random.Shared.Next(1, 8)
                };

                _context.Add(level);
                _context.Add(streak);
            }

            await _context.SaveChangesAsync();
        }
    }
}