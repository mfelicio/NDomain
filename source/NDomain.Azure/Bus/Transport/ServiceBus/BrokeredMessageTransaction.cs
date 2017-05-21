using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NDomain.Bus.Transport;

namespace NDomain.Azure.Bus.Transport.ServiceBus
{
    internal class BrokeredMessageTransaction : IMessageTransaction
    {
        private readonly BrokeredMessage source;

        public BrokeredMessageTransaction(BrokeredMessage source, TransportMessage message)
        {
            this.source = source;
            this.Message = message;
        }

        public TransportMessage Message { get; }

        public int DeliveryCount => this.source.DeliveryCount;

        public Task Commit() => this.source.CompleteAsync();

        public Task Fail() => this.source.AbandonAsync();
    }
}
