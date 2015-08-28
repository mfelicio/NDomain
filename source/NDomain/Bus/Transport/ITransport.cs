using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    public interface ITransport
    {
        //bool IsBrokered { get; }
    }

    public interface IInboundTransport : ITransport
    {
        Task<IMessageTransaction> Receive(TimeSpan? timeout = null);
    }

    public interface IOutboundTransport : ITransport
    {
        Task Send(TransportMessage message);
        Task SendMultiple(IEnumerable<TransportMessage> messages);
    }
}
