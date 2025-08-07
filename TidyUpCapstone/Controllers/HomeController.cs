using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;

namespace TidyUpCapstone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
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
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found in Main action, redirecting to Index");
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation("User {UserId} accessed Main page successfully", currentUser.Id);
            return View(); // This will look for Views/Home/Main.cshtml
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}