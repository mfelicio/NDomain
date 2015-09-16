using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Events
{
    public class SaleStockChanged
    {
        public string SaleId { get; set; }

        public int OldValue { get; set; }
        public int NewValue { get; set; }
    }
}
