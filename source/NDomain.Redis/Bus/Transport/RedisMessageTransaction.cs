using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Redis
{
    public class RedisMessageTransaction : IMessageTransaction
    {
        readonly TransportMessage message;
        readonly int deliveryCount;
        readonly Func<Task> onCommit;
        readonly Func<Task> onFail;

        public RedisMessageTransaction(TransportMessage message, int deliveryCount, Func<Task> onCommit, Func<Task> onFail)
        {
            this.message = message;
            this.deliveryCount = deliveryCount;
            this.onCommit = onCommit;
            this.onFail = onFail;
        }

        public TransportMessage Message
        {
            get { return this.message; }
        }

        public int DeliveryCount
        {
            get { return this.deliveryCount; }
        }

        public Task Commit()
        {
            return this.onCommit();
        }

        public Task Fail()
        {
            return this.onFail();
        }
    }
}
