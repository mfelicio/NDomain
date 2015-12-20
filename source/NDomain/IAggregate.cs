using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Aggregate root abstraction, providing a clear separation between its state and event changes, enabling persistence ignorance.
    /// Snapshotting and/or Event sourcing can be used for persistence, or an object-relational-mapper between the state and data entities.
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Unique identifier for the aggregate
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Original version before any change has been made, used for optimistic concurrency checks.
        /// </summary>
        /// <remarks>
        /// To obtain the current version of the aggregate use State.Version instead.
        /// </remarks>
        int OriginalVersion { get; }

        /// <summary>
        /// Current state of the aggregate
        /// </summary>
        IState State { get; }

        /// <summary>
        /// Changes made to the aggregate in the form of events
        /// </summary>
        IEnumerable<IAggregateEvent> Changes { get; }
    }

    /// <summary>
    /// Aggregate root abstraction with generic state support, providing a clear separation between its state and event changes, enabling persistence ignorance.
    /// Snapshotting and/or Event sourcing can be used for persistence, or an object-relational-mapper between the state and data entities.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public interface IAggregate<TState> : IAggregate
        where TState : IState
    {
        /// <summary>
        /// Current state of the aggregate
        /// </summary>
        new TState State { get; }
    }
}
