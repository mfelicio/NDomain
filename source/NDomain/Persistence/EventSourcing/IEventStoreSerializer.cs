using NDomain.Model;
using Newtonsoft.Json.Linq;

namespace NDomain.Persistence.EventSourcing
{
    /// <summary>
    /// Serializes and deserializes events persisted in the IEventStore
    /// </summary>
    public interface IEventStoreSerializer
    {
        IAggregateEvent<JObject> Serialize(IAggregateEvent @event);
        IAggregateEvent Deserialize(IAggregateEvent<JObject> @event);
    }
}
