using System;
using System.Reflection;
using System.Threading.Tasks;
using NDomain.Bus;
using NDomain.Bus.Subscriptions;
using NDomain.Bus.Transport;
using NDomain.IoC;
using NDomain.Logging;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for message processing capabilities
    /// </summary>
    public class ProcessorConfigurator : Configurator
    {
        private readonly Action<ProcessorConfigurator> configurer;

        private string endpoint;
        private int maxDeliveryCount;
        private bool deadLetterMessages;
        private int concurrencyLevel;

        internal event Action<Processor> Configuring;

        public ProcessorConfigurator(ContextBuilder builder, Action<ProcessorConfigurator> configurer)
            : base(builder)
        {
            this.configurer = configurer;
            this.endpoint = Assembly.GetExecutingAssembly().GetName().Name;
            this.maxDeliveryCount = InboundTransportOptions.DefaultMaxDeliveryCount;
            this.concurrencyLevel = 10; //default, define constant
        }

        /// <summary>
        /// Sets the name of the endpoint for the current processor
        /// </summary>
        /// <param name="name">endpoint name</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        public ProcessorConfigurator Endpoint(string endpoint)
        {
            this.endpoint = endpoint;
            return this;
        }

        /// <summary>
        /// Sets the value for max delivery count of each individual message until it gets deleted and deadlettered
        /// </summary>
        /// <param name="maxDeliveryCount">max delivery count</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        public ProcessorConfigurator MaxDeliveryCount(int maxDeliveryCount)
        {
            this.maxDeliveryCount = maxDeliveryCount;
            return this;
        }

        /// <summary>
        /// Sets whether messages should be deadlettered after reaching failing as many times as defined by MaxDeliveryCount
        /// </summary>
        /// <param name="deadLetterMessages">deadLetterMessages</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        public ProcessorConfigurator DeadLetterMessages(bool deadLetterMessages)
        {
            this.deadLetterMessages = deadLetterMessages;
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
            this.concurrencyLevel = concurrencyLevel;
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

            var options = new InboundTransportOptions(this.endpoint, this.maxDeliveryCount, this.deadLetterMessages);
            
            var processor = new Processor(options, this.concurrencyLevel, subscriptionManager, transportFactory, loggerFactory, resolver);

            // handlers can be registered here
            if (this.Configuring != null)
            {
                this.Configuring(processor);
            }

            return processor;
        }
    }
}