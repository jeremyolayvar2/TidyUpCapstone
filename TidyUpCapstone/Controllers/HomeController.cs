using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TidyUpCapstone.Models;
//using TidyUpCapstone.Models.DTOs;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
//using TidyUpCapstone.Services.Interfaces;
namespace TidyUpCapstone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IItemPostService _itemPostService;
        //private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        public HomeController(
            ILogger<HomeController> logger,
            //IItemPostService itemPostService,
            //IUserService userService,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            //_itemPostService = itemPostService;
            //_userService = userService;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Main()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}