using MedicalOffice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MedicalOffice.Controllers
{
    [AllowAnonymous] // Allows all users, authenticated or not, to access this controller
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Constructor to initialize the logger
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Displays the home page
        public IActionResult Index()
        {
            return View();
        }

        // Displays the privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Displays the error page with response cache settings
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Creates a new ErrorViewModel with the current request ID or trace identifier
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
