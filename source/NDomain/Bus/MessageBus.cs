using NDomain.Logging;
using NDomain.Bus.Transport;
using NDomain.Bus.Subscriptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    /// <summary>
    /// Handles all the logic to Send messages to an IOutboundTransport, based on current subscriptions
    /// Essentially takes Messages, transforms into TransportMessages, setting the appropriate headers for each subscription and sends them to an IOutboundTransport
    /// </summary>
    public class MessageBus : IMessageBus
    {
        readonly ISubscriptionManager subscriptionManager;
        readonly IOutboundTransport transport;
        readonly ILogger logger;

        public MessageBus(ISubscriptionManager subscriptionManager, IOutboundTransport transport, ILoggerFactory loggerFactory)
        {
            this.subscriptionManager = subscriptionManager;
            this.transport = transport;
            this.logger = loggerFactory.GetLogger(typeof(MessageBus));
        }

        public Task Send(Message message)
        {
            return Send(new[] { message });
        }

        public async Task Send(IEnumerable<Message> messages)
        {
            var transportMessages = new List<TransportMessage>();

            foreach (var message in messages)
            {
                var subscriptions = await this.subscriptionManager.GetSubscriptions(message.Name);
                if (subscriptions.Any())
                {
                    transportMessages.AddRange(subscriptions.Select(s => BuildTransportMessage(message, s)));
                }
                else
                {
                    // TODO: retry after some time?
                    this.logger.Warn("Attempt to send message {0} but no subscriptions were found", message.Name);
                }
            }

            if (!transportMessages.Any())
            {
                return;
            }

            await PublishMessages(transportMessages);
        }

        private TransportMessage BuildTransportMessage(Message message, Subscription subscription)
        {
            JObject jsonPayload = message.Payload is JObject ? 
                                    (JObject)message.Payload
                                    : JObject.FromObject(message.Payload);

            var transportMessage = new TransportMessage();
            // reuse existing Id or create a new one
            transportMessage.Id = message.Headers.GetOrDefault(MessageHeaders.Id) ?? Guid.NewGuid().ToString();
            transportMessage.Name = message.Name;
            transportMessage.Body = jsonPayload;

            // copy original headers
            foreach (var header in message.Headers)
            {
                transportMessage.Headers[header.Key] = header.Value;
            }
            
            // add subscription headers
            transportMessage.Headers[MessageHeaders.Endpoint] = subscription.Endpoint; // destination queue / processor
            transportMessage.Headers[MessageHeaders.Component] = subscription.Component; //handler within the processor

            this.logger.Debug("Sending {0} to {1}", transportMessage.Name, subscription.Endpoint);

            return transportMessage;
        }

        private Task PublishMessages(IEnumerable<TransportMessage> messages)
        {
            var count = messages.Count();

            if (count == 0)
            {
                return Task.FromResult(0);
            }

            if (count == 1)
            {
                var message = messages.First();
                return this.transport.Send(message);
            }
            else
            {
                return this.transport.SendMultiple(messages);
            }
        }
    }
}
