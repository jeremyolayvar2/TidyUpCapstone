using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.DTOs.User;
using TidyUpCapstone.Models.ViewModels.Account;
using System.ComponentModel.DataAnnotations;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment; // Added for file handling

        public SettingsController(UserManager<AppUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment; // Added dependency injection
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var profileDto = new UserProfileDto
            {
                UserId = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Location = user.Location,
                Birthday = user.Birthday,
                Gender = user.Gender,
                ProfilePictureUrl = user.ProfilePictureUrl,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                EmailConfirmed = user.EmailConfirmed,
                TokenBalance = user.TokenBalance,
                DateCreated = user.DateCreated,
                Status = Enum.Parse<UserStatus>(user.Status, true),
                LastLogin = user.LastLogin
            };

            var updateDto = new UpdateUserProfileDto
            {
                Username = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.PhoneNumber,
                Email = user.Email ?? string.Empty,
                Location = user.Location,
                Gender = user.Gender,
                Birthday = user.Birthday,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.ProfilePictureUrl
            };

            var viewModel = new ProfileViewModel
            {
                Profile = profileDto,
                UpdateProfile = updateDto,
                ShowPhoneVerification = !user.PhoneNumberConfirmed,
                CanEditProfile = true,
                ProfileImageUrl = user.ProfilePictureUrl
            };

            return View(viewModel);
        }

        // POST: Settings/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto model, IFormFile ProfilePicture)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check your input and try again.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Handle profile picture upload
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(ProfilePicture.ContentType.ToLower()))
                {
                    TempData["ErrorMessage"] = "Please upload a valid image file (JPEG, PNG, or GIF).";
                    return RedirectToAction("Index");
                }

                // Validate file size (5MB limit)
                if (ProfilePicture.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File size must be less than 5MB.";
                    return RedirectToAction("Index");
                }

                try
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
                    Directory.CreateDirectory(uploadsDir);

                    // Delete old profile picture if it exists
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Generate unique filename
                    var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfilePicture.CopyToAsync(stream);
                    }

                    // Update user's profile picture URL
                    user.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Failed to upload profile picture. Please try again.";
                    // Log the exception if you have logging set up
                    Console.WriteLine($"Profile picture upload error: {ex.Message}");
                    return RedirectToAction("Index");
                }
            }

            // Update other user fields (only editable fields based on your requirements)
            user.PhoneNumber = model.Phone;
            user.Location = model.Location;
            user.Gender = model.Gender;
            user.Birthday = model.Birthday;

            // Handle profile picture URL if provided (for cases without file upload)
            if (!string.IsNullOrEmpty(model.AvatarUrl) && ProfilePicture == null)
            {
                user.ProfilePictureUrl = model.AvatarUrl;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction("Index");
        }

        // POST: Settings/SendVerificationCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationCode([Required] string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return Json(new { success = false, message = "Phone number is required." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Generate 6-digit verification code
            var verificationCode = GenerateVerificationCode();

            // Store code and expiry (5 minutes from now)
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // TODO: Replace with actual SMS service - for now just log to console
                Console.WriteLine($"SMS Verification Code for {phoneNumber}: {verificationCode}");

                return Json(new { success = true, message = "Verification code sent successfully!" });
            }

            return Json(new { success = false, message = "Failed to send verification code." });
        }

        // POST: Settings/VerifyPhone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone([Required] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Verification code is required." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Check if code matches and hasn't expired
            if (user.VerificationCode != code)
            {
                return Json(new { success = false, message = "Invalid verification code." });
            }

            if (user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return Json(new { success = false, message = "Verification code has expired." });
            }

            // Mark phone as verified and clear verification code
            user.PhoneNumberConfirmed = true;
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Phone number verified successfully!" });
            }

            return Json(new { success = false, message = "Failed to verify phone number." });
        }

        // Helper method to generate 6-digit verification code
        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // POST: Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please check your input." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Password changed successfully!" });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = errors });
        }
    }
}