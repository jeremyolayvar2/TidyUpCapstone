using Microsoft.AspNetCore.Mvc.Filters;

namespace TidyUpCapstone.Filters
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            context.HttpContext.Response.Headers.Add("Pragma", "no-cache");
            context.HttpContext.Response.Headers.Add("Expires", "0");

            base.OnActionExecuting(context);
        }
    }
}