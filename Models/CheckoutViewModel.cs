namespace SuvanaFoods.Models
{
    public class CheckoutViewModel
    {
        public Customer Customer { get; set; }
        public List<Cart> CartItems { get; set; }
        public string DeliveryMode { get; set; } // Pickup or Delivery
        public string Address { get; set; } // Optional delivery address
        public string PaymentMethod { get; set; } // Cash or Card
    }

}
