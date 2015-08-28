using NDomain.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    public class IoCConfigurator : Configurator
    {
        public IDependencyResolver Resolver { get; set; }
        public List<Type> KnownTypes { get; private set; }

        public Action<DomainContext> Configured { get; set; }

        public IoCConfigurator(ContextBuilder builder)
            :base(builder)
        {
            this.KnownTypes = new List<Type>();
            builder.Configuring += this.OnConfiguring;
            builder.Configured += this.OnConfigured;
        }

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
