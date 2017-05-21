using Moq;
using NDomain.CQRS.Projections;
using NDomain.Model.EventSourcing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDomain.Model;
using NDomain.Tests.Common.Sample;

namespace NDomain.Tests.CQRS.Projections
{
    [TestFixture]
    public class QueryEventsHandlerTests
    {
        private const string AggregateId = "counter 1";

        private IQueryStore<CounterStats> queryStore;
        private IEventStore eventStore;

        [SetUp]
        public void SetUp()
        {
            this.eventStore = new EventStore(
                                new LocalEventStore(),
                                new Mock<IEventStoreBus>().Object,
                                EventStoreSerializer.FromAggregateTypes(typeof(Counter)));

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
            var counter = new Counter(AggregateId, new CounterState());

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
            if (ev is IAggregateEvent<CounterIncremented>)
            {
                return handler.On(ev as IAggregateEvent<CounterIncremented>);
            }
            else if (ev is IAggregateEvent<CounterMultiplied>)
            {
                return handler.On(ev as IAggregateEvent<CounterMultiplied>);
            }
            else
            {
                return handler.On(ev as IAggregateEvent<CounterReset>);
            }
        }

        /// <summary>
        /// Compares counter stats against the expected values
        /// </summary>
        /// <param name="actual">actual</param>
        /// <param name="expected">expected</param>
        private void AssertStats(CounterStats actual, CounterStats expected)
        {
            Assert.That(actual.NumberOfIncrements, Is.EqualTo(expected.NumberOfIncrements));
            Assert.That(actual.NumberOfMultiplications, Is.EqualTo(expected.NumberOfMultiplications));
            Assert.That(actual.NumberOfResets, Is.EqualTo(expected.NumberOfResets));
        }

        private CounterStats CreateStats(string statsStr)
        {
            var values = statsStr.Split(":".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(int.Parse).ToArray();

            return new CounterStats
            {
                NumberOfIncrements = values[0],
                NumberOfMultiplications = values[1],
                NumberOfResets = values[2]
            };
        }
    }
}
