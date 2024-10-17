namespace AgriEnergyConnect.Models
{
    // A model to set the sessions for an Employee or Farmer 
    public class EmployeeSession
    {
        public int UserID { get; set; }
        public string Username { get; set; }
    }

    public class FarmerSession
    {
        public int UserID { get; set; }
        public string Username { get; set; }
    }

    public class CustomerSession
    {
        public int UserID { get; set; }
        public string Username { get; set; }
    }

    public class AdminSession
    {
        public int UserID { get; set; }
        public string Username { get; set; }
    }
}
