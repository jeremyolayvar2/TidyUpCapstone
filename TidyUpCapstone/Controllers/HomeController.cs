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

namespace TidyUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
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