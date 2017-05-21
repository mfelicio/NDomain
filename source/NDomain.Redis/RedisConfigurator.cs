using NDomain.Redis.Bus.Subscriptions;
using NDomain.Redis.Bus.Transport;
using StackExchange.Redis;

// ReSharper disable once CheckNamespace
namespace NDomain.Configuration
{
    /// <summary>
    /// BusConfigurator extensions to use Redis
    /// </summary>
    public static class RedisConfigurator
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
            configurator.TransportFactory = new RedisTransportFactory(connection, prefix);

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
            prefix = GetPrefix(prefix);

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
            prefix = GetPrefix(prefix);

            configurator.SubscriptionBroker = new RedisSubscriptionBroker(connection, prefix);

            return configurator;
        }

        private static string GetPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "ndomain";
            }
            else
            {
                prefix = $"{prefix}.ndomain";
            }
            return prefix;
        }
    }
}
