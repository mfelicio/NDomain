using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Values
{
    public class Order
    {
        /// <summary>
        /// Id of the order, for idempotency and correlation purposes
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the member who ordered
        /// </summary>
        public string MemberId { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Date in UTC when the order was placed
        /// </summary>
        public DateTime DateUtc { get; set; }
    }
}
