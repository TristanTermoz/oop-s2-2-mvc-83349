using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoodSafetyTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Unhandled error – RequestId: {RequestId}", requestId); // LOG EVENT (Error)
            return View(new ErrorViewModel { RequestId = requestId });
        }

    }
}
