using NDomain.Bus.Transport;
using NDomain.CQRS;
using NDomain.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public interface IMessageBus
    {
        Task Send(Message message);
        Task Send(IEnumerable<Message> messages);
    }

    public static class MessageBusExtensions
    {
        public static Task Send<TMessage>(this IMessageBus bus, TMessage message, Dictionary<string, string> headers = null)
        {
            var msg = new Message(message, typeof(TMessage).Name, headers);
            return bus.Send(msg);
        }
    }
}
