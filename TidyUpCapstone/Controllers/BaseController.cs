using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TidyUpCapstone.Models.Entities.User;

namespace TidyUpCapstone.Controllers
{
    public class BaseController : Controller
    {
        protected readonly UserManager<AppUser> _userManager;

        public BaseController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Set global ViewBag data for all authenticated users
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    // Set the current user's avatar for the sidebar
                    ViewBag.CurrentUserAvatar = !string.IsNullOrEmpty(currentUser.ProfilePictureUrl)
                        ? currentUser.ProfilePictureUrl
                        : "/assets/default-avatar.svg";

                    ViewBag.CurrentUserName = currentUser.UserName;
                    ViewBag.CurrentUserId = currentUser.Id;
                    ViewBag.CurrentUserTokenBalance = currentUser.TokenBalance;
                }
            }
            else
            {
                // Set defaults for anonymous users
                ViewBag.CurrentUserAvatar = "/assets/default-avatar.svg";
                ViewBag.CurrentUserName = "Guest";
                ViewBag.CurrentUserId = null;
                ViewBag.CurrentUserTokenBalance = 0;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}