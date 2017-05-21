using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;
using System.Threading.Tasks;

namespace NDomain.Model
{
    public class AggregateRepository<T> : IAggregateRepository<T>
        where T : IAggregate
    {
        private readonly IAggregateRepository<T> inner;

        public AggregateRepository(IEventStore eventStore, ISnapshotStore snapshotStore)
        {
            if (typeof(IEventSourcedAggregate).IsAssignableFrom(typeof(T)))
            {
                this.inner = new EventSourcedRepository<T>(eventStore);
            }
            else
            {
                this.inner = new SnapshotRepository<T>(snapshotStore);
            }
        }

        public Task<T> Find(string id)
        {
            return this.inner.Find(id);
        }

        public Task<T> FindOrDefault(string id)
        {
            return this.inner.FindOrDefault(id);
        }

        public Task<T> Save(T aggregate)
        {
            return this.inner.Save(aggregate);
        }
    }
}
