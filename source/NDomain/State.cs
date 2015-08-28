using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public abstract class State : IState
    {
        readonly IStateMutator mutator;

        public State()
        {
            this.mutator = StateMutator.For(this.GetType());
            this.Version = 0;
        }

        public int Version
        {
            get;
            private set;
        }

        public void Mutate(IAggregateEvent @event)
        {
            this.mutator.Mutate(this, @event);
            this.Version++;
        }
    }
}
