using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class CateringOrder
{
    public int OrderId { get; set; }

    public int BookingId { get; set; }

    public int FoodItemId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual BookingEvent Booking { get; set; } = null!;

    public virtual FoodItem FoodItem { get; set; } = null!;
}
