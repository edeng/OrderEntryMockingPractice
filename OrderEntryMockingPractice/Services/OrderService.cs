using System;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderFulfillmentService _orderFulfillmentService;
        private readonly ITaxRateService _taxRateService;
        private readonly ICustomerRepository _customerRepository;


        public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillmentService, ITaxRateService taxRateService, ICustomerRepository customerRepository)
        {
            _productRepository = productRepository;
            _orderFulfillmentService = orderFulfillmentService;
            _taxRateService = taxRateService;
            _customerRepository = customerRepository;
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


            var customer = _customerRepository.Get(order.CustomerId.Value);
            orderSummary.Taxes = _taxRateService.GetTaxEntries(customer.PostalCode, customer.Country);

            return orderSummary;
        }
    }
}
