using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Win32;
using System.Text;
using System.Text.Encodings.Web;
using TidyUpCapstone.Models.Entities.User;
using TidyUpCapstone.Models.Entities;
using TidyUpCapstone.Services.Interfaces;

namespace TidyUp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountController(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Main", "Home");
            return RedirectToAction("Index", "Home");
        }

        //    [HttpPost]
        //    public async Task<IActionResult> Login([FromBody] LoginView model)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var user = await _userManager.FindByNameAsync(model.Username);

        //            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
        //            {
        //                return BadRequest(new { message = "Please confirm your email before logging in." });
        //            }

        //            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

        //            if (result.Succeeded)
        //            {
        //                return Json(new
        //                {
        //                    success = true,
        //                    redirectUrl = Url.Action("Main", "Home")
        //                });
        //            }
        //        }

        //        return BadRequest(new { message = "Email or password is incorrect." });
        //    }


        //    [HttpGet]
        //    public IActionResult Register()
        //    {
        //        if (User.Identity.IsAuthenticated)
        //            return RedirectToAction("Main", "Home");
        //        return View();
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> Register([FromBody] RegisterView model)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new { message = "Invalid input. Check required fields." });
        //        }

        //        var user = new ApplicationUser
        //        {
        //            UserName = model.Username,
        //            Email = model.Email
        //        };

        //        var result = await _userManager.CreateAsync(user, model.Password);

        //        if (result.Succeeded)
        //        {
        //            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        //            var confirmationLink = Url.Action(
        //                "ConfirmEmail",
        //                "Account",
        //                new { userId = user.Id, token = encodedToken },
        //                Request.Scheme);

        //            await _emailService.SendEmailAsync(
        //                model.Email,
        //                "Confirm your email",
        //                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.");

        //            return Json(new { success = true, message = "Registration successful. Please check your email to confirm your account." });
        //        }

        //        return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        //    }

        //    [HttpGet]
        //    public async Task<IActionResult> ConfirmEmail(string userId, string token)
        //    {
        //        if (userId == null || token == null)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }

        //        var user = await _userManager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return NotFound($"Unable to load user with ID '{userId}'.");
        //        }

        //        token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        //        var result = await _userManager.ConfirmEmailAsync(user, token);

        //        if (result.Succeeded)
        //        {

        //            if (!user.WelcomeTokenGranted)
        //            {
        //                user.TokenBalance += 50;
        //                user.WelcomeTokenGranted = true;
        //                await _userManager.UpdateAsync(user);
        //            }

        //            return View("EmailConfirmed");
        //        }

        //        return View("Error");
        //    }


        //    public IActionResult VerifyEmail()
        //    {
        //        return View();
        //    }

        //    [HttpPost]
        //    public IActionResult ChangePassword(string username)
        //    {
        //        if (string.IsNullOrEmpty(username))
        //        {
        //            return RedirectToAction("VerifyEmail", "Account");
        //        }

        //        return View(new ResetPasswordViewModel
        //        {
        //            Email = username,
        //            NewPassword = string.Empty,
        //            ConfirmNewPassword = string.Empty
        //        });
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var user = await _userManager.FindByNameAsync(model.Email);
        //            if (user == null)
        //            {
        //                ModelState.AddModelError("", "Email not found!");
        //                return View(model);
        //            }

        //            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword!);

        //            if (result.Succeeded)
        //            {
        //                return RedirectToAction("Login", "Account");
        //            }


        //            foreach (var error in result.Errors)
        //            {
        //                ModelState.AddModelError("", error.Description);
        //            }
        //        }

        //        return View(model);
        //    }

        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> Logout()
        //    {
        //        await _signInManager.SignOutAsync();
        //        return RedirectToAction("Index", "Home");
        //    }
    }
}