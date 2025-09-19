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
using TidyUpCapstone.Helpers;


namespace TidyUpCapstone.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService _languageService;
        private readonly SignInManager<AppUser> _signInManager;

        public SettingsController(UserManager<AppUser> userManager, IWebHostEnvironment environment, ApplicationDbContext context, ILanguageService languageService, SignInManager<AppUser> signInManager) : base(userManager)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
            _languageService = languageService;
            _signInManager = signInManager;
            
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
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

            // FIXED: Include properties that exist in UpdateUserProfileDto
            var updateDto = new UpdateUserProfileDto
            {
                Username = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Phone = user.PhoneNumber, // Map to Phone as well for compatibility
                Location = user.Location,
                Gender = user.Gender,
                Birthday = user.Birthday,
                AvatarUrl = user.ProfilePictureUrl,
                MarketingEmailsEnabled = false // Default value
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

        // POST: Settings/UpdateProfile - PROPERLY FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto model, IFormFile ProfilePicture)
        {
            try
            {
                Console.WriteLine("=== ENHANCED DEBUG ===");
                Console.WriteLine($"Request received at: {DateTime.Now}");
                Console.WriteLine($"Model is null: {model == null}");

                if (model != null)
                {
                    Console.WriteLine($"Username: '{model.Username}' (null: {model.Username == null})");
                    Console.WriteLine($"Phone: '{model.Phone}' (null: {model.Phone == null})");
                    Console.WriteLine($"PhoneNumber: '{model.PhoneNumber}' (null: {model.PhoneNumber == null})");
                    Console.WriteLine($"Location: '{model.Location}' (null: {model.Location == null})");
                    Console.WriteLine($"Gender: '{model.Gender}' (null: {model.Gender == null})");
                    Console.WriteLine($"Birthday: '{model.Birthday}' (null: {model.Birthday == null})");
                    Console.WriteLine($"MarketingEmailsEnabled: {model.MarketingEmailsEnabled}");
                    Console.WriteLine($"AvatarUrl: '{model.AvatarUrl}' (null: {model.AvatarUrl == null})");
                }

                Console.WriteLine($"ProfilePicture: {(ProfilePicture != null ? ProfilePicture.FileName : "null")}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== DETAILED Validation Errors ===");
                    foreach (var kvp in ModelState)
                    {
                        Console.WriteLine($"Key: '{kvp.Key}', HasErrors: {kvp.Value.Errors.Count > 0}");
                        foreach (var error in kvp.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                Console.WriteLine($"✅ Found user: {user.UserName} (ID: {user.Id})");

                // HANDLE PROFILE PICTURE UPLOAD
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    Console.WriteLine("=== Profile Picture Upload Debug ===");
                    Console.WriteLine($"File name: {ProfilePicture.FileName}");
                    Console.WriteLine($"File size: {ProfilePicture.Length} bytes");
                    Console.WriteLine($"Content type: {ProfilePicture.ContentType}");

                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(ProfilePicture.ContentType.ToLower()))
                    {
                        Console.WriteLine($"❌ Invalid file type: {ProfilePicture.ContentType}");
                        return Json(new { success = false, message = "Please upload a valid image file (JPEG, PNG, or GIF)." });
                    }

                    // Validate file size (5MB limit)
                    if (ProfilePicture.Length > 5 * 1024 * 1024)
                    {
                        Console.WriteLine($"❌ File too large: {ProfilePicture.Length} bytes");
                        return Json(new { success = false, message = "File size must be less than 5MB." });
                    }

                    try
                    {
                        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
                        Console.WriteLine($"Upload directory path: {uploadsDir}");
                        Console.WriteLine($"Directory exists: {Directory.Exists(uploadsDir)}");

                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                            Console.WriteLine($"✅ Created directory: {uploadsDir}");
                        }

                        // Delete old profile picture if it exists
                        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                        {
                            var oldImagePath = Path.Combine(_environment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                            Console.WriteLine($"Attempting to delete old image: {oldImagePath}");
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                Console.WriteLine($"✅ Deleted old image successfully");
                            }
                        }

                        var fileName = UserHelper.GenerateProfilePictureFileName(user.Id, ProfilePicture.FileName);
                        var filePath = Path.Combine(uploadsDir, fileName);
                        Console.WriteLine($"Full file path: {filePath}");

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfilePicture.CopyToAsync(stream);
                        }

                        Console.WriteLine($"✅ File saved successfully to: {filePath}");
                        Console.WriteLine($"File exists after save: {System.IO.File.Exists(filePath)}");

                        if (System.IO.File.Exists(filePath))
                        {
                            var fileInfo = new FileInfo(filePath);
                            Console.WriteLine($"Saved file size: {fileInfo.Length} bytes");
                        }

                        user.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
                        Console.WriteLine($"✅ Updated ProfilePictureUrl: {user.ProfilePictureUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ File upload error: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        return Json(new { success = false, message = "Failed to upload profile picture. Please try again." });
                    }
                }

                // UPDATE USER FIELDS - PROPERLY HANDLE ALL FIELDS
                Console.WriteLine("=== BEFORE UPDATE VALUES ===");
                Console.WriteLine($"Original - Username: '{user.UserName}', Phone: '{user.PhoneNumber}', Location: '{user.Location}', Gender: '{user.Gender}', Birthday: '{user.Birthday}'");

                // Update username if provided and valid
                if (!string.IsNullOrEmpty(model?.Username) && model.Username.Length >= 3)
                {
                    user.UserName = model.Username;
                }

                // Use Phone field first, then fall back to PhoneNumber
                user.PhoneNumber = model?.Phone ?? model?.PhoneNumber;
                user.Location = model?.Location;
                user.Gender = model?.Gender;
                user.Birthday = model?.Birthday;

                Console.WriteLine("=== AFTER SETTING VALUES ===");
                Console.WriteLine($"Updated - Username: '{user.UserName}', Phone: '{user.PhoneNumber}', Location: '{user.Location}', Gender: '{user.Gender}', Birthday: '{user.Birthday}'");

                Console.WriteLine("Calling _userManager.UpdateAsync...");
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    Console.WriteLine("✅ User update succeeded");

                    // 🔥 ADD THIS: Refresh authentication claims for global avatar sync
                    await _signInManager.RefreshSignInAsync(user);
                    Console.WriteLine("✅ Authentication claims refreshed");

                    return Json(new
                    {
                        success = true,
                        message = "Profile updated successfully!",
                        profilePictureUrl = user.ProfilePictureUrl  // Add this for JavaScript
                    });
                }
                else
                {
                    Console.WriteLine("❌ User update failed:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  - Code: {error.Code}, Description: {error.Description}");
                    }
                    return Json(new { success = false, message = $"Update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CRITICAL EXCEPTION in UpdateProfile:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        // GET: Get Profile Data for JS
        [HttpGet]
        public async Task<IActionResult> GetProfileData()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync("test@tidyup.com");
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" });
                    }
                }

                // ADD THIS DEBUG LOGGING
                Console.WriteLine("=== GetProfileData Debug ===");
                Console.WriteLine($"User ID: {user.Id}");
                Console.WriteLine($"Raw Database Values:");
                Console.WriteLine($"  PhoneNumber: '{user.PhoneNumber}'");
                Console.WriteLine($"  Location: '{user.Location}'");
                Console.WriteLine($"  Gender: '{user.Gender}'");
                Console.WriteLine($"  Birthday: '{user.Birthday}'");
                Console.WriteLine($"  FirstName: '{user.FirstName}'");
                Console.WriteLine($"  LastName: '{user.LastName}'");

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
                    phoneNumber = user.PhoneNumber ?? string.Empty
                };

                Console.WriteLine("=== JSON Response Debug ===");
                Console.WriteLine($"profile.Location: '{profileData.profile.Location}'");
                Console.WriteLine($"profile.Gender: '{profileData.profile.Gender}'");
                Console.WriteLine($"profile.Birthday: '{profileData.profile.Birthday}'");
                Console.WriteLine($"phoneNumber: '{profileData.phoneNumber}'");


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
                user = await _userManager.FindByEmailAsync("test@tidyup.com");
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
            }

            var verificationCode = GenerateVerificationCode();

            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
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
                user = await _userManager.FindByEmailAsync("test@tidyup.com");
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }
            }

            if (user.VerificationCode != code)
            {
                return Json(new { success = false, message = "Invalid verification code." });
            }

            if (user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                return Json(new { success = false, message = "Verification code has expired." });
            }

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

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // POST: Settings/ChangePassword - FIXED TO USE EXISTING DTO FROM AccountViewModel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            try
            {
                Console.WriteLine("=== CHANGE PASSWORD DEBUG ===");
                Console.WriteLine($"Model validation: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("; ", errors) });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                Console.WriteLine($"✅ Found user: {user.UserName} (ID: {user.Id})");

                // Verify current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    Console.WriteLine("❌ Current password is invalid");
                    return Json(new { success = false, message = "Current password is incorrect." });
                }

                Console.WriteLine("✅ Current password verified");

                // FIXED: Use the existing DTO structure with ConfirmNewPassword
                // Note: The validation will ensure NewPassword matches ConfirmNewPassword
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    Console.WriteLine("✅ Password changed successfully");

                    // Update security stamp to invalidate other sessions (optional)
                    await _userManager.UpdateSecurityStampAsync(user);

                    return Json(new { success = true, message = "Password changed successfully!" });
                }
                else
                {
                    Console.WriteLine("❌ Password change failed:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  - Code: {error.Code}, Description: {error.Description}");
                    }

                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = errors });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION in ChangePassword:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while changing the password. Please try again." });
            }
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

        [HttpGet]
        public async Task<IActionResult> GetLanguageSettings()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
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