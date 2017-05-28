using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json.Linq;
using Dapper;
using NDomain.Model;
using NDomain.Persistence.EventSourcing;

namespace NDomain.Sql.Model.EventSourcing
{
    public class SqlEventStoreDb : IEventStoreDb
    {
        private readonly string _connectionString;
        private readonly SqlObjectNames _sqlNames;

        public SqlEventStoreDb(string connectionString, SqlObjectNames sqlNames)
        {
            this._connectionString = connectionString;
            this._sqlNames = sqlNames;
            SqlMapper.AddTypeMap(typeof(DateTime), System.Data.DbType.DateTime2);
        }

        public void Init()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = string.Format(
                    SqlStatements.TablesInitialization,
                    _sqlNames.SchemaName,
                    _sqlNames.AggregateTableName,
                    _sqlNames.EventTableName);

                connection.Open();

                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public async Task Append(string eventStreamId, string transactionId, int expectedVersion, IEnumerable<IAggregateEvent<JObject>> events)
        {
            if (!events.Any())
            {
                return;
            }

            string aggregateName = "undefined"; // TODO: find a way to pass the aggregate name here
            await Append(eventStreamId, transactionId, expectedVersion, events, aggregateName).ConfigureAwait(false);
        }

        private async Task Append(string eventStreamId, string transactionId, int expectedVersion, IEnumerable<IAggregateEvent<JObject>> events, string aggregateType)
        {
            if (events.Count() == 0)
            {
                return;
            }

            // TODO: current implementation uses several round trips to send data to sql. We
            // may want to re-implement this to do it with only one round trip 
            int newVersion = expectedVersion + events.Count();

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                //TODO: Change to await inside catch (unavailable as long as we don't have any vs2015 build agents - we cannot leverage C#6 in TeamCity)
                var sqlExceptionThrown = false;

                try
                {
                    using (var con = new SqlConnection(_connectionString))
                    {
                        await con.OpenAsync().ConfigureAwait(false);

                        if (expectedVersion == 0)
                        {
                            // create the aggregate, throws if someone else created it
                            await SQLCreateAggregateEntry(con, eventStreamId, aggregateType, 0, 0).ConfigureAwait(false);
                        }

                        await SQLAppendEvents(con, eventStreamId, transactionId, events).ConfigureAwait(false);
                        await SQLUpdateAggregateVersion(con, eventStreamId, expectedVersion, newVersion).ConfigureAwait(false);
                        transaction.Complete();
                    }
                }
                catch (SqlException e)
                {                    
                    if (e.Number == SqlErrors.DuplicateKeyError || e.Number == SqlErrors.UniqueConstraintError)
                    {
                        sqlExceptionThrown = true;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (sqlExceptionThrown)
                {
                    int curVersion = await SQLReadAggregateVersion(eventStreamId).ConfigureAwait(false);
                    throw new ConcurrencyException(eventStreamId, expectedVersion, curVersion);
                }
            }
            
        }

        private async Task SQLCreateAggregateEntry(SqlConnection con, string eventStreamId, string aggregateType, int aggregateEventSeq, int snapshotEventSeq)
        {
            var args = new { eventStreamId, aggregateType, aggregateEventSeq, snapshotEventSeq };
            var statement = string.Format(
                SqlStatements.InsertAggregate,
                _sqlNames.SchemaName,
                _sqlNames.AggregateTableName);
            await con.ExecuteAsync(statement, args).ConfigureAwait(false);
        }

        private async Task SQLAppendEvents(SqlConnection con, string eventStreamId, string transactionId, IEnumerable<IAggregateEvent<JObject>> events)
        {
            var sqlEvents = events.Select(e => new
            {
                aggregateId = eventStreamId,
                aggregateType = "undefined", // TODO: find a way to pass the aggregate name here
                eventSeq = e.SequenceId,
                timestamp = e.DateUtc,
                msgType = e.Name,
                msgVersion = 1,   // not supported versioning yet. just hardcode it here!
                msgData = e.Payload.ToString(),
                committed = 0,
                transactionId = transactionId
            });

            string statement = string.Format(
                SqlStatements.InsertEvents,
                _sqlNames.SchemaName,
                _sqlNames.EventTableName);

            await con.ExecuteAsync(statement, sqlEvents).ConfigureAwait(false);
        }

        private async Task SQLUpdateAggregateVersion(SqlConnection con, string eventStreamId, int expectedVersion, int newVersion)
        {
            string statement = string.Format(
                SqlStatements.UpdateAggregateVersion,
                _sqlNames.SchemaName,
                _sqlNames.AggregateTableName);

            var args = new {
                aggregateId = eventStreamId,
                expectedVersion = expectedVersion,
                newVersion = newVersion };

            int affectedRows = await con.ExecuteAsync(statement, args).ConfigureAwait(false);

            if (affectedRows == 0)
            {
                int currVersion = await SQLReadAggregateVersion(eventStreamId, con).ConfigureAwait(false);
                throw new ConcurrencyException(eventStreamId, expectedVersion, currVersion);
            }
        }

        private async Task<int> SQLReadAggregateVersion(string eventStreamId, SqlConnection con = null)
        {
            string statement = string.Format(
                SqlStatements.ReadAggregateVersion,
                _sqlNames.SchemaName,
                _sqlNames.AggregateTableName);

            bool reuseConnection = con != null;
            if (!reuseConnection)
            {
                con = new SqlConnection(_connectionString);
            }

            try
            {
                if (!reuseConnection)
                {
                    await con.OpenAsync().ConfigureAwait(false);
                }

                var args = new { aggregateId = eventStreamId };
                var res = await con.QueryAsync<int>(statement, args).ConfigureAwait(false);
                int version = res.Single();
                return version;
            }
            finally
            {
                if (!reuseConnection)
                {
                    con.Dispose();
                }
            }               
        }

        public async Task Commit(string eventStreamId, string transactionId)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync().ConfigureAwait(false);

                    string cmd = string.Format(
                        SqlStatements.CommitEvents,
                        _sqlNames.SchemaName,
                        _sqlNames.EventTableName);

                    await con.ExecuteAsync(cmd, new { aggregateId = eventStreamId, transactionId }).ConfigureAwait(false);
                    transaction.Complete();
                }
            }
        }

        public async Task<IEnumerable<IAggregateEvent<JObject>>> Load(string eventStreamId)
        {
            string query = string.Format(
                SqlStatements.GetEventsByStreamId,
                _sqlNames.SchemaName,
                _sqlNames.EventTableName);

            var args = new { aggregateId = eventStreamId };
            return await Load(query, args).ConfigureAwait(false);
        }

        public async Task<IEnumerable<IAggregateEvent<JObject>>> LoadRange(string eventStreamId, int start, int end)
        {
            string query = string.Format(
                SqlStatements.GetEventsInRangeQuery,
                _sqlNames.SchemaName,
                _sqlNames.EventTableName);

            var args = new { aggregateId = eventStreamId, start, end };
            return await Load(query, args).ConfigureAwait(false);
        }

        public async Task<IEnumerable<IAggregateEvent<JObject>>> LoadUncommitted(string eventStreamId, string transactionId)
        {
            string query = string.Format(
                SqlStatements.GetUncommittedEventsQuery,
                _sqlNames.SchemaName,
                _sqlNames.EventTableName);

            var args = new { aggregateId = eventStreamId, transactionId };
            return await Load(query, args).ConfigureAwait(false);
        }

        private async Task<IEnumerable<IAggregateEvent<JObject>>> Load(string query, object args)
        {
            IEnumerable<EventEntity> res = Enumerable.Empty<EventEntity>();

            using (var con = new SqlConnection(_connectionString))
            {
                res = await con.QueryAsync<EventEntity>(query, args).ConfigureAwait(false);
            }

            return res.Select(e => ToAggregateEvent(e)).ToList();
        }

        private IAggregateEvent<JObject> ToAggregateEvent(EventEntity entity)
        {
            return new AggregateEvent<JObject>(
                entity.aggregate_id,
                entity.event_seq,
                entity.timestamp,
                entity.msg_type,
                JObject.Parse(entity.msg_data));
        }

        private static class SqlStatements
        {            
            internal const string InsertAggregate = "insert into {0}.{1} values (@eventStreamId, @aggregateType, @aggregateEventSeq, @snapshotEventSeq)";
            internal const string InsertEvents =
            #region insert events statement
                @"insert into {0}.{1} values (
                    @aggregateId,
                    @aggregateType,
                    @eventSeq,
                    @timestamp,
                    @msgType,
                    @msgVersion,
                    @msgData,
                    @committed,
                    @transactionId)";
            #endregion
            internal const string GetEventsByStreamId = "select * from {0}.{1} where aggregate_id = @aggregateId order by event_seq";
            internal const string GetEventsInRangeQuery = "select * from {0}.{1} where aggregate_id = @aggregateId and event_seq >= @start and event_seq <= @end order by event_seq";
            internal const string GetUncommittedEventsQuery = "select * from {0}.{1} where aggregate_id = @aggregateId and transaction_id = @transactionId and committed = 0 order by event_seq";
            internal const string ReadAggregateVersion = "select [aggregate_event_seq] from {0}.{1} where [aggregate_id] = @aggregateId";
            internal const string UpdateAggregateVersion = @"update {0}.{1} set [aggregate_event_seq] = @newVersion where [aggregate_id] = @aggregateId and [aggregate_event_seq] = @expectedVersion";
            internal const string CommitEvents = "update {0}.{1} set committed = 1 where aggregate_Id = @aggregateId and transaction_id = @transactionId";           
            internal const string TablesInitialization =
            #region table initialization statement
                @"
			    if not exists(select * from information_schema.schemata where schema_name = '{0}')
			    begin
				    exec('create schema {0} authorization dbo');
			    end

                if not exists (select * from sys.objects where object_id = object_id(N'{0}.{1}')
                                                         and type in (N'U'))
                    begin
                        create table {0}.{1}(
                           [aggregate_id] [nvarchar](256) not null,
                           [aggregate_type] [nvarchar](256) not null,
                           [aggregate_event_seq] [int] not null,
                           [snapshot_event_seq] [int] not null default (0),
                           constraint [PK_Aggregates] primary key clustered
                                ([aggregate_id] asc)
                        )
                    end

                if not exists (select * from sys.objects where object_id = object_id(N'{0}.{2}')
                                                         and type in (N'U'))
                    begin
                        create table {0}.{2}(
                            [aggregate_id] [nvarchar](256) not null,
                            [aggregate_type] [nvarchar](256) not null,
                            [event_seq] [int] not null,
                            [timestamp] [datetime2] not null,
                            [msg_type] [nvarchar](256) not null,
                            [msg_ver] [smallint] not null,
                            [msg_data] [nvarchar](max) not null,
                            [committed] [bit] not null default 0,
                            [transaction_id] nvarchar(256) not null,
                            constraint [PK_Events] primary key clustered
                                ([aggregate_id] asc, [event_seq] asc)
                        )

                        create nonclustered index [idx_aggregateId_committed]
                            on {0}.{2}([aggregate_id] asc, [committed] desc )

                        create nonclustered index [idx_transationId]
                            on {0}.{2}([transaction_id] asc)
                    end
                ";
            #endregion            
        }
    }

    internal class SqlErrors
    {
        internal const int DuplicateKeyError = 2601;
        internal const int UniqueConstraintError = 2627;
    }
}
