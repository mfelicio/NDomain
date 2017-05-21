using System.Collections.Generic;

namespace NDomain.Model
{
    public interface IEventSourcedAggregate : IAggregate
    {
        /// <summary>
        /// Changes made to the aggregate in the form of events
        /// </summary>
        IEnumerable<IAggregateEvent> Changes { get; }
    }

    public interface IEventSourcedAggregate<TState> : IAggregate<TState>, IEventSourcedAggregate
        where TState : IState
    {

    }
}
