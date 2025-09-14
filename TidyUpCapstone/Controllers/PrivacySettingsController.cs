using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels;

namespace TidyUpCapstone.Controllers
{
    // [Authorize]
    public class PrivacySettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PrivacySettingsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdString = _userManager.GetUserId(User);
                if (int.TryParse(userIdString, out int userId))
                {
                    return userId;
                }
            }
            return 0; // Return 0 if not authenticated
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var settings = await _context.UserPrivacySettings
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (settings == null)
            {
                settings = new UserPrivacySettings { UserId = userId };
            }

            return Json(new
            {
                profileVisibility = settings.ProfileVisibility,
                locationVisibility = settings.LocationVisibility,
                activityStreaksVisibility = settings.ActivityStreaksVisibility,
                onlineStatus = settings.OnlineStatus,
                searchIndexing = settings.SearchIndexing,
                contactVisibility = settings.ContactVisibility,
                activityHistory = settings.ActivityHistory
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivacy(PrivacySettingsViewModel model)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0)
            {
                return BadRequest("User not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settings = await _context.UserPrivacySettings
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (settings == null)
            {
                settings = new UserPrivacySettings
                {
                    UserId = userId,
                    DateCreated = DateTime.UtcNow
                };
                _context.UserPrivacySettings.Add(settings);
            }

            // Direct mapping - no case conversion needed now
            settings.ProfileVisibility = model.ProfileVisibility;
            settings.LocationVisibility = model.LocationVisibility;
            settings.ActivityStreaksVisibility = model.ActivityStreaksVisibility;
            settings.OnlineStatus = model.OnlineStatus;
            settings.SearchIndexing = model.SearchIndexing;
            settings.ContactVisibility = model.ContactVisibility;
            settings.ActivityHistory = model.ActivityHistory;
            settings.DateUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Settings saved successfully");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == 0)
            {
                return Unauthorized();
            }

            if (!IsValidSettingValue(request.SettingName, request.Value))
            {
                return BadRequest(new { message = "Invalid setting value" });
            }

            var settings = await _context.UserPrivacySettings
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (settings == null)
            {
                settings = new UserPrivacySettings
                {
                    UserId = userId,
                    DateCreated = DateTime.UtcNow
                };
                _context.UserPrivacySettings.Add(settings);
            }

            switch (request.SettingName.ToLower())
            {
                case "profilevisibility":
                    settings.ProfileVisibility = request.Value;
                    break;
                case "locationvisibility":
                    settings.LocationVisibility = request.Value;
                    break;
                case "activitystreaksvisibility":
                    settings.ActivityStreaksVisibility = request.Value;
                    break;
                case "onlinestatus":
                    settings.OnlineStatus = request.Value;
                    break;
                case "searchindexing":
                    settings.SearchIndexing = request.Value;
                    break;
                case "contactvisibility":
                    settings.ContactVisibility = request.Value;
                    break;
                case "activityhistory":
                    settings.ActivityHistory = request.Value;
                    break;
                default:
                    return BadRequest(new { message = "Unknown setting" });
            }

            settings.DateUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Setting updated successfully" });
        }

        private bool IsValidSettingValue(string settingName, string value)
        {
            return settingName.ToLower() switch
            {
                "profilevisibility" or "contactvisibility" => value == "public" || value == "private",
                "locationvisibility" or "activitystreaksvisibility" or "onlinestatus" or "activityhistory" => value == "show" || value == "hide",
                "searchindexing" => value == "allow" || value == "block",
                _ => false
            };
        }
    }

    public class UpdateSettingRequest
    {
        public string SettingName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}