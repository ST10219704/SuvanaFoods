using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuvanaFoods.Models;

namespace SuvanaFoods.Controllers
{
    public class AdminController : Controller
    {
        private readonly SuvanaFoodsDbContext _context;

        public AdminController(SuvanaFoodsDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        // For the employee to log in
        // This code was adapted from Netcode-Hub on YouTube
        // Video name: Multi-Vendor App in ASP.NET MVC - 6 - Add Admin Login page.
        // Link: https://www.youtube.com/watch?v=e2jrXvb1jGI&list=PL285LgYq_FoIpguOrV9XQqj_w9qjfZ0w0&index=11
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLogin ad)
        {
            if (ModelState.IsValid)
            {
                // Get the employee's id from the database
                int userId = GetUserIdFromDatabase(ad.Username, ad.Password);

                if (userId != -1)
                {
                    // A session is created for the Employee 
                    var userSession = new AdminSession
                    {
                        UserID = userId,
                        Username = ad.Username
                    };

                    HttpContext.Session.SetString("IsLoggedIn", "true"); // The user is logged in for the session
                    HttpContext.Session.SetString("UserId", userId.ToString()); // The employee's ID is set for the session
                    HttpContext.Session.SetString("Username", ad.Username); // The employee's username is set for the session
                    HttpContext.Session.SetString("UserRole", "Admin"); // The role for the session is set to Employee
                    HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(userSession));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["LoginStatus"] = "Invalid username or password. Please try again."; //If the login credentials are incorrect
                }
            }

            return View(ad);
        }


        // Gets the EmployeeID for an employee from the database
        private int GetUserIdFromDatabase(string username, string password)
        {
            try
            {
                var user = _context.Admins.FirstOrDefault(u => u.Username == username && u.Password == password);

                return user?.AdminId ?? -1;
            }
            catch (Exception ex)
            {
                TempData["LoginStatus"] = $"Error: {ex.Message}";
                return -1;
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // The session is cleared 
            return RedirectToAction("Index", "Home");
        }
    }
}
