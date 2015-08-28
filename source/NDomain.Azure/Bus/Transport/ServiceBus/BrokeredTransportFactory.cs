using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Azure.ServiceBus
{
    public class BrokeredTransportFactory : ITransportFactory
    {
        readonly string connectionString;
        readonly string prefix;

        public BrokeredTransportFactory(string connectionString, string prefix = null)
        {
            this.connectionString = connectionString;
            this.prefix = prefix;
        }

        public IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new BrokeredTransport(this.connectionString, endpoint);
        }

        public IOutboundTransport CreateOutboundTransport()
        {
            return new BrokeredTransport(this.connectionString, null);
        }
    }
}
