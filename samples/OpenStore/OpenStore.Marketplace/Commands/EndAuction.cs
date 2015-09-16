using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class EndAuction
    {
        /// <summary>
        /// Id of the auction to end
        /// </summary>
        public string AuctionId { get; set; }
    }
}
