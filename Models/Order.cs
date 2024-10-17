using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? FoodItemId { get; set; }

    public int? CustomerId { get; set; }

    public int? PaymentId { get; set; }

    public string? OrderNo { get; set; }

    public int? Quantity { get; set; }

    public string? Status { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual FoodItem? FoodItem { get; set; }

    public virtual Payment? Payment { get; set; }
}
