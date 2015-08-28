using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.EventSourcing
{
    class StoredEvent
    {
        public StoredEvent(IAggregateEvent<JObject> source, string transactionId, bool committed)
        {
            this.Source = source;
            this.TransactionId = transactionId;
            this.Committed = committed;
        }

        public IAggregateEvent<JObject> Source { get; private set; }
        public string TransactionId { get; private set; }
        public bool Committed { get; set; }
    }

    public class LocalEventStore : IEventStoreDb
    {
        readonly ConcurrentDictionary<string, List<StoredEvent>> eventStreams;

        public LocalEventStore()
        {
            this.eventStreams = new ConcurrentDictionary<string, List<StoredEvent>>();
        }

        public Task<IEnumerable<IAggregateEvent<JObject>>> Load(string eventStreamId)
        {
            List<StoredEvent> eventStream;
            if (!this.eventStreams.TryGetValue(eventStreamId, out eventStream))
            {
                return Task.FromResult(Enumerable.Empty<IAggregateEvent<JObject>>());
            }


            lock (eventStream)
            {
                IEnumerable<IAggregateEvent<JObject>> events = eventStream.Select(e => e.Source).ToArray();
                return Task.FromResult(events);
            }
        }

        public Task<IEnumerable<IAggregateEvent<JObject>>> LoadRange(string eventStreamId, int start, int end)
        {
            List<StoredEvent> eventStream;
            if (!this.eventStreams.TryGetValue(eventStreamId, out eventStream))
            {
                return Task.FromResult(Enumerable.Empty<IAggregateEvent<JObject>>());
            }


            lock (eventStream)
            {
                IEnumerable<IAggregateEvent<JObject>> events = eventStream.Skip(start - 1)
                                                                          .Take(end - start + 1)
                                                                          .Select(e => e.Source).ToArray();
                return Task.FromResult(events);
            }
        }

        public Task<IEnumerable<IAggregateEvent<JObject>>> LoadUncommitted(string eventStreamId, string transactionId)
        {
            List<StoredEvent> eventStream;
            if (!this.eventStreams.TryGetValue(eventStreamId, out eventStream))
            {
                return Task.FromResult(Enumerable.Empty<IAggregateEvent<JObject>>());
            }


            lock (eventStream)
            {
                IEnumerable<IAggregateEvent<JObject>> events = eventStream.Where(e => e.TransactionId == transactionId && !e.Committed)
                                                                          .Select(e => e.Source).ToArray();
                return Task.FromResult(events);
            }
        }

        public Task Append(string eventStreamId, string transactionId, int expectedVersion, IEnumerable<IAggregateEvent<JObject>> events)
        {
            var eventStream = this.eventStreams.GetOrAdd(eventStreamId, id => new List<StoredEvent>());

            lock (eventStream)
            {
                var currentVersion = eventStream.Count;
                if (currentVersion != expectedVersion)
                {
                    var tcs = new TaskCompletionSource<bool>();
                    tcs.SetException(new ConcurrencyException(eventStreamId, expectedVersion, currentVersion));
                    return tcs.Task;
                }

                eventStream.AddRange(events.Select(e => new StoredEvent(e, transactionId, false)).ToArray());
            }

            return Task.FromResult(true);
        }


        public Task Commit(string eventStreamId, string transactionId)
        {
            var eventStream = this.eventStreams.GetOrAdd(eventStreamId, id => new List<StoredEvent>());

            lock (eventStream)
            {
                var uncommitted = eventStream.Where(e => e.TransactionId == transactionId && !e.Committed).ToArray();

                foreach (var ev in uncommitted)
                {
                    ev.Committed = true;
                }
            }

            return Task.FromResult(true);
        }
    }
}
