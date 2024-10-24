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

    public class OrderConfirmedViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string OrderNo { get; set; }  // Add OrderNo property
        public string DeliveryMode { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string ConfirmationMessage { get; set; }
    }

    public class OrderItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

    public class OrdersViewModel
    {
        public List<Order> CurrentOrders { get; set; }
        public List<Order> PastOrders { get; set; }
    }
}
