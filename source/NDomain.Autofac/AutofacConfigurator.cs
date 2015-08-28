using Autofac;
using NDomain.Autofac;
using NDomain.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    public static class AutofacConfigurator
    {
        public static IoCConfigurator WithAutofac(this IoCConfigurator b, IContainer container)
        {
            b.Resolver = new AutofacDependencyResolver(container.BeginLifetimeScope());

            b.Configured += context =>
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(context);

                builder.RegisterInstance(context.CommandBus);
                builder.RegisterInstance(context.EventBus);
                builder.RegisterInstance(context.EventStore).As<IEventStore>();
                builder.RegisterGeneric(typeof(AggregateRepository<>))
                       .As(typeof(IAggregateRepository<>)).SingleInstance();

                // usually command/event handlers
                foreach (var knownType in b.KnownTypes)
                {
                    builder.RegisterType(knownType)
                           .AsSelf()
                           .PreserveExistingDefaults();
                }

                builder.Update(container);
            };

            return b;
        }
    }
}
