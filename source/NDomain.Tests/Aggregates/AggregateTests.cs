using NUnit.Framework;
using System;
using System.Linq;
using NDomain.Model;
using NDomain.Tests.Common.Sample;

namespace NDomain.Tests.Aggregates
{
    /// <summary>
    /// Aggregate tests based on the sample Race aggregate
    /// </summary>
    public class AggregateTests
    {
        [Test]
        public void CanCreateNewAggregate()
        {
            // arrange
            var factory = AggregateFactory.For<Counter>();
            var aggregateId = "some id";

            // act
            var aggregate = factory.CreateNew(aggregateId);

            // assert
            Assert.NotNull(aggregate);
            Assert.AreEqual(aggregateId, aggregate.Id);
            Assert.AreEqual(0, aggregate.OriginalVersion);
            Assert.AreEqual(0, aggregate.Changes.Count());

            Assert.NotNull(aggregate.State);
            Assert.AreEqual(0, aggregate.State.Version);
        }

        [Test]
        public void CanCreateFromEvents()
        {
            // arrange
            var factory = AggregateFactory.For<Counter>();
            var aggregateId = "some id";
            var events = new IAggregateEvent[] {
                new AggregateEvent<CounterIncremented>(
                    aggregateId, 1, DateTime.UtcNow, new CounterIncremented { Increment = 1 }),
                new AggregateEvent<CounterMultiplied>(
                    aggregateId, 2, DateTime.UtcNow, new CounterMultiplied{ Factor = 2}),
                new AggregateEvent<CounterIncremented>(
                    aggregateId, 3, DateTime.UtcNow, new CounterIncremented { Increment = 5 }),
            };

            // act
            var aggregate = factory.CreateFromEvents(aggregateId, events);

            // assert
            Assert.NotNull(aggregate);
            Assert.AreEqual(aggregateId, aggregate.Id);
            Assert.AreEqual(events.Last().SequenceId, aggregate.OriginalVersion);
            Assert.AreEqual(0, aggregate.Changes.Count());

            Assert.NotNull(aggregate.State);
            // no changes, so state version is the same
            Assert.AreEqual(events.Last().SequenceId, aggregate.State.Version);
        }

        [Test]
        public void CanCreateFromState()
        {
            // arrange
            var factory = AggregateFactory.For<Counter>();
            var aggregateId = "some id";
            var state = new CounterState();
            for (var i = 0; i < 10; ++i)
            {
                state.Mutate(new AggregateEvent<CounterIncremented>(
                                    aggregateId, i, DateTime.UtcNow, new CounterIncremented()));
            }

            // act
            var aggregate = factory.CreateFromState(aggregateId, state);

            // assert
            Assert.NotNull(aggregate);
            Assert.AreEqual(aggregateId, aggregate.Id);
            Assert.AreEqual(state.Version, aggregate.OriginalVersion);
            Assert.AreEqual(0, aggregate.Changes.Count());

            Assert.NotNull(aggregate.State);
            // no changes, so state version is the same
            Assert.AreEqual(state.Version, aggregate.State.Version);
        }

        [Test]
        public void CannotCreateFromUnknownEvents()
        {
            // arrange
            var factory = AggregateFactory.For<Counter>();
            var aggregateId = "some id";
            var events = new IAggregateEvent[] {
                new AggregateEvent<CounterIncremented>(aggregateId, 1, DateTime.UtcNow, new CounterIncremented { Increment = 1 }),
                new AggregateEvent<EventThatNoAggregateKnows>(aggregateId, 2, DateTime.UtcNow, new EventThatNoAggregateKnows()),
            };

            // act and assert
            try
            {
                // TODO: should throw a specific known exception for this scenario
                factory.CreateFromEvents(aggregateId, events);
                Assert.Fail("should not have been possible to create the aggregate using an unknown event");
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void WhenNewEvents_StateChanges()
        {
            // arrange
            var factory = AggregateFactory.For<Counter>();
            var aggregateId = "some id";
            var events = new IAggregateEvent[] {
                new AggregateEvent<CounterIncremented>(aggregateId, 1, DateTime.UtcNow, new CounterIncremented { Increment = 1 }),
                new AggregateEvent<CounterMultiplied>(aggregateId, 2, DateTime.UtcNow, new CounterMultiplied{ Factor = 2}),
            };
            var counter = factory.CreateFromEvents(aggregateId, events);

            // act
            counter.Increment(5);

            // assert
            Assert.AreEqual(2, counter.OriginalVersion);
            Assert.AreEqual(3, counter.State.Version);
            Assert.AreEqual(1, counter.Changes.Count());
            Assert.AreEqual(5, counter.Changes
                                      .OfType<IAggregateEvent<CounterIncremented>>()
                                      .First().Payload.Increment);
        }

        class EventThatNoAggregateKnows
        {

        }
    }
}
