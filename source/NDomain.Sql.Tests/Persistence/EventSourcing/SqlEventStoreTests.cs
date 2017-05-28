using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using NDomain.Persistence.EventSourcing;
using NDomain.Sql.Model.EventSourcing;
using NDomain.Tests.Common.Specs;
using NUnit.Framework;

namespace NDomain.Sql.Tests.Persistence.EventSourcing
{
    [TestFixture]
    [Category("integration")]
    public class SqlEventStoreTests : EventStoreSpecs
    {
        private readonly string _connectionString;
        private readonly SqlEventStoreDb _store;
        private readonly SqlObjectNames _sqlNames;

        public SqlEventStoreTests()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["EventStore.Database"].ConnectionString;
            _sqlNames = new SqlObjectNames("atest", "Aggregates", "Events");
            _store = new SqlEventStoreDb(_connectionString, _sqlNames);
        }

        protected override IEventStoreDb CreateEventStorage()
        {
            return _store;
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

        protected override void OnTearDown()
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
    }
}
