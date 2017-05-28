using System;
using System.Threading.Tasks;
using NDomain.Model;
using NDomain.Persistence.Snapshot;

namespace NDomain.Persistence
{
    /// <summary>
    /// Repository pattern implementation using a SnapshotStore to persist and retrieve aggregates.
    /// Aggregates are created from their persisted state.
    /// </summary>
    /// <typeparam name="T">Type of the aggregate</typeparam>
    internal class SnapshotRepository<T> : IAggregateRepository<T>
            where T : IAggregate
    {
        /// <summary>
        /// Factory object that can create new aggregates of <typeparamref name="T"/>. 
        /// This is stateless and costly to instantiate, so it's static and reused.
        /// </summary>
        private static readonly IAggregateFactory<T> Factory = AggregateFactory.For<T>();

        /// <summary>
        /// The snapshot store where aggregate state is persisted
        /// </summary>
        private readonly ISnapshotStore snapshotStore;

        public SnapshotRepository(ISnapshotStore snapshotStore)
        {
            this.snapshotStore = snapshotStore;
        }

        // TODO: remove this method
        public async Task<T> Find(string id)
        {
            var state = await this.snapshotStore.Load(id);

            if (state == null)
            {
                throw new Exception("not found");
            }

            var aggregate = Factory.CreateFromState(id, state);

            return aggregate;
        }

        public async Task<T> FindOrDefault(string id)
        {
            var aggregate = await LoadFromState(id);

            return aggregate;
        }

        public async Task<T> Save(T aggregate)
        {
            if (aggregate.OriginalVersion == aggregate.State.Version)
            {
                return aggregate;
            }

            await PersistState(aggregate);

            // TODO: aggregates changes should be reset, so that the returned aggregate is equivalent to loading the persistent aggregate from the eventstore

            return aggregate;
        }

        private async Task<T> LoadFromState(string id)
        {
            var state = await this.snapshotStore.Load(id);
            var aggregate = state == null ? 
                                Factory.CreateNew(id) : 
                                Factory.CreateFromState(id, state);
            return aggregate;
        }

        private Task PersistState(IAggregate aggregate)
        {
            return this.snapshotStore.Save(aggregate.Id, aggregate.State, aggregate.OriginalVersion);
        }
    }
}
