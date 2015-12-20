using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Repository pattern implementation using an EventStore to persist and retrieve aggregates.
    /// Aggregates are created from event streams through factories.
    /// </summary>
    /// <typeparam name="T">Type of the aggregate</typeparam>
    public class AggregateRepository<T> : IAggregateRepository<T>
            where T : IAggregate
    {
        /// <summary>
        /// Factory object that can create new aggregates of <typeparamref name="T"/>. 
        /// This is stateless and costly to instantiate, so it's static and reused.
        /// </summary>
        static readonly IAggregateFactory<T> Factory = AggregateFactory.For<T>();

        /// <summary>
        /// The event store where aggregate events are persisted.
        /// </summary>
        readonly IEventStore eventStore;

        public AggregateRepository(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task<T> Find(string id)
        {
            var events = await this.eventStore.Load(id);

            if (!events.Any())
            {
                // not found
                throw new Exception("not found");
            }

            var aggregate = Factory.CreateFromEvents(id, events.ToArray());

            return aggregate;
        }

        public async Task<T> FindOrDefault(string id)
        {
            var events = await this.eventStore.Load(id);

            var aggregate = Factory.CreateFromEvents(id, events.ToArray());

            return aggregate;
        }

        public async Task<T> Save(T aggregate)
        {
            if (!aggregate.Changes.Any())
            {
                return aggregate;
            }

            await this.eventStore.Append(aggregate.Id, aggregate.OriginalVersion, aggregate.Changes);

            // TODO: aggregates changes should be reset, so that the returned aggregate is equivalent to loading the persistent aggregate from the eventstore

            return aggregate;
        }
    }
}
