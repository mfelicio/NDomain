using NDomain.Bus.Transport;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    /// <summary>
    /// Responsible for processing an incoming message, which involves finding its destination subscription, handler and invoking it.
    /// </summary>
    public interface IMessageDispatcher
    {
        Task ProcessMessage(TransportMessage message);
    }
}
