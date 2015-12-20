using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Represents a service that knows how to mutate an aggregate state by applying state change events to it.
    /// Tipically the StateMutator should find a suitable method on the state instance and invoke it using the @event as argument.
    /// </summary>
    internal interface IStateMutator
    {
        /// <summary>
        /// Modifies a given state by applying a state change event.
        /// </summary>
        /// <param name="state">state</param>
        /// <param name="event">event</param>
        void Mutate(IState state, IAggregateEvent @event);
    }
}
