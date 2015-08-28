using NDomain.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public class CommandBus : ICommandBus
    {
        readonly IMessageBus messageBus;

        public CommandBus(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public Task Send(ICommand command)
        {
            var message = BuildMessage(command);
            return messageBus.Send(message);
        }

        public Task Send<T>(ICommand<T> command)
        {
            var message = BuildMessage(command);
            return messageBus.Send(message);
        }

        private Message BuildMessage(ICommand command)
        {
            var headers = new Dictionary<string, string>
            {
                { Bus.MessageHeaders.Id, command.Id },
            };

            var message = new Message(command.Payload, command.Name, headers);
            return message;
        }
    }
}
