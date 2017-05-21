using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace NDomain.Bus.Transport.Azure.Queues
{
    public class QueueTransport : IInboundTransport, IOutboundTransport
    {
        readonly CloudStorageAccount account;
        readonly CloudQueueClient client;

        readonly ConcurrentDictionary<string, Lazy<Task<CloudQueue>>> queues;

        readonly string queueNameFormat;
        readonly Lazy<Task<CloudQueue>> inputQueue;

        public QueueTransport(CloudStorageAccount account, string prefix, string inputQueueName)
        {
            this.account = account;
            this.client = account.CreateCloudQueueClient();

            this.queues = new ConcurrentDictionary<string, Lazy<Task<CloudQueue>>>();

            if (prefix == null)
            {
                this.queueNameFormat = "{0}";
            }
            else
            {
                this.queueNameFormat = string.Format("{0}-{{0}}", prefix);
            }

            this.inputQueue = new Lazy<Task<CloudQueue>>(() => GetOrCreateQueue(inputQueueName));
        }

        private string GetQueueName(string endpoint)
        {
            return string.Format(this.queueNameFormat, endpoint);
        }

        private Task<CloudQueue> GetQueue(string endpoint)
        {
            return this.queues.GetOrAdd(endpoint, new Lazy<Task<CloudQueue>>(() => GetOrCreateQueue(endpoint)))
                              .Value;
        }

        private async Task<CloudQueue> GetOrCreateQueue(string endpoint)
        {
            var queueName = GetQueueName(endpoint);
            var queue = this.client.GetQueueReference(queueName);
            queue.EncodeMessage = true; // used with byte[] contents
            try
            {
                await queue.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {

            }
            return queue;
        }

        public async Task Send(TransportMessage message)
        {
            var endpoint = message.Headers[MessageHeaders.Endpoint];
            var queue = await GetQueue(endpoint);

            var queueMsg = BuildCloudQueueMessage(message);
            await queue.AddMessageAsync(queueMsg);
        }

        public async Task SendMultiple(IEnumerable<TransportMessage> messages)
        {
            foreach (var message in messages)
            {
                var endpoint = message.Headers[MessageHeaders.Endpoint];
                var queue = await GetQueue(endpoint);

                var queueMsg = BuildCloudQueueMessage(message);
                await queue.AddMessageAsync(queueMsg);
            }
        }

        public async Task<IMessageTransaction> Receive(TimeSpan? timeout = null)
        {
            var inputQueue = await this.inputQueue.Value;

            var queueMessage = await this.GetMessageWithTimeout(inputQueue, timeout ?? TimeSpan.FromSeconds(60));

            if (queueMessage == null)
            {
                return null;
            }

            var message = BuildMessage(queueMessage);
            
            var transaction = new QueueMessageTransaction(
                                    message,
                                    queueMessage.DequeueCount,
                                    () => inputQueue.DeleteMessageAsync(queueMessage),
                                    () => inputQueue.UpdateMessageAsync(queueMessage, TimeSpan.Zero, MessageUpdateFields.Visibility));
            return transaction;
        }

        private async Task<CloudQueueMessage> GetMessageWithTimeout(CloudQueue queue, TimeSpan timeout)
        {
            var queueMessage = await queue.GetMessageAsync();
            if (queueMessage != null)
            {
                return queueMessage;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var backOffs = new int[] { 1, 1 }//, 3, 5, 10, 10, 30 }
                               .Select(s => TimeSpan.FromSeconds(s)).ToArray();

            int backOffIdx = 0;
            while (queueMessage == null && sw.Elapsed < timeout)
            {
                var delay = backOffIdx < backOffs.Length ? backOffs[backOffIdx] : backOffs[backOffs.Length - 1];
                await Task.Delay(delay);
                queueMessage = await queue.GetMessageAsync();
                backOffIdx++;
            }

            sw.Stop();

            return queueMessage;
        }

        private CloudQueueMessage BuildCloudQueueMessage(TransportMessage message)
        {
            var jsonMsg = JObject.FromObject(message);
            var msgBytes = (Serializer.Serialize(jsonMsg) as MemoryStream).ToArray();

            var queueMessage = new CloudQueueMessage(message.Id, "");
            queueMessage.SetMessageContent(msgBytes);

            return queueMessage;
        }

        private TransportMessage BuildMessage(CloudQueueMessage queueMessage)
        {
            using (var stream = new MemoryStream(queueMessage.AsBytes))
            {
                var jsonMsg = Serializer.Deserialize(stream);

                var message = jsonMsg.ToObject<TransportMessage>();
                return message;
            }
        }
    }
}
