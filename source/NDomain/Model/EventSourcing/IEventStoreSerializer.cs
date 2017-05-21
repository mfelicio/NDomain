using Newtonsoft.Json.Linq;

namespace NDomain.Model.EventSourcing
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
