using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Reflection;

namespace AgriEnergyConnect.Controllers
{
    // A controller to manage the functions for farmer users
    // This code was adapted from Tutorialspoint
    // Link: https://www.tutorialspoint.com/mvc_framework/mvc_framework_controllers.htm#:~:text=The%20Controller%20is%20responsible%20for,results%20back%20to%20the%20View 
    public class FarmerController : Controller
    {
        private readonly AgriEnergyConnectDbContext _context;

        public FarmerController(AgriEnergyConnectDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Login(FarmerLogin far)
        {
            if (ModelState.IsValid)
            {
                // Get the farmer's id from the database
                int userId = GetUserIdFromDatabase(far.Username, far.Password);

                if (userId != -1)
                {
                    // A session is created for the Farmer
                    var userSession = new FarmerSession
                    {
                        UserID = userId,
                        Username = far.Username
                    };

                    HttpContext.Session.SetString("IsLoggedIn", "true"); // The user is logged in for the session
                    HttpContext.Session.SetString("UserId", userId.ToString()); // The farmer's ID is set for the session
                    HttpContext.Session.SetString("Username", far.Username); // The farmer's username is set for the session
                    HttpContext.Session.SetString("UserRole", "Farmer"); // The role for the session is set to Farmer
                    HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(userSession));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["LoginStatus"] = "Invalid username or password. Please try again."; //If the login credentials are incorrect

                }
            }

            return View(far);
        }

        // Gets the FarmerID for an employee from the database
        private int GetUserIdFromDatabase(string username, string password)
        {
            try
            {
                var user = _context.Farmers.FirstOrDefault(u => u.FarUsername == username && u.FarPassword == password);

                return user?.FarId ?? -1;
            }
            catch (Exception ex)
            {
                TempData["LoginStatus"] = $"Error: {ex.Message}";
                return -1;
            }
        }

        public IActionResult AddProduct()
        {
            return View();
        }

        //To add a product for a farmer to the database
        // This method was adapted from C# Corner
        // Link: https://www.c-sharpcorner.com/article/how-to-connect-sql-database-in-asp-net-using-c-sharp-and-insert-and-view-the-data-usi/
        // Author: Muthuramalingam Duraipandi 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct([Bind("ProName,ProCategory,ProProductionDate")] Product pro)
        {
            // Gets the id for the farmer currently logged in
            string userIdString = HttpContext.Session.GetString("UserId");

            // Connects to the database 
            using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-JP19TVO; Initial Catalog=AgriEnergyConnectDB; Encrypt=False; Integrated Security=True;"))
            {
                await connection.OpenAsync();

                // Inserts the data for a product into the database 
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Product (Far_ID, Pro_Name, Pro_Category, Pro_ProductionDate) VALUES (@UserID, @ProName, @ProCategory, @ProProductionDate)", connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userIdString);
                    cmd.Parameters.AddWithValue("@ProName", pro.ProName);
                    cmd.Parameters.AddWithValue("@ProCategory", pro.ProCategory);
                    cmd.Parameters.AddWithValue("@ProProductionDate", pro.ProProductionDate);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            TempData["CreateStatus"] = "Product successfully added!";

            return RedirectToAction("AddProduct");
        }

        // For the farmer to view their products
        // This method was adapted from Learn Entity Framework Core
        // Link: https://www.learnentityframeworkcore.com/dbset/querying-data
        public async Task<IActionResult> ViewProducts()
        {
            var userSessionString = HttpContext.Session.GetString("UserSession");

            if (!string.IsNullOrEmpty(userSessionString))
            {
                var userSession = JsonConvert.DeserializeObject<FarmerSession>(userSessionString);

                if (userSession != null)
                {
                    var products = await _context.Products
                        .Where(p => p.FarId == userSession.UserID)
                        .ToListAsync();

                    return View(products);
                }
            }

            TempData["ViewStatus"] = "Error trying to retrieve data";
            return RedirectToAction("Index", "Home");
        }

        // For the user to log out
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // The session is cleared 
            return RedirectToAction("Index", "Home");
        }

    }
}
