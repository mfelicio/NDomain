using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NDomain.Sql.EventSourcing;
using NUnit.Framework;
using Dapper;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture;
using System.Threading;
using FluentAssertions;

namespace NDomain.Sql.Tests
{
    [TestFixture]
    [Category("integration")]
    public class SqlEventStoreDbTests
    {
        private readonly string _connectionString;
        private readonly SqlEventStoreDb _store;
        private readonly Fixture _fixture;
        private readonly TestDataBuilder _testData;
        private readonly SqlObjectNames _sqlNames;

        public SqlEventStoreDbTests()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["EventStore.Database"].ConnectionString;
            _sqlNames = new SqlObjectNames("atest", "Aggregates", "Events");
            _store = new SqlEventStoreDb(_connectionString, _sqlNames);
            _fixture = new Fixture();
            _testData = new TestDataBuilder();
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {            
            _store.Init();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            string oneTimeTearDownString = @"
                drop table {0}.{1}
                drop table {0}.{2}
                drop schema {0}";

            string cmd = string.Format(
                oneTimeTearDownString,
                _sqlNames.SchemaName,
                _sqlNames.AggregateTableName,
                _sqlNames.EventTableName);

            using (var con = new SqlConnection(_connectionString))
            {
                con.Execute(cmd);
            }
        }

        [TearDown]
        public void TearDown()
        {
            string tearDownString = @"
                DELETE FROM {0}.{1}
                DELETE FROM {0}.{2}";

            string cmd = string.Format(
                tearDownString,
                _sqlNames.SchemaName,
                _sqlNames.AggregateTableName,
                _sqlNames.EventTableName);

            using (var con = new SqlConnection(_connectionString))
            {
                con.Execute(cmd);
            }
        }

        [Test]
        public void Init_Shoud_BeIdempotent()
        {
            // it is assumed here that the init is always called in Setup()

            _store.Init();

            string findTableQuery =
                @"select count(*) from sys.objects 
                  where object_id = object_id(N'{0}.{1}') 
                        and type in (N'U')";

            string findSchemaQuery = @"select count(*) from information_schema.schemata where schema_name  = '{0}'";

            int schemaRes;
            int aggregateTableRes;
            int eventTableRes;

            using (var con = new SqlConnection(_connectionString))
            {
                var schemaQuery = string.Format(findSchemaQuery, _sqlNames.SchemaName);
                schemaRes = con.Query<int>(schemaQuery).Single();

                var aggregateTableQuery = string.Format(findTableQuery, _sqlNames.SchemaName, _sqlNames.AggregateTableName);
                aggregateTableRes = con.Query<int>(aggregateTableQuery).Single();

                var eventTableQuery = string.Format(findTableQuery, _sqlNames.SchemaName, _sqlNames.EventTableName);
                eventTableRes = con.Query<int>(eventTableQuery).Single();
            }

            Assert.AreEqual(1, schemaRes);
            Assert.AreEqual(1, aggregateTableRes);
            Assert.AreEqual(1, eventTableRes);
        }
        
        [Test]
        public async Task Append_Should_CreateAggregateEntryAndEvents_When_CalledForTheFirstTimeForAggregate()
        {
            int numEvents = 5;
            string aggregateId = _fixture.Create<string>();
            int fromVersion = 1;
            string transactionId = _fixture.Create<string>();

            var events = _testData.CreateEvents<TestAggregateRoot>(numEvents, aggregateId, fromVersion).ToArray();

            await _store.Append(aggregateId, transactionId, 0, events);

            var aggregate = await GetAggregate(aggregateId);
            var eRes = await GetEvents(aggregateId);
            EventEntity[] eventRes = eRes.ToArray();

            Assert.AreEqual(5, aggregate.aggregate_event_seq);
            Assert.IsNotNull(eventRes);
            Assert.IsNotEmpty(eventRes);
            Assert.AreEqual(numEvents, eventRes.Count());

            for (int i = 0; i < eventRes.Length; ++i)
            {
                var originalEvent = events[i];
                var returnedEvent = eventRes[i];

                Assert.AreEqual(aggregateId, returnedEvent.aggregate_id);
                Assert.AreEqual(i + 1, returnedEvent.event_seq);
                Assert.AreEqual(originalEvent.DateUtc, returnedEvent.timestamp);

                var returnedPayload = JObject.Parse(returnedEvent.msg_data);
                Assert.IsTrue(JObject.EqualityComparer.Equals(originalEvent.Payload, returnedPayload));
            }
        }

