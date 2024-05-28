using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MedicalOffice.CustomControllers
{
    public class CognizantController : Controller
    {
        // Retrieve the name of the current controller
        internal string ControllerName()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }

        // Retrieve the name of the current action
        internal string ActionName()
        {
            return ControllerContext.RouteData.Values["action"].ToString();
        }

        // Convert CamelCase to a more readable format with spaces
        internal static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        // Set ViewData values before the action executes
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = SplitCamelCase(ControllerName());
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerName() + " " + ActionName();
            base.OnActionExecuting(context);
        }

        // Set ViewData values asynchronously before the action executes
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = SplitCamelCase(ControllerName());
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerName() + " " + ActionName();
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
