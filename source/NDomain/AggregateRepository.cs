using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public class AggregateRepository<T> : IAggregateRepository<T>
            where T : IAggregate
    {
        static readonly IAggregateFactory<T> Factory = AggregateFactory.For<T>();

        readonly IEventStore eventStore;

        public AggregateRepository(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        IAggregateFactory<T> IAggregateRepository<T>.Factory { get { return AggregateRepository<T>.Factory; } }

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

            return aggregate;
        }
    }
}
