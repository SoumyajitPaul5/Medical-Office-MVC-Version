namespace MedicalOffice.Utilities
{
    public static class CookieHelper
    {
        // Method to set a cookie with a specified key, value, and expiration time
        public static void CookieSet(HttpContext _context, string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();

            // Set the expiration time of the cookie
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);

            // Append the cookie to the response
            _context.Response.Cookies.Append(key, value, option);
        }
    }
}
