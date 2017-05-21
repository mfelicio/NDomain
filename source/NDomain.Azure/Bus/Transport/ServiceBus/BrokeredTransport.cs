using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NDomain.Bus;
using NDomain.Bus.Transport;

namespace NDomain.Azure.Bus.Transport.ServiceBus
{
    public class BrokeredTransport : IInboundTransport, IOutboundTransport
    {
        private readonly string inputQueueName;
        private readonly MessagingFactory factory;
        private readonly ConcurrentDictionary<string, QueueClient> queues;

        public BrokeredTransport(string connectionString, string inputQueueName)
        {
            this.inputQueueName = inputQueueName;
            this.factory = MessagingFactory.CreateFromConnectionString(connectionString);
            this.queues = new ConcurrentDictionary<string, QueueClient>();
        }

        private QueueClient GetQueue(string queueName)
        {
            return this.queues.GetOrAdd(queueName,
                e => this.factory.CreateQueueClient(queueName, ReceiveMode.PeekLock));
        }

        public Task Send(TransportMessage message)
        {
            var endpoint = message.Headers[MessageHeaders.Endpoint];
            var queue = GetQueue(endpoint);

            var msg = BuildBrokeredMessage(message);
            return queue.SendAsync(msg);
        }

        public Task SendMultiple(IEnumerable<TransportMessage> messages)
        {
            // TODO: fix this, per message
            var endpoint = messages.First().Headers[MessageHeaders.Endpoint];
            var queue = GetQueue(endpoint);

            var msgs = messages.Select(msg => BuildBrokeredMessage(msg)).ToArray();
            return queue.SendBatchAsync(msgs);
        }

        public async Task<IMessageTransaction> Receive(TimeSpan? timeout = null)
        {
            var inputQueue = GetQueue(this.inputQueueName);
            var brokeredMessage = await inputQueue.ReceiveAsync(timeout ?? TimeSpan.FromMinutes(1));

            if (brokeredMessage == null)
            {
                return null;
            }

            var message = BuildMessage(brokeredMessage);

            return new BrokeredMessageTransaction(brokeredMessage, message);
        }

        protected BrokeredMessage BuildBrokeredMessage(TransportMessage message)
        {
            var brokeredMessage = new BrokeredMessage(Serializer.Serialize(message.Body));
            brokeredMessage.MessageId = message.Id;
            brokeredMessage.Label = message.Name;

            foreach (var header in message.Headers)
            {
                brokeredMessage.Properties[header.Key] = header.Value;
            }

            return brokeredMessage;
        }

        protected TransportMessage BuildMessage(BrokeredMessage brokeredMessage)
        {
            var message = new TransportMessage();
            message.Body = Serializer.Deserialize(brokeredMessage.GetBody<Stream>());
            message.Id = brokeredMessage.MessageId;
            message.Name = brokeredMessage.Label;

            foreach (var property in brokeredMessage.Properties)
            {
                message.Headers[property.Key] = property.Value.ToString();
            }

            return message;
        }
    }
}
