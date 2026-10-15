using System;

namespace WpfApp1.Models
{
    public class OrderRow
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }
}