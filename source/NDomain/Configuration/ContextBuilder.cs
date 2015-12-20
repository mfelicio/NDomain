using NDomain.EventSourcing;
using NDomain.IoC;
using NDomain.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NDomain.Bus;
using NDomain.Bus.Subscriptions;
using NDomain.CQRS;

namespace NDomain.Configuration
{
    /// <summary>
    /// Manages the DomainContext build pipeline
    /// </summary>
    public class ContextBuilder
    {
        public ContextBuilder()
        {
            this.EventSourcingConfigurator = new EventSourcingConfigurator(this);
            this.BusConfigurator = new BusConfigurator(this);
            this.LoggingConfigurator = new LoggingConfigurator(this);
            this.IoCConfigurator = new IoCConfigurator(this);
        }

        internal EventSourcingConfigurator EventSourcingConfigurator { get; private set; }
        internal BusConfigurator BusConfigurator { get; private set; }
        internal LoggingConfigurator LoggingConfigurator { get; private set; }
        internal IoCConfigurator IoCConfigurator { get; private set; }

        // using Lazy's to avoid managing dependencies between configurators and to ensure no circular references exist
        internal Lazy<IEventStore> EventStore { get; set; }
        internal Lazy<IMessageBus> MessageBus { get; set; }
        internal Lazy<ISubscriptionManager> SubscriptionManager { get; set; }
        internal Lazy<IEnumerable<IProcessor>> Processors { get; set; }
        internal Lazy<ILoggerFactory> LoggerFactory { get; set; }
        internal Lazy<IDependencyResolver> Resolver { get; set; }

        internal event Action<ContextBuilder> Configuring;
        internal event Action<DomainContext> Configured;

        /// <summary>
        /// Creates a new DomainContext and starts its processors, using the previously applied configurations.
        /// </summary>
        /// <returns>A new DomainContext</returns>
        public IDomainContext Start()
        {
            if (this.Configuring != null)
            {
                this.Configuring(this);
            }

            var context = new DomainContext(this.EventStore.Value,
                                            new EventBus(this.MessageBus.Value),
                                            new CommandBus(this.MessageBus.Value),
                                            this.Processors.Value,
                                            this.LoggerFactory.Value,
                                            this.Resolver.Value);

            if (this.Configured != null)
            {
                this.Configured(context);
            }

            this.Configuring = null;
            this.Configured = null;

            context.StartProcessors();

            return context;
        }

        /// <summary>
        /// Configures event sourcing capabilities
        /// </summary>
        /// <param name="configurer">configurer handler</param>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public ContextBuilder EventSourcing(Action<EventSourcingConfigurator> configurer)
        {
            configurer(this.EventSourcingConfigurator);
            return this;
        }

        /// <summary>
        /// Configures messaging capabilities
        /// </summary>
        /// <param name="configurer">configurer handler</param>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public ContextBuilder Bus(Action<BusConfigurator> configurer)
        {
            configurer(this.BusConfigurator);
            return this;
        }

        /// <summary>
        /// Configures logging capabilities
        /// </summary>
        /// <param name="configurer">configurer handler</param>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public ContextBuilder Logging(Action<LoggingConfigurator> configurer)
        {
            configurer(this.LoggingConfigurator);
            return this;
        }

        /// <summary>
        /// Configures IoC capabilities
        /// </summary>
        /// <param name="configurer">configurer handler</param>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public ContextBuilder IoC(Action<IoCConfigurator> configurer)
        {
            configurer(this.IoCConfigurator);
            return this;
        }
    }
}
