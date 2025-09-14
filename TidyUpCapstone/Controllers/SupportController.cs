using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Support;
using TidyUpCapstone.ViewModels.Support;

namespace TidyUpCapstone.Controllers
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Help Center main page
        public IActionResult HelpCenter()
        {
            return View();
        }

        // Help pages
        public IActionResult GettingStarted()
        {
            return View();
        }

        public IActionResult SafetyTips()
        {
            return View();
        }

        // Contact Support - GET
        public IActionResult ContactUs()
        {
            var model = new ContactViewModel();
            return View(model);
        }

        // Contact Support - POST
        [HttpPost]
        public async Task<IActionResult> ContactUs(string name, string email, string category, string subject, string message)
        {
            try
            {
                // Log what we received
                Console.WriteLine($"Received: name={name}, email={email}, category={category}");

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(category) || string.IsNullOrEmpty(subject) ||
                    string.IsNullOrEmpty(message))
                {
                    return Json(new { success = false, message = "All fields are required." });
                }

                var contactMessage = new ContactMessage
                {
                    Name = name,
                    Email = email,
                    Category = category,
                    Subject = subject,
                    Message = message,
                    CreatedAt = DateTime.Now
                };

                _context.ContactMessages.Add(contactMessage);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Contact success page
        public IActionResult ContactSuccess()
        {
            return View();
        }
    }
}