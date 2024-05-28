using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalOffice.Utilities
{
    public static class PageSizeHelper
    {
        // Method to set the page size based on user input and cookies
        public static int SetPageSize(HttpContext httpContext, int? pageSizeID, string ControllerName)
        {
            int pageSize = 0;
            if (pageSizeID.HasValue)
            {
                pageSize = pageSizeID.GetValueOrDefault();
                // Set cookies for the page size values
                CookieHelper.CookieSet(httpContext, ControllerName + "pageSizeValue", pageSize.ToString(), 480);
                CookieHelper.CookieSet(httpContext, "DefaultpageSizeValue", pageSize.ToString(), 480);
            }
            else
            {
                // Retrieve the page size value from cookies
                pageSize = Convert.ToInt32(httpContext.Request.Cookies[ControllerName + "pageSizeValue"]);
            }
            if (pageSize == 0)
            {
                pageSize = Convert.ToInt32(httpContext.Request.Cookies["DefaultpageSizeValue"]);
            }
            return (pageSize == 0) ? 5 : pageSize;
        }

        // Method to return a SelectList of page size options
        public static SelectList PageSizeList(int? pageSize)
        {
            return new SelectList(new[] { "3", "5", "10", "20", "30", "40", "50", "100", "500" }, pageSize.ToString());
        }
    }
}
