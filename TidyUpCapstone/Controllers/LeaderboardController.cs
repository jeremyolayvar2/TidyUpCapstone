using Microsoft.AspNetCore.Mvc;

namespace TidyUpCapstone.Controllers
{
    public class LeaderboardController : Controller
    {
        public IActionResult Index()
        {
            return View("LeaderboardPage");
        }
    }
}