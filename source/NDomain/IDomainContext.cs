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
    public interface IDomainContext : IDisposable
    {
        IEventStore EventStore { get; }

        IEventBus EventBus { get; }
        ICommandBus CommandBus { get; }

        IAggregateRepository<T> GetRepository<T>()
            where T : IAggregate;

        void StartProcessors();
        void StopProcessors();
    }
}
