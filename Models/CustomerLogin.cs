using System.ComponentModel.DataAnnotations;

namespace SuvanaFoods.Models
{
    public class CustomerLogin
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
