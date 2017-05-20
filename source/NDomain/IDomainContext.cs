using NDomain.CQRS;
using NDomain.IoC;
using NDomain.Logging;
using NDomain.Model;
using NDomain.Model.EventSourcing;
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
        /// EventBus that can be used to publish events to subscribed listeners
        /// </summary>
        IEventBus EventBus { get; }

        /// <summary>
        /// CommandBus that can be used to send commands to a subscribed listener
        /// </summary>
        ICommandBus CommandBus { get; }

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
