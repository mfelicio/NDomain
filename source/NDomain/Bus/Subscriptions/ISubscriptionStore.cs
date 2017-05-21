using System.Collections.Generic;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// Persistent store for subscriptions
    /// </summary>
    public interface ISubscriptionStore
    {
        Task<IEnumerable<Subscription>> GetAll();
        Task<IEnumerable<Subscription>> GetByTopic(string topic);
        Task<IEnumerable<Subscription>> GetByEndpoint(string endpoint);

        Task<bool> AddSubscription(Subscription subscription);
        Task<bool> RemoveSubscription(Subscription subscription);
    }
}
