using NDomain.Bus.Transport;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Azure.Queues
{
    public class QueueTransportFactory : ITransportFactory
    {
        readonly CloudStorageAccount account;
        readonly string prefix;

        public QueueTransportFactory(CloudStorageAccount account, string prefix)
        {
            this.account = account;
            this.prefix = prefix;
        }

        public IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new QueueTransport(this.account, prefix, endpoint);
        }

        public IOutboundTransport CreateOutboundTransport()
        {
            return new QueueTransport(this.account, prefix, null);
        }
    }
}
