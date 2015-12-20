using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    /// <summary>
    /// Represents a transaction for a received message that is processed and can either fail or succeed
    /// </summary>
    public interface IMessageTransaction
    {
        /// <summary>
        /// Received message
        /// </summary>
        TransportMessage Message { get; }

        /// <summary>
        /// Indicates the number of times this message has been retried. 
        /// The first time a message is received this value is 0.
        /// Whenever a message fails to be processed, it is retried later and its RetryCount is incremented.
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        /// Signals this transaction that the message was processed successfully.
        /// </summary>
        /// <returns>Task</returns>
        Task Commit();

        /// <summary>
        /// Signals this transaction that there were errors processing the message, so it should be retried later.
        /// </summary>
        /// <returns>Task</returns>
        Task Fail();
    }
}
