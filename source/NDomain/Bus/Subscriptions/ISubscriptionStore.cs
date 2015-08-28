using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    public interface ISubscriptionStore
    {
        Task<IEnumerable<Subscription>> GetAll();
        Task<IEnumerable<Subscription>> GetByTopic(string topic);
        Task<IEnumerable<Subscription>> GetByEndpoint(string endpoint);

        Task<bool> AddSubscription(Subscription subscription);
        Task<bool> RemoveSubscription(Subscription subscription);
    }
}
