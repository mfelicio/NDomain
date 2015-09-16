using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Events
{
    public class OrderPlaced
    {
        /// <summary>
        /// The placed order
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// Id of the sale where the order was placed
        /// </summary>
        public string SaleId { get; set; }
    }
}
