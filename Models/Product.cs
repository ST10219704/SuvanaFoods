using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Models;

// Model for the products 
public partial class Product
{
    public int ProId { get; set; }

    public int FarId { get; set; }

    [Display(Name = "Product Name:")]

    [Required(ErrorMessage = "Product name is required")]
    public string ProName { get; set; } = null!;
    [Display(Name = "Category:")]
    [Required(ErrorMessage = "Category is required")]
    public string ProCategory { get; set; } = null!;
    [Display(Name = "Production Date:")]
    [Required(ErrorMessage = "Production date is required")]
    public DateTime ProProductionDate { get; set; }

    public virtual Farmer Far { get; set; } = null!;
}
