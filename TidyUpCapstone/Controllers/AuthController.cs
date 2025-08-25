using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthController(
            ApplicationDbContext context,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> TestLogin()
        {
            var user = await _userManager.FindByEmailAsync("test@tidyup.com");
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("SettingsPage", "Home");
            }
            return Content("Test user not found. Make sure your seeded user exists.");
        }

        [HttpGet]
        public async Task<IActionResult> RecreateTestUser()
        {
            var testUser = new AppUser
            {
                UserName = "testuser",
                Email = "test@tidyup.com",
                EmailConfirmed = true,
                DateCreated = DateTime.UtcNow,
                Status = "active"
            };

            var result = await _userManager.CreateAsync(testUser, "Test123!");
            if (result.Succeeded)
            {
                return Content("Test user recreated successfully! You can now use /Auth/TestLogin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Content($"Failed to recreate user: {errors}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CleanupAndRecreate()
        {
            try
            {
                // Find all users with test email or username
                var usersByEmail = await _userManager.FindByEmailAsync("test@tidyup.com");
                var usersByUsername = await _userManager.FindByNameAsync("testuser");

                var usersToDelete = new List<AppUser>();
                if (usersByEmail != null) usersToDelete.Add(usersByEmail);
                if (usersByUsername != null && !usersToDelete.Contains(usersByUsername))
                    usersToDelete.Add(usersByUsername);

                // Delete existing test users
                foreach (var user in usersToDelete)
                {
                    await CleanupUserData(user.Id);
                    await _userManager.DeleteAsync(user);
                }

                // Create fresh test user
                var testUser = new AppUser
                {
                    UserName = "testuser",
                    Email = "test@tidyup.com",
                    EmailConfirmed = true,
                    DateCreated = DateTime.UtcNow,
                    Status = "active"
                };

                var result = await _userManager.CreateAsync(testUser, "Test123!");
                if (result.Succeeded)
                {
                    return Content("Cleaned up old users and created fresh test user successfully!");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Content($"Failed to create new user: {errors}");
                }
            }
            catch (Exception ex)
            {
                return Content($"Error during cleanup: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Delete Account - Show confirmation page
        [HttpGet]
        [Authorize]
        public IActionResult DeleteAccount()
        {
            return View();
        }

        // Delete Account - Process deletion
        [HttpPost]
       // [Authorize]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteAccount(string confirmPassword, bool confirmDeletion)
        {
            Console.WriteLine($"=== ConfirmDeleteAccount called ===");
            Console.WriteLine($"confirmPassword: '{confirmPassword}'");
            Console.WriteLine($"confirmDeletion: {confirmDeletion}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return Json(new { success = false, message = "User not found." });
                }

                Console.WriteLine($"Found user: {user.Email}, ID: {user.Id}");

                // Verify checkbox confirmation
                if (!confirmDeletion)
                {
                    return Json(new { success = false, message = "Please confirm that you understand this action cannot be undone." });
                }

                // Verify password for security
                var passwordValid = await _userManager.CheckPasswordAsync(user, confirmPassword);
                Console.WriteLine($"Password validation result: {passwordValid}");

                if (!passwordValid)
                {
                    return Json(new { success = false, message = "Incorrect password. Please try again." });
                }

                // Clean up user data
                Console.WriteLine("Starting cleanup process...");
                await CleanupUserData(user.Id);
                Console.WriteLine("Cleanup completed");

                // Delete the user account
                Console.WriteLine("Deleting user account...");
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    Console.WriteLine($"User deletion failed: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
                    return Json(new { success = false, message = "Failed to delete account. Please contact support." });
                }
                Console.WriteLine("User account deleted successfully");

                // Sign out the user
                await _signInManager.SignOutAsync();

                // Clear session and cookies
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                

                return Json(new
                {
                    success = true,
                    message = "Your account has been successfully deleted.",
                    redirectUrl = Url.Action("Index", "Home")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ConfirmDeleteAccount: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Debug error: {ex.Message}" });
            }
        }

        private async Task CleanupUserData(int userId)
        {
            try
            {
                Console.WriteLine($"Starting cleanup for user ID: {userId}");

                // Get user email for contact message cleanup
                var userEmail = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                Console.WriteLine($"User email: {userEmail}");

                // Remove user's items
                var userItems = _context.Items.Where(i => i.UserId == userId);
                if (userItems.Any())
                {
                    _context.Items.RemoveRange(userItems);
                    Console.WriteLine($"Removed {userItems.Count()} items");
                }

                // Remove user's transactions
                var userTransactions = _context.Transactions.Where(t => t.BuyerId == userId || t.SellerId == userId);
                if (userTransactions.Any())
                {
                    _context.Transactions.RemoveRange(userTransactions);
                    Console.WriteLine($"Removed {userTransactions.Count()} transactions");
                }

                // Remove user's posts
                var userPosts = _context.Posts.Where(p => p.AuthorId == userId);
                if (userPosts.Any())
                {
                    _context.Posts.RemoveRange(userPosts);
                    Console.WriteLine($"Removed {userPosts.Count()} posts");
                }

                // Remove user's comments
                var userComments = _context.Comments.Where(c => c.UserId == userId);
                if (userComments.Any())
                {
                    _context.Comments.RemoveRange(userComments);
                    Console.WriteLine($"Removed {userComments.Count()} comments");
                }

                // Remove contact messages
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var userMessages = _context.ContactMessages.Where(c => c.Email == userEmail);
                    if (userMessages.Any())
                    {
                        _context.ContactMessages.RemoveRange(userMessages);
                        Console.WriteLine($"Removed {userMessages.Count()} contact messages");
                    }
                }

                // Remove user's notifications
                var userNotifications = _context.Notifications.Where(n => n.UserId == userId);
                if (userNotifications.Any())
                {
                    _context.Notifications.RemoveRange(userNotifications);
                    Console.WriteLine($"Removed {userNotifications.Count()} notifications");
                }

                // Remove user's quests and achievements
                var userQuests = _context.UserQuests.Where(uq => uq.UserId == userId);
                if (userQuests.Any())
                {
                    _context.UserQuests.RemoveRange(userQuests);
                    Console.WriteLine($"Removed {userQuests.Count()} user quests");
                }

                var userAchievements = _context.UserAchievements.Where(ua => ua.UserId == userId);
                if (userAchievements.Any())
                {
                    _context.UserAchievements.RemoveRange(userAchievements);
                    Console.WriteLine($"Removed {userAchievements.Count()} user achievements");
                }

                // Remove user's streaks
                var userStreaks = _context.UserStreaks.Where(us => us.UserId == userId);
                if (userStreaks.Any())
                {
                    _context.UserStreaks.RemoveRange(userStreaks);
                    Console.WriteLine($"Removed {userStreaks.Count()} user streaks");
                }

                // Remove user's visual purchases
                var userVisuals = _context.UserVisualsPurchases.Where(uvp => uvp.UserId == userId);
                if (userVisuals.Any())
                {
                    _context.UserVisualsPurchases.RemoveRange(userVisuals);
                    Console.WriteLine($"Removed {userVisuals.Count()} visual purchases");
                }

                // Remove user's location preferences
                var userLocationPref = _context.UserLocationPreferences.Where(ulp => ulp.UserId == userId);
                if (userLocationPref.Any())
                {
                    _context.UserLocationPreferences.RemoveRange(userLocationPref);
                    Console.WriteLine($"Removed {userLocationPref.Count()} location preferences");
                }

                // Remove user's privacy settings
                var userPrivacySettings = _context.UserPrivacySettings.Where(ups => ups.UserId == userId);
                if (userPrivacySettings.Any())
                {
                    _context.UserPrivacySettings.RemoveRange(userPrivacySettings);
                    Console.WriteLine($"Removed {userPrivacySettings.Count()} privacy settings");
                }

                // Save all changes
                Console.WriteLine("Saving cleanup changes...");
                await _context.SaveChangesAsync();
                Console.WriteLine("Cleanup data saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Log the error but don't prevent account deletion
                throw new Exception("Error cleaning up user data", ex);
            }
        }
    }
}