using NDomain.Model;

namespace NDomain.Tests.Common.Sample
{
    public class Counter : EventSourcedAggregate<CounterState>
    {
        public Counter(string id, CounterState state)
            : base(id, state)
        {

        }

        public int Value => this.State.Value;

        public void Increment(int increment = 1)
        {
            On(new CounterIncremented { Increment = increment });
        }

        public void Multiply(int factor)
        {
            On(new CounterMultiplied { Factor = factor });
        }

        public void Reset()
        {
            On(new CounterReset());
        }
    }
}
