using System.Collections.Generic;
using System.Threading.Tasks;
using NDomain.Model;
using Newtonsoft.Json.Linq;

namespace NDomain.Persistence.EventSourcing
{
    public interface IEventStoreBus
    {
        Task Publish(IAggregateEvent<JObject> @event);
        Task Publish(IEnumerable<IAggregateEvent<JObject>> events);
    }
}
