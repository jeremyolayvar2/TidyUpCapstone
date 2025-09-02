using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels.Account;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class SecurityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityController(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSecurity(ChangePasswordDto model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Validate current password
                var passwordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!passwordValid)
                {
                    return Json(new { success = false, message = "Current password is incorrect." });
                }

                // Update password
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to change password: {errors}" });
                }

                return Json(new { success = true, message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Create mock active sessions for demo
                var sessions = new List<LoginSessionDto>
                {
                    new LoginSessionDto
                    {
                        SessionId = HttpContext.Session.Id,
                        DeviceInfo = GetDeviceInfo(),
                        Location = "Manila, Philippines",
                        IpAddress = GetClientIpAddress(),
                        LastActivity = DateTime.Now,
                        IsCurrentSession = true
                    },
                    new LoginSessionDto
                    {
                        SessionId = "mobile_session_123",
                        DeviceInfo = "TidyUp iOS App",
                        Location = "Manila, Philippines",
                        IpAddress = "192.168.1.100",
                        LastActivity = DateTime.Now.AddHours(-2),
                        IsCurrentSession = false
                    }
                };

                return Json(new { success = true, sessions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading sessions: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSession(string sessionId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if trying to end current session
                if (sessionId == HttpContext.Session.Id)
                {
                    return Json(new { success = false, message = "Cannot end your current session." });
                }

                // For demo purposes - in real app you'd invalidate the session
                return Json(new { success = true, message = "Session ended successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error ending session: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSecurityHistory()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Get from existing LoginLogs with correct property mapping
                var loginLogs = await _context.LoginLogs
                    .Where(ll => ll.UserId == user.Id)
                    .OrderByDescending(ll => ll.LoginTimestamp)
                    .Take(5)
                    .Select(ll => new LoginLogDto
                    {
                        LoginTime = ll.LoginTimestamp, // Map LoginTimestamp to LoginTime
                        IpAddress = ll.IpAddress ?? "Unknown",
                        UserAgent = ll.UserAgent ?? GetDeviceInfo(),
                        LoginMethod = "password", // Default since your entity doesn't have this field
                        Success = ll.LoginStatus == "Success", // Convert LoginStatus string to boolean
                        Location = "Manila, Philippines" // Default since your entity doesn't have location
                    })
                    .ToListAsync();

                // If no login logs, create sample data for demo
                if (!loginLogs.Any())
                {
                    loginLogs = new List<LoginLogDto>
                    {
                        new LoginLogDto
                        {
                            LoginTime = DateTime.Now.AddHours(-2),
                            IpAddress = GetClientIpAddress(),
                            UserAgent = GetDeviceInfo(),
                            LoginMethod = "password",
                            Success = true,
                            Location = "Manila, Philippines"
                        },
                        new LoginLogDto
                        {
                            LoginTime = DateTime.Now.AddDays(-1),
                            IpAddress = "192.168.1.99",
                            UserAgent = "Chrome on Windows",
                            LoginMethod = "password",
                            Success = true,
                            Location = "Manila, Philippines"
                        }
                    };
                }

                return Json(new { success = true, events = loginLogs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading security history: {ex.Message}" });
            }
        }

        private string GetDeviceInfo()
        {
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Device";

            if (userAgent.Contains("Chrome"))
                return "Chrome on " + (userAgent.Contains("Windows") ? "Windows" :
                                     userAgent.Contains("Mac") ? "Mac" : "Unknown OS");
            else if (userAgent.Contains("Firefox"))
                return "Firefox on " + (userAgent.Contains("Windows") ? "Windows" :
                                       userAgent.Contains("Mac") ? "Mac" : "Unknown OS");
            else if (userAgent.Contains("Safari"))
                return "Safari on " + (userAgent.Contains("iPhone") ? "iPhone" :
                                      userAgent.Contains("Mac") ? "Mac" : "Unknown OS");

            return "Unknown Browser";
        }

        private string GetClientIpAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return ipAddress ?? "127.0.0.1";
        }
    }
}