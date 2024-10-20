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

        [HttpGet]
        public IActionResult AddFoodItem()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewBag.FoodItems = _context.FoodItems.Include(f => f.Category).ToList();
            return View(new FoodItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFoodItem(FoodItem foodItem, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload here
                if (imageFile != null && imageFile.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot/images", imageFile.FileName); // Make sure to create this directory
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    foodItem.ImageUrl = "/images/" + imageFile.FileName; // Save the relative path
                }

                _context.FoodItems.Add(foodItem);
                _context.SaveChanges();
                return RedirectToAction("AddFoodItem");
            }

            // Reload categories and food items if validation fails
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewBag.FoodItems = _context.FoodItems.Include(f => f.Category).ToList();
            return View(foodItem);
        }




    }
}
