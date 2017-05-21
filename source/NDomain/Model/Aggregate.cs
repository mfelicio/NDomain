namespace NDomain.Model
{
    /// <summary>
    /// Aggregate base class
    /// </summary>
    /// <typeparam name="TState">Type of the aggregate state</typeparam>
    public abstract class Aggregate<TState> : IAggregate<TState>
        where TState : IState
    {
        protected Aggregate(string id, TState state)
        {
            this.Id = id;
            this.OriginalVersion = state.Version;
            this.State = state;
        }

        public string Id { get; }
        public int OriginalVersion { get; }
        public TState State { get; }

        IState IAggregate.State => this.State;
    }

}
