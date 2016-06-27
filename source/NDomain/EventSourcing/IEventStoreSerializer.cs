using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.EventSourcing
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
