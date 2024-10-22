using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string OrderNo { get; set; }
    public decimal Total { get; set; } // Store total price
    public string DeliveryMode { get; set; } // Pickup or Delivery
    public string Address { get; set; } // Optional delivery address
    public string PaymentMethod { get; set; } // Cash or Card
    public string Status { get; set; } = "Confirmed";
    public string PaymentStatus { get; set; } = "Pending";
    public DateTime OrderDate { get; set; } = DateTime.Now;

    // Navigation properties
    public Customer Customer { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Collection of order items
}

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int FoodItemId { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; }
    public virtual FoodItem FoodItem { get; set; }
}
