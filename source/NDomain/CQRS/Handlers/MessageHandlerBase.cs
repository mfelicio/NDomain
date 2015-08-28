using NDomain.Bus;
using NDomain.Bus.Transport;
using NDomain.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Handlers
{
    public abstract class MessageHandlerBase<TMessage, THandler> : IMessageHandler
        where THandler : class
    {
        readonly Func<THandler, TMessage, Task> handlerFunc;
        readonly THandler instance;

        protected MessageHandlerBase(Func<THandler, TMessage, Task> handlerFunc, 
                                     THandler instance = null)
        {
            this.handlerFunc = handlerFunc;
            this.instance = instance;
        }

        public async Task Process(MessageContext context)
        {
            using (var scope = context.Resolver.BeginScope())
            {
                var handler = this.instance ?? CreateInstance(scope);
                var message = CreateMessage(context.Message);

                await this.handlerFunc(handler, message);
            }
        }

        private THandler CreateInstance(IDependencyScope scope)
        {
            return (THandler)scope.Resolve(typeof(THandler));
        }

        protected virtual TMessage CreateMessage(TransportMessage message)
        {
            return message.Body.ToObject<TMessage>();
        }
    }
}
