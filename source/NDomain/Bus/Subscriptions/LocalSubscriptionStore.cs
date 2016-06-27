using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// In-process subscription store
    /// </summary>
    public class LocalSubscriptionStore : ISubscriptionStore
    {
        readonly HashSet<Subscription> subscriptions;

        readonly object subscriptionsLock = new object();

        public LocalSubscriptionStore()
        {
            this.subscriptions = new HashSet<Subscription>();
        }

        public Task<IEnumerable<Subscription>> GetAll()
        {
            lock (this.subscriptionsLock)
            {
                IEnumerable<Subscription> result = this.subscriptions.ToArray();
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Subscription>> GetByTopic(string topic)
        {
            lock (this.subscriptionsLock)
            {
                IEnumerable<Subscription> result = (from subscription in this.subscriptions
                                                    where subscription.Topic == topic
                                                    select subscription).ToArray();

                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Subscription>> GetByEndpoint(string endpoint)
        {
            lock (this.subscriptionsLock)
            {
                IEnumerable<Subscription> result = (from subscription in this.subscriptions
                                                    where subscription.Endpoint == endpoint
                                                    select subscription).ToArray();

                return Task.FromResult(result);
            }
        }

        public Task<bool> AddSubscription(Subscription subscription)
        {
            lock (this.subscriptionsLock)
            {
                if (!this.subscriptions.Contains(subscription))
                {
                    this.subscriptions.Add(subscription);
                    
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        public Task<bool> RemoveSubscription(Subscription subscription)
        {
            lock (this.subscriptionsLock)
            {
                if (this.subscriptions.Contains(subscription))
                {
                    this.subscriptions.Remove(subscription);

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
    }
}
