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
    public class SaleState : State
    {
        public bool Created { get; set; }

        public string SellerId { get; set; }
        public Item Item { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int AvailableStock { get; set; }

        public Dictionary<string, int> PendingOrders { get; set; }

        public SaleState()
        {
            this.PendingOrders = new Dictionary<string, int>();
        }

        private void On(SaleCreated ev)
        {
            this.SellerId = ev.SellerId;
            this.Item = ev.Item;
            this.Price = ev.Price;
            this.Stock = ev.Stock;

            this.Created = true;
        }

        private void On(SaleStockChanged ev)
        {
            this.Stock = ev.NewValue;
        }

        private void On(OrderPlaced ev)
        {
            AvailableStock -= ev.Order.Quantity;
            PendingOrders[ev.Order.Id] = ev.Order.Quantity;
        }

        private void On(OrderCancelled ev)
        {
            AvailableStock += PendingOrders[ev.OrderId];
            PendingOrders.Remove(ev.OrderId);
        }

        private void On(OrderCompleted ev)
        {
            var value = PendingOrders[ev.OrderId];
            
            Stock -= value;
            PendingOrders.Remove(ev.OrderId);
        }
    }
}
