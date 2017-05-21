using NDomain.IoC;
using NDomain.Logging;
using NDomain.Bus.Transport;
using NDomain.Bus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public class Processor : IProcessor,
                             IMessageDispatcher
    {
        private readonly InboundTransportOptions options;
        private readonly MessageWorker worker;
        private readonly ISubscriptionManager subscriptionManager;
        private readonly IDependencyResolver resolver;

        private readonly HashSet<Subscription> subscriptions;
        //<messageName, <handlerName, handler>>
        private readonly Dictionary<string, Dictionary<string, IMessageHandler>> registry;

        public Processor(InboundTransportOptions options,
                         int concurrencyLevel,
                         ISubscriptionManager subscriptionManager,
                         ITransportFactory transportFactory,
                         ILoggerFactory loggerFactory,
                         IDependencyResolver resolver)
        {
            this.options = options;
            this.subscriptionManager = subscriptionManager;
            this.resolver = resolver;

            this.subscriptions = new HashSet<Subscription>();
            this.registry = new Dictionary<string, Dictionary<string, IMessageHandler>>();

            this.worker = new MessageWorker(
                                transportFactory.CreateInboundTransport(options),
                                new DiagnosticsDispatcher(this), // TODO: build proper pipeline support
                                loggerFactory, 
                                concurrencyLevel);
        }

        public void RegisterMessageHandler<TMessage>(string handlerName, IMessageHandler handler)
        {
            var subscription = new Subscription(typeof(TMessage).Name, this.options.Endpoint, handlerName);

            if (this.subscriptions.Contains(subscription))
            {
                throw new InvalidOperationException(string.Format("Duplicate subscription for {0}", subscription.Id));
            }

            CreateSubscription(subscription, handler);
        }

        private void CreateSubscription(Subscription subscription, IMessageHandler handler)
        {
            // add subscription to notify subscription manager
            this.subscriptions.Add(subscription);

            // register handler to process incoming messages on this subscription
            Dictionary<string, IMessageHandler> handlers;
            if (!this.registry.TryGetValue(subscription.Topic, out handlers))
            {
                handlers = this.registry[subscription.Topic] = new Dictionary<string, IMessageHandler>();
            }

            handlers[subscription.Component] = handler;
        }

        private IMessageHandler GetMessageHandler(string messageName, string handlerName)
        {
            Dictionary<string, IMessageHandler> handlers;
            if (this.registry.TryGetValue(messageName, out handlers))
            {
                IMessageHandler handler;
                if (handlers.TryGetValue(handlerName, out handler))
                {
                    return handler;
                }
            }

            return null;
        }

        Task IMessageDispatcher.ProcessMessage(TransportMessage message)
        {
            var handlerName = message.Headers[MessageHeaders.Component];

            // find handler
            var handler = this.GetMessageHandler(message.Name, handlerName);
            
            if (handler == null)
            {
                // TODO: send message to an error queue?
                throw new Exception(
                    string.Format("Handler {0} not found or doesn't process message {1}", handlerName, message.Name));
            }

            // create MessageContext and process
            var context = new MessageContext(message, this.resolver);
            return handler.Process(context);
        }

        public void Start()
        {
            // notify subscription manager about the subscriptions for this processor
            this.subscriptionManager.UpdateEndpointSubscriptions(this.options.Endpoint, this.subscriptions)
                .Wait(); // TODO: async

            // start processing incoming messages
            this.worker.Start();
        }

        public void Stop()
        {
            // stops processing incoming messages
            this.worker.Stop();
        }

        public bool IsRunning => this.worker.IsRunning;

        public void Dispose()
        {
            this.worker.Dispose();
        }
    }
}
