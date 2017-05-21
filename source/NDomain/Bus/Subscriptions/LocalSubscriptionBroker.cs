using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// In-process subscription broker, simply loops back whatever comes
    /// </summary>
    public class LocalSubscriptionBroker : ISubscriptionBroker
    {
        private readonly ConcurrentBag<Action<SubscriptionChange, Subscription>> handlers;

        public LocalSubscriptionBroker()
        {
            this.handlers = new ConcurrentBag<Action<SubscriptionChange, Subscription>>();
        }

        public Task NotifyChange(SubscriptionChange changeType, Subscription subscription)
        {
            foreach (var handler in handlers)
            {
                NotifyHandler(handler, changeType, subscription);
            }

            return Task.FromResult(1);
        }

        public Task SubscribeChangeNotifications(Action<SubscriptionChange, Subscription> handler)
        {
            this.handlers.Add(handler);
            
            return Task.FromResult(1);
        }

        private void NotifyHandler(Action<SubscriptionChange, Subscription> handler, 
                                   SubscriptionChange change, 
                                   Subscription subscription)
        {
            handler(change, subscription);
        }
    }
}
