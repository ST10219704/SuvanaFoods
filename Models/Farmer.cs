using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Models;

// Model for Farmers 
public partial class Farmer
{
    public int FarId { get; set; }
    [Display(Name = "Farmer Name:")]
    [Required(ErrorMessage = "Farmer name is required")]
    public string FarName { get; set; } = null!;
    [Display(Name = "Email Address:")]
    [Required(ErrorMessage = "Email address is required")]
    public string FarEmail { get; set; } = null!;
    [Display(Name = "Phone Number:")]
    [Required(ErrorMessage = "Phone number is required")]
    public string FarPhone { get; set; } = null!;
    [Display(Name = "Location:")]
    [Required(ErrorMessage = "Location is required")]
    public string FarLocation { get; set; } = null!;
    [Display(Name = "Username:")]
    [Required(ErrorMessage = "Username is required")]
    public string FarUsername { get; set; } = null!;
    [Display(Name = "Password:")]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string FarPassword { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
