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

        protected Aggregate(string id, TState state)
        {
            this.id = id;
            this.originalVersion = state.Version;
            this.state = state;
        }

        public string Id { get { return this.id; } }
        public int OriginalVersion { get { return this.originalVersion; } }

        IState IAggregate.State { get { return this.state; } }
        public TState State { get { return this.state; } }
    }

}
