using NDomain.Bus.Transport;
using System;
using System.Threading.Tasks;

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
