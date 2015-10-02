using Autofac;
using Autofac.Integration.WebApi;
using NDomain;
using NDomain.Configuration;
using OpenStore.Api.Configuration;
using OpenStore.Marketplace.Domain;
using OpenStore.Marketplace.Domain.Handlers;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenStore.SingleHost
{
    public class App
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            ApiConfig.Register(config);

            var container = BuildContainer();

            var context = DomainContext.Configure()
                                       .EventSourcing(c => c.BindAggregate<Sale>()
                                                            .BindAggregate<Auction>())
                                       .IoC(c => c.WithAutofac(container))
                                       .Bus(c => c.WithProcessor(p =>
                                                    p.Endpoint("marketplace")
                                                    .RegisterHandler<SaleCommandHandler>()
                                                    .RegisterHandler<AuctionCommandHandler>()))
                                       .Start();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            // TODO

            return builder.Build();
        }
    }
}
