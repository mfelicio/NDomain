using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions.Redis
{

    public class RedisSubscriptionBroker : ISubscriptionBroker
    {
        readonly ConnectionMultiplexer connection;
        readonly string subscriptionChannel;

        readonly ConcurrentBag<Action<SubscriptionChange, Subscription>> handlers;

        readonly Lazy<Task> initializationTask;

        public RedisSubscriptionBroker(ConnectionMultiplexer connection, string prefix)
        {
            this.connection = connection;
            this.subscriptionChannel = string.Format("{0}.subscriptions.broker", prefix);
            this.handlers = new ConcurrentBag<Action<SubscriptionChange, Subscription>>();
            this.initializationTask = new Lazy<Task>(() => this.Initialize());
        }

        public async Task NotifyChange(SubscriptionChange changeType, Subscription subscription)
        {
            var redis = this.connection.GetDatabase();

            var message = string.Format("{0}|{1}", (int)changeType, subscription.Id);
            await redis.PublishAsync(this.subscriptionChannel, message);
        }

        public async Task SubscribeChangeNotifications(Action<SubscriptionChange, Subscription> handler)
        {
            this.handlers.Add(handler);

            await this.initializationTask.Value; // lazy initialization, ensures only one thread runs the initialize function
        }

        private async Task Initialize()
        {
            // first attempt
            var subscriber = this.connection.GetSubscriber();

            await subscriber.SubscribeAsync(this.subscriptionChannel, (channel, message) => OnSubscriptionChanged(message));
        }

        static readonly char[] Separator = new char[] { '|' };
        private void OnSubscriptionChanged(string message)
        {
            var parts = message.Split(Separator, 2);

            var change = (SubscriptionChange)int.Parse(parts[0]);
            var subscriptionId = parts[1];

            var subscription = Subscription.FromId(subscriptionId);

            foreach (var handler in this.handlers)
            {
                var h = handler;
                Task.Run(() => h(change, subscription));
            }
        }

    }
}
