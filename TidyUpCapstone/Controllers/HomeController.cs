using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Models;
using TidyUpCapstone.Services;
using TidyUpCapstone.Data;
using Microsoft.EntityFrameworkCore;

namespace TidyUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            IServiceProvider serviceProvider,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MessagePage(int? otherUserId = null)
        {
            ViewData["Title"] = "Message";
            ViewData["PageType"] = "message";

            try
            {
                // Seed test users if they don't exist
                await DatabaseSeeder.SeedTestUsersAsync(_serviceProvider);

                // Get test users from database
                var testUsers = await _context.Users
                    .Where(u => u.Email == "testuser1@example.com" || u.Email == "testuser2@example.com")
                    .ToListAsync();

                // Set default current user (TestUser1) in session if not set
                var currentUserId = HttpContext.Session.GetInt32("CurrentTestUserId");
                if (currentUserId == null && testUsers.Any())
                {
                    currentUserId = testUsers.First().Id;
                    HttpContext.Session.SetInt32("CurrentTestUserId", currentUserId.Value);
                }

                var currentUser = testUsers.FirstOrDefault(u => u.Id == currentUserId);

                AppUser? otherUser = null;
                if (otherUserId.HasValue)
                {
                    otherUser = testUsers.FirstOrDefault(u => u.Id == otherUserId.Value);
                }
                else if (testUsers.Count > 1 && currentUser != null)
                {
                    // Default to the other test user
                    otherUser = testUsers.FirstOrDefault(u => u.Id != currentUser.Id);
                }

                var viewModel = new MessagePageViewModel
                {
                    CurrentUser = currentUser,
                    OtherUser = otherUser,
                    TestUsers = testUsers,
                    IsTestMode = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading message page");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}