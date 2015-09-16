using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Events
{
    public class BidPlaced
    {
        /// <summary>
        /// The placed bid
        /// </summary>
        public Bid Bid { get; set; }

        /// <summary>
        /// Id of the auction where the bid was placed
        /// </summary>
        public string AuctionId { get; set; }
    }
}
