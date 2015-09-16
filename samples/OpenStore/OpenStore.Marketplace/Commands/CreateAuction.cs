using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class CreateAuction
    {
        public string AuctionId { get; set; }

        public string SellerId { get; set; }
        public Item Item { get; set; }
        public decimal MinPrice { get; set; }
        public Window WindowUtc { get; set; }
    }
}
