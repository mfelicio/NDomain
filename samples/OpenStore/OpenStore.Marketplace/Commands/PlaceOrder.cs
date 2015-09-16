using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class PlaceOrder
    {
        /// <summary>
        /// Order to place
        /// </summary>
        public Order Order { get; set; }

        /// <summary>
        /// Id of the sale to place the order
        /// </summary>
        public string SaleId { get; set; }
    }
}
