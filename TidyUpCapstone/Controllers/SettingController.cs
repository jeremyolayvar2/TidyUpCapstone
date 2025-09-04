using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.User;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels.Account;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService _languageService;

        public SettingsController(UserManager<AppUser> userManager, IWebHostEnvironment environment, ApplicationDbContext context, ILanguageService languageService)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
            _languageService = languageService;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // If no authenticated user, try to get testuser for development
                user = await _userManager.FindByEmailAsync("test@tidyup.com");
                if (user == null)
                {
                    return RedirectToAction("TestLogin", "Auth");
                }
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
                AvatarUrl = user.ProfilePictureUrl, // Map to AvatarUrl as well
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                EmailConfirmed = user.EmailConfirmed,
                TokenBalance = user.TokenBalance,
                DateCreated = user.DateCreated,
                Status = Enum.Parse<UserStatus>(user.Status, true),
                LastLogin = user.LastLogin,
                IsVerified = user.EmailConfirmed && user.PhoneNumberConfirmed,
                Role = UserRole.User, // Default to User role
                TwoFactorEnabled = false, // Add when you implement 2FA
                RegistrationMethod = RegistrationMethod.Email, // Default
                MarketingEmailsEnabled = false // Default
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

        // POST: Settings/UpdateProfile - Fixed to match business requirements
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto model, IFormFile ProfilePicture)
        {
            try
            {
                Console.WriteLine("=== UpdateProfile Debug ===");
                Console.WriteLine($"Username: '{model.Username}' (Length: {model.Username?.Length})");
                Console.WriteLine($"FirstName: '{model.FirstName}'");
                Console.WriteLine($"LastName: '{model.LastName}'");
                Console.WriteLine($"Phone: '{model.Phone}'");
                Console.WriteLine($"Email: '{model.Email}'");
                Console.WriteLine($"Location: '{model.Location}'");
                Console.WriteLine($"Gender: '{model.Gender}'");
                Console.WriteLine($"Birthday: '{model.Birthday}'");
                Console.WriteLine($"ProfilePicture: {(ProfilePicture != null ? ProfilePicture.FileName : "null")}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== Validation Errors ===");
                    foreach (var error in ModelState)
                    {
                        foreach (var err in error.Value.Errors)
                        {
                            Console.WriteLine($"Field '{error.Key}': {err.ErrorMessage}");
                        }
                    }
                    TempData["ErrorMessage"] = "Please check your input and try again.";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Fallback to testuser for development
                    user = await _userManager.FindByEmailAsync("test@tidyup.com");
                    if (user == null)
                    {
                        Console.WriteLine("User not found");
                        return NotFound();
                    }
                }

                Console.WriteLine($"Found user: {user.UserName}");

                // Handle profile picture upload
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    Console.WriteLine("Processing profile picture upload...");

                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(ProfilePicture.ContentType.ToLower()))
                    {
                        Console.WriteLine($"Invalid file type: {ProfilePicture.ContentType}");
                        TempData["ErrorMessage"] = "Please upload a valid image file (JPEG, PNG, or GIF).";
                        return RedirectToAction("Index");
                    }

                    // Validate file size (5MB limit)
                    if (ProfilePicture.Length > 5 * 1024 * 1024)
                    {
                        Console.WriteLine($"File too large: {ProfilePicture.Length} bytes");
                        TempData["ErrorMessage"] = "File size must be less than 5MB.";
                        return RedirectToAction("Index");
                    }

                    try
                    {
                        // Create uploads directory if it doesn't exist
                        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
                        Directory.CreateDirectory(uploadsDir);
                        Console.WriteLine($"Upload directory: {uploadsDir}");

                        // Delete old profile picture if it exists
                        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                        {
                            var oldImagePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine($"Deleted old image: {oldImagePath}");
                            }
                        }

                        // Generate unique filename
                        var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(ProfilePicture.FileName)}";
                        var filePath = Path.Combine(uploadsDir, fileName);
                        Console.WriteLine($"Saving to: {filePath}");

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfilePicture.CopyToAsync(stream);
                        }

                        // Update user's profile picture URL
                        user.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
                        Console.WriteLine($"Updated ProfilePictureUrl: {user.ProfilePictureUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Profile picture upload error: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        TempData["ErrorMessage"] = "Failed to upload profile picture. Please try again.";
                        return RedirectToAction("Index");
                    }
                }

                // FIXED: Only update EDITABLE fields based on business requirements
                Console.WriteLine("Updating user fields (EDITABLE FIELDS ONLY)...");

                // READONLY AFTER SIGNUP - DO NOT UPDATE:
                // user.FirstName = model.FirstName;  // REMOVED
                // user.LastName = model.LastName;    // REMOVED  
                // user.Email = model.Email;          // REMOVED
                // user.UserName = model.Username;    // REMOVED

                // EDITABLE FIELDS - UPDATE ALLOWED:
                user.PhoneNumber = model.Phone;
                user.Location = model.Location;
                user.Gender = model.Gender;
                user.Birthday = model.Birthday;

                // Handle profile picture URL if provided (for cases without file upload)
                if (!string.IsNullOrEmpty(model.AvatarUrl) && ProfilePicture == null)
                {
                    user.ProfilePictureUrl = model.AvatarUrl;
                    Console.WriteLine($"Updated ProfilePictureUrl from AvatarUrl: {user.ProfilePictureUrl}");
                }

                Console.WriteLine("Calling _userManager.UpdateAsync...");
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    Console.WriteLine("User update succeeded");
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
                else
                {
                    Console.WriteLine("User update failed:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  - {error.Description}");
                    }
                    TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                Console.WriteLine("Redirecting to Index");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateProfile Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                return RedirectToAction("Index");
            }
        }

        // GET: Get Profile Data for JS (NEW METHOD for your JS)
        [HttpGet]
        public async Task<IActionResult> GetProfileData()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Fallback to testuser for development
                    user = await _userManager.FindByEmailAsync("test@tidyup.com");
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" });
                    }
                }

                var profileData = new
                {
                    success = true,
                    profile = new UserProfileDto
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
                        AvatarUrl = user.ProfilePictureUrl,
                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                        EmailConfirmed = user.EmailConfirmed,
                        TokenBalance = user.TokenBalance,
                        DateCreated = user.DateCreated,
                        Status = Enum.Parse<UserStatus>(user.Status, true),
                        LastLogin = user.LastLogin,
                        IsVerified = user.EmailConfirmed && user.PhoneNumberConfirmed,
                        Role = UserRole.User,
                        TwoFactorEnabled = false,
                        RegistrationMethod = RegistrationMethod.Email,
                        MarketingEmailsEnabled = false
                    },
                    // Add phone separately since it's not in the DTO
                    phoneNumber = user.PhoneNumber ?? string.Empty
                };

                return Json(profileData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProfileData Exception: {ex.Message}");
                return Json(new { success = false, message = "Error loading profile data" });
            }
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
                // Fallback to testuser for development
                user = await _userManager.FindByEmailAsync("test@tidyup.com");
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
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
                // Fallback to testuser for development
                user = await _userManager.FindByEmailAsync("test@tidyup.com");
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
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
                // Fallback to testuser for development
                user = await _userManager.FindByEmailAsync("test@tidyup.com");
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Password changed successfully!" });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message = errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLanguage(string Language, string Timezone, bool HighContrast = false, bool LargeText = false, bool ReduceMotion = false, bool ScreenReader = false)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Fallback to testuser for development
                    user = await _userManager.FindByEmailAsync("test@tidyup.com");
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found." });
                    }
                }

                if (string.IsNullOrEmpty(Language) || string.IsNullOrEmpty(Timezone))
                {
                    return Json(new { success = false, message = "Language and timezone are required." });
                }

                user.Language = Language;
                user.Timezone = Timezone;
                user.HighContrast = HighContrast;
                user.LargeText = LargeText;
                user.ReduceMotion = ReduceMotion;
                user.ScreenReader = ScreenReader;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Language & Accessibility settings updated successfully!" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to update settings: {errors}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating your settings. Please try again." });
            }
        }

        // GET: Get current language and accessibility settings
        [HttpGet]
        public async Task<IActionResult> GetLanguageSettings()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Fallback to testuser for development
                    user = await _userManager.FindByEmailAsync("test@tidyup.com");
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found." });
                    }
                }

                Console.WriteLine($"=== DEBUG GetLanguageSettings ===");
                Console.WriteLine($"User ID: {user.Id}");
                Console.WriteLine($"Language: '{user.Language}'");
                Console.WriteLine($"Timezone: '{user.Timezone}'");
                Console.WriteLine($"HighContrast: {user.HighContrast}");
                Console.WriteLine($"LargeText: {user.LargeText}");
                Console.WriteLine($"ReduceMotion: {user.ReduceMotion}");
                Console.WriteLine($"ScreenReader: {user.ScreenReader}");

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        language = user.Language,
                        timezone = user.Timezone,
                        highContrast = user.HighContrast,
                        largeText = user.LargeText,
                        reduceMotion = user.ReduceMotion,
                        screenReader = user.ScreenReader
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLanguageSettings Exception: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while loading settings." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTranslations(string languageCode = "en")
        {
            try
            {
                var translations = await _languageService.GetTranslationsAsync(languageCode);
                return Json(new { success = true, translations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load translations" });
            }
        }
    }
}