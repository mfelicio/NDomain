using NDomain.EventSourcing;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.EventSourcing.Azure
{
    public class AzureEventStore : IEventStoreDb
    {
        const string SourceRootRowKey = "root";
        const string UncommittedPrefix = "uncommitted";

        readonly CloudStorageAccount account;
        readonly CloudTableClient client;
        readonly string tableName;

        volatile bool created;

        public AzureEventStore(CloudStorageAccount account, string tableName)
        {
            this.account = account;
            this.client = account.CreateCloudTableClient();
            this.tableName = tableName;

            this.created = false;
        }

        private async Task EnsureTableExists(CloudTable table)
        {
            if (!this.created)
            {
                await table.CreateIfNotExistsAsync();
                this.created = true;
            }
        }

        public async Task<IEnumerable<IAggregateEvent<JObject>>> Load(string eventStreamId)
        {
            var table = this.client.GetTableReference(this.tableName);
            await EnsureTableExists(table);

            var query = new TableQuery().Where(
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, eventStreamId),
                                TableOperators.And,
                                TableQuery.CombineFilters(
                                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, SourceRootRowKey),
                                    TableOperators.And,
                                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, UncommittedPrefix))));

            var entities = await table.ExecuteQueryAsync(query);

            return entities.Select(e => e.ToEvent()).ToArray();
        }

        public async Task<IEnumerable<IAggregateEvent<JObject>>> LoadRange(string eventStreamId, int start, int end)
        {
            var table = this.client.GetTableReference(this.tableName);
            await EnsureTableExists(table);

            var filters = Enumerable.Range(start, end - start + 1)
                                   .Select(rowKey => TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey.ToString()));

            var rowKeyFilter = filters.First();
            foreach (var filter in filters.Skip(1))
            {
                rowKeyFilter = TableQuery.CombineFilters(rowKeyFilter, TableOperators.Or, filter);
            }

            var query = new TableQuery().Where(
                                TableQuery.CombineFilters(
                                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, eventStreamId),
                                    TableOperators.And,
                                    rowKeyFilter));

            var entities = await table.ExecuteQueryAsync(query);

            return entities.Select(e => e.ToEvent()).ToArray();
        }

        public async Task<IEnumerable<IAggregateEvent<JObject>>> LoadUncommitted(string eventStreamId, string transactionId)
        {
            var table = this.client.GetTableReference(this.tableName);
            await EnsureTableExists(table);

            var uncommittedRowKey = string.Format("{0}:{1}", UncommittedPrefix, transactionId);

            var query = new TableQuery().Where(
                                TableQuery.CombineFilters(
                                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, eventStreamId),
                                    TableOperators.And,
                                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, uncommittedRowKey)));

            var uncommittedLog = (await table.ExecuteQueryAsync(query)).FirstOrDefault();
            if (uncommittedLog == null)
            {
                return Enumerable.Empty<IAggregateEvent<JObject>>();
            }

            var seqs = uncommittedLog.Properties["seqs"].StringValue.Split(',').Select(seq => int.Parse(seq));

            return await LoadRange(eventStreamId, seqs.First(), seqs.Last());
        }

        public async Task Append(string eventStreamId, string transactionId, int expectedVersion, IEnumerable<IAggregateEvent<JObject>> events)
        {
            var table = this.client.GetTableReference(this.tableName);
            await EnsureTableExists(table);

            // no need to validate expected version, because the batch transaction will fail if a duplicate exists
            var transaction = new TableBatchOperation();

            transaction.InsertOrReplace(CreateSourceRoot(eventStreamId, expectedVersion + events.Count()));

            foreach (var ev in events)
            {
                transaction.Insert(ev.ToEntity());
            }

            transaction.Insert(CreateUncommittedLog(eventStreamId, transactionId, events.Select(ev => ev.SequenceId)));

            try
            {
                var results = await table.ExecuteBatchAsync(transaction);
                return;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode != (int)HttpStatusCode.Conflict)
                {
                    throw; // different error, rethrow
                }
            }

            // conflict
            var sourceRootVersion = await GetSourceRootVersion(table, eventStreamId);
            throw new ConcurrencyException(eventStreamId, expectedVersion, sourceRootVersion);
        }

        public async Task Commit(string eventStreamId, string transactionId)
        {
            var table = this.client.GetTableReference(this.tableName);
            await EnsureTableExists(table);

            var uncommittedLogEntity = new TableEntity(eventStreamId, string.Format("{0}:{1}", UncommittedPrefix, transactionId));
            uncommittedLogEntity.ETag = "*";
            var deleteOperation = TableOperation.Delete(uncommittedLogEntity);
            
            await table.ExecuteAsync(deleteOperation);
        }

        private DynamicTableEntity CreateSourceRoot(string sourceId, int version)
        {
            var sourceRootEntity = new DynamicTableEntity(sourceId, SourceRootRowKey);
            sourceRootEntity.Properties["Version"] = new EntityProperty(version);
            return sourceRootEntity;
        }

        private DynamicTableEntity CreateUncommittedLog(string sourceId, string transactionId, IEnumerable<int> sequenceIds)
        {
            var uncommittedLogEntity = new DynamicTableEntity(sourceId, string.Format("{0}:{1}", UncommittedPrefix, transactionId));
            var seqs = string.Join(",", sequenceIds);
            uncommittedLogEntity.Properties["seqs"] = new EntityProperty(seqs);
            return uncommittedLogEntity;
        }

        private async Task<int> GetSourceRootVersion(CloudTable table, string sourceId)
        {
            var loadRootQuery = new TableQuery().Where(
                                    TableQuery.CombineFilters(
                                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, sourceId),
                                        TableOperators.And,
                                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, SourceRootRowKey)));

            var query = await table.ExecuteQueryAsync(loadRootQuery);
            var sourceRootEntity = query.First();

            return sourceRootEntity.Properties["Version"].Int32Value.Value;
        }

    }
}
