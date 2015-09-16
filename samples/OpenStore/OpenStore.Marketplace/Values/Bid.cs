using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Values
{
    public class Bid
    {
        /// <summary>
        /// Id of the bid, for idempotency and correlation purposes
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Id of the member that set the bid
        /// </summary>
        public string MemberId { get; set; }

        /// <summary>
        /// Date in UTC when the bid was placed
        /// </summary>
        public DateTime DateUtc { get; set; }
        
        /// <summary>
        /// Value of the bid
        /// </summary>
        public decimal Value { get; set; }
    }
}
