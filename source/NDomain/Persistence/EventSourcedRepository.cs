using System;
using System.Linq;
using System.Threading.Tasks;
using NDomain.Model;
using NDomain.Persistence.EventSourcing;

namespace NDomain.Persistence
{
    /// <summary>
    /// Repository pattern implementation using an EventStore to persist and retrieve aggregates.
    /// Aggregates are created from event streams through factories.
    /// </summary>
    /// <typeparam name="T">Type of the aggregate</typeparam>
    internal class EventSourcedRepository<T> : IAggregateRepository<T>
            where T : IAggregate
    {
        /// <summary>
        /// Factory object that can create new aggregates of <typeparamref name="T"/>. 
        /// This is stateless and costly to instantiate, so it's static and reused.
        /// </summary>
        private static readonly IAggregateFactory<T> Factory = AggregateFactory.For<T>();

        /// <summary>
        /// The event store where aggregate events are persisted.
        /// </summary>
        private readonly IEventStore eventStore;

        public EventSourcedRepository(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        // TODO: remove this method
        public async Task<T> Find(string id)
        {
            var events = await this.eventStore.Load(id);
            if (!events.Any())
            {
                throw new Exception("not found");
            }

            var aggregate = Factory.CreateFromEvents(id, events.ToArray());

            return aggregate;
        }

        public async Task<T> FindOrDefault(string id)
        {
            var aggregate = await LoadFromEvents(id);

            return aggregate;
        }

        public async Task<T> Save(T ag)
        {
            var aggregate = ag as IEventSourcedAggregate;
            if (aggregate.Changes.Any())
            {
                await PersistEventChanges(aggregate);
            }
            // TODO: aggregates changes should be reset, so that the returned aggregate is equivalent to loading the persistent aggregate from the eventstore

            return ag;
        }

        private async Task<T> LoadFromEvents(string id)
        {
            var events = await this.eventStore.Load(id);
            var aggregate = Factory.CreateFromEvents(id, events.ToArray());

            return aggregate;
        }

        private Task PersistEventChanges(IEventSourcedAggregate aggregate)
        {
            return this.eventStore.Append(aggregate.Id, aggregate.OriginalVersion, aggregate.Changes);
        }
    }
}
