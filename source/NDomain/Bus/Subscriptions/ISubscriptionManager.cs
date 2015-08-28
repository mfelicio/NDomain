using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    public interface ISubscriptionManager
    {
        Task<IEnumerable<Subscription>> GetSubscriptions(string topic);

        Task UpdateEndpointSubscriptions(string endpoint, IEnumerable<Subscription> subscriptions);
    }
}
