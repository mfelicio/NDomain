using NDomain.Bus.Subscriptions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions.Redis
{
    public class RedisSubscriptionStore : ISubscriptionStore
    {
        readonly ConnectionMultiplexer connection;
        readonly string storeKeyPrefix;

        public RedisSubscriptionStore(ConnectionMultiplexer connection, string storeKeyPrefix)
        {
            this.connection = connection;
            this.storeKeyPrefix = storeKeyPrefix;
        }

        public Task<IEnumerable<Subscription>> GetAll()
        {
            var set = GetAllSubscriptionsKey();

            return GetSubscriptionsInSet(set);
        }

        public Task<IEnumerable<Subscription>> GetByTopic(string topic)
        {
            var set = GetTopicKey(topic);

            return GetSubscriptionsInSet(set);
        }

        public Task<IEnumerable<Subscription>> GetByEndpoint(string endpoint)
        {
            var set = GetEndpointKey(endpoint);
         
            return GetSubscriptionsInSet(set);
        }

        private async Task<IEnumerable<Subscription>> GetSubscriptionsInSet(string set)
        {
            var redis = this.connection.GetDatabase();

            var allSubscriptions = await redis.SetMembersAsync(set);

            var subscriptions = allSubscriptions.Select(subscriptionId => Subscription.FromId(subscriptionId)).ToArray();
            return subscriptions;
        }

        public async Task<bool> AddSubscription(Subscription subscription)
        {
            var redis = this.connection.GetDatabase();

            var subscriptionKey = GetSubscriptionKey(subscription); // to detect concurrent updates

            var topicKey = GetTopicKey(subscription.Topic);
            var endpointKey = GetEndpointKey(subscription.Endpoint);
            var allSubscriptionsKey = GetAllSubscriptionsKey();
            
            var transaction = redis.CreateTransaction();

            // uses subscriptionId key for optimistic concurrency
            // if another client happens to adding this key concurrently, only the first one will publish changes
            transaction.AddCondition(Condition.KeyNotExists(subscriptionKey));

            transaction.SetAddAsync(topicKey, subscription.Id);
            transaction.SetAddAsync(endpointKey, subscription.Id);
            transaction.SetAddAsync(allSubscriptionsKey, subscription.Id);
            transaction.StringSetAsync(subscriptionKey, ""); // don't really need to have a value here, it's just for optimistic concurrency, it either exists or not

            var success = await transaction.ExecuteAsync();
            return success;
        }

        public async Task<bool> RemoveSubscription(Subscription subscription)
        {
            var redis = this.connection.GetDatabase();

            var subscriptionKey = GetSubscriptionKey(subscription); // to detect concurrent updates

            var topicKey = GetTopicKey(subscription.Topic);
            var endpointKey = GetEndpointKey(subscription.Endpoint);
            var allSubscriptionsKey = GetAllSubscriptionsKey();

            var transaction = redis.CreateTransaction();

            // uses subscriptionId key for optimistic concurrency
            // if another client happens to be removing this key concurrently, only the first one will publish changes
            transaction.AddCondition(Condition.KeyExists(subscriptionKey));

            transaction.SetRemoveAsync(topicKey, subscription.Id);
            transaction.SetRemoveAsync(endpointKey, subscription.Id);
            transaction.SetRemoveAsync(allSubscriptionsKey, subscription.Id);
            transaction.KeyDeleteAsync(subscriptionKey);

            var success = await transaction.ExecuteAsync();
            return success;
        }

        private string GetSubscriptionKey(Subscription subscription)
        {
            return string.Format("{0}.subscriptions.{1}", this.storeKeyPrefix, subscription.Id);
        }

        private string GetTopicKey(string topic)
        {
            return string.Format("{0}.subscriptions.topic.{1}", this.storeKeyPrefix, topic);
        }

        private string GetEndpointKey(string endpoint)
        {
            return string.Format("{0}.subscriptions.endpoint.{1}", this.storeKeyPrefix, endpoint);
        }

        private string GetAllSubscriptionsKey()
        {
            return string.Format("{0}.subscriptions", this.storeKeyPrefix);
        }
    }
}
