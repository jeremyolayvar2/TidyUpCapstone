using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TidyUpCapstone.Data;
using TidyUpCapstone.Filters;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;

namespace TidyUpCapstone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager; 
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Index(string? error = null, string? success = null)
        {
            // Pass any error or success messages to the view
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.ErrorMessage = error;
            }

            if (!string.IsNullOrEmpty(success))
            {
                ViewBag.SuccessMessage = success;
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Main()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("User not found in Main action, redirecting to Index");

                    // If user is not found but claims to be authenticated, sign them out and redirect to home
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        await _signInManager.SignOutAsync(); // NOW WORKS: _signInManager is available
                    }

                    return RedirectToAction("Index", "Home");
                }

                _logger.LogInformation("User {UserId} accessed Main page successfully", currentUser.Id);

                // Set ViewBag properties if needed
                ViewBag.UserName = currentUser.UserName;
                ViewBag.FirstName = currentUser.FirstName;
                ViewBag.LastName = currentUser.LastName;

                return View(); // This will look for Views/Home/Main.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Main action");
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [NoCache]
        public async Task<IActionResult> SettingsPage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Load notification settings
            var settings = await _context.NotificationSettings
                .FirstOrDefaultAsync(ns => ns.UserId == user.Id);

            // Pass settings to view
            ViewBag.EmailNewMessages = settings?.EmailNewMessages ?? true;
            ViewBag.EmailItemUpdates = settings?.EmailItemUpdates ?? true;
            ViewBag.EmailWeeklySummary = settings?.EmailWeeklySummary ?? false;
            ViewBag.DesktopNotifications = settings?.DesktopNotifications ?? true;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}