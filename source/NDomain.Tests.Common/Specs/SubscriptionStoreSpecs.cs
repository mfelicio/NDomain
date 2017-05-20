using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDomain.Bus.Subscriptions;
using NUnit.Framework;

namespace NDomain.Tests.Common.Specs
{
    /// <summary>
    /// Tests ISubscriptionStore behavior and ensures it can be used concurrently
    /// </summary>
    public abstract class SubscriptionStoreSpecs
    {
        protected ISubscriptionStore store;

        protected abstract ISubscriptionStore CreateSubscriptionStore();

        protected virtual void OnSetUp() { }
        protected virtual void OnTearDown() { }

        [SetUp]
        public void Setup()
        {
            this.store = CreateSubscriptionStore();

            this.OnSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            this.OnTearDown();
        }

        [Test]
        public async Task CanAddSubscription()
        {
            // arrange
            var subscription = new Subscription("topic1", "endpoint1", "handler1");

            //act
            var success = await this.store.AddSubscription(subscription);

            //assert
            Assert.IsTrue(success);
        }

        [Test]
        public async Task CanRemoveSubscription()
        {
            // arrange
            var existingSubscription = new Subscription("topic1", "endpoint1", "handler1");
            await this.store.AddSubscription(existingSubscription);

            //act
            var subscriptionToRemove = new Subscription("topic1", "endpoint1", "handler1"); // duplicating object, to simulate real usage
            var success = await this.store.RemoveSubscription(subscriptionToRemove);

            //assert
            Assert.IsTrue(success);
        }

        [Test]
        public async Task CanGetSubscriptions()
        {
            // setting up subscriptions for 3 endpoints and 3 different topics
            // component names are not important, so all will be dummy values
            var entries = new[] {
                new Subscription("topic1", "endpoint1", "_"),
                new Subscription("topic1", "endpoint2", "_"),
                new Subscription("topic2", "endpoint3", "_"),
                new Subscription("topic3", "endpoint2", "_"),
                new Subscription("topic3", "endpoint1", "_"),
                new Subscription("topic3", "endpoint3", "_"),
            };

            // arrange
            await Task.WhenAll(
                entries.Select(
                    subscription => this.store.AddSubscription(subscription))
                .ToArray());

            // act
            var topic1 = await this.store.GetByTopic("topic1");
            var topic2 = await this.store.GetByTopic("topic2");
            var topic3 = await this.store.GetByTopic("topic3");
            var endpoint1 = await this.store.GetByEndpoint("endpoint1");
            var endpoint2 = await this.store.GetByEndpoint("endpoint2");
            var endpoint3 = await this.store.GetByEndpoint("endpoint3");
            var all = await this.store.GetAll();

            // assert
            // no need to preserve order
            CollectionAssert.AreEquivalent(entries, all);

            CollectionAssert.AreEquivalent(
                entries.Where(e => e.Topic == "topic1"),
                topic1);
            CollectionAssert.AreEquivalent(
                entries.Where(e => e.Topic == "topic2"),
                topic2);
            CollectionAssert.AreEquivalent(
                entries.Where(e => e.Topic == "topic3"),
                topic3);
            CollectionAssert.AreEquivalent(
                entries.Where(e => e.Endpoint == "endpoint1"),
                endpoint1);
            CollectionAssert.AreEquivalent(
                entries.Where(e => e.Endpoint == "endpoint2"),
                endpoint2);
            CollectionAssert.AreEquivalent(
                entries.Where(e => e.Endpoint == "endpoint3"),
                endpoint3);
        }

        // edge cases
        [Test]
        public async Task CannotAddTheSameSubscriptionTwice()
        {
            // arrange
            var subscription = new Subscription("topic", "endpoint", "component");
            await this.store.AddSubscription(subscription);

            // act
            var success = await this.store.AddSubscription(subscription);

            // assert
            Assert.IsFalse(success);
        }

        [Test]
        public async Task CannotRemoveSubscriptionThatDoesntExist()
        {
            // arrange
            var subscription = new Subscription("topic", "endpoint", "component");

            // act
            var success = await this.store.RemoveSubscription(subscription);

            // assert
            Assert.IsFalse(success);
        }

        [TestCase(100, 10)]
        [TestCase(10, 10)]
        [TestCase(2, 10)]
        public async Task CannotAddTheSameSubscriptionTwiceConcurrently(int concurrencyLevel, int nSubscriptions)
        {
            // arrange
            var subscriptions = Enumerable.Range(1, nSubscriptions)
                                          .Select(n => new Subscription("topic" + n, "endpoint" + n, "dummy"))
                                          .ToArray();

            // act & assert

            // for each subscription, tries to add it concurrently
            foreach (var subscription in subscriptions)
            {
                var tasks = new ConcurrentBag<Task<bool>>();

                var subscriptionToAdd = subscription; //capture context 
                Parallel.For(0, concurrencyLevel, c =>
                {
                    tasks.Add(this.store.AddSubscription(subscription));
                });

                await Task.WhenAll(tasks);

                // ensures only one concurrent task was able to add the subscription
                Assert.AreEqual(1, tasks.Count(t => t.Result == true));
            }
        }

        [TestCase(100, 10)]
        [TestCase(10, 10)]
        [TestCase(2, 10)]
        public async Task CannotRemoveTheSameSubscriptionConcurrently(int concurrencyLevel, int nSubscriptions)
        {
            // arrange
            var subscriptions = Enumerable.Range(1, nSubscriptions)
                                          .Select(n => new Subscription("topic" + n, "endpoint" + n, "dummy"))
                                          .ToArray();
            
            // add the subscriptions so they can be removed
            foreach (var subscription in subscriptions)
            {
                await this.store.AddSubscription(subscription);
            }

            // act & assert

            // for each subscription, tries to add it concurrently
            foreach (var subscription in subscriptions)
            {
                var tasks = new ConcurrentBag<Task<bool>>();

                var subscriptionToAdd = subscription; //capture context 
                Parallel.For(0, concurrencyLevel, c =>
                {
                    tasks.Add(this.store.RemoveSubscription(subscription));
                });

                await Task.WhenAll(tasks);

                // ensures only one concurrent task was able to add the subscription
                Assert.AreEqual(1, tasks.Count(t => t.Result == true));
            }
        }

        private static HashSet<string> Set(params string[] values)
        {
            return new HashSet<string>(values);
        }
    }
}
