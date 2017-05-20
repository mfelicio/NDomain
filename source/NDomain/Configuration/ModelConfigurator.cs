using NDomain.CQRS;
using NDomain.Model;
using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;
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
    public class ModelConfigurator : Configurator
    {
        private readonly HashSet<Type> aggregateTypes;

        private IEventStoreDb eventStoreDb;
        private ISnapshotStore snapshotStore;

        /// <summary>
        /// Gets or sets the IEventStoreDb to be used. 
        /// Eg: Azure tables, SqlServer, RavenDb, etc.
        /// </summary>
        /// <remarks>
        /// Note that the IEventStoreDb only handles the persistence features, while the IEventStore is a higher level concept 
        /// which handles deserialization and coordinates event storage and publishing between the IEventStoreDb and IEventStoreBus.
        /// </remarks>

        protected ModelConfigurator UseEventStoreDb(IEventStoreDb eventStoreDb)
        {
            this.eventStoreDb = eventStoreDb;
            return this;
        }

        protected ModelConfigurator UseSnapshotStore(ISnapshotStore snapshotStore)
        {
            this.snapshotStore = snapshotStore;
            return this;
        }

        public IEventStoreDb EventStoreDb { get; set; }

        public ModelConfigurator(ContextBuilder builder)
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
        public ModelConfigurator BindAggregate<TAggregate>()
            where TAggregate : IAggregate
        {
            this.aggregateTypes.Add(typeof(TAggregate));

            return this;
        }

        /// <summary>
        /// Configures the local EventStore, which should be used only for test and learning purposes.
        /// </summary>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public ModelConfigurator UseLocalEventStore()
        {
            return UseEventStoreDb(new LocalEventStore());
        }

        public ModelConfigurator UseLocalSnapshotStore()
        {
            return UseSnapshotStore(new LocalSnapshotStore());
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            var serializer = EventStoreSerializer.FromAggregateTypes(this.aggregateTypes);

            builder.EventStore = new Lazy<IEventStore>(
                () => new EventStore(this.eventStoreDb ?? new LocalEventStore(),
                                     new EventBus(builder.MessageBus.Value),
                                     serializer));

            builder.SnapshotStore = new Lazy<ISnapshotStore>(
                () => this.snapshotStore ?? new LocalSnapshotStore());
        }
    }
}
