using System.Collections.Generic;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
        [Test]
        public void OrderItemsAreUniqueByProductSku_TheOrderIsValid()
        {
            // Arrange
            Order order = new Order();

            OrderItem orderItem1 = new OrderItem()
            {
                Quantity = 2,
                Product = new Product()
                {
                    ProductId = 123,
                    Sku = "456",
                    Name = "Beans",
                    Description = "Top of the line beans",
                    Price = 1
                }
            };

            OrderItem orderItem2 = new OrderItem()
            {
                Quantity = 2,
                Product = new Product()
                {
                    ProductId = 123,
                    Sku = "789",
                    Name = "Beans",
                    Description = "Top of the line beans",
                    Price = 1
                }
            };

            order.OrderItems.Add(orderItem1);
            order.OrderItems.Add(orderItem2);

            // Act
            bool result = order.OrderItemsAreUnique(); 

            // Assert
            Assert.True(result);
        }

        [Test]
        public void OrderItemsAreNotUniqueByProductSku_TheOrderIsInvalid()
        {
            // Arrange
            Order order = new Order();

            OrderItem orderItem1 = new OrderItem()
            {
                Quantity = 2,
                Product = new Product()
                {
                    ProductId = 123,
                    Sku = "456",
                    Name = "Beans",
                    Description = "Top of the line beans",
                    Price = 1
                }
            };

            OrderItem orderItem2 = new OrderItem()
            {
                Quantity = 2,
                Product = new Product()
                {
                    ProductId = 123,
                    Sku = "456",
                    Name = "Beans",
                    Description = "Top of the line beans",
                    Price = 1
                }
            };

            order.OrderItems.Add(orderItem1);
            order.OrderItems.Add(orderItem2);

            // Act
            bool result = order.OrderItemsAreUnique();

            // Assert
            Assert.False(result);
        }
    }
}
