namespace NDomain.Tests.Common.Sample
{
    public class CounterState : State
    {
        public int Value { get; private set; }

        public void OnCounterIncremented(CounterIncremented ev) => Value += ev.Increment;
        public void OnCounterMultiplied(CounterMultiplied ev) => Value *= ev.Factor;
        public void OnCounterReset(CounterReset ev) => Value = 0;
    }
}