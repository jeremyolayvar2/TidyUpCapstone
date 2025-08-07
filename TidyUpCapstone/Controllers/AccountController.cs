using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TidyUpCapstone.Data;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities.SSO;
using TidyUpCapstone.Models.Entities.Core;
using TidyUpCapstone.Models.ViewModels.Account;
using TidyUpCapstone.Models.DTOs.Authentication;
using TidyUpCapstone.Controllers;
using TidyUpCapstone.Services.Interfaces;
using TidyUpCapstone.Services;

namespace TidyUpCapstone.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDbContext context, ILogger<AccountController> logger, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // ===== MODAL LOGIN/REGISTER ENDPOINTS =====

        /// <summary>
        /// Handle modal login requests - returns JSON for AJAX
        /// </summary>
        /// <summary>
        /// Handle modal login requests - returns JSON for AJAX - FIXED REDIRECT
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModalLogin([FromForm] string Email, [FromForm] string Password, [FromForm] bool RememberMe = false, [FromForm] string? returnUrl = null)
        {
            _logger.LogInformation("=== MODAL LOGIN DEBUG ===");
            _logger.LogInformation("Email: {Email}", Email);
            _logger.LogInformation("Password length: {Length}", Password?.Length ?? 0);

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return Json(new { success = false, message = "Email and password are required." });
            }

            // Check if user exists and email is verified
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return Json(new { success = false, message = "Invalid email or password." });
            }

            if (!user.EmailConfirmed)
            {
                return Json(new { success = false, message = "Please verify your email before logging in. Check your inbox for verification link." });
            }

            // Try login with email first
            var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                // Try with username if email didn't work
                var usernameResult = await _signInManager.PasswordSignInAsync(user.UserName, Password, RememberMe, lockoutOnFailure: false);
                result = usernameResult;
            }

            if (result.Succeeded)
            {
                // Update LastLogin timestamp (Philippines time)
                user.LastLogin = GetPhilippinesTime();
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in successfully", Email);

                // FIXED: Use proper action redirect instead of tilde path
                return Json(new { success = true, redirectUrl = returnUrl ?? "/Home/Main" });
            }

            return Json(new { success = false, message = "Invalid email or password." });
        }

        /// <summary>
        /// Handle modal registration requests - returns JSON for AJAX
        /// </summary>

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModalRegister(RegisterDto model, string? returnUrl = null)
        {
            // QUICK FIX: Handle "on" values from HTML checkboxes
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
                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            try
            {
                // 1. Check email uniqueness across ALL users (OAuth + Manual)
                var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmailUser != null)
                {
                    if (existingEmailUser.EmailConfirmed)
                    {
                        return Json(new { success = false, message = "This email address is already registered and verified." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "This email is already registered but not verified. Check your email for verification link." });
                    }
                }

                // 2. Check username uniqueness
                var existingUsernameUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUsernameUser != null)
                {
                    return Json(new { success = false, message = "This username is already taken. Please choose another one." });
                }

                // 3. Create new user account (inactive until email verified)
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false,  // CRITICAL: User cannot login until verified
                    DateCreated = GetPhilippinesTime(), // Use Philippines time like other methods
                    Status = "pending_verification",
                    TokenBalance = 0.00m
                };

                // 4. Create user with password
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return Json(new { success = false, message = string.Join("; ", errors) });
                }

                // 5. Generate email verification token and send email
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await SendVerificationEmail(user.Email, user.FirstName, emailToken);

                // 6. Log registration attempt
                await LogRegistrationAttempt(user.Id, "Manual", "Success",
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString());

                _logger.LogInformation("Manual user {UserId} registered with email {Email}. Verification email sent.",
                    user.Id, user.Email);

                return Json(new
                {
                    success = true,
                    message = "Account created successfully! Please check your email to verify your account before signing in.",
                    redirectUrl = "/" // Redirect to home page with success message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during modal registration for email {Email}", model.Email);
                return Json(new { success = false, message = "An error occurred during registration. Please try again." });
            }
        }

        // ===== KEEP EXISTING METHODS FOR BACKWARD COMPATIBILITY =====

        // ✅ LOGIN VIEW (Enhanced to support both OAuth + Manual) - Keep for direct access
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                GoogleEnabled = true,
                FacebookEnabled = false
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualLogin(string Email, string Password, bool RememberMe = false, string? returnUrl = null)
        {
            _logger.LogInformation("=== MANUAL LOGIN DEBUG ===");
            _logger.LogInformation("Email: {Email}", Email);
            _logger.LogInformation("Password length: {Length}", Password?.Length ?? 0);

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                _logger.LogWarning("Empty email or password");
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("Login", new LoginViewModel
                {
                    ReturnUrl = returnUrl,
                    GoogleEnabled = true,
                    FacebookEnabled = false
                });
            }

            // Check if user exists and email is verified
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", Email);
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login", new LoginViewModel
                {
                    ReturnUrl = returnUrl,
                    GoogleEnabled = true,
                    FacebookEnabled = false
                });
            }

            _logger.LogInformation("User found - ID: {UserId}, Username: {Username}, EmailConfirmed: {EmailConfirmed}",
                user.Id, user.UserName, user.EmailConfirmed);

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Email not confirmed for user: {Email}", Email);
                ViewBag.ErrorMessage = "Please verify your email before logging in. Check your inbox for verification link.";
                return View("Login", new LoginViewModel
                {
                    ReturnUrl = returnUrl,
                    GoogleEnabled = true,
                    FacebookEnabled = false
                });
            }

            // Try login with email first
            var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: false);
            _logger.LogInformation("PasswordSignInAsync with email - Succeeded: {Succeeded}, IsLockedOut: {IsLockedOut}, RequiresTwoFactor: {RequiresTwoFactor}",
                result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor);

            if (!result.Succeeded)
            {
                // Try with username if email didn't work
                var usernameResult = await _signInManager.PasswordSignInAsync(user.UserName, Password, RememberMe, lockoutOnFailure: false);
                _logger.LogInformation("PasswordSignInAsync with username - Succeeded: {Succeeded}, IsLockedOut: {IsLockedOut}, RequiresTwoFactor: {RequiresTwoFactor}",
                    usernameResult.Succeeded, usernameResult.IsLockedOut, usernameResult.RequiresTwoFactor);

                result = usernameResult;
            }

            if (result.Succeeded)
            {
                // Update LastLogin timestamp (Philippines time)
                user.LastLogin = GetPhilippinesTime();
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in successfully", Email);
                return LocalRedirect(returnUrl ?? "/Home/Main"); // FIXED: Changed from "~/Home/Main"
            }

            _logger.LogWarning("Login failed for user: {Email}", Email);
            ViewBag.ErrorMessage = "Invalid email or password.";
            return View("Login", new LoginViewModel
            {
                ReturnUrl = returnUrl,
                GoogleEnabled = true,
                FacebookEnabled = false
            });
        }

        // ✅ GOOGLE / FACEBOOK LOGIN CHALLENGE (Preserved exactly)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // ✅ CALLBACK AFTER GOOGLE LOGIN (Preserved + Enhanced with FirstName/LastName)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                _logger.LogError("External provider error: {Error}", remoteError);
                return RedirectToAction("Index", "Home", new { error = $"External provider error: {remoteError}" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Index", "Home", new { error = "Error loading external login info." });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            if (result.Succeeded)
            {
                // Update LastLogin for existing OAuth users (Philippines time)
                var existingUser = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                if (existingUser != null)
                {
                    existingUser.LastLogin = GetPhilippinesTime();
                    await _userManager.UpdateAsync(existingUser);
                }
                return LocalRedirect("/Home/Main"); // FIXED: Changed from "~/Home/Main"
            }

            // If no account exists, show confirmation page
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
            {
                Email = email ?? "",
                FirstName = givenName ?? "",
                LastName = surname ?? "",
                Username = name ?? email ?? "",
                Provider = info.LoginProvider,
                ProviderUserId = info.ProviderKey,
                ProviderDisplayName = name,
                ReturnUrl = returnUrl
            });
        }

        // ✅ CONFIRM OAUTH REGISTRATION (Enhanced to save FirstName/LastName)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", "Error loading external login info.");
                return View(model);
            }

            // Check for duplicate email across OAuth + Manual users
            var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmailUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            // Clean the username - remove invalid characters
            var cleanUsername = System.Text.RegularExpressions.Regex.Replace(
                model.Username ?? model.Email,
                @"[^a-zA-Z0-9\-\._@\+ ]",
                ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(cleanUsername) || cleanUsername.Length < 3)
            {
                cleanUsername = model.Email;
            }

            // Check username uniqueness
            var existingUsernameUser = await _userManager.FindByNameAsync(cleanUsername);
            if (existingUsernameUser != null)
            {
                // Generate unique username for OAuth users
                cleanUsername = await GenerateUniqueUsername(model.Email, model.FirstName, model.LastName);
            }

            var user = new AppUser
            {
                UserName = cleanUsername,
                Email = model.Email,
                FirstName = model.FirstName,  // ✅ Now saves FirstName
                LastName = model.LastName,    // ✅ Now saves LastName
                EmailConfirmed = true,        // OAuth emails are pre-verified
                DateCreated = GetPhilippinesTime(),
                Status = "active",
                TokenBalance = 0.00m,
                LastLogin = GetPhilippinesTime()
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, false);

                _logger.LogInformation("OAuth user {UserId} created account using {Provider}", user.Id, info.LoginProvider);

                return LocalRedirect(model.ReturnUrl ?? "/Home/Main"); 
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // ===== KEEP EXISTING REGISTER METHODS FOR BACKWARD COMPATIBILITY =====

        // ✅ MANUAL REGISTRATION FORM
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.HideNavigation = true;
            ViewData["ReturnUrl"] = returnUrl;

            var model = new RegisterDto();
            return View(model);
        }

        // ✅ PROCESS MANUAL REGISTRATION
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model, string? returnUrl = null)
        {
            ViewBag.HideNavigation = true;
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. Check email uniqueness across ALL users (OAuth + Manual)
                var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingEmailUser != null)
                {
                    if (existingEmailUser.EmailConfirmed)
                    {
                        ModelState.AddModelError("Email", "This email address is already registered and verified.");
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "This email is already registered but not verified. Check your email for verification link or request a new one.");
                    }
                    return View(model);
                }

                // 2. Check username uniqueness
                var existingUsernameUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUsernameUser != null)
                {
                    ModelState.AddModelError("Username", "This username is already taken. Please choose another one.");
                    return View(model);
                }

                // 3. Create new user account (inactive until email verified)
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false,  // CRITICAL: User cannot login until verified
                    DateCreated = DateTime.UtcNow,
                    Status = "pending_verification",
                    TokenBalance = 0.00m
                };

                // 4. Create user with password
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // 5. Generate email verification token
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // 6. Create email verification link
                var confirmationLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { token = emailToken, email = user.Email },
                    Request.Scheme);

                // 7. Send verification email (placeholder for now)
                await SendVerificationEmail(user.Email, user.FirstName, emailToken);

                // 8. Log registration attempt
                await LogRegistrationAttempt(user.Id, "Manual", "Success",
                    Request.HttpContext.Connection.RemoteIpAddress?.ToString());

                _logger.LogInformation("Manual user {UserId} registered with email {Email}. Verification email sent.",
                    user.Id, user.Email);

                // 9. Redirect to verification sent page
                return RedirectToAction(nameof(EmailVerificationSent), new { email = user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual registration for email {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // ===== REST OF THE EXISTING METHODS REMAIN THE SAME =====

        // ✅ EMAIL VERIFICATION HANDLER
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            _logger.LogInformation("=== CONFIRM EMAIL DEBUG ===");
            _logger.LogInformation("Received email: {Email}", email);
            _logger.LogInformation("Received token: {Token}", token?.Substring(0, Math.Min(50, token?.Length ?? 0)) + "...");
            _logger.LogInformation("Received token length: {Length}", token?.Length ?? 0);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("FAILED: Missing token or email");
                return RedirectToAction(nameof(EmailConfirmationError),
                    new { message = "Invalid verification link. Please request a new verification email." });
            }

            try
            {
                // URL decode the token since it was encoded in the email
                var decodedToken = Uri.UnescapeDataString(token);
                _logger.LogInformation("Decoded token: {Token}", decodedToken.Substring(0, Math.Min(50, decodedToken.Length)) + "...");
                _logger.LogInformation("Decoded token length: {Length}", decodedToken.Length);

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("FAILED: User not found for email: {Email}", email);
                    return RedirectToAction(nameof(EmailConfirmationError),
                        new { message = "User not found. The verification link may be invalid." });
                }

                _logger.LogInformation("User found - ID: {UserId}, EmailConfirmed: {EmailConfirmed}", user.Id, user.EmailConfirmed);

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user {UserId}", user.Id);
                    return RedirectToAction(nameof(EmailAlreadyConfirmed));
                }

                // Generate a fresh token to compare
                var freshToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation("Fresh token: {Token}", freshToken.Substring(0, Math.Min(50, freshToken.Length)) + "...");
                _logger.LogInformation("Fresh token length: {Length}", freshToken.Length);
                _logger.LogInformation("Tokens match: {Match}", decodedToken == freshToken);

                // Use the decoded token
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                _logger.LogInformation("ConfirmEmailAsync result - Succeeded: {Succeeded}", result.Succeeded);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("ConfirmEmailAsync errors: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
                }

                if (result.Succeeded)
                {
                    user.Status = "active";
                    await _userManager.UpdateAsync(user);
                    await LogEmailVerification(user.Id, "Success");
                    _logger.LogInformation("SUCCESS: User {UserId} successfully verified email {Email}", user.Id, user.Email);
                    return RedirectToAction(nameof(EmailConfirmed), new { email = user.Email });
                }
                else
                {
                    return RedirectToAction(nameof(EmailConfirmationError),
                        new { message = "The verification link is invalid or has expired. Please request a new verification email." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION during email confirmation for email {Email}", email);
                return RedirectToAction(nameof(EmailConfirmationError),
                    new { message = "An error occurred during email verification. Please try again." });
            }
        }

        // ✅ RESEND VERIFICATION EMAIL
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendVerification(string? email = null)
        {
            var model = new ResendVerificationDto
            {
                Email = email ?? ""
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(ResendVerificationDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal if email exists for security
                    return RedirectToAction(nameof(EmailVerificationSent), new { email = model.Email });
                }

                if (user.EmailConfirmed)
                {
                    return RedirectToAction(nameof(EmailAlreadyConfirmed));
                }

                // Rate limiting check (max 3 resends per day)
                var todayResendCount = await GetTodayResendCount(user.Id);
                if (todayResendCount >= 3)
                {
                    ModelState.AddModelError("", "You have reached the maximum number of verification emails for today. Please try again tomorrow.");
                    return View(model);
                }

                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { token = emailToken, email = user.Email },
                    Request.Scheme);

                await SendVerificationEmail(user.Email, user.FirstName, emailToken);
                await LogEmailResend(user.Id);

                _logger.LogInformation("Verification email resent to user {UserId} at {Email}", user.Id, user.Email);

                return RedirectToAction(nameof(EmailVerificationSent), new { email = user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email to {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred while sending the verification email. Please try again.");
                return View(model);
            }
        }

        // ===== SUPPORT PAGES =====

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailVerificationSent(string email)
        {
            ViewBag.Email = email;
            ViewBag.HideNavigation = true;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailConfirmed(string email)
        {
            ViewBag.Email = email;
            ViewBag.HideNavigation = true;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailAlreadyConfirmed()
        {
            ViewBag.HideNavigation = true;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailConfirmationError(string message)
        {
            ViewBag.ErrorMessage = message;
            ViewBag.HideNavigation = true;
            return View();
        }

        // ✅ LOGOUT (Preserved exactly)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // ===== PRIVATE HELPER METHODS =====

        /// <summary>
        /// Generate unique username for OAuth users (enhanced from existing logic)
        /// </summary>
        private async Task<string> GenerateUniqueUsername(string email, string? firstName, string? lastName)
        {
            string baseUsername = email.Split('@')[0];

            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                baseUsername = $"{firstName.ToLower()}{lastName.ToLower()}";
            }

            baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, @"[^a-zA-Z0-9]", "");

            if (baseUsername.Length < 3) baseUsername = "user" + baseUsername;
            if (baseUsername.Length > 20) baseUsername = baseUsername.Substring(0, 20);

            string username = baseUsername;
            int counter = 1;

            while (await _userManager.FindByNameAsync(username) != null)
            {
                username = $"{baseUsername}{counter}";
                counter++;
                if (counter > 999) break;
            }

            return username;
        }

        /// <summary>
        /// Send verification email using SendGrid service
        /// </summary>
        private async Task SendVerificationEmail(string email, string? firstName, string emailToken)
        {
            try
            {
                _logger.LogInformation("=== SEND VERIFICATION EMAIL DEBUG ===");
                _logger.LogInformation("Email: {Email}", email);
                _logger.LogInformation("FirstName: {FirstName}", firstName);
                _logger.LogInformation("EmailToken: {Token}", emailToken.Substring(0, Math.Min(50, emailToken.Length)) + "...");
                _logger.LogInformation("EmailToken length: {Length}", emailToken.Length);

                var success = await _emailService.SendEmailVerificationAsync(email, firstName ?? "User", emailToken);

                if (success)
                {
                    _logger.LogInformation("SUCCESS: Verification email sent successfully to {Email}", email);
                }
                else
                {
                    _logger.LogWarning("FAILED: Failed to send verification email to {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION: Error sending verification email to {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Log registration attempt for audit trail
        /// </summary>
        private async Task LogRegistrationAttempt(int userId, string method, string status, string? ipAddress)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    UserId = userId,
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    LoginStatus = $"Registration_{status}",
                    LoginTimestamp = DateTime.UtcNow,
                    SessionId = HttpContext.Session.Id
                };

                _context.Set<LoginLog>().Add(loginLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging registration attempt for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Log email verification attempt
        /// </summary>
        private async Task LogEmailVerification(int userId, string status)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    UserId = userId,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    LoginStatus = $"EmailVerification_{status}",
                    LoginTimestamp = DateTime.UtcNow,
                    SessionId = HttpContext.Session.Id
                };

                _context.Set<LoginLog>().Add(loginLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging email verification for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Log email resend attempt
        /// </summary>
        private async Task LogEmailResend(int userId)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    UserId = userId,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    LoginStatus = "EmailResend",
                    LoginTimestamp = DateTime.UtcNow,
                    SessionId = HttpContext.Session.Id
                };

                _context.Set<LoginLog>().Add(loginLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging email resend for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get today's resend count for rate limiting
        /// </summary>
        private async Task<int> GetTodayResendCount(int userId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var count = _context.Set<LoginLog>()
                    .Where(l => l.UserId == userId
                        && l.LoginStatus == "EmailResend"
                        && l.LoginTimestamp >= today
                        && l.LoginTimestamp < tomorrow)
                    .Count();
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resend count for user {UserId}", userId);
                return 0;
            }
        }

        // ===== PASSWORD RESET SYSTEM =====

        // ✅ FORGOT PASSWORD FORM
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                // Don't reveal if email exists or not (security best practice)
                if (user == null || !user.EmailConfirmed)
                {
                    // Always show success message even if user doesn't exist
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // Generate password reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Send password reset email
                var success = await _emailService.SendPasswordResetAsync(user.Email, user.FirstName, resetToken);

                if (success)
                {
                    _logger.LogInformation("Password reset email sent to {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to send password reset email to {Email}", user.Email);
                }

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordDto
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal if user exists (security)
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }

                // Decode the token (same as email verification)
                var decodedToken = Uri.UnescapeDataString(model.Token);

                // Reset the password
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for user {Email}", user.Email);

                    // Optional: Send confirmation email
                    await _emailService.SendEmailAsync(user.Email, "Password Reset Successful",
                        $"Hi {user.FirstName}, your password has been successfully reset. You can now log in with your new password.");

                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred while resetting your password. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Get current Philippines time
        /// </summary>
        private DateTime GetPhilippinesTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
        }
    }
}