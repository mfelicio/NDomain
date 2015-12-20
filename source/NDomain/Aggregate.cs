using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    /// <summary>
    /// Aggregate base class
    /// </summary>
    /// <typeparam name="TState">Type of the aggregate state</typeparam>
    public abstract class Aggregate<TState> : IAggregate<TState>
        where TState : IState
    {
        readonly string id;
        readonly int originalVersion;

        readonly TState state;
        readonly List<IAggregateEvent> events;

        protected Aggregate(string id, TState state)
        {
            this.id = id;
            this.originalVersion = state.Version;
            this.state = state;
            this.events = new List<IAggregateEvent>();
        }

        public string Id { get { return this.id; } }
        public int OriginalVersion { get { return this.originalVersion; } }

        IState IAggregate.State { get { return this.state; } }
        public TState State { get { return this.state; } }

        public IEnumerable<IAggregateEvent> Changes { get { return this.events; } }

        /// <summary>
        /// Updates the aggregate State by applying the event. 
        /// A new IAggregateEvent is added to the Changes collection, having the event as its payload.
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="ev">event</param>
        protected void On<T>(T ev)
        {
            var sequenceId = this.originalVersion + this.events.Count + 1;
            var @event = new AggregateEvent<T>(this.id, sequenceId, DateTime.UtcNow, ev);

            this.state.Mutate(@event);
            this.events.Add(@event);
        }
    }

}
