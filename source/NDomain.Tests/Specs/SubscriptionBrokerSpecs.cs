using NDomain.Bus.Subscriptions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NDomain.Tests.Specs
{
    public abstract class SubscriptionBrokerSpecs
    {
        protected ISubscriptionBroker broker;

        protected abstract ISubscriptionBroker CreateSubscriptionBroker();

        protected virtual void OnSetUp() { }
        protected virtual void OnTearDown() { }

        [SetUp]
        public void Setup()
        {
            this.broker = CreateSubscriptionBroker();

            this.OnSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            this.OnTearDown();
        }

        [Test]
        public async Task CanNotifyChanges()
        {
            // arrange
            var subscription1 = new Subscription("t1", "e1", "c1");
            var subscription2 = new Subscription("t2", "e2", "c2");
            var subscription3 = new Subscription("t3", "e3", "c3");

            // will be published, should be received
            var notifications = new [] {
                new Tuple<SubscriptionChange, Subscription>(SubscriptionChange.Add, subscription1),
                new Tuple<SubscriptionChange, Subscription>(SubscriptionChange.Remove, subscription2),
                new Tuple<SubscriptionChange, Subscription>(SubscriptionChange.Add, subscription3),
                new Tuple<SubscriptionChange, Subscription>(SubscriptionChange.Remove, subscription1)
            };
            var pending = notifications.ToList();

            var done = new TaskCompletionSource<bool>();
            var expectedNotificationsReceived = pending.Count;
            int receivedNotifications = 0;

            // subscribe
            await this.broker.SubscribeChangeNotifications(
                (c, s) =>
                {
                    lock (pending)
                    {
                        // remove from pending list after we receive it, 
                        // to ensure we don't process the same notification twice
                        var item = pending.Single(n => n.Item1 == c && n.Item2.Equals(s));
                        pending.Remove(item);

                        if (Interlocked.Increment(ref receivedNotifications) == expectedNotificationsReceived)
                        {
                            done.SetResult(true);
                        }
                    }
                });

            // act
            foreach (var notification in notifications)
            {
                await this.broker.NotifyChange(notification.Item1, notification.Item2);
            }

            // awaits either for timeout or receivers completion
            await Task.WhenAny(done.Task, Task.Delay(TimeSpan.FromSeconds(2))); 

            // assert
            Assert.AreEqual(TaskStatus.RanToCompletion, done.Task.Status);
        }

        [TestCase(5, 10)]
        [TestCase(5, 1)]
        [TestCase(1, 10)]
        [TestCase(1, 1)]
        public async Task CanReceiveNotifications(int nSubscribers, int nSubscriptions)
        {
            // arrange
            var done = new TaskCompletionSource<bool>();
            var subscription = new Subscription("topic1", "endpoint1", "component1");

            int expectedNotificationsToBeReceived = nSubscribers * nSubscriptions;
            int notificationsReceived = 0;

            for (var i = 0; i < nSubscribers; ++i)
            {
                await this.broker.SubscribeChangeNotifications(
                    (c, s) =>
                    {
                        if (s.Equals(subscription))
                        {
                            if (Interlocked.Increment(ref notificationsReceived) == expectedNotificationsToBeReceived)
                            {
                                done.SetResult(true);
                            }
                        }
                    });
            }

            // act
            // publish the changes
            Parallel.For(0, nSubscriptions, 
                async i =>
                {
                    await this.broker.NotifyChange(SubscriptionChange.Add, subscription);
                });

            // awaits either for timeout or receivers completion
            await Task.WhenAny(done.Task, Task.Delay(TimeSpan.FromSeconds(2))); 

            // assert
            Assert.AreEqual(TaskStatus.RanToCompletion, done.Task.Status);
        }
    }
}
