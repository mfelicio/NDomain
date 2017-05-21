using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NDomain.Model;
using Newtonsoft.Json.Linq;

namespace NDomain.Azure.Model.EventSourcing
{
    public static class Extensions
    {
        public static async Task<IList<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query, CancellationToken ct = default(CancellationToken), Action<IList<DynamicTableEntity>> onProgress = null)
        {
            var items = new List<DynamicTableEntity>();
            TableContinuationToken token = null;

            do
            {

                TableQuerySegment<DynamicTableEntity> seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                if (onProgress != null) onProgress(items);

            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }

        public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query, CancellationToken ct = default(CancellationToken), Action<IList<T>> onProgress = null) where T : ITableEntity, new()
        {
            var items = new List<T>();
            TableContinuationToken token = null;

            do
            {

                TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);

                if (onProgress != null)
                {
                    onProgress(items);
                }

            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }

        public static IAggregateEvent<JObject> ToEvent(this DynamicTableEntity entity)
        {
            return new AggregateEvent<JObject>(
                        entity.PartitionKey,
                        int.Parse(entity.RowKey),
                        entity.Properties["DateUtc"].DateTime.Value,
                        entity.Properties["Name"].StringValue,
                        JObject.Parse(entity.Properties["Payload"].StringValue));
        }

        public static DynamicTableEntity ToEntity(this IAggregateEvent<JObject> ev)
        {
            var entity = new DynamicTableEntity(ev.AggregateId, ev.SequenceId.ToString());
            entity.Properties["DateUtc"] = new EntityProperty(ev.DateUtc);
            entity.Properties["Name"] = new EntityProperty(ev.Name);
            entity.Properties["Payload"] = new EntityProperty(ev.Payload.ToString());
            return entity;
        }
    }
}
