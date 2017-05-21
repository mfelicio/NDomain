using System;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    public class RetryingInboundTransportDecorator : IInboundTransport
    {
        private readonly IInboundTransport inbound;
        private readonly IOutboundTransport outbound;
        private readonly InboundTransportOptions options;

        public RetryingInboundTransportDecorator(
            IInboundTransport inbound,
            IOutboundTransport outbound,
            InboundTransportOptions options)
        {
            this.inbound = inbound;
            this.outbound = outbound;
            this.options = options;
        }

        public async Task<IMessageTransaction> Receive(TimeSpan? timeout = null)
        {
            var message = await this.inbound.Receive(timeout);

            if (message == null || message.DeliveryCount < this.options.MaxDeliveryCount)
            {
                return message;
            }

            if (message.DeliveryCount == this.options.MaxDeliveryCount)
            {
                return new MessageTransactionDecorator(message, outbound, this.options);
            }

            // received a message with RetryCount > MaxRetries
            // it means that the source message was sent to dead letter endpoint but not deleted on source endpoint
            
            // in this case we delete it and silently reject it
            await message.Commit();
            
            return null;
        }

        class MessageTransactionDecorator : IMessageTransaction
        {
            private readonly IMessageTransaction source;
            private readonly IOutboundTransport outbound;
            private readonly InboundTransportOptions options;

            public MessageTransactionDecorator(
                IMessageTransaction source,
                IOutboundTransport outbound,
                InboundTransportOptions options)
            {
                this.source = source;
                this.outbound = outbound;
                this.options = options;
            }

            public TransportMessage Message => this.source.Message;

            public int DeliveryCount => this.source.DeliveryCount;

            public Task Commit()
            {
                return this.source.Commit();
            }

            public async Task Fail()
            {
                if (this.options.DeadLeterMessages)
                {
                    var deadLetterMessage = BuildDeadLetterMessage();

                    // send dead letter msg to dead letter endpoint
                    await this.outbound.Send(deadLetterMessage);
                }

                // delete the source message
                // in case this fails, the source message will be retried with a RetryCounter higher than max retries
                // if/when that happens the DeadLetterTransportDecorator will reject it
                await this.source.Commit();
            }

            private TransportMessage BuildDeadLetterMessage()
            {
                this.Message.Headers[MessageHeaders.OriginalEndpoint] = this.Message.Headers[MessageHeaders.Endpoint];
                this.Message.Headers[MessageHeaders.Endpoint] = this.options.GetDeadLetterEndpoint();

                return this.Message;
            }
        }
    }
}
