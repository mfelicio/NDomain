using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Events
{
    public class AuctionEnded
    {
        public string AuctionId { get; set; }

        public DateTime DateUtc { get; set; }
        public Bid HigherBid { get; set; }

        public bool HasWinner { get; set; }
    }
}
