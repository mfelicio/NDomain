using NDomain.Bus.Transport;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Azure.Queues
{
    public class QueueTransportFactory : BrokerlessTransportFactory
    {
        readonly CloudStorageAccount account;
        readonly string prefix;

        public QueueTransportFactory(CloudStorageAccount account, string prefix)
        {
            this.account = account;
            this.prefix = prefix;
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new QueueTransport(this.account, prefix, endpoint);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new QueueTransport(this.account, prefix, null);
        }
    }
}
