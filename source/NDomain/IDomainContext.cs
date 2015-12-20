using NDomain.CQRS;
using NDomain.IoC;
using NDomain.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// NDomain's main container. Exposes persistence and messaging components, as well as a means to start and stop processing messages.
    /// </summary>
    public interface IDomainContext : IDisposable
    {
        /// <summary>
        /// EventStore that can be used to persist aggregate event streams
        /// </summary>
        IEventStore EventStore { get; }

        /// <summary>
        /// EventBus that can be used to publish events to subscribed listeners
        /// </summary>
        IEventBus EventBus { get; }

        /// <summary>
        /// CommandBus that can be used to send commands to a subscribed listener
        /// </summary>
        ICommandBus CommandBus { get; }

        /// <summary>
        /// Gets an IAggregateRepository that is a higher level persistence abstraction for aggregates
        /// </summary>
        /// <typeparam name="T">Type of the aggregate</typeparam>
        /// <returns>Returns an IAggregateRepository for the given aggregate Type</returns>
        IAggregateRepository<T> GetRepository<T>()
            where T : IAggregate;

        /// <summary>
        /// Starts processing incoming messages
        /// </summary>
        void StartProcessors();

        /// <summary>
        /// Stops processing incoming messages
        /// </summary>
        void StopProcessors();
    }
}
