using Microsoft.AspNetCore.Mvc;

namespace TidyUpCapstone.Controllers
{
    public class LegalController : Controller
    {
        // Privacy Policy
        [HttpGet("/privacy-policy")]
        public IActionResult Privacy()
        {
            return View();
        }

        // Terms of Service
        [HttpGet("/terms-of-service")]
        public IActionResult Terms()
        {
            return View();
        }

        // Data Deletion Policy
        [HttpGet("/data-deletion")]
        public IActionResult DataDeletion()
        {
            return View();
        }
    }
}