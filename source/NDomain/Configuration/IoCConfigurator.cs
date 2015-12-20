using NDomain.IoC;
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

        /// <summary>
        /// Allows specific IoC container integrations to register NDomain's components
        /// </summary>
        public Action<DomainContext> Configured { get; set; }

        public IoCConfigurator(ContextBuilder builder)
            :base(builder)
        {
            this.KnownTypes = new List<Type>();
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
            if (context.Resolver is DefaultDependencyResolver)
            {
                var resolver = context.Resolver as DefaultDependencyResolver;

                resolver.Register(context);
                resolver.Register(context.EventStore);
                resolver.Register(context.EventBus);
                resolver.Register(context.CommandBus);
                resolver.RegisterGenericTypeDef(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));
            }

            // hack, so that AutofacConfigurator can also register components on container
            if (this.Configured != null)
            {
                this.Configured(context);
            }
        }
    }
}
