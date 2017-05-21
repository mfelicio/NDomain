using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NDomain.Model.EventSourcing
{
    public interface IEventStoreBus
    {
        Task Publish(IAggregateEvent<JObject> @event);
        Task Publish(IEnumerable<IAggregateEvent<JObject>> events);
    }
}
