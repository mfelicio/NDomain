using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public interface IMessageHandler
    {
        Task Process(MessageContext message);
    }
}