        [Test]
        public async Task Append_Should_UpdateAggregateVersion()
        {
            string aggregateId = _fixture.Create<string>();
            var batch1 = _testData.CreateEvents<TestAggregateRoot>(10, aggregateId, 1);

            await _store.Append(aggregateId, _fixture.Create<string>(), 0, batch1);
            var aggregateV1 = await GetAggregate(aggregateId);

            Assert.AreEqual(10, aggregateV1.aggregate_event_seq);

            var batch2 = _testData.CreateEvents<TestAggregateRoot>(7, aggregateId, 11);
            await _store.Append(aggregateId, _fixture.Create<string>(), 10, batch2);

            var aggregateV2 = await GetAggregate(aggregateId);
            Assert.AreEqual(17, aggregateV2.aggregate_event_seq);
        }

        [Test]
        public async Task Append_Should_ThrowException_When_ExpectedVersionChangedFromTheOneExpected()
        {
            string aggregateId = _fixture.Create<string>();
            var batch1 = _testData.CreateEvents<TestAggregateRoot>(10, aggregateId, 1);

            await _store.Append(aggregateId, _fixture.Create<string>(), 0, batch1);
                       
            var batch2 = _testData.CreateEvents<TestAggregateRoot>(7, aggregateId, 1);

            Assert.ThrowsAsync(typeof(ConcurrencyException),
                () => _store.Append(aggregateId, _fixture.Create<string>(), 0, batch2));
        }

        [Test]
        public async Task Append_Should_ThrowException_When_ExpectedVersionAfterTheFirstAppendChanged()
        {
            string aggregateId = _fixture.Create<string>();
            var batch1 = _testData.CreateEvents<TestAggregateRoot>(10, aggregateId, 1);

            await _store.Append(aggregateId, _fixture.Create<string>(), 0, batch1);

            var batch2 = _testData.CreateEvents<TestAggregateRoot>(7, aggregateId, 11);
            await _store.Append(aggregateId, _fixture.Create<string>(), 10, batch2);

            var batch3 = _testData.CreateEvents<TestAggregateRoot>(4, aggregateId, 11);

            Assert.ThrowsAsync(typeof(ConcurrencyException),
                () => _store.Append(aggregateId, _fixture.Create<string>(), 10, batch3));
        }

        [Test]
        public async Task Append_Should_DoNothing_When_CalledWithNoEventsForNonExistingAggregate()
        {
            string aggregateId = _fixture.Create<string>();
            var events = _testData.CreateEvents<TestAggregateRoot>(0, aggregateId, 0);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, events);

            var aggregate = await GetAggregate(aggregateId);
            Assert.IsNull(aggregate);

            var eventsRes = await GetEvents(aggregateId);
            Assert.IsEmpty(eventsRes);
        }

        [Test]
        public async Task Append_Should_DoNothing_When_CalledWithNoEventsForAlreadyExistingAggregate()
        {
            string aggregateId = _fixture.Create<string>();
            var events = _testData.CreateEvents<TestAggregateRoot>(1, aggregateId, 1);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, events);

