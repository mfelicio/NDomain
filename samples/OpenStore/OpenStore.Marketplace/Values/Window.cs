using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Values
{
    /// <summary>
    /// Value object representing a period with a start and end date
    /// </summary>
    public class Window
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public bool Comprises(DateTime date)
        {
            return date >= Start && date <= End;
        }
    }
}
