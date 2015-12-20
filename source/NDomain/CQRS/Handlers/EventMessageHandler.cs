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
    /// <summary>
    /// MessageHandler for IEvent messages
    /// </summary>
    /// <typeparam name="T">Type of the payload of the IEvent message</typeparam>
    /// <typeparam name="THandler">Type of the actual handler subscribed from the application</typeparam>
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
