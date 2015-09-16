using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class PlaceBid
    {
        /// <summary>
        /// The bid to place
        /// </summary>
        public Bid Bid { get; set; }

        /// <summary>
        /// Id of the auction to place the bid
        /// </summary>
        public string AuctionId { get; set; }
    }
}
