using System;
using System.Threading.Tasks;
using NDomain.Bus;
using NDomain.Bus.Transport;
using NUnit.Framework;

namespace NDomain.Tests.Common.Specs
{
    public abstract class TransportSpecs
    {
        protected ITransportFactory factory;

        public abstract ITransportFactory CreateFactory();

        protected virtual void OnSetUp() { }
        protected virtual void OnTearDown() { }

        [SetUp]
        public void Setup()
        {
            this.factory = CreateFactory();

            this.OnSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            this.OnTearDown();
        }

        [Test]
        public async Task CanDoBasicBehavior()
        {
            var options = new InboundTransportOptions("queue");
            var sender = this.factory.CreateOutboundTransport();
            var receiver = this.factory.CreateInboundTransport(options);

            await sender.Send(
                new TransportMessage
                {
                    Id = "id1",
                    Name = "msg",
                    Body = new Newtonsoft.Json.Linq.JObject()
                }.ForEndpoint(options.Endpoint));

            var transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5));

            Assert.That(transaction.Message.Id, Is.EqualTo("id1"));
            Assert.That(transaction.DeliveryCount, Is.EqualTo(1));

            await transaction.Commit();

            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5)); //should be null because there are no more commands
            Assert.IsNull(transaction);

            await sender.Send(
                new TransportMessage
                {
                    Id = "id2",
                    Name = "msg",
                    Body = new Newtonsoft.Json.Linq.JObject()
                }.ForEndpoint(options.Endpoint));

            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5));

            await transaction.Fail(); //should return to queue, to be retried
            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5)); //TODO: increase this timeout when defer times are supported on Fail

            Assert.NotNull(transaction);
            Assert.That(transaction.Message.Id, Is.EqualTo("id2"));
            Assert.That(transaction.DeliveryCount, Is.EqualTo(2));
            await transaction.Commit();

            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5)); //should return null because there are no more commands
            Assert.IsNull(transaction);
        }

        [TestCase(3, true)]
        [TestCase(3, false)]
        public async Task ShouldDeadLetterAndDelete_When_MaxRetriesExceeded(int maxRetries, bool deadLetterMessages)
        {
            // arrange
            var options = new InboundTransportOptions("queue", maxRetries, deadLetterMessages);
            var deadLetterOptions = new InboundTransportOptions(options.GetDeadLetterEndpoint(), 1, false);

            var sender = this.factory.CreateOutboundTransport();
            var receiver = this.factory.CreateInboundTransport(options);
            var deadletterReceiver = this.factory.CreateInboundTransport(deadLetterOptions);

            await sender.Send(CreateMessageFor(options.Endpoint));

            IMessageTransaction msg;
            for (var i = 0; i < maxRetries - 1; ++i)
            {
                msg = await receiver.Receive(TimeSpan.FromMilliseconds(1));
                await msg.Fail();
            }

            // ensure dead letter is still empty
            var deadLetterMsg = await deadletterReceiver.Receive(TimeSpan.FromMilliseconds(1));
            Assume.That(deadLetterMsg, Is.Null);

            // act

            // last attempt
            msg = await receiver.Receive(TimeSpan.FromMilliseconds(1));
            await msg.Fail();

            // assert

            msg = await receiver.Receive(TimeSpan.FromMilliseconds(1));
            Assert.That(msg, Is.Null);

            deadLetterMsg = await deadletterReceiver.Receive(TimeSpan.FromMilliseconds(1));
            if (deadLetterMessages)
            {
                Assert.That(deadLetterMsg, Is.Not.Null);
                Assert.That(deadLetterMsg.Message.Headers[MessageHeaders.OriginalEndpoint], Is.EqualTo(options.Endpoint));
            }
            else
            {
                Assert.That(deadLetterMsg, Is.Null);
            }
        }

        [Test]
        public async Task ShouldReceiveMessage_When_ReceivingWithEmptyQueue_And_AMessageIsPublished()
        {
            var options = new InboundTransportOptions("queue");
            var receiver = this.factory.CreateInboundTransport(options);
            var sender = this.factory.CreateOutboundTransport();

            IMessageTransaction tr = null;

            var receiveTask = receiver.Receive().ContinueWith(t => tr = t.Result);

            await sender.Send(CreateMessageFor(options.Endpoint));

            await Task.WhenAny(receiveTask, Task.Delay(2000));
            //receiveTask.Wait(TimeSpan.FromSeconds(2)); // can cause deadlocks on sync contexts

            Assert.NotNull(tr);
        }

        private static TransportMessage CreateMessageFor(string endpoint)
        {
            var msg = new TransportMessage
                        {
                            Id = "id1",
                            Name = "msg",
                            Body = new Newtonsoft.Json.Linq.JObject()
                        };

            return msg.ForEndpoint(endpoint);
        }
    }

    static class TransportMessageExtensions
    {
        public static TransportMessage ForEndpoint(this TransportMessage message, string endpoint)
        {
            message.Headers[MessageHeaders.Endpoint] = endpoint;
            return message;
        }
    }
}
