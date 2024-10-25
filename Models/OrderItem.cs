using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int FoodItemId { get; set; }

    public int Quantity { get; set; }

    public virtual FoodItem FoodItem { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
