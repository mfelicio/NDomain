using NDomain.Bus.Transport;

namespace NDomain.Azure.Bus.Transport.ServiceBus
{
    public class BrokeredTransportFactory : BrokerlessTransportFactory
    {
        private readonly string connectionString;
        private readonly string prefix;

        public BrokeredTransportFactory(string connectionString, string prefix = null)
        {
            this.connectionString = connectionString;
            this.prefix = prefix;
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new BrokeredTransport(this.connectionString, endpoint);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new BrokeredTransport(this.connectionString, null);
        }
    }
}
