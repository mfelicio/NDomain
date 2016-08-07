using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NDomain.CQRS.Projections;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Projections.Azure
{
    public class AzureBlobQueryStore<T> : IQueryStore<T>
    {
        readonly CloudStorageAccount account;
        readonly CloudBlobClient client;
        readonly string containerName;

        volatile bool created;

        public AzureBlobQueryStore(CloudStorageAccount account, string containerName)
        {
            this.account = account;
            this.client = account.CreateCloudBlobClient();
            this.containerName = containerName;

            this.created = false;
        }

        private async Task EnsureContainerExists(CloudBlobContainer container)
        {
            if (!this.created)
            {
                await container.CreateIfNotExistsAsync();
                this.created = true;
            }
        }

        public async Task<Query<T>> Get(string id)
        {
            var container = this.client.GetContainerReference(this.containerName);
            await EnsureContainerExists(container);

            var block = container.GetBlockBlobReference(id);

            if (!await block.ExistsAsync())
            {
                return new Query<T>
                {
                    Id = id,
                    Version = 0,
                    DateUtc = DateTime.UtcNow,
                    Data = default(T)
                };
            }

            using (var stream = new MemoryStream())
            {
                await block.DownloadToStreamAsync(stream);
                stream.Position = 0;

                using (var bsonReader = new BsonReader(stream))
                {
                    var obj = JObject.Load(bsonReader);
                    return obj.ToObject(typeof(Query<T>)) as Query<T>;
                }
            }
        }

        public async Task<Query<T>> GetOrWaitUntil(string id, int minExpectedVersion, TimeSpan timeout)
        {
            var query = await Get(id);

            if (query.Version >= minExpectedVersion)
            {
                return query;
            }

            var sw = Stopwatch.StartNew();
            do
            {
                await Task.Delay(50); //wait 50ms , should be exponential
                query = await Get(id);
            } while (query.Version < minExpectedVersion && sw.Elapsed < timeout);

            sw.Stop();

            return query;
        }

        public async Task Set(string id, Query<T> query)
        {
            var container = this.client.GetContainerReference(this.containerName);
            await EnsureContainerExists(container);

            var block = container.GetBlockBlobReference(id);

            using (var stream = new MemoryStream())
            {
                using (var bsonWriter = new BsonWriter(stream))
                {
                    bsonWriter.CloseOutput = false;
                    var serializer = new JsonSerializer();
                    serializer.Serialize(bsonWriter, query);
                    stream.Position = 0;
                }

                await block.UploadFromStreamAsync(stream);
            }
        }
    }
}
