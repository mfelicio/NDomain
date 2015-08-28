using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public interface IAggregate
    {
        string Id { get; }
        int OriginalVersion { get; }

        IState State { get; }
        IEnumerable<IAggregateEvent> Changes { get; }
    }

    public interface IAggregate<TState> : IAggregate
        where TState : IState
    {
        new TState State { get; }
    }
}
