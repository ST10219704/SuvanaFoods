using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
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
                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.FoodItemId == foodItemId);

                if (cartItem != null)
                {
                    var foodItem = await _context.FoodItems.FindAsync(foodItemId);

                    if (foodItem != null && quantity <= foodItem.Quantity)
                    {
                        cartItem.Quantity = quantity;
                        await _context.SaveChangesAsync(); // Save the changes to the database
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


        // Remove item from cart when the user clicks remove
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int foodItemId)
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.FoodItemId == foodItemId);

                if (cartItem != null)
                {
                    _context.Carts.Remove(cartItem);
                    await _context.SaveChangesAsync(); // Save the changes to the database
                    return Json(new { success = true, message = "Item removed" });
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

        public async Task<IActionResult> Checkout()
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var customer = await _context.Customers.FindAsync(customerId);

                var cartItems = await _context.Carts
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.FoodItem)
                    .ToListAsync();

                var model = new CheckoutViewModel
                {
                    Customer = customer,
                    CartItems = cartItems
                };

                return View(model);
            }

            return RedirectToAction("Login", "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> OrderConfirmed(CheckoutViewModel model)
        {
            if (HttpContext.Session.TryGetValue("UserId", out var value))
            {
                var customerId = int.Parse(HttpContext.Session.GetString("UserId"));
                var customer = await _context.Customers.FindAsync(customerId);

                var cartItems = await _context.Carts
                    .Where(c => c.CustomerId == customerId)
                    .Include(c => c.FoodItem) // Ensure FoodItem is included with Price
                    .ToListAsync();

                var order = new Order
                {
                    CustomerId = customerId,
                    DeliveryMode = model.DeliveryMode,
                    Address = model.DeliveryMode == "Delivery" ? model.Address : null,
                    PaymentMethod = model.PaymentMethod,
                    Status = "Confirmed",
                    PaymentStatus = "Pending",
                    OrderDate = DateTime.Now,
                    OrderNo = GenerateOrderNumber(), // Assign random OrderNo here
                    OrderItems = new List<OrderItem>() // Ensure this is initialized
                };


                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        FoodItemId = cartItem.FoodItem.FoodItemId,
                        Quantity = (int)cartItem.Quantity
                    };
                    order.OrderItems.Add(orderItem);
                }

                // Calculate the total based on the cart items' prices and quantities
                order.Total = (decimal)cartItems
                    .Where(item => item.FoodItem != null && item.FoodItem.Price.HasValue) // Check for null FoodItem or Price
                    .Sum(item => item.FoodItem.Price.GetValueOrDefault() * item.Quantity); // Calculate total

                // Add the order to the database and clear the cart
                _context.Orders.Add(order);
                _context.Carts.RemoveRange(cartItems); // Clear the cart
                await _context.SaveChangesAsync();

                // Prepare the order confirmation message
                var confirmationMessage = model.DeliveryMode == "Pickup"
                    ? "Thank you! Your order is being processed and will be ready for pickup in 30-45 mins."
                    : "Thank you! Your order is being processed and will be delivered in 45-60 mins.";

                // Create the OrderConfirmedViewModel
                var orderConfirmedViewModel = new OrderConfirmedViewModel
                {
                    OrderId = order.OrderId,
                    CustomerName = customer.Name,
                    DeliveryMode = order.DeliveryMode,
                    Address = order.Address,
                    OrderDate = (DateTime)order.OrderDate,
                    TotalAmount = order.Total,
                    ConfirmationMessage = confirmationMessage,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Name = oi.FoodItem.Name,
                        Quantity = oi.Quantity,
                        Price = oi.FoodItem.Price.GetValueOrDefault(),
                        Total = oi.FoodItem.Price.GetValueOrDefault() * oi.Quantity
                    }).ToList()
                };

                return View("OrderConfirmed", orderConfirmedViewModel);
            }

            return RedirectToAction("Login", "Customer");
        }



        public IActionResult OrderConfirmed(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderConfirmedViewModel
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer.Name,
                OrderNo = order.OrderNo,
                DeliveryMode = order.DeliveryMode,
                TotalAmount = order.Total,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    Name = oi.FoodItem.Name,
                    Quantity = oi.Quantity,
                    Price = (decimal)oi.FoodItem.Price
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DownloadReceipt(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var receiptViewModel = new ReceiptViewModel
            {
                OrderId = order.OrderId,
                OrderDate = (DateTime)order.OrderDate,
                CustomerName = order.Customer.Name,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    Name = oi.FoodItem.Name,
                    Quantity = oi.Quantity,
                    Price = oi.FoodItem.Price.GetValueOrDefault(),
                    Total = oi.FoodItem.Price.GetValueOrDefault() * oi.Quantity
                }).ToList(),
                TotalAmount = order.Total
            };

            return new ViewAsPdf("Receipt", receiptViewModel)
            {
                FileName = $"OrderReceipt_{order.OrderId}.pdf"
            };
        }

        private string GenerateOrderNumber(int length = 6)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // GET: Create Booking Event
        [HttpGet]
        public IActionResult Events()
        {
            return View(new BookingEvent()); // Return the booking event view
        }


        // POST: Save the booking event and display the catering menu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvents(BookingEvent bookingEvent)
        {
            if (HttpContext.Session.TryGetValue("UserId", out var userIdValue))
            {
                int customerId = int.Parse(userIdValue);

                // Retrieve the customer asynchronously
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    ModelState.AddModelError("", "Customer not found.");
                    return View("Events", bookingEvent);
                }
                    if (ModelState.IsValid)
                    {
                        bookingEvent.CustomerId = customerId;
                        bookingEvent.AdminApproval = "Pending";
                        bookingEvent.CreatedDate = DateTime.Now;

                        // Add the booking event and save changes asynchronously
                        _context.BookingEvents.Add(bookingEvent);
                        await _context.SaveChangesAsync();

                        // Set a success message
                        TempData["SuccessMessage"] = "Event booking created successfully!";
                        return RedirectToAction("CateringMenu", "Customer");
                    }
                
            }
            else
            {
                // Handle case where user is not logged in
                return RedirectToAction("Login", "Customer"); // Redirect to login page
            }

            // Return the Events view with the booking event if invalid
            return View("Events", bookingEvent); // Specify the view name
        }

        // GET: Menu
        public IActionResult CateringMenu()
        {
            var menuItems = _context.FoodItems.Where(m => m.IsActive == true).ToList();
            return View(menuItems); // Return the menu view with active food items
        }

        // POST: Create Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CateringMenu(List<CateringOrder> orders) // Ensure you have an OrderItem model
        {
            if (orders != null && orders.Any(o => o.Quantity > 0)) // Check if any order is selected
            {
                foreach (var order in orders)
                {
                    if (order.Quantity > 0)
                    {
                        // Save to database
                        _context.CateringOrders.Add(order); // Assuming you have an OrderItem DbSet
                    }
                }

                // Save all changes in one call to the database
                _context.SaveChanges();

                return RedirectToAction("Confirmation"); // Redirect after successful submission
            }

            ModelState.AddModelError("", "Please select at least one menu item.");
            return View("CateringMenu", _context.FoodItems.Where(m => m.IsActive == true).ToList()); // Reload the menu items if nothing is selected
        }


        // GET: Confirmation
        public IActionResult EventDetails()
        {
            return View();
        }

        // GET: Event Details
        public IActionResult EventDetails(int id)
        {
            var bookingEvent = _context.BookingEvents
                .Include(be => be.CateringOrders) // Include related OrderItems
                .ThenInclude(oi => oi.FoodItem) // Include FoodItem details for each order
                .FirstOrDefault(be => be.BookingId == id);

            if (bookingEvent == null)
            {
                return NotFound();
            }

            return View(bookingEvent); // Return the booking event with order details
        }
    }
}
