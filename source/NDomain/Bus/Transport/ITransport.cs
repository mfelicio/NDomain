using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    /// <summary>
    /// Represents a message transport.
    /// </summary>
    public interface ITransport
    {
        //bool IsBrokered { get; }
    }

    /// <summary>
    /// Represents a message transport where messages can be received from
    /// </summary>
    public interface IInboundTransport : ITransport
    {
        Task<IMessageTransaction> Receive(TimeSpan? timeout = null);
    }

    /// <summary>
    /// Represents a message transport where messages can be sent to
    /// </summary>
    public interface IOutboundTransport : ITransport
    {
        Task Send(TransportMessage message);
        Task SendMultiple(IEnumerable<TransportMessage> messages);
    }
}
