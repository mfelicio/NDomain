using StackExchange.Redis;

namespace NDomain.Bus.Transport.Redis
{
    public class RedisTransportFactory : BrokerlessTransportFactory
    {
        readonly ConnectionMultiplexer connection;
        readonly string prefix;

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
