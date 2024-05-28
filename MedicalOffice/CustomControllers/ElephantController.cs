using MedicalOffice.Utilities;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedicalOffice.CustomControllers
{
    public class ElephantController : CognizantController
    {
        // List of actions that require a return URL
        internal string[] ActionWithURL = new string[] { "Details", "Create", "Edit", "Delete", "Add", "Update", "Remove" };

        // Set return URL or clear URL before the action executes
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (ActionWithURL.Contains(ActionName()))
            {
                // Set the return URL for specified actions
                ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
            }
            else if (ActionName() == "Index")
            {
                // Clear the URL for the Index action
                CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            }
            base.OnActionExecuting(context);
        }

        // Asynchronously set return URL or clear URL before the action executes
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (ActionWithURL.Contains(ActionName()))
            {
                // Set the return URL for specified actions
                ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
            }
            else if (ActionName() == "Index")
            {
                // Clear the URL for the Index action
                CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            }
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
