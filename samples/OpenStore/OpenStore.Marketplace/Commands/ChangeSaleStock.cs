using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Commands
{
    public class ChangeSaleStock
    {
        public string SaleId { get; set; }
        public int Value { get; set; }
    }
}
