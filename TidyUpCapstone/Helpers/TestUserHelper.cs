using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Helpers
{
    public interface ITestUserHelper
    {
        Task<AppUser?> GetCurrentUserAsync(HttpContext httpContext, UserManager<AppUser> userManager, ClaimsPrincipal user);
        Task EnsureTestUsersExistAsync();
    }

    public class TestUserHelper : ITestUserHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestUserHelper> _logger;

        public TestUserHelper(ApplicationDbContext context, ILogger<TestUserHelper> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AppUser?> GetCurrentUserAsync(HttpContext httpContext, UserManager<AppUser> userManager, ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Real authenticated user
                return await userManager.GetUserAsync(user);
            }
            else
            {
                // Test mode: Create all test users if they don't exist
                await EnsureTestUsersExistAsync();

                // Check session first, then query parameter, then default to Alice
                var testUserParam = httpContext.Request.Query["testUser"].ToString();
                var currentTestUser = !string.IsNullOrEmpty(testUserParam) ? testUserParam :
                                     httpContext.Session.GetString("CurrentTestUser") ?? "Alice";

                // Save to session for subsequent requests
                httpContext.Session.SetString("CurrentTestUser", currentTestUser);

                _logger.LogInformation("Test mode: Using test user '{TestUser}'", currentTestUser);

                var testUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentTestUser);
                return testUser;
            }
        }

        public async Task EnsureTestUsersExistAsync()
        {
            var testUsers = new[]
            {
                new { Id = 1, UserName = "Alice", Email = "alice@test.com" },
                new { Id = 2, UserName = "Bob", Email = "bob@test.com" },
                new { Id = 3, UserName = "Charlie", Email = "charlie@test.com" }
            };

            foreach (var testUserData in testUsers)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == testUserData.UserName);

                if (existingUser == null)
                {
                    var newUser = new AppUser
                    {
                        UserName = testUserData.UserName,
                        Email = testUserData.Email,
                        EmailConfirmed = true,
                        DateCreated = GetPhilippinesTime(),
                        Status = "active"
                    };

                    _context.Users.Add(newUser);
                    _logger.LogInformation($"Created test user: {testUserData.UserName}");
                }
            }

            await _context.SaveChangesAsync();
        }

        private static DateTime GetPhilippinesTime()
        {
            var utcTime = DateTime.UtcNow;
            var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, philippinesTimeZone);
        }
    }
}