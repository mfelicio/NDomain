using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.WindowsAzure.Storage;
using NDomain;
using NDomain.Configuration;
using OpenStore.Api.Configuration;
using Owin;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenStore.Api.Azure
{
    public class App
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            ApiConfig.Register(config);

            var azureAccount = GetAzureAccont();
            var redisConnection = GetRedisConnection();
            var container = BuildContainer();

            var appPrefix = "openstore";

            var context = DomainContext.Configure()
                                       .EventSourcing(c => c.WithAzureTableStorage(azureAccount, "events"))
                                       .IoC(c => c.WithAutofac(container))
                                       .Bus(c => c.WithAzureQueues(azureAccount, appPrefix)
                                                  .WithRedisSubscriptionBroker(redisConnection, appPrefix)
                                                  .WithRedisSubscriptionStore(redisConnection, appPrefix))
                                       .Start();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);
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
    }
}
