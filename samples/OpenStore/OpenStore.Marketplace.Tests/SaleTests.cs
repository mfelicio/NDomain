using NUnit.Framework;
using OpenStore.Marketplace.Domain;
using OpenStore.Marketplace.Events;
using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Tests
{
    public class SaleTests
    {
        [Test]
        public void CreateSale_Is_Idempotent()
        {
            // arrange
            var sale = new Sale("s1", new SaleState());

            // act
            sale.Create("seller", new Values.Item { Id = "item-id", Name = "item-name" }, price: 10, stock: 5);
            sale.Create("seller", new Values.Item { Id = "item-id", Name = "item-name" }, price: 10, stock: 5);

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(1));
            Assert.That(sale.Changes.First().Payload, Is.InstanceOf<SaleCreated>());
        }

        [TestCase(10, 5, true)]
        [TestCase(10, 10, true)]
        [TestCase(5, 10, false)]
        public void PlaceOrder_Should_PlaceOrder_WhenStockIsSufficient(int saleStock, int orderQuantity, bool should)
        {
            // arrange
            var sale = Samples.CreateSale(saleStock);

            // act
            sale.PlaceOrder(Samples.CreateTestOrder(orderQuantity));

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(should ? 1 : 0));
        }

        [Test]
        public void CancelOrder_Should_Cancel_WhenOrderExists()
        {
            // arrange
            var sale = Samples.CreateSale(saleStock: 10);
            var order = Samples.CreateTestOrder(quantity: 5);
            sale.PlaceOrder(order); // changes: 1

            // act
            sale.CancelOrder(order.Id); // changes: 2
            sale.CancelOrder(order.Id); // idempotency check

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(2));
            Assert.That(sale.Changes.Last().Payload, Is.InstanceOf<OrderCancelled>());
        }

        [Test]
        public void CompleteOrder_Should_Complete_WhenOrderExists()
        {
            // arrange
            var sale = Samples.CreateSale(saleStock: 10);
            var order = Samples.CreateTestOrder(quantity: 5);
            sale.PlaceOrder(order); // changes: 1

            // act
            sale.CompleteOrder(order.Id); // changes: 2
            sale.CompleteOrder(order.Id); // idempotency check

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(2));
            Assert.That(sale.Changes.Last().Payload, Is.InstanceOf<OrderCompleted>());
        }

        [Test]
        public void CancelOrder_ShouldNot_Cancel_WhenOrderDoesntExist()
        {
            // arrange
            var sale = Samples.CreateSale(saleStock: 10);
            var order = Samples.CreateTestOrder(quantity: 5);
            sale.PlaceOrder(order); // changes: 1

            // act
            sale.CancelOrder("a-different-order-id");

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void CompleteOrder_ShouldNot_Complete_WhenOrderDoesntExist()
        {
            // arrange
            var sale = Samples.CreateSale(saleStock: 10);
            var order = Samples.CreateTestOrder(quantity: 5);
            sale.PlaceOrder(order); // changes: 1

            // act
            sale.CompleteOrder("a-different-order-id");

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void CompleteOrder_ShouldNot_Complete_WhenThereIsntEnoughStock()
        {
            // arrange
            var sale = Samples.CreateSale(saleStock: 10);
            var order = Samples.CreateTestOrder(quantity: 5);
            sale.PlaceOrder(order); // changes: 1

            // act
            // change stock to 4, means it should not complete
            sale.ChangeStock(4); // changes: 2
            sale.CompleteOrder(order.Id);

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(2));
            Assert.That(sale.Changes.Last().Payload, Is.InstanceOf<SaleStockChanged>());
        }

        [Test]
        public void ChangeStock_Should_IncreaseAvailableStockForNewOrders()
        {
            // arrange
            var sale = Samples.CreateSale(saleStock: 10);
            var order = Samples.CreateTestOrder(quantity: 5); // reserves 5 units
            sale.PlaceOrder(order); // changes: 1

            sale.PlaceOrder(Samples.CreateTestOrder(7)); // ensure this order isn't placed
            Assert.That(sale.Changes.Count(), Is.EqualTo(1));

            // act
            // change stock to 12, means it should have 12 stock, 7 available
            sale.ChangeStock(12); // changes: 2
            
            var order2 = Samples.CreateTestOrder(7);
            sale.PlaceOrder(order2); // changes: 3

            // assert
            Assert.That(sale.Changes.Count(), Is.EqualTo(3));
            Assert.That(sale.Changes.Last().Payload, Is.InstanceOf<OrderPlaced>());
        }
    }
}
