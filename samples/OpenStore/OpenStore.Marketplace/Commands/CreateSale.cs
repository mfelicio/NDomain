using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class CreateSale
    {
        public string SaleId { get; set; }

        public string SellerId { get; set; }
        public Item Item { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
