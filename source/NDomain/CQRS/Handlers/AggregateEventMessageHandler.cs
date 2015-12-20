using NDomain.Bus;
using NDomain.IoC;
using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Handlers
{
    /// <summary>
    /// MessageHandler for IAggregateEvent messages
    /// </summary>
    /// <typeparam name="T">Type of the payload of the IAggregateEvent message</typeparam>
    /// <typeparam name="THandler">Type of the actual handler subscribed from the application</typeparam>
    public class AggregateEventMessageHandler<T, THandler> : MessageHandlerBase<IAggregateEvent<T>, THandler>
        where THandler : class
    {
        public AggregateEventMessageHandler(Func<THandler, IAggregateEvent<T>, Task> handlerFunc, 
                                            THandler instance = null)
            :base(handlerFunc, instance)
        {
        }

        protected override IAggregateEvent<T> CreateMessage(TransportMessage message)
        {
            return new AggregateEvent<T>(
                        message.Headers[CqrsMessageHeaders.AggregateId],
                        int.Parse(message.Headers[CqrsMessageHeaders.SequenceId]),
                        DateTime.FromBinary(long.Parse(message.Headers[CqrsMessageHeaders.DateUtc])),
                        message.Body.ToObject<T>());
        }
    }
}
