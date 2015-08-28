using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public abstract class Aggregate : IAggregate
    {
        readonly string id;
        readonly int originalVersion;

        readonly IState state;
        readonly List<IAggregateEvent> events;

        protected Aggregate(string id, IState state)
        {
            this.id = id;
            this.originalVersion = state.Version;
            this.state = state;
            this.events = new List<IAggregateEvent>();
        }

        public string Id { get { return this.id; } }
        public int OriginalVersion { get { return this.originalVersion; } }

        public IState State { get { return this.state; } }

        public IEnumerable<IAggregateEvent> Changes { get { return this.events; } }

        protected void On<T>(T ev)
        {
            var sequenceId = this.originalVersion + this.events.Count + 1;
            var @event = new AggregateEvent<T>(this.id, sequenceId, DateTime.UtcNow, ev);

            this.state.Mutate(@event);
            this.events.Add(@event);
        }
    }

    public abstract class Aggregate<TState> : Aggregate, IAggregate<TState>
        where TState : IState
    {
        readonly TState state;

        protected Aggregate(string id, TState state)
            : base(id, state)
        {
            this.state = state;
        }
        
        public new TState State { get { return this.state; } }
    }
}
