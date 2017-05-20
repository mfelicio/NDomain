namespace NDomain.Tests.Common.Sample
{
    public class StateOnlyAggregate : Aggregate<CounterState>
    {
        public StateOnlyAggregate(string id, CounterState state)
            : base(id, state)
        {

        }
    }
}
