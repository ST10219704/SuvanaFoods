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

    }
}
