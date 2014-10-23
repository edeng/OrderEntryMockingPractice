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
            this.TaxRateService = Substitute.For<ITaxRateService>();
            this.CustomerRepository = Substitute.For<ICustomerRepository>();
            this.EmailService = Substitute.For<IEmailService>();
            this.OrderService = new OrderService(ProductRepository, OrderFulfillmentService, TaxRateService, CustomerRepository, EmailService);            
        }

        public OrderService OrderService { get; set; }

        public IOrderFulfillmentService OrderFulfillmentService { get; set; }

        public IProductRepository ProductRepository { get; set; }

        public ICustomerRepository CustomerRepository { get; set; }

        public ITaxRateService TaxRateService { get; set; }

        public IEmailService EmailService { get; set; }

        [Test]
        public void ValidOrder()
        {
            // Arrange
            const int customerId = 123;
            const int orderId = 500;
            const string sku1 = "1234";
            const string sku2 = "4567";

            var order = Substitute.For<Order>();
            order.CustomerId = customerId;

            CustomerRepository.Get(customerId).Returns(new Customer()
            {
                CustomerId = customerId
            });

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation()
            {
                OrderId = orderId
            });

            var orderEntries = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Product = new Product()
                    {
                        Sku = sku1
                    },
                }, 

                new OrderItem()
                {
                    Product = new Product()
                    {
                        Sku = sku2
                    }, 
                }
            };

            ProductRepository.IsInStock(sku1).Returns(true);
            ProductRepository.IsInStock(sku2).Returns(true);

            order.OrderItems = orderEntries; 

            // Act / Assert
            Assert.DoesNotThrow(() => OrderService.PlaceOrder(order));
        }

        [Test]
        public void OrderItemsAreNotUniqueByProductSku_TheOrderIsInvalid()
        {
            // Arrange
            const string sku = "123";

            var order = Substitute.For<Order>();

            var orderEntries = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Product = new Product()
                    {
                        Sku = sku
                    },
                }, 

                new OrderItem()
                {
                    Product = new Product()
                    {
                        Sku = sku
                    }, 
                }
            };

            order.OrderItems = orderEntries; 

            ProductRepository.IsInStock(sku).Returns(true);


            // Assert
            Assert.Throws<Exception>(() => OrderService.PlaceOrder(order));
        }

        [Test]
        public void AllProductsNotInStock_ThenOrderIsInValid()
        {
            // Arrange
            const string sku1 = "1234";
            const string sku2 = "45456"; 

            var order = Substitute.For<Order>();

            var orderEntries = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Product = new Product()
                    {
                        Sku = sku1
                    },
                }, 

                new OrderItem()
                {
                    Product = new Product()
                    {
                        Sku = sku2
                    }, 
                }
            };

            ProductRepository.IsInStock(sku1).Returns(false);
            ProductRepository.IsInStock(sku2).Returns(true);

            order.OrderItems = orderEntries; 

            // Assert
            Assert.Throws<Exception>(() => OrderService.PlaceOrder(order));
        }

        [Test]
        public void SubmitsToOrderFulfillmentService()
        {
            // Arrange
            const string expectedOrderNumber = "12341234";
            const int expectedOrderId = 12345;
            const int customerId = 123;
            
            var order = Substitute.For<Order>();
            order.CustomerId = customerId;

            CustomerRepository.Get(customerId).Returns(new Customer()
            {
                CustomerId = customerId
            }); 

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
            const int orderId = 500;

            var order = Substitute.For<Order>();
            order.CustomerId = customerId;

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation()
            {
                OrderId = orderId
            });

            CustomerRepository.Get(customerId).Returns(new Customer()
            {
                PostalCode = postalCode,
                Country = country,
                CustomerId = customerId
            });

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

            TaxRateService.GetTaxEntries(postalCode, country).Returns(taxEntries);

            // Act
            var summary = OrderService.PlaceOrder(order);

            // Assert
            Assert.That(summary.Taxes.Count(), Is.EqualTo(taxEntries.Count));
            for (var i = 0; i < taxEntries.Count; i++)
            {
                var expected = taxEntries[i];
                var actual = summary.Taxes.ElementAt(i);

                Assert.That(actual.Description, Is.EqualTo(expected.Description));
                Assert.That(actual.Rate, Is.EqualTo(expected.Rate));
            }
        }

        [Test]
        public void ValidNetTotal()
        {
            // Arrange
            const decimal price1 = 10.00m;
            const int quantity1 = 3;
            const decimal price2 = 1.25m;
            const int quantity2 = 5;
            const string sku1 = "1234";
            const string sku2 = "23424";
            const decimal expectedNetTotal = (price1*quantity1) + (price2*quantity2);

            const int customerId = 123;
            const int orderId = 400;

            var order = Substitute.For<Order>();
            order.CustomerId = customerId;

            CustomerRepository.Get(customerId).Returns(new Customer()
            {
                CustomerId = customerId
            });

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation()
            {
                OrderId = orderId
            });


            var orderEntries = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Product = new Product()
                    {
                        Price = price1,
                        Sku = sku1
                    },
 
                    Quantity = quantity1
                }, 

                new OrderItem()
                {
                    Product = new Product()
                    {
                        Price = price2,
                        Sku = sku2
                    }, 
                    Quantity = quantity2
                }
            };

            ProductRepository.IsInStock(sku1).Returns(true);
            ProductRepository.IsInStock(sku2).Returns(true);

            order.OrderItems = orderEntries; 

            // Act
            var summary = OrderService.PlaceOrder(order);

            // Assert 
            Assert.That(summary.NetTotal, Is.EqualTo(expectedNetTotal));
        }

        [Test]
        public void ValidOrderTotal()
        {
            // Arrange
            const decimal price1 = 10.00m;
            const int quantity1 = 3;
            const decimal price2 = 1.25m;
            const int quantity2 = 5;
            const decimal rate1 = 0.15m;
            const string postalCode = "98105";
            const string country = "USA";
            const int orderId = 123;
            const string sku1 = "1234";
            const string sku2 = "23424";
            const decimal expectedOrderTotal = ((price1 * quantity1) + (price2 * quantity2)) * rate1;

            const int customerId = 123;

            var order = Substitute.For<Order>();
            order.CustomerId = customerId;

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation()
            {
                OrderId = orderId
            });

            CustomerRepository.Get(customerId).Returns(new Customer()
            {
                PostalCode = postalCode,
                Country = country,
                CustomerId = customerId
            });

            var taxEntries = new List<TaxEntry>()
            {
                new TaxEntry()
                {
                    Rate = rate1
                }
            };

            TaxRateService.GetTaxEntries(postalCode, country).Returns(taxEntries);

            var orderEntries = new List<OrderItem>()
            {
                new OrderItem()
                {
                    Product = new Product()
                    {
                        Price = price1,
                        Sku = sku1
                    },
 
                    Quantity = quantity1
                }, 

                new OrderItem()
                {
                    Product = new Product()
                    {
                        Price = price2,
                        Sku = sku2
                    }, 
                    Quantity = quantity2
                }
            };

            ProductRepository.IsInStock(sku1).Returns(true);
            ProductRepository.IsInStock(sku2).Returns(true);

            order.OrderItems = orderEntries;

            // Act
            var summary = OrderService.PlaceOrder(order);

            // Assert 
            Assert.That(summary.Total, Is.EqualTo(expectedOrderTotal));
        }

        [Test]
        public void EmailConfirmationSent()
        {
            // Arrange
            const int customerId = 123;
            const int orderId = 500;

            var order = Substitute.For<Order>();
            order.CustomerId = customerId;

            CustomerRepository.Get(customerId).Returns(new Customer()
            {
                CustomerId = customerId
            });

            OrderFulfillmentService.Fulfill(order).Returns(new OrderConfirmation()
            {
                OrderId = orderId
            });

            // Act
            OrderService.PlaceOrder(order);

            // Assert
            EmailService.Received().SendOrderConfirmationEmail(customerId, orderId);
        }
    }
}
