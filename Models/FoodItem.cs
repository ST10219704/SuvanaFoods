using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class FoodItem
{
    public int FoodItemId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
