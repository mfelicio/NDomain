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
    public class ContextBuilder
    {
        public ContextBuilder()
        {
            this.EventSourcingConfigurator = new EventSourcingConfigurator(this);
            this.BusConfigurator = new BusConfigurator(this);
            this.LoggingConfigurator = new LoggingConfigurator(this);
            this.IoCConfigurator = new IoCConfigurator(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public EventSourcingConfigurator EventSourcingConfigurator { get; private set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public BusConfigurator BusConfigurator { get; private set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public LoggingConfigurator LoggingConfigurator { get; private set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IoCConfigurator IoCConfigurator { get; private set; }

        // using Lazy's to avoid managing dependencies between configurators and to ensure no circular references exist
        public Lazy<IEventStore> EventStore { get; set; }
        public Lazy<IMessageBus> MessageBus { get; set; }
        public Lazy<ISubscriptionManager> SubscriptionManager { get; set; }
        public Lazy<IEnumerable<IProcessor>> Processors { get; set; }
        public Lazy<ILoggerFactory> LoggerFactory { get; set; }
        public Lazy<IDependencyResolver> Resolver { get; set; }

        public event Action<ContextBuilder> Configuring;
        public event Action<DomainContext> Configured;

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

        public ContextBuilder EventSourcing(Action<EventSourcingConfigurator> configurer)
        {
            configurer(this.EventSourcingConfigurator);
            return this;
        }

        public ContextBuilder Bus(Action<BusConfigurator> configurer)
        {
            configurer(this.BusConfigurator);
            return this;
        }

        public ContextBuilder Logging(Action<LoggingConfigurator> configurer)
        {
            configurer(this.LoggingConfigurator);
            return this;
        }

        public ContextBuilder IoC(Action<IoCConfigurator> configurer)
        {
            configurer(this.IoCConfigurator);
            return this;
        }
    }
}
