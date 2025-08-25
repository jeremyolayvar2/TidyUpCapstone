using Microsoft.AspNetCore.Mvc;

namespace TidyUpCapstone.Controllers
{
    public class LegalController : Controller
    {
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        public IActionResult CookiePolicy()
        {
            return View();
        }

        public IActionResult DataProtection()
        {
            return View();
        }
    }
}