using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public interface IAggregateFactory<TAggregate>
        where TAggregate : IAggregate
    {
        TAggregate CreateNew(string id);
        TAggregate CreateFromEvents(string id, IAggregateEvent[] events);
        TAggregate CreateFromState(string id, IState state);
    }
}
