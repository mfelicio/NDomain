using NDomain.Bus;
using NDomain.Model.EventSourcing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    /// <summary>
    /// Implementation of an EventBus on top of the IMessageBus, treating events as an higher level concept for Message objects
    /// </summary>
    public class EventBus : IEventBus, IEventStoreBus
    {
        readonly IMessageBus messageBus;

        public EventBus(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public Task Publish<T>(IEvent<T> @event)
        {
            var message = BuildMessage(@event);
            return this.messageBus.Send(message);
        }

        public Task Publish(IEnumerable<IEvent> events)
        {
            var messages = events.Select(e => BuildMessage(e)).ToArray();
            return this.messageBus.Send(messages);
        }

        public Task Publish(IAggregateEvent<JObject> @event)
        {
            var message = BuildMessage(@event);
            return this.messageBus.Send(message);
        }

        public Task Publish(IEnumerable<IAggregateEvent<JObject>> events)
        {
            var messages = events.Select(e => BuildMessage(e)).ToArray();
            return this.messageBus.Send(messages);
        }

        private Message BuildMessage(IEvent ev)
        {
            var headers = new Dictionary<string, string> 
            {
                { CqrsMessageHeaders.DateUtc, ev.DateUtc.ToBinary().ToString() }
            };

            var message = new Message(ev.Payload, ev.Name, headers);
            return message;
        }

        private Message BuildMessage(IAggregateEvent ev)
        {
            var headers = new Dictionary<string, string>
            {
                { CqrsMessageHeaders.DateUtc, ev.DateUtc.ToBinary().ToString() },
                { CqrsMessageHeaders.AggregateId, ev.AggregateId },
                { CqrsMessageHeaders.SequenceId, ev.SequenceId.ToString() }
            };

            var message = new Message(ev.Payload, ev.Name, headers);
            return message;
        }
    }
}
