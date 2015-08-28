using NDomain.CQRS;
using NDomain.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    public class EventSourcingConfigurator : Configurator
    {
        readonly HashSet<Type> aggregateTypes;

        public IEventStoreDb EventStoreDb { get; set; }
        public IEventStoreBus EventStoreBus { get; set; }

        public EventSourcingConfigurator(ContextBuilder builder)
            : base(builder)
        {
            this.aggregateTypes = new HashSet<Type>();

            builder.Configuring += this.OnConfiguring;
        }

        public EventSourcingConfigurator BindAggregate<TAggregate>()
            where TAggregate : IAggregate
        {
            this.aggregateTypes.Add(typeof(TAggregate));

            return this;
        }

        public EventSourcingConfigurator UseLocalEventStore()
        {
            this.EventStoreDb = new LocalEventStore();
            return this;
        }

        public EventSourcingConfigurator UsePublisher(IEventStoreBus eventStoreBus)
        {
            this.EventStoreBus = eventStoreBus;
            return this;
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            var serializer = EventStoreSerializer.FromAggregateTypes(this.aggregateTypes);

            builder.EventStore = new Lazy<IEventStore>(
                () => new EventStore(this.EventStoreDb ?? new LocalEventStore(),
                                     this.EventStoreBus ?? new EventBus(builder.MessageBus.Value),
                                     serializer));
        }
    }
}
