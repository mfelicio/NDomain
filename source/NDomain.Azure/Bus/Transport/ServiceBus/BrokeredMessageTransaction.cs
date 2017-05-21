using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Azure.ServiceBus
{
    class BrokeredMessageTransaction : IMessageTransaction
    {
        readonly BrokeredMessage source;
        readonly TransportMessage message;

        public BrokeredMessageTransaction(BrokeredMessage source, TransportMessage message)
        {
            this.source = source;
            this.message = message;
        }

        public TransportMessage Message { get { return this.message; } }

        public int DeliveryCount { get { return this.source.DeliveryCount; } }

        public Task Commit()
        {
            return this.source.CompleteAsync();
        }

        public Task Fail()
        {
            return this.source.AbandonAsync();
        }
    }
}
