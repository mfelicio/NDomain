using NDomain.Tests.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.Model.EventSourcing;
using NUnit.Framework;
using System.Data.SqlClient;
using NDomain.Sql.Model.EventSourcing;
using System.Configuration;
using Dapper;

namespace NDomain.Sql.Tests.Model.EventSourcing
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
