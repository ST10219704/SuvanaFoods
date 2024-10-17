using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace AgriEnergyConnect.Controllers
{
    // A controller to manage the functions for employee users 
    // This code was adapted from Tutorialspoint
    // Link: https://www.tutorialspoint.com/mvc_framework/mvc_framework_controllers.htm#:~:text=The%20Controller%20is%20responsible%20for,results%20back%20to%20the%20View 
    public class EmployeeController : Controller
    {
        private readonly AgriEnergyConnectDbContext _context;

        public EmployeeController(AgriEnergyConnectDbContext context)
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
        public async Task<IActionResult> Login(EmployeeLogin emp)
        {
            if (ModelState.IsValid)
            {
                // Get the employee's id from the database
                int userId = GetUserIdFromDatabase(emp.Username, emp.Password);

                if (userId != -1)
                {
                    // A session is created for the Employee 
                    var userSession = new EmployeeSession
                    {
                        UserID = userId,
                        Username = emp.Username
                    };

                    HttpContext.Session.SetString("IsLoggedIn", "true"); // The user is logged in for the session
                    HttpContext.Session.SetString("UserId", userId.ToString()); // The employee's ID is set for the session
                    HttpContext.Session.SetString("Username", emp.Username); // The employee's username is set for the session
                    HttpContext.Session.SetString("UserRole", "Employee"); // The role for the session is set to Employee
                    HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(userSession));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["LoginStatus"] = "Invalid username or password. Please try again."; //If the login credentials are incorrect
                }
            }

            return View(emp);
        }


        // Gets the EmployeeID for an employee from the database
        private int GetUserIdFromDatabase(string username, string password)
        {
            try
            {
                var user = _context.Employees.FirstOrDefault(u => u.EmpUsername == username && u.EmpPassword == password);

                return user?.EmpId ?? -1;
            }
            catch (Exception ex)
            {
                TempData["LoginStatus"] = $"Error: {ex.Message}";
                return -1;
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        //To add a farmer to the database
        // This method was adapted from Learn Entity Framework Core
        // Link: https://www.learnentityframeworkcore.com/dbcontext/adding-data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Farmer farmer)
        {
            if (ModelState.IsValid)
            {
                _context.Farmers.Add(farmer);
                _context.SaveChanges();

                TempData["CreateStatus"] = "Farmer successfully added!";

                ModelState.Clear(); // Clear the form
                return RedirectToAction("Create");
            }

            return View(farmer);
        }

        public IActionResult SearchFarmer()
        {
            return View();
        }

        // To search for a farmer from the database 
        // This method was adapted from Learn Entity Framework Core
        // Link: https://www.learnentityframeworkcore.com/dbset/querying-data
        [HttpPost]
        public IActionResult SearchFarmer(string farmerName)
        {
            // Search for a farmer by their name 
            var farmer = _context.Farmers.FirstOrDefault(f => f.FarName == farmerName);

            if (farmer == null)
            {
                TempData["Message"] = "No farmer found.";
                return View();
            }

            // Retrieve the farmer's products
            var products = _context.Products.Where(p => p.FarId == farmer.FarId).ToList();

            ViewBag.FarmerId = farmer.FarId;

            return View("SearchFarmer", products);
        }

        // To filter for products associated with a farmer 
        [HttpPost]
        public IActionResult FilterProducts(int farmerId, string category, DateTime? startDate, DateTime? endDate)
        {
            var products = _context.Products.Where(p => p.FarId == farmerId); // Gets the products for the farmer 

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.ProCategory == category); // Filter by category
            }

            if (startDate != null && endDate != null)
            {
                products = products.Where(p => p.ProProductionDate >= startDate && p.ProProductionDate <= endDate); // Gets the start and end date and checks for a farmer's products in between that time frame
            }

            var filteredProducts = products.ToList();

            if (filteredProducts.Count == 0)
            {
                TempData["Message"] = "No data found.";
                return RedirectToAction("SearchFarmer");
            }

            ViewBag.FarmerId = farmerId;

            return View("SearchFarmer", filteredProducts);
        }

        // For the user to log out
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // The session is cleared 
            return RedirectToAction("Index", "Home");
        }
    }
}
