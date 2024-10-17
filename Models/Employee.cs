using System;
using System.Collections.Generic;

namespace AgriEnergyConnect.Models;

// Model for Employees
public partial class Employee
{
    public int EmpId { get; set; }

    public string EmpUsername { get; set; } = null!;

    public string EmpPassword { get; set; } = null!;
}
