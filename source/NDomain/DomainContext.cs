using NDomain.Configuration;
using NDomain.IoC;
using System.Collections.Generic;
using NDomain.Bus;
using NDomain.CQRS;
using NDomain.Model;
using NDomain.Persistence;

namespace NDomain
{
    /// <summary>
    /// Main container and entry point of the NDomain framework.
    /// </summary>
    public class DomainContext : IDomainContext
    {
        private readonly IEnumerable<IProcessor> processors;


        public DomainContext(IEventBus eventBus,
                             ICommandBus commandBus,
                             IEnumerable<IProcessor> processors,
                             IDependencyResolver resolver)
        {
            this.EventBus = eventBus;
            this.CommandBus = commandBus;
            this.processors = processors;
            this.Resolver = resolver;
        }

        public IEventBus EventBus { get; }
        public ICommandBus CommandBus { get; }
        public IDependencyResolver Resolver { get; }

        public IAggregateRepository<T> GetRepository<T>()
            where T: IAggregate
        {
            return this.Resolver.Resolve<IAggregateRepository<T>>();
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
