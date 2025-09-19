using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Notifications;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class NotificationSettingsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NotificationSettingsController(ApplicationDbContext context, UserManager<AppUser> userManager) : base(userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotifications(
            bool EmailNewMessages = false,
            bool EmailItemUpdates = false,
            bool EmailWeeklySummary = false,
            bool DesktopNotifications = false)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new { success = false });

                var settings = await _context.NotificationSettings
                    .FirstOrDefaultAsync(ns => ns.UserId == user.Id);

                if (settings == null)
                {
                    settings = new NotificationSettings { UserId = user.Id };
                    _context.NotificationSettings.Add(settings);
                }

                settings.EmailNewMessages = EmailNewMessages;
                settings.EmailItemUpdates = EmailItemUpdates;
                settings.EmailWeeklySummary = EmailWeeklySummary;
                settings.DesktopNotifications = DesktopNotifications;
                settings.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}