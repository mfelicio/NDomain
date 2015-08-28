using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public interface IMessageDispatcher
    {
        Task ProcessMessage(TransportMessage message);
    }
}
