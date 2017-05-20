using NDomain.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Model.EventSourcing
{
    public class EventStoreSerializer : IEventStoreSerializer
    {
        readonly Dictionary<string, Func<IAggregateEvent, IAggregateEvent<JObject>>> serializers;
        readonly Dictionary<string, Func<IAggregateEvent<JObject>, IAggregateEvent>> deserializers;

        public EventStoreSerializer(IEnumerable<Type> knownEventTypes)
        {
            this.serializers = new Dictionary<string, Func<IAggregateEvent, IAggregateEvent<JObject>>>();
            this.deserializers = new Dictionary<string, Func<IAggregateEvent<JObject>, IAggregateEvent>>();

            foreach (var type in knownEventTypes.Distinct())
            {
                typeof(EventStoreSerializer).GetMethod("Add", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                                            .MakeGenericMethod(type)
                                            .Invoke(this, null);
            }
        }

        private void Add<TEvent>()
        {
            var name = typeof(TEvent).Name; // TODO: friendly name

            this.serializers[name] = e => new AggregateEvent<JObject>(
                                                e.AggregateId,
                                                e.SequenceId,
                                                e.DateUtc,
                                                e.Name,
                                                JObject.FromObject(e.Payload));

            this.deserializers[name] = e => new AggregateEvent<TEvent>(
                                                e.AggregateId,
                                                e.SequenceId,
                                                e.DateUtc,
                                                e.Payload.ToObject<TEvent>());
        }

        public IAggregateEvent<JObject> Serialize(IAggregateEvent @event)
        {
            var serializer = this.serializers[@event.Name];
            return serializer(@event);
        }

        public IAggregateEvent Deserialize(IAggregateEvent<JObject> @event)
        {
            var deserializer = this.deserializers[@event.Name];
            return deserializer(@event);
        }


        public static EventStoreSerializer FromAggregateTypes(params Type[] aggregateTypes)
        {
            return FromAggregateTypes(aggregateTypes.AsEnumerable());
        }

        public static EventStoreSerializer FromAggregateTypes(IEnumerable<Type> aggregateTypes)
        {
            var eventTypes = aggregateTypes.SelectMany(t => ReflectionUtils.FindEventTypes(t))
                                           .Distinct();

            return new EventStoreSerializer(eventTypes);
        }
    }
}
