using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TidyUpCapstone.Models;
using TidyUpCapstone.Models.DTOs;
using TidyUpCapstone.Models.Entities;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IItemPostService _itemPostService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager; // Changed from ApplicationUser to AppUser

        public HomeController(
            ILogger<HomeController> logger,
            //IItemPostService itemPostService,
            //IUserService userService,
            UserManager<AppUser> userManager) // Changed from ApplicationUser to AppUser
        {
            _logger = logger;
            //_itemPostService = itemPostService;
            //_userService = userService;
            //_userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Main()
        {
            return View();
        }

        //[Authorize]
        //public async Task<IActionResult> Main()
        //{
        //    try
        //    {
        //        var currentUser = await _userManager.GetUserAsync(User);
        //        if (currentUser == null)
        //        {
        //            _logger.LogWarning("Current user is null, redirecting to login");
        //            return RedirectToAction("Login", "Account");
        //        }

        //        _logger.LogInformation("Loading main page for user: {UserId}", currentUser.Id);

        //        var items = await _itemPostService.GetAllItemPostsAsync();
        //        var tokenBalance = await _userService.GetUserTokenBalanceAsync(currentUser.Id);

        //        var viewModel = new MainPageViewModel
        //        {
        //            ItemPosts = items ?? new List<ItemPost>(),
        //            NewItemPost = new ItemPostDto(),
        //            CurrentUserTokenBalance = tokenBalance
        //        };

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading main page");
        //        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //    }
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}