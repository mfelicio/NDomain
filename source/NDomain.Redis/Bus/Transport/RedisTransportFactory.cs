using NDomain.Bus.Transport;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Redis
{
    public class RedisTransportFactory : ITransportFactory
    {
        readonly ConnectionMultiplexer connection;
        readonly string prefix;

        public RedisTransportFactory(ConnectionMultiplexer connection, string prefix)
        {
            this.connection = connection;
            this.prefix = prefix;
        }

        public IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new RedisTransport(this.connection, this.prefix, endpoint);
        }

        public IOutboundTransport CreateOutboundTransport()
        {
            return new RedisTransport(this.connection, this.prefix, null);
        }
    }
}
