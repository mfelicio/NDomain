using NDomain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Projections
{
    /// <summary>
    /// [Experimental] Provides support for projections from aggregate events into eventually-consistent read models.
    /// The idea is that the read models can be disposed of, and be rebuilt from the events projection, making it suitable for LRU caches.
    /// Ensures that events that update read models are processed according to its sequence numbers within the event stream.
    /// Concurrent updates are not an issue, because everytime a new event updates the read model, the source projection events are checked to verify if the read model's version is behind and in that case, missed events will be reprocessed.
    /// </summary>
    /// <remarks>This is still experimental work and most likely will undergo changes in newer versions.</remarks>
    /// <typeparam name="T"></typeparam>
    public abstract class QueryEventsHandler<T>
        where T : new()
    {
        static readonly TimeSpan WaitForPreviousVersionTimeout = TimeSpan.FromSeconds(1);

        readonly Dictionary<string, Action<T, IAggregateEvent>> handlers;

        readonly IQueryStore<T> queryStore;
        readonly IEventStore eventStore;


        protected QueryEventsHandler(IQueryStore<T> queryStore, IEventStore eventStore)
        {
            this.queryStore = queryStore;
            this.eventStore = eventStore;

            this.handlers = ReflectionUtils.FindQueryEventHandlerMethods<T>(this);
        }

        private bool TryGetEventHandler(string name, out Action<T, IAggregateEvent> handler)
        {
            return this.handlers.TryGetValue(name, out handler);
        }

        private Action<T, IAggregateEvent> GetEventHandler(string name)
        {
            return this.handlers[name];
        }

        protected async Task OnEvent(IAggregateEvent ev, string queryId = null)
        {
            var eventHandler = this.GetEventHandler(ev.Name);
            queryId = queryId ?? ev.AggregateId;

            var expectedPreviousVersion = ev.SequenceId - 1;

            var query = await this.queryStore.GetOrWaitUntil(
                                                queryId, 
                                                expectedPreviousVersion, 
                                                WaitForPreviousVersionTimeout);

            if (query.Version >= ev.SequenceId)
            {
                // event already applied
                return;
            }

            if (query.Version == 0)
            {
                query.Data = new T();
            }

            if (query.Version < ev.SequenceId - 1)
            {
                var start = query.Version + 1;
                var end = ev.SequenceId - 1;
                var events = await this.eventStore.LoadRange(ev.AggregateId, start, end);

                foreach (var @event in events)
                {
                    Action<T, IAggregateEvent> evHandler;
                    if (this.TryGetEventHandler(@event.Name, out evHandler))
                    {
                        evHandler(query.Data, @event);
                    }
                }
            }

            eventHandler(query.Data, ev);

            query.Version = ev.SequenceId;
            query.DateUtc = DateTime.UtcNow;

            await this.queryStore.Set(queryId, query);
        }


    }
}
