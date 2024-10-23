using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public string? OrderNo { get; set; }

    public decimal Total { get; set; }

    public string DeliveryMode { get; set; } = null!;

    public string? Address { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? PaymentStatus { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
