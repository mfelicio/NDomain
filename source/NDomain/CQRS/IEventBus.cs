using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public interface IEventBus
    {
        Task Publish<T>(IEvent<T> @event);
        Task Publish(IEnumerable<IEvent> events);
    }
}
