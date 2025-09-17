using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.DTOs.Authentication;
using TidyUpCapstone.Models.DTOs.User;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.ViewModels.Account;
using TidyUpCapstone.Services.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace TidyUpCapstone.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            ILogger<AccountController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        // ===== UTILITY METHODS =====

        /// <summary>
        /// Get current Philippines time
        /// </summary>
        private DateTime GetPhilippinesTime()
        {
            try
            {
                var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippinesTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback for different OS or if timezone not found
                try
                {
                    var philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippinesTimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Final fallback - add 8 hours to UTC (Philippines is UTC+8)
                    _logger.LogWarning("Philippines timezone not found, using UTC+8 as fallback");
                    return DateTime.UtcNow.AddHours(8);
                }
            }
        }

        /// <summary>
        /// Generate a unique username for OAuth users
        /// </summary>
        private async Task<string> GenerateUniqueUsername(string email, string? firstName = null, string? lastName = null)
        {
            try
            {
                var baseUsername = string.Empty;

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    baseUsername = $"{firstName.ToLower()}.{lastName.ToLower()}";
                }
                else if (!string.IsNullOrEmpty(firstName))
                {
                    baseUsername = firstName.ToLower();
                }
                else
                {
                    baseUsername = email.Split('@')[0].ToLower();
                }

                // Clean the username
                baseUsername = Regex.Replace(baseUsername, @"[^a-zA-Z0-9\-\._]", "");

                if (string.IsNullOrWhiteSpace(baseUsername) || baseUsername.Length < 3)
                {
                    baseUsername = email.Split('@')[0].ToLower();
                }

                var username = baseUsername;
                var counter = 1;

                while (await _userManager.FindByNameAsync(username) != null)
                {
                    username = $"{baseUsername}{counter}";
                    counter++;
                }

                return username;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating unique username for email: {Email}", email);
                return email.Split('@')[0].ToLower() + DateTime.Now.Ticks.ToString();
            }
        }

        /// <summary>
        /// Send verification email
        /// </summary>
        private async Task SendVerificationEmail(string email, string firstName, string token)
        {
            try
            {
                var encodedToken = Uri.EscapeDataString(token);
                var encodedEmail = Uri.EscapeDataString(email);
                var verificationUrl = $"{Request.Scheme}://{Request.Host}/Account/VerifyEmail?email={encodedEmail}&token={encodedToken}";

                var subject = "Verify Your TidyUp Account";
                var body = $@"
                    <h2>Welcome to TidyUp, {firstName}!</h2>
                    <p>Thank you for creating your account. Please click the link below to verify your email address:</p>
                    <p><a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Verify Email</a></p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{verificationUrl}</p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't create this account, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>The TidyUp Team</p>
                ";

                await _emailService.SendEmailAsync(email, subject, body);
                _logger.LogInformation("Verification email sent successfully to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Log registration attempts
        /// </summary>
        private async Task LogRegistrationAttempt(int userId, string method, string status, string? ipAddress)
        {
            try
            {
                // Add your registration logging logic here if you have a logging table
                _logger.LogInformation("Registration logged - UserId: {UserId}, Method: {Method}, Status: {Status}, IP: {IP}",
                    userId, method, status, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log registration attempt for user: {UserId}", userId);
            }
        }

        // ===== LOGIN METHODS =====

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null, string? error = null)
        {
            // FIXED: Since you don't have Login.cshtml and use modals, redirect to home with modal trigger
            _logger.LogInformation("Login GET requested, redirecting to home with modal trigger. Error: {Error}", error);

            var routeValues = new RouteValueDictionary { { "showLogin", "true" } };

            if (!string.IsNullOrEmpty(returnUrl))
            {
                routeValues.Add("returnUrl", returnUrl);
            }

            if (!string.IsNullOrEmpty(error))
            {
                routeValues.Add("error", error);
            }

            return RedirectToAction("Index", "Home", routeValues);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualLogin(string Email, string Password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["ErrorMessage"] = "Email and password are required.";
                return RedirectToAction("Login", new { returnUrl });
            }

            try
            {
                _logger.LogInformation("Manual login attempt for email: {Email}", Email);

                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed - user not found: {Email}", Email);
                    TempData["ErrorMessage"] = "Invalid email or password.";
                    return RedirectToAction("Login", new { returnUrl });
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login failed - email not confirmed: {Email}", Email);
                    TempData["ErrorMessage"] = "Please verify your email before signing in.";
                    return RedirectToAction("Login", new { returnUrl });
                }

                var result = await _signInManager.PasswordSignInAsync(user, Password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Manual login successful for user: {UserId}", user.Id);

                    // Update last login
                    user.LastLogin = GetPhilippinesTime();
                    await _userManager.UpdateAsync(user);

                    // FIXED: Use consistent URL generation
                    var redirectUrl = returnUrl ?? Url.Action("Main", "Home");
                    _logger.LogInformation("Manual login successful, redirecting to: {RedirectUrl}", redirectUrl);
                    return LocalRedirect(redirectUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Login failed - account locked: {Email}", Email);
                    TempData["ErrorMessage"] = "Account is locked. Please try again later.";
                }
                else
                {
                    _logger.LogWarning("Login failed - invalid credentials: {Email}", Email);
                    TempData["ErrorMessage"] = "Invalid email or password.";
                }

                return RedirectToAction("Login", new { returnUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual login for email: {Email}", Email);
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return RedirectToAction("Login", new { returnUrl });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModalLogin(string Email, string Password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                return Json(new { success = false, message = "Email and password are required." });
            }

            try
            {
                _logger.LogInformation("Modal login attempt for email: {Email}", Email);

                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    _logger.LogWarning("Modal login failed - user not found: {Email}", Email);
                    return Json(new { success = false, message = "Invalid email or password." });
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Modal login failed - email not confirmed: {Email}", Email);
                    return Json(new { success = false, message = "Please verify your email before signing in." });
                }

                var result = await _signInManager.PasswordSignInAsync(user, Password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Modal login successful for user: {UserId}", user.Id);

                    // Update last login
                    user.LastLogin = GetPhilippinesTime();
                    await _userManager.UpdateAsync(user);

                    // FIXED: Generate proper action URL
                    var redirectUrl = returnUrl ?? Url.Action("Main", "Home");
                    _logger.LogInformation("Modal login successful, redirecting to: {RedirectUrl}", redirectUrl);

                    return Json(new { success = true, redirectUrl = redirectUrl });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Modal login failed - account locked: {Email}", Email);
                    return Json(new { success = false, message = "Account is locked. Please try again later." });
                }
                else
                {
                    _logger.LogWarning("Modal login failed - invalid credentials: {Email}", Email);
                    return Json(new { success = false, message = "Invalid email or password." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during modal login for email: {Email}", Email);
                return Json(new { success = false, message = "An error occurred during login. Please try again." });
            }
        }

        // ===== EXTERNAL LOGIN METHODS =====

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("External login initiated for provider: {Provider}", provider);

                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

                return new ChallengeResult(provider, properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating external login for provider: {Provider}", provider);
                TempData["ErrorMessage"] = "Failed to initiate external login. Please try again.";
                return RedirectToAction("Login", new { returnUrl });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError("External provider error: {Error}", remoteError);
                TempData["ErrorMessage"] = $"External provider error: {remoteError}";
                return RedirectToAction("Index", "Home", new { showLogin = true });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Error loading external login info");
                TempData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToAction("Index", "Home", new { showLogin = true });
            }

            try
            {
                _logger.LogInformation("Processing external login callback for provider: {Provider}", info.LoginProvider);

                // Try to sign in with existing external login
                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in with {Provider}", info.LoginProvider);

                    // Update LastLogin for existing OAuth users
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    if (!string.IsNullOrEmpty(email))
                    {
                        var existingUser = await _userManager.FindByEmailAsync(email);
                        if (existingUser != null)
                        {
                            existingUser.LastLogin = GetPhilippinesTime();
                            await _userManager.UpdateAsync(existingUser);
                        }
                    }

                    // FIXED: Generate proper action URL
                    var redirectUrl = returnUrl ?? Url.Action("Main", "Home");
                    _logger.LogInformation("OAuth login successful, redirecting to: {RedirectUrl}", redirectUrl);
                    return LocalRedirect(redirectUrl);
                }

                // If user doesn't exist, redirect to confirmation page
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var picture = info.Principal.FindFirstValue("picture") ?? info.Principal.FindFirstValue("avatar_url");

                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("Email claim not found in external login info from {Provider}", info.LoginProvider);
                    TempData["ErrorMessage"] = "Could not retrieve email from external provider.";
                    return RedirectToAction("Index", "Home", new { showLogin = true });
                }

                var model = new ExternalLoginConfirmationViewModel
                {
                    Email = userEmail,
                    FirstName = givenName ?? "",
                    LastName = surname ?? "",
                    Username = name ?? userEmail.Split('@')[0],
                    Provider = info.LoginProvider,
                    ProviderUserId = info.ProviderKey,
                    ProviderAvatarUrl = picture,
                    ReturnUrl = returnUrl
                };

                return View("ExternalLoginConfirmation", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during external login callback from {Provider}", info.LoginProvider);
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return RedirectToAction("Index", "Home", new { showLogin = true });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    ModelState.AddModelError("", "Error loading external login information.");
                    return View(model);
                }

                // Check for duplicate email across OAuth + Manual users
                var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmailUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);
                }

                // Generate unique username
                var cleanUsername = await GenerateUniqueUsername(model.Email, model.FirstName, model.LastName);

                var user = new AppUser
                {
                    UserName = cleanUsername,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true, // OAuth emails are pre-verified
                    DateCreated = GetPhilippinesTime(),
                    Status = "active",
                    TokenBalance = 0.00m,
                    LastLogin = GetPhilippinesTime(),
                    ProfilePictureUrl = model.ProviderAvatarUrl
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, false);

                    _logger.LogInformation("OAuth user {UserId} created account using {Provider}", user.Id, info.LoginProvider);

                    // Log registration attempt
                    await LogRegistrationAttempt(user.Id, info.LoginProvider, "Success",
                        Request.HttpContext.Connection.RemoteIpAddress?.ToString());

                    // FIXED: Generate proper action URL
                    var redirectUrl = model.ReturnUrl ?? Url.Action("Main", "Home");
                    _logger.LogInformation("OAuth registration successful, redirecting to: {RedirectUrl}", redirectUrl);
                    return LocalRedirect(redirectUrl);
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogError("OAuth user creation error: {Error}", error.Description);
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during external login confirmation for email: {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during account creation. Please try again.");
                return View(model);
            }
        }

        // ===== REGISTRATION METHODS =====

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.HideNavigation = true;
            ViewData["ReturnUrl"] = returnUrl;

            var model = new RegisterDto();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", model.Email);

                // Check for existing email
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed - email already exists: {Email}", model.Email);
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);
                }

                // Check for existing username
                var existingUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUsername != null)
                {
                    _logger.LogWarning("Registration failed - username already exists: {Username}", model.Username);
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View(model);
                }

                // Create user with better error handling
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false,
                    DateCreated = GetPhilippinesTime(),
                    Status = "pending_verification",
                    TokenBalance = 0.00m
                };

                _logger.LogInformation("Creating user account for: {Email}", model.Email);

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("User creation failed for {Email}. Errors: {@Errors}",
                        model.Email, result.Errors);

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                _logger.LogInformation("User created successfully: {UserId}", user.Id);

                // Send verification email with error handling
                try
                {
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await SendVerificationEmail(user.Email, user.FirstName, emailToken);
                    _logger.LogInformation("Verification email sent to: {Email}", user.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send verification email to: {Email}", user.Email);
                    // Don't fail registration if email sending fails
                }

                // Log registration attempt
                try
                {
                    await LogRegistrationAttempt(user.Id, "Manual", "Success",
                        Request.HttpContext.Connection.RemoteIpAddress?.ToString());
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to log registration attempt for user: {UserId}", user.Id);
                }

                TempData["SuccessMessage"] = "Account created successfully! Please check your email to verify your account.";
                return RedirectToAction("Login", new { returnUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModalRegister(RegisterDto model, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("Modal registration attempt for email: {Email}", model?.Email ?? "unknown");

                // Handle HTML checkbox values
                var request = HttpContext.Request;
                if (request.Form.ContainsKey("AcceptTerms"))
                {
                    var acceptTermsValue = request.Form["AcceptTerms"].ToString();
                    model.AcceptTerms = acceptTermsValue == "on" || acceptTermsValue == "true" || acceptTermsValue.Contains("true");
                }

                if (request.Form.ContainsKey("AcceptPrivacy"))
                {
                    var acceptPrivacyValue = request.Form["AcceptPrivacy"].ToString();
                    model.AcceptPrivacy = acceptPrivacyValue == "on" || acceptPrivacyValue == "true" || acceptPrivacyValue.Contains("true");
                }

                if (request.Form.ContainsKey("MarketingEmails"))
                {
                    var marketingEmailsValue = request.Form["MarketingEmails"].ToString();
                    model.MarketingEmails = marketingEmailsValue == "on" || marketingEmailsValue == "true" || marketingEmailsValue.Contains("true");
                }

                // Clear model state for these fields so validation runs on the corrected values
                ModelState.Remove("AcceptTerms");
                ModelState.Remove("AcceptPrivacy");
                ModelState.Remove("MarketingEmails");

                // Re-validate manually
                if (!model.AcceptTerms)
                {
                    ModelState.AddModelError("AcceptTerms", "You must accept the Terms of Service");
                }

                if (!model.AcceptPrivacy)
                {
                    ModelState.AddModelError("AcceptPrivacy", "You must accept the Privacy Policy");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Modal registration validation failed: {Errors}", string.Join("; ", errors));
                    return Json(new { success = false, message = string.Join("; ", errors) });
                }

                // Check email uniqueness across ALL users (OAuth + Manual)
                var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmailUser != null)
                {
                    if (existingEmailUser.EmailConfirmed)
                    {
                        _logger.LogWarning("Modal registration failed - email already exists and verified: {Email}", model.Email);
                        return Json(new { success = false, message = "This email address is already registered and verified." });
                    }
                    else
                    {
                        _logger.LogWarning("Modal registration failed - email already exists but not verified: {Email}", model.Email);
                        return Json(new { success = false, message = "This email is already registered but not verified. Please check your email or request a new verification link." });
                    }
                }

                // Check username uniqueness
                var existingUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUsername != null)
                {
                    _logger.LogWarning("Modal registration failed - username already exists: {Username}", model.Username);
                    return Json(new { success = false, message = "This username is already taken. Please choose another." });
                }

                // Create user
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false,
                    DateCreated = GetPhilippinesTime(),
                    Status = "pending_verification",
                    TokenBalance = 0.00m
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Modal registration user creation failed for {Email}: {Errors}", model.Email, errors);
                    return Json(new { success = false, message = errors });
                }

                _logger.LogInformation("Modal registration user created successfully: {UserId}", user.Id);

                // Send verification email
                try
                {
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await SendVerificationEmail(user.Email, user.FirstName, emailToken);
                    _logger.LogInformation("Verification email sent for modal registration: {Email}", user.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send verification email for modal registration: {Email}", user.Email);
                    // Continue with success response even if email fails
                }

                // Log registration
                try
                {
                    await LogRegistrationAttempt(user.Id, "Modal", "Success",
                        Request.HttpContext.Connection.RemoteIpAddress?.ToString());
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to log modal registration attempt for user: {UserId}", user.Id);
                }

                _logger.LogInformation("Modal registration successful for user: {UserId}", user.Id);

                // FIXED: Return success without automatic redirect for email verification
                return Json(new
                {
                    success = true,
                    message = "Account created successfully! Please check your email to verify your account before signing in.",
                    showLogin = true // This will trigger the login modal to open
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during modal registration for email: {Email}", model?.Email ?? "unknown");
                return Json(new { success = false, message = "An unexpected error occurred. Please try again." });
            }
        }

        // ===== EMAIL VERIFICATION =====

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Email verification failed - missing email or token");
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("Index", "Home", new { showLogin = true });
            }

            try
            {
                _logger.LogInformation("Email verification attempt for: {Email}", email);

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Email verification failed - user not found: {Email}", email);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "Home", new { showLogin = true });
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already verified for user: {Email}", email);
                    TempData["SuccessMessage"] = "Email is already verified. You can now sign in.";
                    return RedirectToAction("Index", "Home", new { showLogin = true });
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    user.Status = "active";
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);
                    TempData["SuccessMessage"] = "Email verified successfully! You can now sign in.";
                    return RedirectToAction("Index", "Home", new { showLogin = true });
                }
                else
                {
                    _logger.LogWarning("Email verification failed for user: {UserId}. Errors: {@Errors}", user.Id, result.Errors);
                    TempData["ErrorMessage"] = "Email verification failed. The link may be expired or invalid.";
                    return RedirectToAction("Index", "Home", new { showLogin = true });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for email: {Email}", email);
                TempData["ErrorMessage"] = "An error occurred during email verification.";
                return RedirectToAction("Index", "Home", new { showLogin = true });
            }
        }

        // ===== LOGOUT =====

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User {UserId} logged out successfully", userId);

                TempData["SuccessMessage"] = "You have been logged out successfully.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
        }

        // ===== ERROR HANDLING =====

        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.HideNavigation = true;
            ViewData["ReturnUrl"] = returnUrl;

            _logger.LogWarning("Access denied for return URL: {ReturnUrl}", returnUrl);
            return View();
        }
    }
}