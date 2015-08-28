using NDomain.Bus.Subscriptions.Redis;
using NDomain.Bus.Transport.Redis;
using NDomain.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain
{
    public static class RedisExtensions
    {
        public static BusConfigurator WithRedisTransport(this BusConfigurator configurator, ConnectionMultiplexer connection, string queueName)
        {
            configurator.MessagingFactory = new RedisTransportFactory(connection, queueName);

            return configurator;
        }

        public static BusConfigurator WithRedisSubscriptionStore(this BusConfigurator configurator, ConnectionMultiplexer connection, string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "ndomain";
            }
            else
            {
                prefix = string.Format("{0}.ndomain", prefix);
            }

            configurator.SubscriptionStore = new RedisSubscriptionStore(connection, prefix);

            return configurator;
        }

        public static BusConfigurator WithRedisSubscriptionBroker(this BusConfigurator configurator, ConnectionMultiplexer connection, string prefix = null) 
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "ndomain";
            }
            else
            {
                prefix = string.Format("{0}.ndomain", prefix);
            }

            configurator.SubscriptionBroker = new RedisSubscriptionBroker(connection, prefix);

            return configurator;
        }
    }
}
