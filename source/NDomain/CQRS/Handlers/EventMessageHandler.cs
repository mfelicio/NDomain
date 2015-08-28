using NDomain.Bus;
using NDomain.IoC;
using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.CQRS;

namespace NDomain.CQRS.Handlers
{
    public class EventMessageHandler<T, THandler> : MessageHandlerBase<IEvent<T>, THandler>
        where THandler : class
    {
        public EventMessageHandler(Func<THandler, IEvent<T>, Task> handlerFunc,
                                   THandler instance = null)
            :base(handlerFunc, instance)
        {
            
        }

        protected override IEvent<T> CreateMessage(TransportMessage message)
        {
            return new Event<T>(
                        DateTime.FromBinary(long.Parse(message.Headers[CqrsMessageHeaders.DateUtc])), 
                        message.Body.ToObject<T>());
        }
    }
}
