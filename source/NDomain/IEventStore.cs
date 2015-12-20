using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public interface IEventStore
    {
        /// <summary>
        /// Loads all events for a given aggregateId
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        Task<IEnumerable<IAggregateEvent>> Load(string aggregateId);

        /// <summary>
        /// Loads events for a given aggregateId, from start to end sequenceIds, inclusive
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<IEnumerable<IAggregateEvent>> LoadRange(string aggregateId, int start, int end);

        /// <summary>
        /// Appends events to a given aggregateId, as long as its version matches the expectedVersion.
        /// </summary>
        /// <exception cref="ConcurrencyException">When current stored version doesn't match the expectedVersion</exception>
        /// <param name="aggregateId"></param>
        /// <param name="expectedVersion"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task Append(string aggregateId, int expectedVersion, IEnumerable<IAggregateEvent> events);
    }

    /// <summary>
    /// Exception raised when there's a concurrency conflict persisting changes to an aggregate
    /// </summary>
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string aggregateId, int expectedVersion, int currentVersion)
        {
            this.AggregateId = aggregateId;
            this.ExpectedVersion = expectedVersion;
            this.CurrentVersion = currentVersion;
        }

        /// <summary>
        /// Id of the aggregate
        /// </summary>
        public string AggregateId { get; private set; }

        /// <summary>
        /// Expected version of the persisted aggregate. This should match the IAggregate.OriginalVersion
        /// </summary>
        public int ExpectedVersion { get; private set; }

        /// <summary>
        /// Current version of the persisted aggregate. This should always be higher than ExpectedVersion.
        /// </summary>
        public int CurrentVersion { get; private set; }
    }
}
