using System;
using System.Threading.Tasks;
using NDomain.Bus.Transport;

namespace NDomain.Redis.Bus.Transport
{
    public class RedisMessageTransaction : IMessageTransaction
    {
        readonly Func<Task> onCommit;
        readonly Func<Task> onFail;

        public RedisMessageTransaction(TransportMessage message, int deliveryCount, Func<Task> onCommit, Func<Task> onFail)
        {
            this.Message = message;
            this.DeliveryCount = deliveryCount;
            this.onCommit = onCommit;
            this.onFail = onFail;
        }

        public TransportMessage Message { get; }

        public int DeliveryCount { get; }

        public Task Commit() => this.onCommit();

        public Task Fail() => this.onFail();
    }
}
