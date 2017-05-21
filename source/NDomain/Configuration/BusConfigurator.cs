using NDomain.Bus;
using NDomain.Bus.Transport;
using NDomain.Bus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using NDomain.CQRS;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for the messaging capabilities
    /// </summary>
    public class BusConfigurator : Configurator
    {
        private readonly List<ProcessorConfigurator> processorConfigurators;
        
        public BusConfigurator(ContextBuilder builder)
            : base(builder)
        {
            this.processorConfigurators = new List<ProcessorConfigurator>();

            builder.Configuring += this.OnConfiguring;
        }

        public ISubscriptionStore SubscriptionStore { get; set; }
        public ISubscriptionBroker SubscriptionBroker { get; set; }
        public ITransportFactory TransportFactory { get; set; }

        /// <summary>
        /// Creates a new ProcessorConfigurator to be configured as part of the current context
        /// </summary>
        /// <param name="configurer">configuration handler</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public BusConfigurator WithProcessor(Action<ProcessorConfigurator> configurer)
        {
            var processorConfigurator = new ProcessorConfigurator(this.Builder, configurer);
            this.processorConfigurators.Add(processorConfigurator);

            return this;
        }

        /// <summary>
        /// Configures a local, in-proc, transport factory
        /// </summary>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public BusConfigurator WithLocalTransportFactory()
        {
            this.TransportFactory = new LocalTransportFactory();
            return this;
        }

        /// <summary>
        /// Configures a custom subscription broker
        /// </summary>
        /// <param name="subscriptionBroker">subscriptionBroker</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public BusConfigurator WithCustomSubscriptionBroker(ISubscriptionBroker subscriptionBroker)
        {
            this.SubscriptionBroker = subscriptionBroker;
            return this;
        }

        /// <summary>
        /// Configures a custom subscription store
        /// </summary>
        /// <param name="subscriptionStore">subscriptionStore</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public BusConfigurator WithCustomSubscriptionStore(ISubscriptionStore subscriptionStore)
        {
            this.SubscriptionStore = subscriptionStore;
            return this;
        }

        /// <summary>
        /// Configures a custom TransportFactory
        /// </summary>
        /// <param name="transportFactory">transportFactory</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public BusConfigurator WithCustomTransportFactory(ITransportFactory transportFactory)
        {
            this.TransportFactory = transportFactory;
            return this;
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            var subscriptionStore = this.SubscriptionStore ?? new LocalSubscriptionStore();
            var subscriptionBroker = this.SubscriptionBroker ?? new LocalSubscriptionBroker();

            var subscriptionManager = new SubscriptionManager(subscriptionStore, subscriptionBroker);

            this.TransportFactory = this.TransportFactory ?? new LocalTransportFactory();

            var transport = this.TransportFactory.CreateOutboundTransport();

            builder.MessageBus = new Lazy<IMessageBus>(
                () => new MessageBus(subscriptionManager, transport, builder.LoggerFactory.Value));

            builder.EventBus = new Lazy<IEventBus>(
                () => new EventBus(builder.MessageBus.Value));

            builder.CommandBus = new Lazy<ICommandBus>(
                () => new CommandBus(builder.MessageBus.Value));

            builder.SubscriptionManager = new Lazy<ISubscriptionManager>(() => subscriptionManager);

            builder.Processors = new Lazy<IEnumerable<IProcessor>>(
                () => this.processorConfigurators
                          .Select(p => p.Configure(subscriptionManager,
                                                   this.TransportFactory,
                                                   builder.LoggerFactory.Value,
                                                   builder.Resolver.Value)).ToArray());

        }

    }
}
