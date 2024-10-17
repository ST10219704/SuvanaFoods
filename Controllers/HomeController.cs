using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AgriEnergyConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // This method was adpated from Stack Overflow 
        // Link: https://stackoverflow.com/questions/37382440/asp-net-core-mvc-6-a-different-home-page-for-authorized-users
        // Author: Dmitry
        // Date: 23 May 2016
        public IActionResult Index()
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn") == "true";
            var userRole = HttpContext.Session.GetString("UserRole");

            if (!isLoggedIn)
            {
                return View(); // The home screen view that will show if the user isn't logged in
            }
            else if (userRole == "Admin")
            {
                return View("EmployeeHome"); // The home screen view that will show if an employee is logged in
            }
            else if (userRole == "Customer")
            {
                return View("FarmerHome"); // The home screen view that will show if a farmer is logged in
            }

            return View(); // Default view if no role matches
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
