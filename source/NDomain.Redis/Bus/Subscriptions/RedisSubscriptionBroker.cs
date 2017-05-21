using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NDomain.Bus.Subscriptions;
using StackExchange.Redis;

namespace NDomain.Redis.Bus.Subscriptions
{
    public class RedisSubscriptionBroker : ISubscriptionBroker
    {
        private readonly ConnectionMultiplexer connection;
        private readonly string subscriptionChannel;

        private readonly ConcurrentBag<Action<SubscriptionChange, Subscription>> handlers;

        private readonly Lazy<Task> initializationTask;

        public RedisSubscriptionBroker(ConnectionMultiplexer connection, string prefix)
        {
            this.connection = connection;
            this.subscriptionChannel = $"{prefix}.subscriptions.broker";
            this.handlers = new ConcurrentBag<Action<SubscriptionChange, Subscription>>();
            this.initializationTask = new Lazy<Task>(this.Initialize);
        }

        public async Task NotifyChange(SubscriptionChange changeType, Subscription subscription)
        {
            var redis = this.connection.GetDatabase();

            var message = $"{(int) changeType}|{subscription.Id}";
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

        private static readonly char[] Separator = new char[] { '|' };
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
