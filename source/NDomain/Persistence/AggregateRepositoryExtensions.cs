using System;
using System.Threading.Tasks;
using NDomain.Model;

namespace NDomain.Persistence
{
    public static class AggregateRepositoryExtensions
    {
        /// <summary>
        /// Finds an aggregate, runs the update handler and saves the aggregate
        /// </summary>
        /// <typeparam name="T">Type of the aggregate</typeparam>
        /// <param name="repository">repository instance</param>
        /// <param name="id">the aggregate id</param>
        /// <param name="handler">the update handler</param>
        /// <returns>A Task containing the updated aggregate</returns>
        public static async Task<T> Update<T>(this IAggregateRepository<T> repository, string id, Action<T> handler)
            where T : IAggregate
        {
            var aggregate = await repository.Find(id);

            handler(aggregate);

            await repository.Save(aggregate);

            return aggregate;
        }

        /// <summary>
        /// Finds or creates a new aggregate if it doesn't exist, runs the update handler and saves it.
        /// </summary>
        /// <typeparam name="T">Type of the aggregate</typeparam>
        /// <param name="repository">repository instance</param>
        /// <param name="id">the aggregate id</param>
        /// <param name="handler">the create/update handler</param>
        /// <returns>A Task containing the created or updated aggregate</returns>
        public static async Task<T> CreateOrUpdate<T>(this IAggregateRepository<T> repository, string id, Action<T> handler)
            where T : IAggregate
        {
            var aggregate = await repository.FindOrDefault(id);

            handler(aggregate);

            await repository.Save(aggregate);

            return aggregate;
        }
    }
}
