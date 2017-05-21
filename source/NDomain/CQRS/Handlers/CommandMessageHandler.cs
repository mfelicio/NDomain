using NDomain.Bus.Transport;
using System;
using System.Threading.Tasks;

namespace NDomain.CQRS.Handlers
{
    /// <summary>
    /// MessageHandler for ICommand messages
    /// </summary>
    /// <typeparam name="T">Type of the payload of the ICommand message</typeparam>
    /// <typeparam name="THandler">Type of the actual handler subscribed from the application</typeparam>
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
