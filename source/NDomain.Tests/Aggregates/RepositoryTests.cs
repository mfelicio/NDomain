using Moq;
using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.Tests.Common.Sample;

namespace NDomain.Tests.Aggregates
{
    public class RepositoryTests
    {
        readonly IAggregateFactory<Counter> factory = AggregateFactory.For<Counter>();

        [Test]
        public async Task SavingNewAggregateWithoutChangesHasNoSideEffects()
        {
            // arrange
            var eventStore = CreateEventStoreMock();
            var repository = new EventSourcedRepository<Counter>(eventStore.Object);
            var aggregate = factory.CreateNew(Guid.NewGuid().ToString());

            // act
            await repository.Save(aggregate);

            // assert
            eventStore.Verify(e =>
                e.Append(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IEnumerable<IAggregateEvent>>()),
                Times.Never);
        }

        [Test]
        public async Task SavingExistingAggregateWithoutChangesHasNoSideEffects()
        {
            // arrange
            var eventStore = CreateEventStoreMock();
            var repository = new EventSourcedRepository<Counter>(eventStore.Object);
            var aggregate = factory.CreateNew(Guid.NewGuid().ToString());

            // act
            await repository.Save(aggregate);

            // assert
            eventStore.Verify(e =>
                e.Append(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IEnumerable<IAggregateEvent>>()),
                Times.Never);
        }

        [Test]
        public async Task CanSaveAndLoadAggregate()
        {
            // arrange
            var repository = CreateRaceRepository();
            var aggregateId = Guid.NewGuid().ToString();

            var aggregate = factory.CreateNew(aggregateId);
            aggregate.Increment();
            await repository.Save(aggregate);

            // act
            var loadedAggregate = await repository.Find(aggregateId);

            // assert
            Assert.AreEqual(1, loadedAggregate.OriginalVersion);
            Assert.AreEqual(0, loadedAggregate.Changes.Count());
        }

        private IAggregateRepository<Counter> CreateRaceRepository()
        {
            var bus = new Mock<IEventStoreBus>();

            bus.Setup(b => b.Publish(It.IsAny<IAggregateEvent<JObject>>()))
                .Returns(Task.FromResult(true));
            bus.Setup(b => b.Publish(It.IsAny<IEnumerable<IAggregateEvent<JObject>>>()))
                .Returns(Task.FromResult(true));

            var eventStore = new EventStore(
                                new LocalEventStore(),
                                bus.Object,
                                EventStoreSerializer.FromAggregateTypes(typeof(Counter)));

            var repository = new EventSourcedRepository<Counter>(eventStore);
            return repository;
        }

        private Mock<IEventStore> CreateEventStoreMock()
        {
            var mock = new Mock<IEventStore>();

            mock.Setup(e => e.Append(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<IEnumerable<IAggregateEvent>>()))
                .Returns(Task.FromResult(true));

            return mock;
        }
    }

}
