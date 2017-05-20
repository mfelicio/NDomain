using NDomain.Configuration;
using NDomain.IoC;
using NDomain.Logging;
using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDomain.Bus;
using NDomain.CQRS;
using NDomain.Model;
using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;

namespace NDomain
{
    /// <summary>
    /// Main container and entry point of the NDomain framework.
    /// </summary>
    public class DomainContext : IDomainContext
    {
        readonly IEventBus eventBus;
        readonly ICommandBus commandBus;
        readonly IEnumerable<IProcessor> processors;
        readonly IDependencyResolver resolver;


        public DomainContext(IEventBus eventBus,
                             ICommandBus commandBus,
                             IEnumerable<IProcessor> processors,
                             IDependencyResolver resolver)
        {
            this.eventBus = eventBus;
            this.commandBus = commandBus;
            this.processors = processors;
            this.resolver = resolver;
        }

        public IEventBus EventBus { get { return this.eventBus; } }
        public ICommandBus CommandBus { get { return this.commandBus; } }
        public IDependencyResolver Resolver { get { return this.resolver; } }
        
        public IAggregateRepository<T> GetRepository<T>()
            where T: IAggregate
        {
            return this.resolver.Resolve<IAggregateRepository<T>>();
        }

        public void StartProcessors()
        {
            foreach (var processor in this.processors)
            {
                processor.Start();
            }
        }

        public void StopProcessors()
        {
            foreach (var processor in this.processors)
            {
                processor.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var processor in processors)
            {
                processor.Dispose();
            }
        }

        /// <summary>
        /// Creates a new ContextBuilder that is used to configure a new DomainContext using a fluent interface.
        /// </summary>
        /// <returns></returns>
        public static ContextBuilder Configure() 
        {
            return new ContextBuilder();
        }
    }
}
