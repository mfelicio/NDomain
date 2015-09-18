using OpenStore.Marketplace.Events;
using OpenStore.Marketplace.Values;
using NDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Domain
{
    public class Sale : Aggregate<SaleState>
    {
        public Sale(string id, SaleState state)
            : base(id, state)
        {

        }

        public void Create(string sellerId, Item item, decimal price, int stock)
        {
            if (State.Created)
            {
                // idempotency
                return;
            }

            this.On(
                new SaleCreated
                {
                    SaleId = this.Id,
                    SellerId = sellerId,
                    Item = item,
                    Price = price,
                    Stock = stock
                });
        }

        public void ChangeStock(int value)
        {
            if (State.Stock == value)
            {
                // idempotency
                return;
            }

            this.On(new SaleStockChanged
            {
                SaleId = this.Id,
                NewValue = value,
                OldValue = State.Stock
            });
        }

        public bool CanPlaceOrder(Order order)
        {
            return State.AvailableStock >= order.Quantity;
        }

        public bool CanCompleteOrder(Order order)
        {
            return State.Stock >= order.Quantity;
        }

        public void PlaceOrder(Order order)
        {
            if (State.PendingOrders.ContainsKey(order.Id))
            {
                // idempotency
                return;
            }

            if (!CanPlaceOrder(order))
            {
                // should return error code
                return;
            }

            this.On(new OrderPlaced { SaleId = this.Id, Order = order});
        }

        public void CancelOrder(string orderId)
        {
            if (!State.PendingOrders.ContainsKey(orderId))
            {
                // idempotency
                return;
            }

            this.On(new OrderCancelled { SaleId = this.Id, OrderId = orderId });
        }

        public void CompleteOrder(string orderId)
        {
            Order order;
            if (!State.PendingOrders.TryGetValue(orderId, out order))
            {
                // idempotency
                return;
            }

            if (!CanCompleteOrder(order))
            {
                // should return error code
                return;
            }

            this.On(new OrderCompleted { SaleId = this.Id, OrderId = orderId });
        }
    }
}
