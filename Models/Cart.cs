using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public int? FoodItemId { get; set; }

    public int? Quantity { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual FoodItem? FoodItem { get; set; }
}
