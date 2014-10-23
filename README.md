#Purpose
The purpose of this project is to provide a practice area for learning to use a mocking framework in C#.
#Instructions
Fork this repository into your own account.
Using your favorite unit test library and mocking framework, write tests for the following requirements.
Do not implement any of the existing interfaces.
Any implementation code you write should exist primarily in the methods being tested.
# TestCases
## OrderService.PlaceOrder
### Order is valid if
* OrderItems are unique by product sku
* All products are in stock
* If order is not valid, an exception is thrown containing a list of reasons why the order is not valid.
* If order is valid, an OrderSummary is returned
  * it is submitted to the OrderFulfillmentService.
  * It contains the order number generated by the OrderFulfillmentService.
  * containing the id generated by the OrderFulfillmentService
  * containing applicable taxes for the customer
  * NetTotal = SUM(Product.Quantity * Product.Price)
  * OrderTotal = SUM(TaxEntry.Rate) * NetTotal
  * an confirmation email is sent to the customer.
* Customer information can be retrieved from the CustomerRepository
* Taxes can be retrieved from the TaxRateService
* The ProductRepository can be used to determine if the products are in stock.
