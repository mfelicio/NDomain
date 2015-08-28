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
    public class BusConfigurator : Configurator
    {
        readonly List<ProcessorConfigurator> processorConfigurators;

        public ISubscriptionStore SubscriptionStore { get; set; }
        public ISubscriptionBroker SubscriptionBroker { get; set; }
        public ITransportFactory MessagingFactory { get; set; }

        public BusConfigurator(ContextBuilder builder)
            : base(builder)
        {
            this.processorConfigurators = new List<ProcessorConfigurator>();

            builder.Configuring += this.OnConfiguring;
        }

        public BusConfigurator WithProcessor(Action<ProcessorConfigurator> configurer)
        {
            var processorConfigurator = new ProcessorConfigurator(this.Builder, configurer);
            this.processorConfigurators.Add(processorConfigurator);

            return this;
        }

        public BusConfigurator WithLocalTransportFactory()
        {
            this.MessagingFactory = new LocalTransportFactory();
            return this;
        }

        public BusConfigurator WithCustomSubscriptionBroker(ISubscriptionBroker subscriptionBroker)
        {
            this.SubscriptionBroker = subscriptionBroker;
            return this;
        }

        public BusConfigurator WithCustomSubscriptionStore(ISubscriptionStore subscriptionStore)
        {
            this.SubscriptionStore = subscriptionStore;
            return this;
        }

        public BusConfigurator WithCustomTransportFactory(ITransportFactory messagingFactory)
        {
            this.MessagingFactory = messagingFactory;
            return this;
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            var subscriptionStore = this.SubscriptionStore ?? new LocalSubscriptionStore();
            var subscriptionBroker = this.SubscriptionBroker ?? new LocalSubscriptionBroker();

            var subscriptionManager = new SubscriptionManager(subscriptionStore, subscriptionBroker);

            this.MessagingFactory = this.MessagingFactory ?? new LocalTransportFactory();

            var transport = this.MessagingFactory.CreateOutboundTransport();

            builder.SubscriptionManager = new Lazy<ISubscriptionManager>(() => subscriptionManager);

            builder.MessageBus = new Lazy<IMessageBus>(
                () => new MessageBus(subscriptionManager, transport, builder.LoggerFactory.Value));

            builder.Processors = new Lazy<IEnumerable<IProcessor>>(
                () => this.processorConfigurators
                          .Select(p => p.Configure(subscriptionManager,
                                                   this.MessagingFactory,
                                                   builder.LoggerFactory.Value,
                                                   builder.Resolver.Value)).ToArray());

        }

    }

    public class ProcessorConfigurator : Configurator
    {
        readonly Action<ProcessorConfigurator> configurer;

        public event Action<Processor> Configuring;
        public string EndpointName { get; set; }
        public int ConcurrencyLevel { get; set; }

        public ProcessorConfigurator(ContextBuilder builder, Action<ProcessorConfigurator> configurer)
            : base(builder)
        {
            this.configurer = configurer;
            this.EndpointName = Assembly.GetExecutingAssembly().GetName().Name;
            this.ConcurrencyLevel = 10; //default, define constant
        }

        public ProcessorConfigurator Endpoint(string name)
        {
            this.EndpointName = name;
            return this;
        }

        public ProcessorConfigurator WithConcurrencyLevel(int concurrencyLevel)
        {
            this.ConcurrencyLevel = concurrencyLevel;
            return this;
        }

        public ProcessorConfigurator RegisterMessageHandler<TMessage>(string handlerName, Func<TMessage, Task> handlerFunc)
        {
            this.Configuring += processor => processor.RegisterMessageHandler<TMessage>(
                                                            handlerName,
                                                            new MessageHandler<TMessage>(handlerFunc));

            return this;
        }

        public IProcessor Configure(ISubscriptionManager subscriptionManager,
                                    ITransportFactory messagingFactory,
                                    ILoggerFactory loggerFactory,
                                    IDependencyResolver resolver)
        {

            this.configurer(this);

            var processor = new Processor(this.EndpointName, this.ConcurrencyLevel, subscriptionManager, messagingFactory, loggerFactory, resolver);

            // handlers can be registered here
            if (this.Configuring != null)
            {
                this.Configuring(processor);
            }

            return processor;
        }
    }
}
