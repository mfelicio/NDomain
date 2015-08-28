using Moq;
using NDomain.Bus;
using NDomain.Bus.Subscriptions;
using NDomain.Bus.Transport;
using NDomain.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.Bus
{
    public class MessageBusTests
    {
        ISubscriptionStore subscriptionStore;
        ISubscriptionManager subscriptionManager;

        [SetUp]
        public void SetUp()
        {
            this.subscriptionStore = new LocalSubscriptionStore();
            this.subscriptionManager = new SubscriptionManager(
                                            this.subscriptionStore,
                                            new LocalSubscriptionBroker());
        }

        [Test]
        public void WhenNoSubscribers_MessageIsNotSent()
        {
            // arrange
            var client = CreateOutboundTransportMock();
            var bus = new MessageBus(this.subscriptionManager, client.Object, NullLoggerFactory.Instance);
            var message = new Message(new object(), "topic");

            // act
            bus.Send(message);

            // assert
            client.Verify(b => b.Send(It.IsAny<TransportMessage>()), Times.Never);
        }

        [Test]
        public void WhenMultipleSubscribers_EachGetOwnMessage()
        {
            // arrange
            var client = CreateOutboundTransportMock();

            // create two subscriptions for "mytopic"
            this.subscriptionStore.AddSubscription(new Subscription("mytopic", "e1", "c1"));
            this.subscriptionStore.AddSubscription(new Subscription("mytopic", "e2", "c2"));

            var bus = new MessageBus(this.subscriptionManager, client.Object, NullLoggerFactory.Instance);
            var message = new Message(new object(), "mytopic");

            // act
            bus.Send(message);

            // assert
            // since multiple messages are to be sent, it should use the SendMultiple method
            client.Verify(b => b.Send(It.IsAny<TransportMessage>()), Times.Never);
            client.Verify(b =>
                b.SendMultiple(It.Is<IEnumerable<TransportMessage>>(msgs =>
                    msgs.Count() == 2 &&
                    msgs.Count(m => m.Headers[MessageHeaders.Endpoint] == "e1" &&
                                    m.Headers[MessageHeaders.Component] == "c1") == 1 &&
                    msgs.Count(m => m.Headers[MessageHeaders.Endpoint] == "e2" &&
                                    m.Headers[MessageHeaders.Component] == "c2") == 1)
                    ), Times.Once);
        }

        private Mock<IOutboundTransport> CreateOutboundTransportMock()
        {
            var mock = new Mock<IOutboundTransport>();

            mock.Setup(c => c.Send(It.IsAny<TransportMessage>()))
                .Returns(Task.FromResult(true));
            mock.Setup(c => c.SendMultiple(It.IsAny<IEnumerable<TransportMessage>>()))
                .Returns(Task.FromResult(true));

            return mock;
        }
    }
}
