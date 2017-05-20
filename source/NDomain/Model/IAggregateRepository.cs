using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Represents a higher level persistence abstraction for aggregates and implements the repository pattern
    /// </summary>
    /// <typeparam name="T">Type of the aggregate</typeparam>
    public interface IAggregateRepository<T>
            where T : IAggregate
    {
        /// <summary>
        /// Returns an aggregate with the given id, or throws an exception if the aggregate isn't found
        /// </summary>
        /// <param name="id">id of the aggregate</param>
        /// <returns>An aggregate with the given id</returns>
        Task<T> Find(string id);

        /// <summary>
        /// Returns an aggregate with the given id or null if the aggregate isn't found.
        /// </summary>
        /// <param name="id">id of the aggregate</param>
        /// <returns>An aggregate with the given id</returns>
        Task<T> FindOrDefault(string id);

        /// <summary>
        /// Persists an aggregate and publishes the event changes in the <see cref="IEventBus"/>
        /// </summary>
        /// <param name="aggregate">a modified aggregate to persist</param>
        /// <returns>The persisted aggregate</returns>
        Task<T> Save(T aggregate);
    }
}
