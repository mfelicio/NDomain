using NDomain.Bus.Transport;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Azure.Queues
{
    class QueueMessageTransaction : IMessageTransaction
    {
        readonly TransportMessage message;
        readonly int retryCount;
        readonly Func<Task> onCommit;
        readonly Func<Task> onFail;

        public QueueMessageTransaction(TransportMessage message, int retryCount, Func<Task> onCommit, Func<Task> onFail)
        {
            this.message = message;
            this.retryCount = retryCount;
            this.onCommit = onCommit;
            this.onFail = onFail;
        }

        public TransportMessage Message
        {
            get { return this.message; }
        }

        public int DeliveryCount
        {
            get { return this.retryCount; }
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
