using Moq;
using NDomain.Bus.Subscriptions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NDomain.Tests.Bus.Subscriptions
{
    [TestFixture]
    public class SubscriptionManagerTests
    {
        private ISubscriptionManager manager;
        private ISubscriptionStore store;
        private ISubscriptionBroker broker;

        [SetUp]
        public void SetUp()
        {
            this.broker = new LocalSubscriptionBroker();
            this.store = new LocalSubscriptionStore();
            this.manager = new SubscriptionManager(this.store, this.broker);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task WhenStoreIsEmpty_CanUpdate_And_GetSubscriptions(bool useRealBroker)
        {
            // arrange
            if (!useRealBroker)
            {
                // tests the same behavior, even without receiving any broker notifications for the updates
                this.manager = new SubscriptionManager(this.store, CreateBrokerMock().Object);
            }

            await manager.UpdateEndpointSubscriptions(
                "c1",
                new[] {
                    new Subscription("t1", "e1", "c1"),
                    new Subscription("t1", "e1", "c2"),
                    new Subscription("t2", "e1", "c2"),
                });

            // act
            var t1Subscriptions = await manager.GetSubscriptions("t1");
            var t2Subscriptions = await manager.GetSubscriptions("t2");

            // assert
            Assert.AreEqual(2, t1Subscriptions.Count());
            Assert.AreEqual(1, t2Subscriptions.Count());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task WhenStoreIsNotEmpty_CanGetExistingSubscriptions(bool useRealBroker)
        {
            // arrange
            if (!useRealBroker)
            {
                // tests the same behavior, even without receiving any broker notifications for the updates
                this.manager = new SubscriptionManager(this.store, CreateBrokerMock().Object);
            }

            var nSubscriptions = 4;
            for (var i = 0; i < nSubscriptions; ++i)
            {
                // adding subscriptions from component2 for topics 1-4
                await this.store.AddSubscription(new Subscription("t" + i, "e2", "c1"));
            }

            // act
            var t2Subs = await this.manager.GetSubscriptions("t2");

            // assert
            Assert.AreEqual(1, t2Subs.Count());
        }

        [Test]
        public async Task WhenStoreHasDeprecatedSubscriptions_OnlyValidOnesAreReturned_AfterUpdatingThem()
        {
            // arrange
            var validSubscription = new Subscription("t1", "e1", "c1");
            var deprecatedSubscription = new Subscription("t1", "e1", "c3"); // deprecated because will not be updated
            await this.store.AddSubscription(validSubscription);
            await this.store.AddSubscription(deprecatedSubscription);

            // ensure that before updating, both subscriptions are returned
            var t1Subs = await this.manager.GetSubscriptions("t1");
            Assert.AreEqual(2, t1Subs.Count());

            // act
            // update c1 subscriptions, only with 'c1', meaning 'c3' should be removed
            await this.manager.UpdateEndpointSubscriptions(
                "e1",
                new[] { new Subscription("t1", "e1", "c1") });

            // assert
            t1Subs = await this.manager.GetSubscriptions("t1");
            // there is only one subscription and is the valid one
            Assert.AreEqual(1, t1Subs.Count());
            Assert.NotNull(t1Subs.SingleOrDefault(s => s.Equals(validSubscription)));
        }

        [Test]
        public async Task WhenBrokerIsNotified_ManagerSubscriptionCacheIsUpdated_RegardlessOfSubscriptionStore()
        {
            // arrange
            // ensures it is initialized and subscribing to notifications
            var t1Subs = await this.manager.GetSubscriptions("t1"); 
            Assert.AreEqual(0, t1Subs.Count()); // you know nothing subscription manager!!

            // act
            // simulates subscriptions being updated from another endpoint
            await this.broker.NotifyChange(SubscriptionChange.Add, new Subscription("t1", "e2", "c1"));
            await this.broker.NotifyChange(SubscriptionChange.Add, new Subscription("t1", "e3", "c1"));

            await Task.Delay(30); // wait some time.. since notifications are asynchronous

            // assert
            t1Subs = await this.manager.GetSubscriptions("t1");
            Assert.AreEqual(2, t1Subs.Count());
        }

        [Test]
        public async Task WhenNewSubscriptionsAreFound_BrokerIsNotified()
        {
            // mock broker
            var broker = CreateBrokerMock();
            this.manager = new SubscriptionManager(this.store, broker.Object);

            // arrange
            var existingSubscription = new Subscription("t1", "e1", "c1");
            await this.store.AddSubscription(existingSubscription);

            // act
            var newSubscription1 = new Subscription("t2", "e1", "c1");
            var newSubscription2 = new Subscription("t3", "e1", "c1");
            await this.manager.UpdateEndpointSubscriptions(
                "e1",
                new[] { existingSubscription, newSubscription1, newSubscription2 });

            // assert
            //verifies that broker was notified two times and for the expected subscriptions
            broker.Verify(b =>
                b.NotifyChange(It.IsAny<SubscriptionChange>(), It.IsAny<Subscription>()),
                Times.Exactly(2));
            broker.Verify(b =>
                b.NotifyChange(SubscriptionChange.Add, It.Is<Subscription>(s => s.Equals(newSubscription1))),
                Times.Once);
            broker.Verify(b =>
                b.NotifyChange(SubscriptionChange.Add, It.Is<Subscription>(s => s.Equals(newSubscription2))),
                Times.Once);
        }

        [Test]
        public async Task WhenDeprecatedSubscriptionsAreFound_BrokerIsNotified()
        {
            // mock broker
            var broker = CreateBrokerMock();
            this.manager = new SubscriptionManager(this.store, broker.Object);

            // arrange
            var deprecated = new Subscription("t2", "e1", "c1");
            await this.store.AddSubscription(new Subscription("t1", "e1", "c1"));
            await this.store.AddSubscription(deprecated);
            await this.store.AddSubscription(new Subscription("t3", "e1", "c1"));

            // act
            // the deprecated subscription isn't included here, meaning the broker must get notified
            await this.manager.UpdateEndpointSubscriptions(
                "e1",
                new[] { new Subscription("t1", "e1", "c1"), new Subscription("t3", "e1", "c1") });

            // assert
            //verifies that broker was notified only once and for the expected subscription
            broker.Verify(b =>
                b.NotifyChange(It.IsAny<SubscriptionChange>(), It.IsAny<Subscription>()),
                Times.Exactly(1));
            broker.Verify(b =>
                b.NotifyChange(SubscriptionChange.Remove, It.Is<Subscription>(s => s.Equals(deprecated))),
                Times.Once);
        }

        private Mock<ISubscriptionBroker> CreateBrokerMock()
        {
            var broker = new Mock<ISubscriptionBroker>();
            broker.Setup(m => m.NotifyChange(It.IsAny<SubscriptionChange>(), It.IsAny<Subscription>()))
                  .Returns(Task.FromResult(true));
            broker.Setup(m => m.SubscribeChangeNotifications(It.IsAny<Action<SubscriptionChange, Subscription>>()))
                  .Returns(Task.FromResult(true));

            return broker;
        }
    }
}
