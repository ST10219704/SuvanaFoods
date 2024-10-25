using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class BookingEvent
{
    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public string EventName { get; set; } = null!;

    public DateTime EventDate { get; set; } = DateTime.Now;

    public string? EventLocation { get; set; }

    public int GuestNumber { get; set; }

    public DateTime CreatedDate { get; set; }

    public string AdminApproval { get; set; } = null!;

    public string? AdminMessage { get; set; }

    public virtual ICollection<CateringOrder> CateringOrders { get; set; } = new List<CateringOrder>();

    public virtual Customer Customer { get; set; } = null!;
}
