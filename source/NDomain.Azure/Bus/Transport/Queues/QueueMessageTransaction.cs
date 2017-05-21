using System;
using System.Threading.Tasks;
using NDomain.Bus.Transport;

namespace NDomain.Azure.Bus.Transport.Queues
{
    class QueueMessageTransaction : IMessageTransaction
    {
        private readonly Func<Task> onCommit;
        private readonly Func<Task> onFail;

        public QueueMessageTransaction(TransportMessage message, int retryCount, Func<Task> onCommit, Func<Task> onFail)
        {
            this.Message = message;
            this.DeliveryCount = retryCount;
            this.onCommit = onCommit;
            this.onFail = onFail;
        }

        public TransportMessage Message { get; }

        public int DeliveryCount { get; }

        public Task Commit() => this.onCommit();

        public Task Fail() => this.onFail();
    }
}
