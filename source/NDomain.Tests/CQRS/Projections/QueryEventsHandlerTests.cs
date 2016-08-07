using Moq;
using NDomain.CQRS.Projections;
using NDomain.EventSourcing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.CQRS.Projections
{
    [TestFixture]
    public class QueryEventsHandlerTests
    {
        private readonly string AggregateId = "counter 1";

        private IQueryStore<CounterStats> queryStore;
        private IEventStore eventStore;

        [SetUp]
        public void SetUp()
        {
            this.eventStore = new EventStore(
                                new LocalEventStore(),
                                new Mock<IEventStoreBus>().Object,
                                EventStoreSerializer.FromAggregateTypes(typeof(Sample.Counter)));

            this.queryStore = new LocalQueryStore<CounterStats>();
        }

        [TestCase("1:0:0")]
        [TestCase("2:0:0")]
        [TestCase("2:2:2")]
        public async Task ShouldHandleEventInCorrectOrderAndStoreQuery(string expectedStatsStr)
        {
            await ShouldHandleEventsAndStoreQuery(expectedStatsStr, HandleChanges);
        }

        [TestCase("1:0:0")]
        [TestCase("2:0:0")]
        [TestCase("2:2:2")]
        public async Task ShouldHandleEventsInWrongOrderAndStoreQuery(string expectedStatsStr)
        {
            await ShouldHandleEventsAndStoreQuery(expectedStatsStr, HandleChangesInWrongOrder);
        }

        [TestCase("1:0:0")]
        [TestCase("2:0:0")]
        [TestCase("5:3:4")]
        public async Task ShouldHandleEventsAsynchronouslyAndStoreQuery(string expectedStatsStr)
        {
            await ShouldHandleEventsAndStoreQuery(expectedStatsStr, HandleChangesAsynchronously);
        }

        private async Task ShouldHandleEventsAndStoreQuery(string expectedStatsStr, Func<CounterQueryEventsHandler, IEnumerable<IAggregateEvent>, Task> handleChanges)
        {
            // arrange
            var expected = CreateStats(expectedStatsStr);
            var changes = await MakeExpectedChanges(expected);

            // act
            var handler = new CounterQueryEventsHandler(this.queryStore, this.eventStore);
            await handleChanges(handler, changes);

            // assert
            await Task.Delay(100);

            var query = await queryStore.Get(AggregateId);

            if (query.Version != changes.Count())
            {

            }

            Assert.That(query.Version, Is.EqualTo(changes.Count()));
            AssertStats(query.Data, expected);
        }

        private async Task<IEnumerable<IAggregateEvent>> MakeExpectedChanges(CounterStats expected)
        {
            var counter = new Sample.Counter(AggregateId, new Sample.CounterState());

            for (var i = 0; i < expected.NumberOfIncrements; ++i)
            {
                counter.Increment();
            }

            for (var i = 0; i < expected.NumberOfMultiplications; ++i)
            {
                counter.Multiply(1);
            }

            for (var i = 0; i < expected.NumberOfResets; ++i)
            {
                counter.Reset();
            }
            await this.eventStore.Append(AggregateId, 0, counter.Changes);

            return counter.Changes;
        }

        private async Task HandleChanges(CounterQueryEventsHandler handler, IEnumerable<IAggregateEvent> changes)
        {
            foreach (var ev in changes)
            {
                await HandleEvent(handler, ev);
            }
        }

        private async Task HandleChangesInWrongOrder(CounterQueryEventsHandler handler, IEnumerable<IAggregateEvent> changes)
        {
            foreach (var ev in changes.Reverse())
            {
                await HandleEvent(handler, ev);
            }
        }

        private Task HandleChangesAsynchronously(CounterQueryEventsHandler handler, IEnumerable<IAggregateEvent> changes)
        {
            var tasks = new List<Task>();

            foreach (var ev in changes)
            {
                tasks.Add(Task.Run(async () => await HandleEvent(handler, ev)));
            }

            return Task.WhenAll(tasks);
        }

        private Task HandleEvent(CounterQueryEventsHandler handler, IAggregateEvent ev)
        {
            if (ev is IAggregateEvent<Sample.CounterIncremented>)
            {
                return handler.On(ev as IAggregateEvent<Sample.CounterIncremented>);
            }
            else if (ev is IAggregateEvent<Sample.CounterMultiplied>)
            {
                return handler.On(ev as IAggregateEvent<Sample.CounterMultiplied>);
            }
            else
            {
                return handler.On(ev as IAggregateEvent<Sample.CounterReset>);
            }
        }

        /// <summary>
        /// Compares counter stats against the expected values
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="expected">string in the form of {incremented}:{multiplied}:{reseted}</param>
        private void AssertStats(CounterStats actual, CounterStats expected)
        {
            Assert.That(actual.NumberOfIncrements, Is.EqualTo(expected.NumberOfIncrements));
            Assert.That(actual.NumberOfMultiplications, Is.EqualTo(expected.NumberOfMultiplications));
            Assert.That(actual.NumberOfResets, Is.EqualTo(expected.NumberOfResets));
        }

        private CounterStats CreateStats(string statsStr)
        {
            var values = statsStr.Split(":".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(v => int.Parse(v)).ToArray();

            return new CounterStats
            {
                NumberOfIncrements = values[0],
                NumberOfMultiplications = values[1],
                NumberOfResets = values[2]
            };
        }
    }
}
