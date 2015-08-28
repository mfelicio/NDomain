using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport
{
    public interface IMessageTransaction
    {
        TransportMessage Message { get; }
        int RetryCount { get; }

        Task Commit();
        Task Fail();
    }
}
