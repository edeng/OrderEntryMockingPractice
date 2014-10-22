using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NSubstitute; 
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        [SetUp]
        public void BeforeEachTest()
        {
             this.ProductRepository = Substitute.For<IProductRepository>();
            this.OrderFulfillmentService = Substitute.For<IOrderFulfillmentService>();
            this.OrderService = new OrderService(ProductRepository, OrderFulfillmentService);            
        }

        public OrderService OrderService { get; set; }

        public IOrderFulfillmentService OrderFulfillmentService { get; set; }

        public IProductRepository ProductRepository { get; set; }

        [Test]
        public void ValidOrder()
        {
            // Arrange
            Order order = Substitute.For<Order>();

            order.ContainsUniqueSkus().Returns(true);
            order.AllProductsInStock().Returns(true);  // TODO: Eackeghasdfag, can't get away with this: DI problem

            // Act / Assert
            Assert.DoesNotThrow(() => OrderService.PlaceOrder(order));
        }

        [Test]
        public void OrderItemsAreNotUniqueByProductSku_TheOrderIsInvalid()
        {
            // Arrange
            Order order = Substitute.For<Order>();


            order.ContainsUniqueSkus().Returns(false);
            order.AllProductsInStock().Returns(true);

            // Assert
            Assert.Throws<Exception>(() => OrderService.PlaceOrder(order));
        }

        [Test]
        public void AllProductsNotInStock_ThenOrderIsInValid()
        {
            // Arrange
            Order order = Substitute.For<Order>();


            order.ContainsUniqueSkus().Returns(true);
            order.AllProductsInStock().Returns(false);

            // Assert
            Assert.Throws<Exception>(() => OrderService.PlaceOrder(order));
        }

        [Test]
        public void SubmitsToOrderFulfillmentService()
        {
            // Arrange
            const string expectedOrderNumber = "12341234";
            const int expectedOrderId = 12345;

            Order order = Substitute.For<Order>();
            order.ContainsUniqueSkus().Returns(true);
            order.AllProductsInStock().Returns(true);

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation()
            {
                OrderNumber = expectedOrderNumber, 
                OrderId = expectedOrderId
            });


            // Act
            var summary = OrderService.PlaceOrder(order);

            // Assert
            Assert.That(summary.OrderNumber, Is.EqualTo(expectedOrderNumber));
            Assert.That(summary.OrderId, Is.EqualTo(expectedOrderId));

        }

        [Test]
        public void ContainsApplicableTaxesForCustomer()
        {
            // Arrange
            const decimal expectedRate = 500.0m;
            const int customerId = 123;
            const string postalCode = "98105";
            const string country = "USA";
            const string expectedDescription = "Expected Description"; 

            var order = Substitute.For<Order>();
            order.ContainsUniqueSkus().Returns(true);
            order.AllProductsInStock().Returns(true);
            order.CustomerId = customerId;

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation());

            var customerRepository = Substitute.For<ICustomerRepository>();
            customerRepository.Get(customerId).Returns(new Customer()
            {
                PostalCode = postalCode,
                Country = country
            });

            var taxRateService = Substitute.For<ITaxRateService>();
            var taxEntries = new List<TaxEntry>()
            {
                new TaxEntry()
                {
                    Description = expectedDescription,
                    Rate = expectedRate
                },
                new TaxEntry()
                {
                    Description = "asdlkfjas;dlfkasdf",
                    Rate = 0.2134m
                }
            };
            taxRateService.GetTaxEntries(postalCode, country).Returns(taxEntries);

            // Act
            var summary = OrderService.PlaceOrder(order);

            // Assert
            Assert.That(summary.Taxes.Count(), Is.EqualTo(taxEntries.Count));
            for (int i = 0; i < taxEntries.Count; i++)
            {
                var expected = taxEntries[i];
                var actual = summary.Taxes.ElementAt(i);

                Assert.That(actual.Description, Is.EqualTo(expected.Description));
                Assert.That(actual.Rate, Is.EqualTo(expected.Rate));
            }
        }
    }
}
