using NDomain.CQRS;
using NDomain.CQRS.Projections;
using NDomain.Model.EventSourcing;
using System;
using System.Threading.Tasks;
using NDomain.Model;
using NDomain.Tests.Common.Sample;

namespace NDomain.Tests.CQRS
{
    public class TestCommandHandler
    {
        readonly Action<ICommand> onMsg;

        public TestCommandHandler(Action<ICommand> onMsg)
        {
            this.onMsg = onMsg;
        }

        public Task Handle(ICommand<DoSimple> cmd)
        {
            this.onMsg(cmd);
            return Task.FromResult(true);
        }

        public Task Handle(ICommand<DoComplex> cmd)
        {
            this.onMsg(cmd);
            return Task.FromResult(true);
        }

        public Task Handle(ICommand<DoSimpleStuff> cmd)
        {
            this.onMsg(cmd);
            return Task.FromResult(true);
        }

        public Task Handle(ICommand<DoComplexStuff> cmd)
        {
            this.onMsg(cmd);
            return Task.FromResult(true);
        }

        public Task Handle(ICommand<DoGenericStuff<DoComplexStuff>> cmd)
        {
            this.onMsg(cmd);
            return Task.FromResult(true);
        }

        public Task Handle(ICommand<DoNonGenericStuff> cmd)
        {
            this.onMsg(cmd);
            return Task.FromResult(true);
        }
    }

    public class CounterEventsHandler
    {
        readonly Action<IAggregateEvent> onMsg;

        public CounterEventsHandler(Action<IAggregateEvent> onMsg)
        {
            this.onMsg = onMsg;
        }

        public Task On(IAggregateEvent<CounterIncremented> ev)
        {
            this.onMsg(ev);
            return Task.FromResult(true);
        }

        public Task On(IAggregateEvent<CounterMultiplied> ev)
        {
            this.onMsg(ev);
            return Task.FromResult(true);
        }

        public Task On(IAggregateEvent<CounterReset> ev)
        {
            this.onMsg(ev);
            return Task.FromResult(true);
        }
    }

    class CounterStats
    {
        public int NumberOfIncrements { get; set; }
        public int NumberOfMultiplications { get; set; }
        public int NumberOfResets { get; set; }
    }

    class CounterQueryEventsHandler : QueryEventsHandler<CounterStats>
    {
        public CounterQueryEventsHandler(IQueryStore<CounterStats> queryStore, IEventStore eventStore)
            : base(queryStore, eventStore)
        {

        }

        internal Task On(IAggregateEvent<CounterReset> ev)
        {
            return base.OnEvent(ev);
        }

        internal void On(CounterStats query, CounterReset ev)
        {
            query.NumberOfResets++;
        }

        internal Task On(IAggregateEvent<CounterMultiplied> ev)
        {
            return base.OnEvent(ev);
        }

        internal void On(CounterStats query, CounterMultiplied ev)
        {
            query.NumberOfMultiplications++;
        }

        internal Task On(IAggregateEvent<CounterIncremented> ev)
        {
            return base.OnEvent(ev);
        }

        internal void On(CounterStats query, CounterIncremented ev)
        {
            query.NumberOfIncrements++;
        }
    }
}
