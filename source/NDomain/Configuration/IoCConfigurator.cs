using NDomain.CQRS;
using NDomain.IoC;
using NDomain.Model;
using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for the IoC capabilities
    /// </summary>
    public class IoCConfigurator : Configurator
    {
        /// <summary>
        /// Gets or sets the IDependencyResolver implementation to be used
        /// </summary>
        public IDependencyResolver Resolver { get; set; }

        /// <summary>
        /// Application types that can be resolved by the IDependencyResolver, such as message handlers.
        /// </summary>
        /// <remarks>
        /// This may be removed or changed in future versions. Application code should not use it.
        /// </remarks>
        public List<Type> KnownTypes { get; private set; }

        public Dictionary<Type, object> KnownInstances { get; private set; }

        /// <summary>
        /// Allows specific IoC container integrations to register NDomain's components
        /// </summary>
        public Action<DomainContext> Configured { get; set; }

        public IoCConfigurator(ContextBuilder builder)
            :base(builder)
        {
            this.KnownTypes = new List<Type>();
            this.KnownInstances = new Dictionary<Type, object>();

            builder.Configuring += this.OnConfiguring;
            builder.Configured += this.OnConfigured;
        }

        /// <summary>
        /// Configures the default IoC container, which only resolves message handlers that depend only on NDomain's components.
        /// </summary>
        /// <param name="configurer">configurer handler</param>
        /// <returns>The current instance, to be used in a fluent manner</returns>
        public IoCConfigurator UseDefault(Action<DefaultDependencyResolver> configurer)
        {
            this.Resolver = new DefaultDependencyResolver();

            configurer(this.Resolver as DefaultDependencyResolver);

            return this;
        }

        private void OnConfiguring(ContextBuilder builder)
        {
            builder.Resolver = new Lazy<IDependencyResolver>(
                () => this.Resolver ?? new DefaultDependencyResolver());
        }

        private void OnConfigured(DomainContext context)
        {
            this.KnownInstances.Add(typeof(IDomainContext), context);
            this.KnownInstances.Add(typeof(ISnapshotStore), Builder.SnapshotStore.Value);
            this.KnownInstances.Add(typeof(IEventStore), Builder.EventStore.Value);
            this.KnownInstances.Add(typeof(IEventBus), Builder.EventBus.Value);
            this.KnownInstances.Add(typeof(IEventStoreBus), Builder.EventBus.Value);
            this.KnownInstances.Add(typeof(ICommandBus), Builder.CommandBus.Value);
            this.KnownInstances.Add(typeof(IDependencyResolver), Builder.Resolver.Value);

            // default resolver
            if (Builder.Resolver.Value is DefaultDependencyResolver)
            {
                var resolver = Builder.Resolver.Value as DefaultDependencyResolver;

                resolver.RegisterInstances(this.KnownInstances);

                resolver.RegisterGenericTypeDef(
                    typeof(IAggregateRepository<>),
                    typeof(AggregateRepository<>));
            }

            // hack, so that AutofacConfigurator can also register components on container
            if (this.Configured != null)
            {
                this.Configured(context);
            }
        }
    }
}
