using NDomain.IoC;
using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    /// <summary>
    /// Context a message being processed.
    /// </summary>
    public class MessageContext
    {
        public MessageContext(TransportMessage message, IDependencyResolver resolver)
        {
            this.Message = message;
            this.Resolver = resolver;
        }

        /// <summary>
        /// Gets the original TransportMessage
        /// </summary>
        public TransportMessage Message { get; private set; }

        /// <summary>
        /// Gets the IDependencyResolver.
        /// </summary>
        /// <remarks>This is useful to resolve custom message handlers</remarks>
        public IDependencyResolver Resolver { get; private set; }
    }
}
