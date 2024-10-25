using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SuvanaFoods.Models;

namespace SuvanaFoods.Controllers
{
    public class AdminController : Controller
    {
        private readonly SuvanaFoodsDbContext _context;
        private const string ImageDirectory = "images";

        public AdminController(SuvanaFoodsDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        // For the employee to log in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLogin ad)
        {
            if (ModelState.IsValid)
            {
                int userId = GetUserIdFromDatabase(ad.Username, ad.Password);

                if (userId != -1)
                {
                    var userSession = new AdminSession
                    {
                        UserID = userId,
                        Username = ad.Username
                    };

                    HttpContext.Session.SetString("IsLoggedIn", "true");
                    HttpContext.Session.SetString("UserId", userId.ToString());
                    HttpContext.Session.SetString("Username", ad.Username);
                    HttpContext.Session.SetString("UserRole", "Admin");
                    HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(userSession));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["LoginStatus"] = "Invalid username or password. Please try again.";
                }
            }

            return View(ad);
        }

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
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Category Management
        public IActionResult CategoryManagement()
        {
            return View(_context.Categories.ToList());
        }

        // GET: Create Category
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Add Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CategoryManagement));
            }
            return View(category);
        }

        // GET: Edit Category
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Edit Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category updatedCategory)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                category.Name = updatedCategory.Name;
                category.IsActive = updatedCategory.IsActive;

                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CategoryManagement));
            }
            return View(updatedCategory);
        }

        // POST: Search Categories
        public JsonResult Search(string searchString)
        {
            var categories = _context.Categories
                .Where(c => c.Name.Contains(searchString))
                .Select(c => new { c.CategoryId, c.Name, c.IsActive });
            return Json(categories);
        }

        public async Task<IActionResult> AddFoodItem()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => (bool)c.IsActive).ToListAsync(), "Name", "Name");

            // Create an instance of AddFoodItemView to pass to the view
            var model = new AddFoodItemView();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFoodItem(AddFoodItemView foodItem, IFormFile? imageFile) // Nullable imageFile
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Create a unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(ImageDirectory, uniqueFileName);

                    // Ensure the directory exists
                    Directory.CreateDirectory(ImageDirectory);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    foodItem.ImageUrl = $"/{imagePath}"; // Store the image URL
                }

                // Map the ViewModel to the Entity Model
                var newFoodItem = new FoodItem
                {
                    Name = foodItem.Name,
                    Description = foodItem.Description,
                    Price = foodItem.Price,
                    Quantity = foodItem.Quantity,
                    Category = foodItem.Category,
                    ImageUrl = foodItem.ImageUrl,
                    IsActive = foodItem.IsActive
                };

                _context.FoodItems.Add(newFoodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("AddFoodItem"); // Redirect to another action after successful addition
            }

            // Repopulate categories in case of an error
            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => (bool)c.IsActive).ToListAsync(), "Name", "Name");
            return View(foodItem);
        }

        public IActionResult ManageStock()
        {
            // Fetch all food items from the database
            var foodItems = _context.FoodItems
                .ToList();

            return View(foodItems);
        }

        // GET: Edit Food Item
        public async Task<IActionResult> EditFoodItem(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => (bool)c.IsActive).ToListAsync(), "Name", "Name");
            var model = new AddFoodItemView
            {
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = (decimal)foodItem.Price,
                Quantity = (int)foodItem.Quantity,
                Category = foodItem.Category,
                ImageUrl = foodItem.ImageUrl,
                IsActive = (bool)foodItem.IsActive
            };

            return View(model);
        }

        // POST: Edit Food Item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFoodItem(int id, AddFoodItemView foodItem, IFormFile? imageFile)
        {
            var existingFoodItem = await _context.FoodItems.FindAsync(id);
            if (existingFoodItem == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // If a new image file is uploaded, update the image
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Create a unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var imagePath = Path.Combine(ImageDirectory, uniqueFileName);

                    // Ensure the directory exists
                    Directory.CreateDirectory(ImageDirectory);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingFoodItem.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), existingFoodItem.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    existingFoodItem.ImageUrl = $"/{imagePath}";
                }

                // Update existing food item details
                existingFoodItem.Name = foodItem.Name;
                existingFoodItem.Description = foodItem.Description;
                existingFoodItem.Price = foodItem.Price;
                existingFoodItem.Quantity = foodItem.Quantity;
                existingFoodItem.Category = foodItem.Category;
                existingFoodItem.IsActive = foodItem.IsActive;

                _context.Update(existingFoodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageStock");
            }

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => (bool)c.IsActive).ToListAsync(), "Name", "Name");
            return View(foodItem);
        }

        // GET: the list of queries
        [HttpGet]
        public ActionResult Queries()
        {
            var unresolvedQueries = _context.Contacts.Where(c => !c.IsResolved).ToList();
            var resolvedQueries = _context.Contacts.Where(c => c.IsResolved).ToList();

            // Pass both lists via ViewBag
            ViewBag.UnresolvedQueries = unresolvedQueries;
            ViewBag.ResolvedQueries = resolvedQueries;

            return View(unresolvedQueries); // Pass unresolved queries as the main mode
        }

        // POST: Mark the query as resolved
        [HttpPost]
        public IActionResult MarkAsResolved(int id)
        {
            var query = _context.Contacts.Find(id);
            if (query != null)
            {
                query.IsResolved = true; // Mark the query as resolved
                _context.SaveChanges();
            }
            return RedirectToAction("Queries", "Admin");
        }


        // GET: the contact details of the user.
        [HttpGet]
        public IActionResult Feedback(int id)
        {
            var contact = _context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact); 
        }

        // POST: processes the admins feedback after it is submitted.
        [HttpPost]
        public IActionResult SendFeedback(Contact model)
        {
            var contact = _context.Contacts.Find(model.ContactId);

            if (contact == null)
            {
                return NotFound();
            }

            // Store the feedback
            contact.AdminFeedback = model.AdminFeedback;
            _context.SaveChanges();

            // Optionally, you can send an email notification to the user here

            return RedirectToAction("Queries", "Admin");
        }

        // GET: Admin/Customers
        public IActionResult Customers()
        {
            var customers = _context.Customers.ToList();
            return View(customers);
        }

        // GET: Admin/EditCustomer/5
        public IActionResult EditCustomer(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Admin/EditCustomer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Customers));
            }
            return View(customer);
        }

        // GET: Admin/DeleteCustomer/5
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Admin/DeleteCustomer/5
        [HttpPost, ActionName("DeleteCustomer")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = _context.Customers.Find(id);
            _context.Customers.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction(nameof(Customers));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        // GET: Admin/ViewOrders
        public IActionResult ViewOrders()
        {
            // Retrieve current orders (Status: Confirmed, PaymentStatus: Pending) and include the Customer data
            var currentOrders = _context.Orders
                .Where(o => o.Status == "Confirmed" && o.PaymentStatus == "Pending")
                .Include(o => o.Customer) // Include customer to avoid null reference
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Retrieve past orders (Status: Completed or PaymentStatus: Paid) and include the Customer data
            var pastOrders = _context.Orders
                .Where(o => o.Status == "Completed" || o.PaymentStatus == "Paid")
                .Include(o => o.Customer) // Include customer to avoid null reference
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var viewModel = new OrdersViewModel
            {
                CurrentOrders = currentOrders,
                PastOrders = pastOrders
            };

            return View(viewModel);
        }


        // GET: Admin/UpdateOrderStatus
        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string status, string paymentStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order != null)
            {
                order.Status = status;
                order.PaymentStatus = paymentStatus;
                _context.SaveChanges();
            }
            return RedirectToAction("ViewOrders");
        }

        // GET: Admin/CateringOrders
        public async Task<IActionResult> CateringOrders()
        {
            var cateringOrders = await _context.BookingEvents.ToListAsync();

            // Filter confirmed and denied events
            var confirmedEvents = cateringOrders
                .Where(order => order.AdminApproval == "Confirmed")
                .ToList();

            var deniedEvents = cateringOrders
                .Where(order => order.AdminApproval == "Denied")
                .ToList();

            ViewBag.ConfirmedEvents = confirmedEvents; // Pass confirmed events to the view
            ViewBag.DeniedEvents = deniedEvents; // Pass denied events to the view

            return View(cateringOrders);
        }

        // POST: Admin/ApproveOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var bookingEvent = await _context.BookingEvents.FindAsync(id);
            if (bookingEvent == null)
            {
                return NotFound();
            }

            bookingEvent.AdminApproval = "Confirmed"; // Update approval status
            bookingEvent.AdminMessage = "Your booking has been confirmed."; // Optional confirmation message
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Booking confirmed successfully!";
            return RedirectToAction(nameof(CateringOrders));
        }

        // POST: Admin/DenyCateringOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyCateringOrder(int bookingId, string adminMessage)
        {
            // Find the booking event by ID
            var bookingEvent = await _context.BookingEvents.FindAsync(bookingId);

            if (bookingEvent != null)
            {
                bookingEvent.AdminApproval = "Denied"; // Set the approval status to Denied
                bookingEvent.AdminMessage = adminMessage; // Set the admin message
                await _context.SaveChangesAsync(); // Save the changes

                TempData["SuccessMessage"] = "Catering order denied successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Catering order not found!";
            }

            return RedirectToAction(nameof(CateringOrders)); // Redirect to the CateringOrders view
        }
    }
}