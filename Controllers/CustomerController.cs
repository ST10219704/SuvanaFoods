using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SuvanaFoods.Models;
using System.Linq;
using System.Threading.Tasks;

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
                    TempData["LoginStatus"] = "Invalid username or password. Please try again."; // If the login credentials are incorrect
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

        // GET: Customer/ViewProfile
        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            // Get the logged-in customer ID from session
            if (!HttpContext.Session.TryGetValue("UserId", out var value))
            {
                return RedirectToAction("Login", "Customer");
            }

            int customerId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Fetch the customer details from the database
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customer/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!HttpContext.Session.TryGetValue("UserId", out var value))
            {
                return RedirectToAction("Login", "Customer");
            }

            int customerId = int.Parse(HttpContext.Session.GetString("UserId"));

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Customer model)
        {
            if (!HttpContext.Session.TryGetValue("UserId", out var value))
            {
                return RedirectToAction("Login", "Customer");
            }

            int customerId = int.Parse(HttpContext.Session.GetString("UserId"));

            if (ModelState.IsValid)
            {
                var customer = await _context.Customers.FindAsync(customerId);

                if (customer == null)
                {
                    return NotFound();
                }

                // Only update the fields that have changed
                if (!string.IsNullOrEmpty(model.Name) && model.Name != customer.Name)
                    customer.Name = model.Name;

                if (!string.IsNullOrEmpty(model.Username) && model.Username != customer.Username)
                    customer.Username = model.Username;

                if (!string.IsNullOrEmpty(model.Email) && model.Email != customer.Email)
                    customer.Email = model.Email;

                if (!string.IsNullOrEmpty(model.Address) && model.Address != customer.Address)
                    customer.Address = model.Address;

                if (!string.IsNullOrEmpty(model.Mobile) && model.Mobile != customer.Mobile)
                    customer.Mobile = model.Mobile;

                if (!string.IsNullOrEmpty(model.Password))
                    customer.Password = model.Password; // Hash the password in real scenarios

                if (model.ImageUrl != null)
                {
                    // Assume you've handled file uploads somewhere, or keep the old image
                    customer.ImageUrl = model.ImageUrl;
                }

                _context.Update(customer);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ViewProfile));
            }

            return View(model);
        }

        public IActionResult Menu()
        {
            // Fetch all active food items from the database
            var foodItems = _context.FoodItems
                .Where(f => (bool)f.IsActive) // Only fetch active items
                .ToList();

            return View(foodItems);
        }


        // Add item to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int foodItemId)
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var existingCartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.FoodItemId == foodItemId);

                if (existingCartItem == null)
                {
                    var cartItem = new Cart
                    {
                        CustomerId = customerId,
                        FoodItemId = foodItemId,
                        Quantity = 1 // Set the initial quantity to 1
                    };
                    _context.Carts.Add(cartItem);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Item added to cart" });
                }

                return Json(new { success = false, message = "Item already in cart" });
            }

            return Json(new { success = false, message = "User not logged in" });
        }


        // Update quantity
        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(int foodItemId, int quantity)
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.FoodItemId == foodItemId);

                if (cartItem != null)
                {
                    var foodItem = await _context.FoodItems.FindAsync(foodItemId);

                    if (foodItem != null && quantity <= foodItem.Quantity)
                    {
                        cartItem.Quantity = quantity;
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Cart updated" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Quantity exceeds available stock" });
                    }
                }

                return Json(new { success = false, message = "Item not found in cart" });
            }

            return Json(new { success = false, message = "User not logged in" });
        }

        // Remove item from cart
        public async Task<IActionResult> RemoveFromCart(int foodItemId)
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.CustomerId == customerId && c.FoodItemId == foodItemId);

                if (cartItem != null)
                {
                    _context.Carts.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Item removed from cart" });
                }

                return Json(new { success = false, message = "Item not found in cart" });
            }

            return Json(new { success = false, message = "User not logged in" });
        }


        // View cart
        public async Task<IActionResult> ViewCart()
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var cartItems = await _context.Carts
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.FoodItem) // Include food item details
                    .ToListAsync();

                return View(cartItems);
            }

            return RedirectToAction("Login", "Customer");
        }

        // Returns the Contact_Us page to the user
        [HttpGet]
        public ActionResult ContactUs()
        {
            return View(new Contact());
        }

        // POST: customer post requests/queries
        [HttpPost]
        public ActionResult SubmitContact(Contact model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                _context.Contacts.Add(model);
                _context.SaveChanges();
                return RedirectToAction("ThankYou", "Customer");
            }
            return View("ContactUs", model);
        }

        // Displays the Thank you page to the user
        public ActionResult ThankYou()
        {
            return View();
        }

        // GET: Returns the feedback page to user
        public ActionResult Feedback()
        {
            return View();
        }

        // GET: Messages
        [HttpGet]
        // GET: Messages
        [HttpGet]
        public IActionResult Messages()
        {
            var userId = User.Identity.Name;

            var messages = _context.Contacts
                .Where(c => c.Email == userId)
                .ToList();

            return View(messages); // Pass the list of messages to the view
        }

        // GET: the admins feedback on a query
        [HttpGet]
        public IActionResult UserFeedback(int id)
        {
            var contact = _context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact); // Pass the contact (with AdminFeedback) to the view
        }
    }
}
