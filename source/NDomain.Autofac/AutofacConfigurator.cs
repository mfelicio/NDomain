using Autofac;
using NDomain.Autofac;
using NDomain.Model;

namespace NDomain.Configuration
{
    /// <summary>
    /// Configurator for Autofac
    /// </summary>
    public static class AutofacConfigurator
    {
        /// <summary>
        /// Configures a <see cref="NDomain.IoC.IDependencyResolver"/> to use Autofac 
        /// based on the <paramref name="container"/>. Useful to resolve application message handlers
        /// that depend on other components external to NDomain.
        /// The application's Autofac container will also get updated with registries for NDomain's components.
        /// </summary>
        /// <param name="b">configurator instance</param>
        /// <param name="container">application's Autofac container</param>
        /// <returns>Current configurator instance, to be used in a fluent manner.</returns>
        public static IoCConfigurator WithAutofac(this IoCConfigurator b, IContainer container)
        {
            b.Resolver = new AutofacDependencyResolver(container.BeginLifetimeScope());
            
            b.Configured += context =>
            {
                var builder = new ContainerBuilder();

                foreach(var registration in b.KnownInstances)
                {
                    builder.RegisterInstance(registration.Value).As(registration.Key);
                }

                // usually command/event handlers
                foreach (var knownType in b.KnownTypes)
                {
                    builder.RegisterType(knownType)
                           .AsSelf()
                           .PreserveExistingDefaults();
                }

                builder.RegisterGeneric(typeof(AggregateRepository<>))
                       .As(typeof(IAggregateRepository<>)).SingleInstance();

                builder.Update(container);
            };

            return b;
        }
    }
}
