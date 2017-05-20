using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.Model.EventSourcing;

namespace NDomain.Tests.Specs
{
    [TestFixture]
    public abstract class EventStoreSpecs
    {
        readonly IEventStoreSerializer serializer;
        readonly IEventStoreBus bus;

        protected IEventStore eventStore;
        protected IEventStoreDb eventStorage;

        public EventStoreSpecs()
        {
            this.serializer = new EventStoreSerializer(new[] { typeof(Event1), typeof(Event2) });
            this.bus = new Mock<IEventStoreBus>().Object;
        }

        protected abstract IEventStoreDb CreateEventStorage();

        protected virtual void OnSetUp() { }
        protected virtual void OnTearDown() { }

        [SetUp]
        public void Setup()
        {
            this.eventStorage = CreateEventStorage();
            this.eventStore = new EventStore(this.eventStorage, this.bus, this.serializer);

            this.OnSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            this.OnTearDown();
        }

        [Test]
        public async Task CanSaveAndReadPayloadsAsExpected()
        {
            var date = DateTime.UtcNow;

            var aggregateId = "aggregateId";

            var guid = Guid.NewGuid();

            await this.eventStore.Append(aggregateId, 0, new IAggregateEvent[] { 
                new AggregateEvent<Event1>(aggregateId, 1, date, new Event1 { Value = "vv" }),  
                new AggregateEvent<Event2>(aggregateId, 2, date, new Event2 { AnotherValue = guid })
            });

            var events = await this.eventStore.Load(aggregateId);
            Assert.AreEqual(2, events.Count());

            var e1 = events.First() as IAggregateEvent<Event1>;
            var e2 = events.Last() as IAggregateEvent<Event2>;

            Assert.AreEqual(aggregateId, e1.AggregateId);
            Assert.AreEqual(1, e1.SequenceId);
            Assert.AreEqual(typeof(Event1).Name, e1.Name);
            Assert.AreEqual(date, e1.DateUtc);
            Assert.AreEqual("vv", e1.Payload.Value);

            Assert.AreEqual(aggregateId, e2.AggregateId);
            Assert.AreEqual(2, e2.SequenceId);
            Assert.AreEqual(typeof(Event2).Name, e2.Name);
            Assert.AreEqual(date, e2.DateUtc);
            Assert.AreEqual(guid, e2.Payload.AnotherValue);
        }

        [Test]
        public async Task CanDoBasicBehavior()
        {
            var aggregateId = "aggregateId";

            var events = await this.eventStore.Load(aggregateId);
            Assert.IsEmpty(events);

            await this.eventStore.Append(aggregateId, 0, new IAggregateEvent[] { 
                new AggregateEvent<Event1>(aggregateId, 1, DateTime.UtcNow, new Event1 { Value = "vv" }),  
                new AggregateEvent<Event2>(aggregateId, 2, DateTime.UtcNow, new Event2 { AnotherValue = Guid.NewGuid() })
            });

            events = await this.eventStore.Load(aggregateId);
            Assert.AreEqual(2, events.Count());

            await this.eventStore.Append(aggregateId, 2, new IAggregateEvent[] { new AggregateEvent<Event1>(aggregateId, 3, DateTime.UtcNow, new Event1 { Value = "vvvv" }) });

            events = await this.eventStore.LoadRange(aggregateId, 2, 3);
            Assert.AreEqual(2, events.Count());
            Assert.AreEqual(2, events.First().SequenceId);
            Assert.AreEqual(3, events.Last().SequenceId);

            events = await this.eventStore.LoadRange(aggregateId, 3, 3);
            Assert.AreEqual(1, events.Count());
            Assert.AreEqual(3, events.Single().SequenceId);

            await this.eventStore.Append(aggregateId, 3, new IAggregateEvent[] { new AggregateEvent<Event1>(aggregateId, 4, DateTime.UtcNow, new Event1 { Value = "vvvv" }) });

            // simulate conflict
            try
            {
                await this.eventStore.Append(aggregateId, 2, new IAggregateEvent[] { new AggregateEvent<Event2>(aggregateId, 3, DateTime.UtcNow, new Event2 { AnotherValue = Guid.NewGuid() }) });
                Assert.Fail("Should have thrown ConcurrencyException");
            }
            catch (ConcurrencyException ex)
            {
                Assert.AreEqual(4, ex.CurrentVersion);
            }
        }

        [Test]
        public async Task CanCommitEvents()
        {
            var eventStreamId = Guid.NewGuid().ToString();
            var tr1 = Guid.NewGuid().ToString();

            await this.eventStorage.Append(eventStreamId, tr1, 0, new IAggregateEvent<JObject>[] { 
                new AggregateEvent<JObject>(eventStreamId, 1, DateTime.UtcNow, JObject.FromObject(new Event1 { Value = "vv" })),  
                new AggregateEvent<JObject>(eventStreamId, 2, DateTime.UtcNow, JObject.FromObject(new Event2 { AnotherValue = Guid.NewGuid() }))
            });

            var uncommittedLog = await this.eventStorage.LoadUncommitted(eventStreamId, tr1);
            Assert.AreEqual(2, uncommittedLog.Count());

            var tr2 = Guid.NewGuid().ToString();

            await this.eventStorage.Append(eventStreamId, tr2, 2, new IAggregateEvent<JObject>[] { 
                new AggregateEvent<JObject>(eventStreamId, 3, DateTime.UtcNow, JObject.FromObject(new Event1 { Value = "vv" })),  
                new AggregateEvent<JObject>(eventStreamId, 4, DateTime.UtcNow, JObject.FromObject(new Event2 { AnotherValue = Guid.NewGuid() })),
                new AggregateEvent<JObject>(eventStreamId, 5, DateTime.UtcNow, JObject.FromObject(new Event2 { AnotherValue = Guid.NewGuid() }))
            });

            uncommittedLog = await this.eventStorage.LoadUncommitted(eventStreamId, tr2);
            Assert.AreEqual(3, uncommittedLog.Count());

            await this.eventStorage.Commit(eventStreamId, tr2);

            uncommittedLog = await this.eventStorage.LoadUncommitted(eventStreamId, tr2);
            Assert.AreEqual(0, uncommittedLog.Count());

            uncommittedLog = await this.eventStorage.LoadUncommitted(eventStreamId, tr1);
            Assert.AreEqual(2, uncommittedLog.Count());

            await this.eventStorage.Commit(eventStreamId, tr1);

            uncommittedLog = await this.eventStorage.LoadUncommitted(eventStreamId, tr1);
            Assert.AreEqual(0, uncommittedLog.Count());

            var eventStream = await this.eventStorage.Load(eventStreamId);
            Assert.AreEqual(5, eventStream.Count());
        }
    }

    public class Event1
    {
        public string Value { get; set; }
    }

    public class Event2
    {
        public Guid AnotherValue { get; set; }
    }
}
