using System.ComponentModel.DataAnnotations;

namespace SuvanaFoods.Models
{
    public class AddFoodItemView
    {
        public int FoodItemId { get; set; }

        [Required(ErrorMessage = "Food Item Name is required")]
        public string Name { get; set; }

        public string? Description { get; set; } // Nullable

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Category is required")] // Ensure category is required
        public string? Category { get; set; } // Nullable

        public string? ImageUrl { get; set; } // Nullable

        public bool IsActive { get; set; } = true;
    }
}
