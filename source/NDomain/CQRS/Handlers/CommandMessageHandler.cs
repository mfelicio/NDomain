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
    public class CommandMessageHandler<T, THandler> : MessageHandlerBase<ICommand<T>, THandler>
        where THandler : class
    {
        public CommandMessageHandler(Func<THandler, ICommand<T>, Task> handlerFunc, 
                                     THandler instance = null)
            : base(handlerFunc, instance)
        {
        }

        protected override ICommand<T> CreateMessage(TransportMessage message)
        {
            return new Command<T>(message.Id, message.Body.ToObject<T>());
        }
    }
}
