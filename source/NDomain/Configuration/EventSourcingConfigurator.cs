using NDomain.CQRS;
using NDomain.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for the event sourcing persistence capabilities
    /// </summary>
    public class EventSourcingConfigurator : Configurator
    {
        readonly HashSet<Type> aggregateTypes;

        /// <summary>
        /// Gets or sets the IEventStoreDb to be used. 
        /// Eg: Azure tables, SqlServer, RavenDb, etc.
        /// </summary>
        /// <remarks>
        /// Note that the IEventStoreDb only handles the persistence features, while the IEventStore is a higher level concept 
        /// which handles deserialization and coordinates event storage and publishing between the IEventStoreDb and IEventStoreBus.
        /// </remarks>
        public IEventStoreDb EventStoreDb { get; set; }

        public EventSourcingConfigurator(ContextBuilder builder)
            : base(builder)
        {
            this.aggregateTypes = new HashSet<Type>();

            builder.Configuring += this.OnConfiguring;
        }

        /// <summary>
        /// Registers an aggregate type to be used with EventSourcing. 
        /// This allows NDomain to generate serializer and deserializer functions for the aggregate events.
        /// </summary>
        /// <typeparam name="TAggregate">Type of the aggregate</typeparam>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public EventSourcingConfigurator BindAggregate<TAggregate>()
            where TAggregate : IAggregate
        {
            this.aggregateTypes.Add(typeof(TAggregate));

            return this;
        }

        /// <summary>
        /// Configures the local EventStore, which should be used only for test and learning purposes.
        /// </summary>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public EventSourcingConfigurator UseLocalEventStore()
        {
            this.EventStoreDb = new LocalEventStore();
            return this;
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            var serializer = EventStoreSerializer.FromAggregateTypes(this.aggregateTypes);

            builder.EventStore = new Lazy<IEventStore>(
                () => new EventStore(this.EventStoreDb ?? new LocalEventStore(),
                                     new EventBus(builder.MessageBus.Value),
                                     serializer));
        }
    }
}
