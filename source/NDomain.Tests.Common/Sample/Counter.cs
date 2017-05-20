using NDomain.Model;

namespace NDomain.Tests.Common.Sample
{
    public class CounterIncremented
    {
        public int Increment { get; set; }
    }

    // reset, in past tense..
    public class CounterReset { }

    public class CounterMultiplied
    {
        public int Factor { get; set; }
    }

    public class CounterState : State
    {
        public int Value { get; private set; }

        public void OnCounterIncremented(CounterIncremented ev) { Value += ev.Increment; }
        public void OnCounterMultiplied(CounterMultiplied ev) { Value *= ev.Factor; }
        public void OnCounterReset(CounterReset ev) { Value = 0; }
    }

    public class Counter : EventSourcedAggregate<CounterState>
    {
        public Counter(string id, CounterState state)
            : base(id, state)
        {

        }

        public int Value { get { return this.State.Value; } }

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
