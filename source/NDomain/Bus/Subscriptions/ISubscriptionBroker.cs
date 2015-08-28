using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    public interface ISubscriptionBroker
    {
        Task NotifyChange(SubscriptionChange changeType, Subscription subscription);
        Task SubscribeChangeNotifications(Action<SubscriptionChange, Subscription> handler);
    }
}
