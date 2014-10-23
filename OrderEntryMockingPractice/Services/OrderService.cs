using System;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderFulfillmentService _orderFulfillmentService;
        private readonly ITaxRateService _taxRateService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailService _emailService;

        public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillmentService, ITaxRateService taxRateService, ICustomerRepository customerRepository, IEmailService emailService)
        {
            _productRepository = productRepository;
            _orderFulfillmentService = orderFulfillmentService;
            _taxRateService = taxRateService;
            _customerRepository = customerRepository;
            _emailService = emailService;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            var isvalid = order.ContainsUniqueSkus() && order.AllProductsInStock();
            if (!isvalid)
                throw new Exception("Order is invalid");

            var orderConfirmation = _orderFulfillmentService.Fulfill(order);
            var netTotal = order.OrderItems.Sum(orderItem => orderItem.Quantity * orderItem.Product.Price);

            var customer = _customerRepository.Get(order.CustomerId.Value);
            var orderId = orderConfirmation.OrderId;
            var taxes = _taxRateService.GetTaxEntries(customer.PostalCode, customer.Country);
            var total = taxes.Sum(taxEntry => netTotal*taxEntry.Rate);

            var orderSummary = new OrderSummary
            {
                OrderNumber = orderConfirmation.OrderNumber,
                OrderId = orderId,
                NetTotal = netTotal,
                Taxes = taxes,
                Total = total
            };

            _emailService.SendOrderConfirmationEmail(customer.CustomerId.Value, orderId);

            return orderSummary;
        }
    }
}
