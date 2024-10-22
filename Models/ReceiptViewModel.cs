namespace SuvanaFoods.Models
{
    public class ReceiptViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

}
