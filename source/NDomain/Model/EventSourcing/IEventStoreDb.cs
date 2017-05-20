using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Model.EventSourcing
{
    //TODO: find a better name
    public interface IEventStoreDb
    {
        /// <summary>
        /// Loads all events for a given event stream
        /// </summary>
        /// <param name="eventStreamId"></param>
        /// <returns></returns>
        Task<IEnumerable<IAggregateEvent<JObject>>> Load(string eventStreamId);

        /// <summary>
        /// Loads events for a given event stream, from start to end sequenceIds, inclusive
        /// </summary>
        /// <param name="eventStreamId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<IEnumerable<IAggregateEvent<JObject>>> LoadRange(string eventStreamId, int start, int end);

        /// <summary>
        /// Loads uncommitted events for the given transactionId, if any
        /// </summary>
        /// <param name="eventStreamId"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        Task<IEnumerable<IAggregateEvent<JObject>>> LoadUncommitted(string eventStreamId, string transactionId);

        /// <summary>
        /// Appends events to a given eventStreamId, as long as its version matches the expectedVersion.
        /// </summary>
        /// <exception cref="ConcurrencyException">When version doesn't match the expectedVersion</exception>
        /// <param name="eventStreamId"></param>
        /// <param name="transactionId"></param>
        /// <param name="expectedVersion"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task Append(string eventStreamId, string transactionId, int expectedVersion, IEnumerable<IAggregateEvent<JObject>> events);

        /// <summary>
        /// Commits events on a given eventStreamId
        /// </summary>
        /// <param name="eventStreamId"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        Task Commit(string eventStreamId, string transactionId);
    }
}
