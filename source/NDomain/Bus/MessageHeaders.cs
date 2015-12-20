using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    /// <summary>
    /// Contains headers used internally when sending and receiving messages
    /// </summary>
    public class MessageHeaders
    {
        /// <summary>
        /// Identifies the ID of the message.
        /// </summary>
        public const string Id = "ndomain.id";

        /// <summary>
        /// Identifies the endpoint of the destination of the message. 
        /// In queue based endpoints, it will be the name of the queue.
        /// </summary>
        public const string Endpoint = "ndomain.endpoint";

        /// <summary>
        /// Identifies the component that handles the message once it arrives at its destination.
        /// Typically this is the name of a message handler.
        /// </summary>
        public const string Component = "ndomain.component";
    }
}
