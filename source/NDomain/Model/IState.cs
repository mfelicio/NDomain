namespace NDomain.Model
{
    /// <summary>
    /// Represents the state of an aggregate root, which is used when loading aggregates.
    /// The state can be persisted as is, or rebuilt by replaying aggregate state change events.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Current version of the state, used for optimistic concurrency checks together with the IAggregate.OriginalVersion
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Mutates the state by applying the aggregate event. After every mutation, the Version is increased.
        /// </summary>
        /// <param name="event">event that represents a state change in the aggregate</param>
        void Mutate(IAggregateEvent @event);
    }
}
