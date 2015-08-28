using NDomain.IoC;
using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public class MessageContext
    {
        public MessageContext(TransportMessage message, IDependencyResolver resolver)
        {
            this.Message = message;
            this.Resolver = resolver;
        }

        public TransportMessage Message { get; private set; }
        public IDependencyResolver Resolver { get; private set; }
    }
}
