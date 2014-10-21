using System.Collections.Generic;

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

        public bool OrderItemsAreUnique()
        {
            HashSet<string> uniqueItems = new HashSet<string>();
            foreach (var orderItem in OrderItems)
            {
                var sku = orderItem.Product.Sku;
                if (uniqueItems.Contains(sku))
                {
                    return false; 
                }
                else
                {
                    uniqueItems.Add(sku); 
                }

            }
            return true; 
        }
    }
}
