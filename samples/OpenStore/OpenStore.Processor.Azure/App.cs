using Autofac;
using Microsoft.WindowsAzure.Storage;
using NDomain;
using NDomain.Configuration;
using OpenStore.Marketplace.Domain;
using OpenStore.Marketplace.Domain.Handlers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Processor.Azure
{
    public class App : IDisposable
    {
        readonly IContainer container;
        readonly IDomainContext context;

        public App(IContainer container, IDomainContext context)
        {
            this.container = container;
            this.context = context;
        }

        public static IDisposable Start()
        {
            var azureAccount = GetAzureAccont();
            var redisConnection = GetRedisConnection();
            var container = BuildContainer();

            var appPrefix = "openstore";

            var context = DomainContext.Configure()
                                       .IoC(c => c.WithAutofac(container))
                                       .EventSourcing(c => c.WithAzureTableStorage(azureAccount, "events")
                                                            .BindAggregate<Sale>()
                                                            .BindAggregate<Auction>())
                                       .Bus(c => c.WithAzureQueues(azureAccount, appPrefix)
                                                  .WithRedisSubscriptionBroker(redisConnection, appPrefix)
                                                  .WithRedisSubscriptionStore(redisConnection, appPrefix)
                                                  .WithProcessor(p => p.Endpoint("marketplace")
                                                                       .RegisterHandler<SaleCommandHandler>()
                                                                       .RegisterHandler<AuctionCommandHandler>()))
                                       .Start();

            var app = new App(container, context);
            return app;
        }

        private static CloudStorageAccount GetAzureAccont()
        {
            // TODO: use development or real one based on configuration

            // return CloudStorageAccount.Parse(storageConnectionString);
            return CloudStorageAccount.DevelopmentStorageAccount;
        }

        private static ConnectionMultiplexer GetRedisConnection()
        {
            // TODO: config
            var options = new ConfigurationOptions();
            options.EndPoints.Add("localhost", 6379);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            // TODO

            return builder.Build();
        }

        public void Dispose()
        {
            this.context.Dispose();
            this.container.Dispose();
        }
    }
}
