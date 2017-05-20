using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Model.EventSourcing
{
    public interface IEventStoreBus
    {
        Task Publish(IAggregateEvent<JObject> @event);
        Task Publish(IEnumerable<IAggregateEvent<JObject>> events);
    }
}
