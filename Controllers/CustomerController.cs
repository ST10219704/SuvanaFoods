using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuvanaFoods.Models;
using System.Linq;

namespace SuvanaFoods.Controllers
{
    public class CustomerController : Controller
    {
        private readonly SuvanaFoodsDbContext _context;

        public CustomerController(SuvanaFoodsDbContext context)
        {
            _context = context;
        }

        // GET: Customer/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Customer/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(CustomerRegistration model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username or email is already taken
                if (_context.Customers.Any(c => c.Username == model.Username || c.Email == model.Email))
                {
                    ModelState.AddModelError("", "Username or Email already taken.");
                    return View(model);
                }

                // Create new customer
                var customer = new Customer
                {
                    Name = model.Name,
                    Email = model.Email,
                    Mobile = model.Mobile,
                    Address = model.Address,
                    Username = model.Username,
                    Password = model.Password // You may want to hash the password in a real application
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home"); // Redirect to a different page after successful registration
            }

            return View(model); // In case of an error, return the view with validation messages
        }

        public IActionResult Login()
        {
            return View();
        }

        // For the farmer to log in
        // This code was adapted from Netcode-Hub on YouTube
        // Video name: Multi-Vendor App in ASP.NET MVC - 6 - Add Admin Login page.
        // Link: https://www.youtube.com/watch?v=e2jrXvb1jGI&list=PL285LgYq_FoIpguOrV9XQqj_w9qjfZ0w0&index=11
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CustomerLogin cus)
        {
            if (ModelState.IsValid)
            {
                // Get the farmer's id from the database
                int userId = GetUserIdFromDatabase(cus.Username, cus.Password);

                if (userId != -1)
                {
                    // A session is created for the Farmer
                    var userSession = new CustomerSession
                    {
                        UserID = userId,
                        Username = cus.Username
                    };

                    HttpContext.Session.SetString("IsLoggedIn", "true"); // The user is logged in for the session
                    HttpContext.Session.SetString("UserId", userId.ToString()); // The farmer's ID is set for the session
                    HttpContext.Session.SetString("Username", cus.Username); // The farmer's username is set for the session
                    HttpContext.Session.SetString("UserRole", "Customer"); // The role for the session is set to Farmer
                    HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(userSession));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["LoginStatus"] = "Invalid username or password. Please try again."; //If the login credentials are incorrect

                }
            }

            return View(cus);
        }

        // Gets the FarmerID for an employee from the database
        private int GetUserIdFromDatabase(string username, string password)
        {
            try
            {
                var user = _context.Customers.FirstOrDefault(u => u.Username == username && u.Password == password);

                return user?.CustomerId ?? -1;
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
