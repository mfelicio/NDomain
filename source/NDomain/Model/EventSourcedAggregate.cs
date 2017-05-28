using System;
using System.Collections.Generic;
using NDomain.Persistence;

namespace NDomain.Model
{
    public class EventSourcedAggregate<TState> : Aggregate<TState>, IEventSourcedAggregate<TState>
        where TState: IState
    {
        private readonly List<IAggregateEvent> events;

        public EventSourcedAggregate(string id, TState state)
            :base(id, state)
        {
            this.events = new List<IAggregateEvent>();
        }

        public IEnumerable<IAggregateEvent> Changes => this.events;

        /// <summary>
        /// Updates the aggregate State by applying the event. 
        /// A new IAggregateEvent is added to the Changes collection, having the event as its payload.
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="ev">event</param>
        protected void On<T>(T ev)
        {
            var sequenceId = this.OriginalVersion + this.events.Count + 1;
            var @event = new AggregateEvent<T>(this.Id, sequenceId, DateTime.UtcNow, ev);

            this.State.Mutate(@event);
            this.events.Add(@event);
        }
    }
}
