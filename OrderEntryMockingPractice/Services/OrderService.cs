using System;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderFulfillmentService _orderFulfillmentService;

        public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillmentService)
        {
            _productRepository = productRepository;
            _orderFulfillmentService = orderFulfillmentService;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            var isvalid = order.ContainsUniqueSkus() && order.AllProductsInStock();
            if (!isvalid)
                throw new Exception("Order is invalid");

            var orderConfirmation = _orderFulfillmentService.Fulfill(order);
            var orderSummary = new OrderSummary
            {
                OrderNumber = orderConfirmation.OrderNumber,
                OrderId = orderConfirmation.OrderId
            };

            return orderSummary;
        }
    }
}
