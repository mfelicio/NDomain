using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Api.Models.Requests
{
    public class CreateAuctionRequest
    {
        public Item Item { get; set; }
        public decimal MinPrice { get; set; }
        public Window WindowUtc { get; set; }
    }
}
