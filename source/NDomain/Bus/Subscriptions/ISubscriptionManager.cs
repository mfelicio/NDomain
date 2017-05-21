using System.Collections.Generic;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// Manages subscriptions for a given endpoint
    /// </summary>
    public interface ISubscriptionManager
    {
        Task<IEnumerable<Subscription>> GetSubscriptions(string topic);

        /// <summary>
        /// Changes to the endpoint's subscriptions will be broadcasted to other processes via ISubscriptionBroker
        /// </summary>
        /// <param name="endpoint">endpoint</param>
        /// <param name="subscriptions">list of subscriptions</param>
        /// <returns>Task</returns>
        Task UpdateEndpointSubscriptions(string endpoint, IEnumerable<Subscription> subscriptions);
    }
}
