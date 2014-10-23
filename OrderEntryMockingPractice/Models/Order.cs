using System.Collections.Generic;
using System.Linq;

namespace OrderEntryMockingPractice.Models
{
    public class Order
    {
        public Order()
        {
            this.OrderItems = new List<OrderItem>();
        }
        
        public int? CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
