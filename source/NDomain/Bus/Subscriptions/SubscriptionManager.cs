using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// SubscriptionManager is intended to use SubscriptionStore to store and cache subscriptions
    /// </summary>
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ISubscriptionStore store;
        private readonly ISubscriptionBroker broker;
        private readonly Dictionary<string, SubscriptionSet> cache;
        private readonly object cacheLock;
        private readonly Lazy<Task> initializationTask;

        public SubscriptionManager(ISubscriptionStore store, ISubscriptionBroker broker)
        {
            this.store = store;
            this.broker = broker;
            this.cache = new Dictionary<string, SubscriptionSet>();
            this.cacheLock = new object();
            this.initializationTask = new Lazy<Task>(Initialize);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptions(string topic)
        {
            // lazy initialization, ensures only one thread runs the initialize function
            await this.initializationTask.Value; 

            lock (this.cacheLock)
            {
                var set = this.cache.GetOrAdd(topic, t => new SubscriptionSet());
                return set.Subscriptions;
            }
        }

        public async Task UpdateEndpointSubscriptions(string endpoint, IEnumerable<Subscription> subscriptions)
        {
            // lazy initialization, ensures only one thread runs the initialize function
            await this.initializationTask.Value;

            var storedSubscriptions = await this.store.GetByEndpoint(endpoint);

            var subscriptionsToAdd = subscriptions.Except(storedSubscriptions);
            var subscriptionsToRemove = storedSubscriptions.Except(subscriptions);

            var tasks = new List<Task>();

            foreach (var subscription in subscriptionsToAdd)
            {
                tasks.Add(this.AddSubscription(subscription));
            }

            foreach (var subscription in subscriptionsToRemove)
            {
                tasks.Add(this.RemoveSubscription(subscription));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks.ToArray());
            }
        }

        private async Task AddSubscription(Subscription subscription)
        {
            var success = await this.store.AddSubscription(subscription);
            if (success)
            {
                // update current cache without waiting for broker notification to be received
                OnSubscriptionAdded(subscription);

                //notify other processes of this change
                await this.broker.NotifyChange(SubscriptionChange.Add, subscription);
            }
        }

        private async Task RemoveSubscription(Subscription subscription)
        {
            var success = await this.store.RemoveSubscription(subscription);
            if (success)
            {
                // update current cache
                OnSubscriptionRemoved(subscription);

                //notify other processes of this change
                await this.broker.NotifyChange(SubscriptionChange.Remove, subscription);
            }
        }

        private async Task Initialize()
        {
            await this.broker.SubscribeChangeNotifications(this.OnSubscriptionChanged);

            var subscriptions = await this.store.GetAll();

            lock (this.cacheLock)
            {
                SubscriptionSet set;
                foreach (var subscription in subscriptions)
                {
                    set = cache.GetOrAdd(subscription.Topic, s => new SubscriptionSet());
                    set.Add(subscription);
                }
            }
        }

        private void OnSubscriptionChanged(SubscriptionChange change, Subscription subscription)
        {
            if (change == SubscriptionChange.Add)
            {
                OnSubscriptionAdded(subscription);
            }
            else
            {
                OnSubscriptionRemoved(subscription);
            }
        }

        private void OnSubscriptionAdded(Subscription subscription)
        {
            lock (this.cacheLock)
            {
                var set = this.cache.GetOrAdd(subscription.Topic, s => new SubscriptionSet());
                set.Add(subscription);
            }
        }

        private void OnSubscriptionRemoved(Subscription subscription)
        {
            lock (this.cacheLock)
            {
                var set = this.cache.GetOrAdd(subscription.Topic, s => new SubscriptionSet());
                set.Remove(subscription);
            }
        }
    }
}
