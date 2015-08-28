using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NDomain
{
    public class DomainTransactionScope : IDisposable
    {
        public DomainTransactionScope(string transactionId, int retryCount = 0)
        {
            CallContext.LogicalSetData("ndomain:transaction", new DomainTransaction(transactionId, retryCount));
        }

        public void Dispose()
        {
            if (DomainTransaction.Current != null)
            {
                CallContext.LogicalSetData("ndomain:transaction", null);
            }
        }
    }

    public class DomainTransaction
    {
        readonly string id;
        readonly int retryCount;

        internal DomainTransaction(string id, int retryCount)
        {
            this.id = id;
            this.retryCount = retryCount;
        }

        public string Id { get { return this.id; } }
        public int RetryCount { get { return this.retryCount; } }

        public static DomainTransaction Current
        {
            get
            {
                return CallContext.LogicalGetData("cqrs:transaction") as DomainTransaction;
            }
        }
    }
}
