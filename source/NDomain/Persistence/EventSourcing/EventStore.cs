using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDomain.Model;

namespace NDomain.Persistence.EventSourcing
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreDb db;
        private readonly IEventStoreBus bus;
        private readonly IEventStoreSerializer serializer;

        public EventStore(IEventStoreDb db, 
                          IEventStoreBus bus,
                          IEventStoreSerializer serializer)
        {
            this.db = db;
            this.bus = bus;
            this.serializer = serializer;
        }

        public async Task<IEnumerable<IAggregateEvent>> Load(string aggregateId)
        {
            var transaction = DomainTransaction.Current;
            if (transaction != null && transaction.DeliveryCount > 1)
            {
                await CheckAndProcessUncommittedEvents(aggregateId, transaction.Id);
            }

            var sourceEvents = await this.db.Load(aggregateId);

            var events = sourceEvents.Select(e => this.serializer.Deserialize(e));
            return events.ToArray();
        }

        public async Task<IEnumerable<IAggregateEvent>> LoadRange(string aggregateId, int start, int end)
        {
            var transaction = DomainTransaction.Current;
            if (transaction != null && transaction.DeliveryCount > 1)
            {
                await CheckAndProcessUncommittedEvents(aggregateId, transaction.Id);
            }

            var sourceEvents = await this.db.LoadRange(aggregateId, start, end);

            var events = sourceEvents.Select(e => this.serializer.Deserialize(e));
            return events.ToArray();
        }

        public async Task Append(string aggregateId, int expectedVersion, IEnumerable<IAggregateEvent> events)
        {
            var sourceEvents = events.Select(e => this.serializer.Serialize(e))
                                     .ToArray();

            var transaction = DomainTransaction.Current;
            var transactionId = transaction != null ? transaction.Id : Guid.NewGuid().ToString();

            await this.db.Append(aggregateId, transactionId, expectedVersion, sourceEvents);

            foreach (var ev in sourceEvents)
            {
                await this.bus.Publish(ev);
            }

            await this.db.Commit(aggregateId, transactionId);
        }

        private async Task CheckAndProcessUncommittedEvents(string aggregateId, string transactionId)
        {
            var uncommittedEvents = await this.db.LoadUncommitted(aggregateId, transactionId);
            if (uncommittedEvents.Any())
            {
                foreach (var ev in uncommittedEvents)
                {
                    await this.bus.Publish(ev);
                }

                await this.db.Commit(aggregateId, transactionId);
            }
        }
    }
}
