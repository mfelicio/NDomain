using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    /// <summary>
    /// Factory that creates transports that are capable of sending and receiving messages
    /// </summary>
    public interface ITransportFactory
    {
        /// <summary>
        /// Creates an inbound transport for the specific endpoint
        /// </summary>
        /// <returns></returns>
        IInboundTransport CreateInboundTransport(InboundTransportOptions options);

        /// <summary>
        /// Creates an outbound transport
        /// </summary>
        /// <returns></returns>
        IOutboundTransport CreateOutboundTransport();
    }
}
