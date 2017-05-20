using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.Sample
{
    public class StateOnlyAggregate : Aggregate<CounterState>
    {
        public StateOnlyAggregate(string id, CounterState state)
            : base(id, state)
        {

        }
    }
}
