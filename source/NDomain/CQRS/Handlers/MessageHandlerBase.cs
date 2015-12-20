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
    /// <summary>
    /// Base message handler class used internally by CQRS specific handlers
    /// </summary>
    /// <typeparam name="TMessage">Type of the message to handle</typeparam>
    /// <typeparam name="THandler">Type of the actual message handler class subscribed from the application</typeparam>
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

        /// <summary>
        /// Processes a message within a dependency scope, resolving the actual message handler class with an IoC container.
        /// </summary>
        /// <param name="context">message context</param>
        /// <returns>Task completed when the message processing completes</returns>
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
