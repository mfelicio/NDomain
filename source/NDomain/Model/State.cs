using NDomain.Model;

namespace NDomain
{
    /// <summary>
    /// Base class for aggregate states, that already has State Mutator support.
    /// It uses a convention based for applying events.
    /// 
    /// Subclasses should have public or private methods using the following convention:
    ///     void On[NameOfTheEventType]([NameOfTheEventType] ev)
    ///
    /// These methods are called by a state mutator in order to apply events that modify the current state
    /// </summary>
    public abstract class State : IState
    {
        private readonly IStateMutator mutator;

        protected State()
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
