using System;
using System.Collections.Generic;

namespace SuvanaFoods.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }
}