            var emptyEvents = _testData.CreateEvents<TestAggregateRoot>(0, aggregateId, 0);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, emptyEvents);

            var aggregate = await GetAggregate(aggregateId);
            Assert.AreEqual(1, aggregate.aggregate_event_seq);

            var evtRes = await GetEvents(aggregateId);
            Assert.AreEqual(1, evtRes.Count());
        }

        [Test]
        public async Task Append_Should_Succeed_When_EmptyAggregateExistsAndThenSomeEventsAreAppended()
        {
            string aggregateId = _fixture.Create<string>();
            var emptyEvents = _testData.CreateEvents<TestAggregateRoot>(0, aggregateId, 0);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, emptyEvents);

            var someEvents = _testData.CreateEvents<TestAggregateRoot>(3, aggregateId, 1);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, someEvents);

            var aggregate = await GetAggregate(aggregateId);
            Assert.IsNotNull(aggregate);
            Assert.AreEqual(3, aggregate.aggregate_event_seq);

            var eventsRes = await GetEvents(aggregateId);
            Assert.AreEqual(3, eventsRes.Count());
        }

        [Test]
        public async Task Commit_Shoud_MarkRespectiveEventsAsCommitted()
        {
            string aggregateId = _fixture.Create<string>();
            var batch1 = _testData.CreateEvents<TestAggregateRoot>(7, aggregateId, 1);
            var batch2 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 8);

            string transactionId1 = Guid.NewGuid().ToString();
            string transactionId2 = Guid.NewGuid().ToString();

            await _store.Append(aggregateId, transactionId1, 0, batch1);
            await _store.Append(aggregateId, transactionId2, 7, batch2);
            await _store.Commit(aggregateId, transactionId1);

            var events = await GetEvents(aggregateId);

            var eventsBatch1 = events.Where(e => e.transaction_id == transactionId1);
            var eventsBatch2 = events.Where(e => e.transaction_id == transactionId2);

            Assert.AreEqual(7, eventsBatch1.Count());
            Assert.AreEqual(5, eventsBatch2.Count());

            Assert.IsTrue(eventsBatch1.All(e => e.committed));
            Assert.IsTrue(eventsBatch2.All(e => !e.committed));
        }

        [Test]
        public async Task Commit_Should_DoNothing_When_WrongTransactionDoesNotExist()
        {
            string aggregateId = _fixture.Create<string>();
            await _store.Commit(aggregateId, _fixture.Create<string>());

            var someEvents = _testData.CreateEvents<TestAggregateRoot>(2, aggregateId, 1);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, someEvents);
            await _store.Commit(aggregateId, _fixture.Create<string>());

            var evtRes = await GetEvents(aggregateId);
            Assert.AreEqual(2, evtRes.Count());

            foreach (var evt in evtRes)
            {
                Assert.IsFalse(evt.committed);
            }
        }

        [Test]
        public async Task Load_Should_ReturnAllAggregateEvents()
        {
            var aggregateId = _fixture.Create<string>();

            var batch1 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 1);
            var batch2 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 6);
            var batch3 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 11);

            var transaction1 = _fixture.Create<string>();

            await _store.Append(aggregateId, transaction1, 0, batch1);
            await _store.Append(aggregateId, _fixture.Create<string>(), 5, batch2);
            await _store.Append(aggregateId, _fixture.Create<string>(), 10, batch3);
            await _store.Commit(aggregateId, transaction1);

            var events = await _store.Load(aggregateId);
            var evtArray = events.ToArray();

            Assert.AreEqual(15, events.Count());

            for (int i = 0; i < evtArray.Length; ++i)
            {
                var evt = evtArray[i];
                Assert.AreEqual(aggregateId, evt.AggregateId);
                Assert.AreEqual(i + 1, evt.SequenceId);                
            }
        }

        [Test]
        public async Task LoadRange_Should_ReturnAllAggregateEventsContainedInRange()
        {
            var aggregateId = _fixture.Create<string>();

            var batch1 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 1);
            var batch2 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 6);
            var batch3 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 11);

            var transaction1 = _fixture.Create<string>();
            var transaction2 = _fixture.Create<string>();

            await _store.Append(aggregateId, transaction1, 0, batch1);
            await _store.Append(aggregateId, transaction2, 5, batch2);
            await _store.Append(aggregateId, _fixture.Create<string>(), 10, batch3);
            await _store.Commit(aggregateId, transaction1);
            await _store.Commit(aggregateId, transaction2);

            var events = await _store.LoadRange(aggregateId, 8, 15);
            var evtArray = events.ToArray();

            Assert.AreEqual(8, events.Count());

            for (int i = 0; i < evtArray.Length; ++i)
            {
                var evt = evtArray[i];
                Assert.AreEqual(aggregateId, evt.AggregateId);
                Assert.AreEqual(i + 8, evt.SequenceId);
            }
        }

        [Test]
        public async Task LoadRange_Should_ReturnEmpty_When_NoEventsAreInRange()
        {
            var aggregateId = _fixture.Create<string>();
            var batch1 = _testData.CreateEvents<TestAggregateRoot>(10, aggregateId, 1);
            var transaction1 = _fixture.Create<string>();

            await _store.Append(aggregateId, transaction1, 0, batch1);
            await _store.Commit(aggregateId, transaction1);

            var events = await _store.LoadRange(aggregateId, 11, 15);
            Assert.AreEqual(0, events.Count());
        }

        [Test]
        public async Task LoadUncommitted_Should_ReturnOnlyEventsRegardingProvidedTransaction()
        {
            var aggregateId = _fixture.Create<string>();

            var batch1 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 1);
            var batch2 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 6);
            var batch3 = _testData.CreateEvents<TestAggregateRoot>(5, aggregateId, 11);

            var transaction1 = _fixture.Create<string>();
            var transaction2 = _fixture.Create<string>();
            var transaction3 = _fixture.Create<string>();

            await _store.Append(aggregateId, transaction1, 0, batch1);
            await _store.Append(aggregateId, transaction2, 5, batch2);
            await _store.Append(aggregateId, transaction3, 10, batch3);
            await _store.Commit(aggregateId, transaction1);

            var trans2Events = await _store.LoadUncommitted(aggregateId, transaction2);
            var trans3Events = await _store.LoadUncommitted(aggregateId, transaction3);

            var trans2EvtArray = trans2Events.ToArray();
            var trans3EvtArray = trans3Events.ToArray();

            Assert.AreEqual(5, trans2Events.Count());
            Assert.AreEqual(5, trans3Events.Count());

            for (int i = 0; i < trans2EvtArray.Length; ++i)
            {
                var evt = trans2EvtArray[i];
                Assert.AreEqual(aggregateId, evt.AggregateId);
                Assert.AreEqual(i + 6, evt.SequenceId);
            }

            for (int i = 0; i < trans3EvtArray.Length; ++i)
            {
                var evt = trans3EvtArray[i];
                Assert.AreEqual(aggregateId, evt.AggregateId);
                Assert.AreEqual(i + 11, evt.SequenceId);
            }
        }

        [Test]
        public async Task LoadUncommitted_Should_ReturnEmpty_When_TheTransactionDoesNotExists()
        {
            string aggregateId = _fixture.Create<string>();

            var res1 = await _store.LoadUncommitted(aggregateId, _fixture.Create<string>());
            Assert.AreEqual(0, res1.Count());

            var events = _testData.CreateEvents<TestAggregateRoot>(10, aggregateId, 1);
            await _store.Append(aggregateId, _fixture.Create<string>(), 0, events);

            var res2 = await _store.LoadUncommitted(aggregateId, _fixture.Create<string>());
            Assert.AreEqual(0, res2.Count());
        }

        [Test]
        public async Task LoadUncommitted_Should_ReturnEmpty_When_TheEventsAreAlreadyCommitted()
        {
            string aggregateId = _fixture.Create<string>();
            string transactionId = _fixture.Create<string>();

            var events = _testData.CreateEvents<TestAggregateRoot>(10, aggregateId, 1);
            await _store.Append(aggregateId, transactionId, 0, events);
            await _store.Commit(aggregateId, transactionId);

            var res = await _store.LoadUncommitted(aggregateId, transactionId);
            Assert.AreEqual(0, res.Count());
        }

        [Test]
        [Explicit]
        public async Task Concurrent_Appends_In_Same_Aggregate_Should_Succeed_And_Detect_Concurrency_Conflicts()
        {
            int numTasks = 10;
            int executionDurationInSeconds = 60;

            ConcurrentTestContext testContext = new ConcurrentTestContext(_testData, _store);

            var tasks = Enumerable.Range(0, numTasks)
                .Select(i => testContext.Run())
                .ToArray();

            await Task.Delay(executionDurationInSeconds * 1000);

            testContext.Stop();

            Task.WaitAll(tasks);

            var aggregate = await GetAggregate(testContext.AggregateId);
            Assert.AreEqual(testContext.AggregateVersion, aggregate.aggregate_event_seq);

            var createdEvents = await GetEvents(testContext.AggregateId);
            Assert.AreEqual(
                testContext.AggregateVersion,
                createdEvents.Count());

            string message =
                @"Aggregate Version: {0}
                  Succeeded Appends: {1}
                  Concurrency Errors Detected: {2}";

            Assert.Pass(message,
                testContext.AggregateVersion,
                testContext.SucceededApends,
                testContext.ConcurrentErrorsDetected);
        }

        [Test]
        [Explicit]
        public async Task Concurrent_Appends_In_Different_Aggregates_Should_Succeed_Without_Concurrency_Conflicts()
        {
            const int numTasks = 10;
            int executionDurationInSeconds = 60;

            List<ConcurrentTestContext> testContexts = new List<ConcurrentTestContext>();

            var tasks = Enumerable.Range(0, numTasks)
                .Select(i =>
                {
                    ConcurrentTestContext testContext =
                        new ConcurrentTestContext(_testData, _store);

                    testContexts.Add(testContext);

                    return testContext.Run();
                }).ToArray();

            await Task.Delay(executionDurationInSeconds * 1000);

            foreach (var tctx in testContexts)
            {
                tctx.Stop();
            }

            Task.WaitAll(tasks);

            foreach (var tctx in testContexts)
            {
                Assert.AreEqual(0, tctx.ConcurrentErrorsDetected);

                var aggregate = await GetAggregate(tctx.AggregateId);
                Assert.AreEqual(tctx.AggregateVersion, aggregate.aggregate_event_seq);

                var createdEvents = await GetEvents(tctx.AggregateId);
                Assert.AreEqual(
                    tctx.AggregateVersion,
                    createdEvents.Count());               
            }

            string message =
                  @"Num of concurrent aggregates: {0}
                    Total Succeeded Appends: {1}";

            Assert.Pass(message,
                testContexts.Count,
                testContexts.Select(c => c.SucceededApends).Sum());

        }

        private async Task CreateAggregateWithEvents<TAggregateRoot>(string aggregateId)
        {
            var evt = _testData
                .CreateEvents<TAggregateRoot>(1, aggregateId, 1)
                .Single();
            var transactionId = _fixture.Create<Guid>().ToString();

            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync().ConfigureAwait(false);

                var insertAggregateWithEvent = string.Format(
                    @"INSERT INTO {0}.{1}([aggregate_id], [aggregate_type], [aggregate_event_seq])
                    VALUES (@aggregate_id, @aggregate_type, 1);

                    INSERT INTO {0}.{2}([aggregate_id], [aggregate_type], [event_seq], [timestamp], [msg_type], [msg_ver], [msg_data], [committed], [transaction_id])
                    VALUES (@aggregate_id, @aggregate_type, 1, @timestamp, @msg_type, 1, @msg_data, 1, @transaction_id)",
                    _sqlNames.SchemaName,
                    _sqlNames.AggregateTableName,
                    _sqlNames.EventTableName);

                var numberOfInsertedRows = await con
                    .ExecuteAsync(
                        insertAggregateWithEvent,
                        new
                        {
                            aggregate_id = aggregateId,
                            aggregate_type = typeof(TAggregateRoot).Name,
                            timestamp = evt.DateUtc,
                            msg_type = evt.Payload.Type,
                            msg_data = evt.Payload.ToString(),
                            transaction_id = transactionId
                        })
                    .ConfigureAwait(false);

                if(numberOfInsertedRows != 2)
                    Assert.Inconclusive("Failed to arrange the test");
            }
        }

        private async Task<AggregateEntity> GetAggregate(string aggregateId)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync().ConfigureAwait(false);

                string aggregateQuery = string.Format(
                    @"select * from {0}.{1} where aggregate_id = @aggregateId",
                    _sqlNames.SchemaName,
                    _sqlNames.AggregateTableName);

                var aggRes = await con
                    .QueryAsync<AggregateEntity>(aggregateQuery, new { aggregateId })
                    .ConfigureAwait(false);

                return aggRes.SingleOrDefault();
            }
        }

        private async Task<IEnumerable<EventEntity>> GetEvents(string aggregateId)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync().ConfigureAwait(false);

                string eventQuery = string.Format(
                    @"select * from {0}.{1} where aggregate_id = @aggregateId",
                    _sqlNames.SchemaName,
                    _sqlNames.EventTableName);

                var eventRes = await con
                    .QueryAsync<EventEntity>(eventQuery, new { aggregateId })
                    .ConfigureAwait(false);

                return eventRes.ToList();
            }
        }
    }

    class TestDataBuilder
    {
        private readonly Fixture _fixture = new Fixture();

        public IEnumerable<IAggregateEvent<JObject>> CreateEvents<TAggregateRoot>(
            int numEvents,
            string aggregateId,
            int fromVersion)
        {
            DateTime nowUtc = DateTime.UtcNow;
            // ensure datetime2 precision
            DateTime referenceDate = new DateTime(
                nowUtc.Year,
                nowUtc.Month,
                nowUtc.Day,
                nowUtc.Hour,
                nowUtc.Minute,
                nowUtc.Second,
                nowUtc.Millisecond);

            var builder = _fixture.Build<TestAggregateEvent>()
                .With(e => e.AggregateId, aggregateId)
                .With(e => e.DateUtc, referenceDate)
                .With(e => e.AggregateName, typeof(TAggregateRoot).Name);

            return Enumerable.Range(0, numEvents)
                .Select(x =>
                    {
                        var obj = builder.Create();
                        obj.SequenceId = x + fromVersion;
                        return obj;
                    }
                )
                .OfType<IAggregateEvent<JObject>>()
                .ToList();
        }
    }

    class TestAggregateEvent : IAggregateEvent<JObject>
    {
        public string AggregateId
        {
            get; set;
        }

        public string AggregateName
        {
            get; set;
        }

        public DateTime DateUtc
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public JObject Payload
        {
            get; set;
        }

        public int SequenceId
        {
            get; set;
        }

        object IAggregateEvent.Payload
        {
            get
            {
                return Payload;
            }
        }
    }

    class TestAggregateState : IState
    {
        public int Version { get; set; }
        public void Mutate(IAggregateEvent @event)
        {
            // do some thangs to it self!!!
        }
    }

    class TestAggregateRoot : Aggregate<TestAggregateState>
    {
        public TestAggregateRoot(string id, TestAggregateState state) 
            : base(id, state)
        {
        }
    }

    class TestAggregateState2 : IState
    {
        public int Version { get; set; }
        public void Mutate(IAggregateEvent @event)
        {
            // do some thangs to it self!!!
        }
    }

    class TestAggregateRoot2 : Aggregate<TestAggregateState>
    {
        public TestAggregateRoot2(string id, TestAggregateState state)
            : base(id, state)
        {
        }
    }

    class ConcurrentTestContext
    {
        public readonly string AggregateId;
        public readonly int NumEventsPerBucket = 5;

        private TestDataBuilder _testData;
        private Fixture _fixture;
        private SqlEventStoreDb _store;

        private int _aggregateVersion = 0;
        private int _concurrencyErrorsDetected = 0;
        private int _succeededApends = 0;
        private bool _isStoped = false;

        public int AggregateVersion { get { return _aggregateVersion; } }
        public int ConcurrentErrorsDetected { get { return _concurrencyErrorsDetected; } }
        public int SucceededApends { get { return _succeededApends; } }

        public ConcurrentTestContext(TestDataBuilder testData, SqlEventStoreDb store)
        {
            _fixture = new Fixture();
            this.AggregateId = _fixture.Create<string>();
            _testData = testData;
            _store = store;
        }

        public async Task Run()
        {
            while (!Volatile.Read(ref _isStoped))
            {
                var currVersion = Volatile.Read(ref _aggregateVersion);

                var events = _testData.CreateEvents<TestAggregateRoot>(
                    NumEventsPerBucket,
                    AggregateId,
                    currVersion + NumEventsPerBucket + 1);

                try
                {
                    var transactionId = _fixture.Create<string>();
                    await _store.Append(AggregateId, transactionId, currVersion, events);
                    await _store.Commit(AggregateId, transactionId);
                    Interlocked.Add(ref _aggregateVersion, NumEventsPerBucket);
                    Interlocked.Increment(ref _succeededApends);
                }
                catch (ConcurrencyException)
                {                    
                    Interlocked.Increment(ref _concurrencyErrorsDetected);
                }
            }
        }

        public void Stop()
        {
            Volatile.Write(ref _isStoped, true);
        }
    }
}
