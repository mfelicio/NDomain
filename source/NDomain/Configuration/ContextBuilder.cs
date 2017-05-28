using NDomain.IoC;
using NDomain.Logging;
using System;
using System.Collections.Generic;
using NDomain.Bus;
using NDomain.Bus.Subscriptions;
using NDomain.CQRS;
using NDomain.Persistence.EventSourcing;
using NDomain.Persistence.Snapshot;

namespace NDomain.Configuration
{
    /// <summary>
    /// Manages the DomainContext build pipeline
    /// </summary>
    public class ContextBuilder
    {
        public ContextBuilder()
        {
            this.EventSourcingConfigurator = new ModelConfigurator(this);
            this.BusConfigurator = new BusConfigurator(this);
            this.LoggingConfigurator = new LoggingConfigurator(this);
            this.IoCConfigurator = new IoCConfigurator(this);
        }

        internal ModelConfigurator EventSourcingConfigurator { get; }
        internal BusConfigurator BusConfigurator { get; }
        internal LoggingConfigurator LoggingConfigurator { get; }
        internal IoCConfigurator IoCConfigurator { get; }

        // using Lazy's to avoid managing dependencies between configurators and to ensure no circular references exist
        internal Lazy<IEventStore> EventStore { get; set; }
        internal Lazy<ISnapshotStore> SnapshotStore { get; set; }
        public Lazy<IMessageBus> MessageBus { get; set; }
        internal Lazy<IEventBus> EventBus { get; set; }
        internal Lazy<ICommandBus> CommandBus { get; set; }
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

            var context = new DomainContext(this.EventBus.Value,
                                            this.CommandBus.Value,
                                            this.Processors.Value,
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
        public ContextBuilder EventSourcing(Action<ModelConfigurator> configurer)
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
