using NDomain.Bus.Transport;
using StackExchange.Redis;

namespace NDomain.Redis.Bus.Transport
{
    public class RedisTransportFactory : BrokerlessTransportFactory
    {
        private readonly ConnectionMultiplexer connection;
        private readonly string prefix;

        public RedisTransportFactory(ConnectionMultiplexer connection, string prefix)
        {
            this.connection = connection;
            this.prefix = prefix;
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new RedisTransport(this.connection, this.prefix, endpoint);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new RedisTransport(this.connection, this.prefix, null);
        }
    }
}
