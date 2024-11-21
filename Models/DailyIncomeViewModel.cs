namespace SuvanaFoods.Models
{
    public class DailyIncomeViewModel
    {
        public DateTime Date { get; set; }
        public List<OrderSummary> Orders { get; set; }
        public decimal DailyTotal { get; set; }
    }

    public class OrderSummary
    {
        public int OrderNo { get; set; }
        public decimal Total { get; set; }
    }

}
