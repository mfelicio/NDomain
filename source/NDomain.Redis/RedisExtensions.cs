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
    /// <summary>
    /// BusConfigurator extensions to use Redis
    /// </summary>
    public static class RedisExtensions
    {
        /// <summary>
        /// Configures the MessageBus to use a Redis transport
        /// </summary>
        /// <param name="configurator">configurator</param>
        /// <param name="connection">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        /// <returns>The current configurator instance to be used in a fluent manner</returns>
        public static BusConfigurator WithRedisTransport(this BusConfigurator configurator, ConnectionMultiplexer connection, string prefix)
        {
            configurator.MessagingFactory = new RedisTransportFactory(connection, prefix);

            return configurator;
        }

        /// <summary>
        /// Configures the MessageBus to use a Redis subscription store
        /// </summary>
        /// <param name="configurator">configurator</param>
        /// <param name="connection">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        /// <returns>The current configurator instance to be used in a fluent manner</returns>
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

        /// <summary>
        /// Configures the MessageBus to use a Redis subscription broker
        /// </summary>
        /// <param name="configurator">configurator</param>
        /// <param name="connection">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        /// <returns>The current configurator instance to be used in a fluent manner</returns>
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
