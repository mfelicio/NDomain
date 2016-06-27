using NDomain.IoC;
using NDomain.Logging;
using NDomain.Bus;
using NDomain.Bus.Transport;
using NDomain.Bus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for the messaging capabilities
    /// </summary>
    public class BusConfigurator : Configurator
    {
        readonly List<ProcessorConfigurator> processorConfigurators;

        public ISubscriptionStore SubscriptionStore { get; set; }
        public ISubscriptionBroker SubscriptionBroker { get; set; }
        public ITransportFactory TransportFactory { get; set; }

        public BusConfigurator(ContextBuilder builder)
            : base(builder)
        {
            this.processorConfigurators = new List<ProcessorConfigurator>();

            builder.Configuring += this.OnConfiguring;
        }

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

            builder.SubscriptionManager = new Lazy<ISubscriptionManager>(() => subscriptionManager);

            builder.MessageBus = new Lazy<IMessageBus>(
                () => new MessageBus(subscriptionManager, transport, builder.LoggerFactory.Value));

            builder.Processors = new Lazy<IEnumerable<IProcessor>>(
                () => this.processorConfigurators
                          .Select(p => p.Configure(subscriptionManager,
                                                   this.TransportFactory,
                                                   builder.LoggerFactory.Value,
                                                   builder.Resolver.Value)).ToArray());

        }

    }

    /// <summary>
    /// Configurator for message processing capabilities
    /// </summary>
    public class ProcessorConfigurator : Configurator
    {
        readonly Action<ProcessorConfigurator> configurer;

        internal event Action<Processor> Configuring;
        internal string EndpointName { get; set; }
        internal int ConcurrencyLevel { get; set; }

        public ProcessorConfigurator(ContextBuilder builder, Action<ProcessorConfigurator> configurer)
            : base(builder)
        {
            this.configurer = configurer;
            this.EndpointName = Assembly.GetExecutingAssembly().GetName().Name;
            this.ConcurrencyLevel = 10; //default, define constant
        }

        /// <summary>
        /// Sets the name of the endpoint for the current processor
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        public ProcessorConfigurator Endpoint(string name)
        {
            this.EndpointName = name;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of concurrent messages being processed by this instance
        /// </summary>
        /// <remarks>
        /// On a scale out scenario, where multiple processes have the same Processor configured, the concurrencyLevel is per instance (process), not global per Processor definition
        /// </remarks>
        /// <param name="concurrencyLevel">concurrencyLevel</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        public ProcessorConfigurator WithConcurrencyLevel(int concurrencyLevel)
        {
            this.ConcurrencyLevel = concurrencyLevel;
            return this;
        }

        /// <summary>
        /// Registers a custom handler to handle messages of type TMessage.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message to handle</typeparam>
        /// <param name="handlerName">handlerName</param>
        /// <param name="handlerFunc">handlerFunc</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public ProcessorConfigurator RegisterMessageHandler<TMessage>(string handlerName, Func<TMessage, Task> handlerFunc)
        {
            this.Configuring += processor => processor.RegisterMessageHandler<TMessage>(
                                                            handlerName,
                                                            new MessageHandler<TMessage>(handlerFunc));

            return this;
        }

        /// <summary>
        /// Configures the current processor instance with the previously specified settings.
        /// </summary>
        /// <param name="subscriptionManager">subscriptionManager</param>
        /// <param name="transportFactory">transportFactory</param>
        /// <param name="loggerFactory">loggerFactory</param>
        /// <param name="resolver">resolver</param>
        /// <returns>A configured IProcessor instance</returns>
        public IProcessor Configure(ISubscriptionManager subscriptionManager,
                                    ITransportFactory transportFactory,
                                    ILoggerFactory loggerFactory,
                                    IDependencyResolver resolver)
        {

            this.configurer(this);

            var processor = new Processor(this.EndpointName, this.ConcurrencyLevel, subscriptionManager, transportFactory, loggerFactory, resolver);

            // handlers can be registered here
            if (this.Configuring != null)
            {
                this.Configuring(processor);
            }

            return processor;
        }
    }
}
