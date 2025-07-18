using Microsoft.AspNetCore.Mvc;

namespace TidyUpCapstone.Models.Entities.Authentication
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
