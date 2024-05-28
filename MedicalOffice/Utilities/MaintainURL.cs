namespace MedicalOffice.Utilities
{
    public static class MaintainURL
    {
        // Method to return the URL based on the controller name and store it in a cookie
        public static string ReturnURL(HttpContext httpContext, string ControllerName)
        {
            string cookieName = ControllerName + "URL";
            string SearchText = "/" + ControllerName + "?";
            string returnURL = httpContext.Request.Headers["Referer"].ToString();

            // Check if the referrer URL contains the search text
            if (returnURL.Contains(SearchText))
            {
                returnURL = returnURL[returnURL.LastIndexOf(SearchText)..];
                // Set the cookie with the return URL
                CookieHelper.CookieSet(httpContext, cookieName, returnURL, 30);
                return returnURL;
            }
            else
            {
                // Get the return URL from the cookie if it exists
                returnURL = httpContext.Request.Cookies[cookieName];
                return returnURL ?? "/" + ControllerName;
            }
        }
    }
}
