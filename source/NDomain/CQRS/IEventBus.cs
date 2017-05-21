using System.Collections.Generic;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    /// <summary>
    /// Publishes event messages in the bus
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event message
        /// </summary>
        /// <typeparam name="T">Type of the event payload</typeparam>
        /// <param name="event">event to publish</param>
        /// <returns>Task</returns>
        Task Publish<T>(IEvent<T> @event);

        /// <summary>
        /// Publishes multiple IEvent messages in a batch, atomically.
        /// </summary>
        /// <param name="events">events to publish</param>
        /// <returns>Task</returns>
        Task Publish(IEnumerable<IEvent> events);
    }
}
