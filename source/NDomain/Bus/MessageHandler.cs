using System;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    /// <summary>
    /// Default implementation for a message handler that delegates the handling to a Func it receives
    /// </summary>
    /// <typeparam name="TMessage">Type of the message to handle</typeparam>
    public class MessageHandler<TMessage> : IMessageHandler
    {
        private readonly Func<TMessage, Task> handlerFunc;

        public MessageHandler(Func<TMessage, Task> handlerFunc)
        {
            this.handlerFunc = handlerFunc;
        }

        public Task Process(MessageContext context)
        {
            var message = context.Message.Body.ToObject<TMessage>();
            return handlerFunc(message);
        }
    }
}
