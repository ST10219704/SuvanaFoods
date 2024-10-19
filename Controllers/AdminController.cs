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
            // Fetch active categories for the dropdown
            ViewBag.Categories = _context.Categories
                                    .Where(c => c.IsActive == true)
                                    .Select(c => new SelectListItem
                                    {
                                        Value = c.CategoryId.ToString(),
                                        Text = c.Name
                                    })
                                    .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFoodItem(FoodItem model, IFormFile ImageUrl, int selectedCategoryId)
        {
            if (ModelState.IsValid)
            {
                model.CategoryId = selectedCategoryId; // Ensure CategoryId is set

                // Save the uploaded image to wwwroot/images/fooditems folder
                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/fooditems");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageUrl.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageUrl.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = "/images/fooditems/" + uniqueFileName;
                }

                try
                {
                    _context.FoodItems.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "FoodItem");
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging
                    ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
                    Console.WriteLine($"Error: {ex.Message}"); // Simple console log for now
                }
            }

            // Reload active categories in case of validation failure
            ViewBag.Categories = _context.Categories
                                        .Where(c => c.IsActive == true)
                                        .Select(c => new SelectListItem
                                        {
                                            Value = c.CategoryId.ToString(),
                                            Text = c.Name
                                        })
                                        .ToList();

            return View(model);
        }


    }
}
