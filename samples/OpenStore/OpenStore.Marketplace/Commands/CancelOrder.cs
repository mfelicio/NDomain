using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class CancelOrder
    {
        /// <summary>
        /// Id of the order to cancel
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Id of the sale where the order was placed
        /// </summary>
        public string SaleId { get; set; }
    }
}
