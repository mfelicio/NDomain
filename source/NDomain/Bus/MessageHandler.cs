using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus
{
    public class MessageHandler<TMessage> : IMessageHandler
    {
        readonly Func<TMessage, Task> handlerFunc;

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
