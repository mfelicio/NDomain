using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// Communication broker to send and receive notifications about added or removed subscriptions
    /// </summary>
    public interface ISubscriptionBroker
    {
        Task NotifyChange(SubscriptionChange changeType, Subscription subscription);
        Task SubscribeChangeNotifications(Action<SubscriptionChange, Subscription> handler);
    }
}
