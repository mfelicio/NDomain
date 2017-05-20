using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Factory that creates aggregates from previous state, from event streams or simply new aggregates
    /// </summary>
    /// <typeparam name="TAggregate">Type of the aggregate to create</typeparam>
    public interface IAggregateFactory<TAggregate>
        where TAggregate : IAggregate
    {
        /// <summary>
        /// Creates a new aggregate with the given id
        /// </summary>
        /// <param name="id">id that uniquely identifies the aggregate</param>
        /// <returns>A new instance of <typeparamref name="TAggregate"/></returns>
        TAggregate CreateNew(string id);

        /// <summary>
        /// Creates a new aggregate with the given id by replaying all past events
        /// </summary>
        /// <param name="id">id that uniquely identifies the aggregate</param>
        /// <param name="events">event stream to be replayed</param>
        /// <returns>A new instance of <typeparamref name="TAggregate"/> created from past events</returns>
        TAggregate CreateFromEvents(string id, IAggregateEvent[] events);

        /// <summary>
        /// Creates a new aggregate with the given id and state
        /// </summary>
        /// <param name="id">id that uniquely identifies the aggregate</param>
        /// <param name="state">state of the aggregate</param>
        /// <returns>A new instance of <typeparamref name="TAggregate"/> created from the state</returns>
        TAggregate CreateFromState(string id, IState state);
    }
}
