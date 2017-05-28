using System;
using System.Threading.Tasks;
using NDomain.Bus;
using NDomain.Bus.Transport;
using NUnit.Framework;

namespace NDomain.Tests.Common.Specs
{
    public abstract class TransportSpecs
    {
        private const string TestInboundEndpoint = "myqueue";

        private ITransportFactory factory;
        private IInboundTransport receiver;
        private IOutboundTransport sender;

        protected abstract ITransportFactory CreateFactory();

        protected virtual void OnSetUp() { }
        protected virtual void OnTearDown() { }

        [SetUp]
        public void Setup()
        {
            this.factory = CreateFactory();

            var options = new InboundTransportOptions(TestInboundEndpoint);
            this.receiver = this.factory.CreateInboundTransport(options);
            this.sender = this.factory.CreateOutboundTransport();

            this.OnSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            this.OnTearDown();
        }

        [Test]
        public async Task CanReceiveMessages()
        {
            // arrange
            var expectedMessage = CreateMessageFor(TestInboundEndpoint);

            await this.sender.Send(expectedMessage);

            // act
            var transaction = await this.receiver.Receive(TimeSpan.FromMilliseconds(5));

            // assert
            Assert.That(transaction.Message.Id, Is.EqualTo(expectedMessage.Id));
            Assert.That(transaction.DeliveryCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Does_Not_Receive_Committed_Messages()
        {
            // arrange
            var expectedMessage = CreateMessageFor(TestInboundEndpoint);

            await this.sender.Send(expectedMessage);

            var transaction = await this.receiver.Receive(TimeSpan.FromMilliseconds(5));
         
            // act
            await transaction.Commit();

            // assert
            transaction = await this.receiver.Receive(TimeSpan.FromMilliseconds(5)); //should be null because there are no more commands

            Assert.IsNull(transaction);
        }

        [Test]
        public async Task Receives_Failed_Messages_With_Increased_DeliveryCount()
        {
            // arrange
            var expectedMessage = CreateMessageFor(TestInboundEndpoint);

            await this.sender.Send(expectedMessage);

            var transaction = await this.receiver.Receive(TimeSpan.FromMilliseconds(5));

            // act
            await transaction.Fail(); //should return to queue, to be retried
            transaction = await this.receiver.Receive(TimeSpan.FromMilliseconds(5)); //TODO: increase this timeout when defer times are supported on Fail

            Assert.NotNull(transaction);
            Assert.That(transaction.Message.Id, Is.EqualTo(expectedMessage.Id));
            Assert.That(transaction.DeliveryCount, Is.EqualTo(2));
        }

        [TestCase(3, true)]
        [TestCase(3, false)]
        public async Task ShouldDeadLetterAndDelete_When_MaxRetriesExceeded(int maxRetries, bool deadLetterMessages)
        {
            // arrange
            var options = new InboundTransportOptions(TestInboundEndpoint, maxRetries, deadLetterMessages);
            var deadLetterOptions = new InboundTransportOptions(options.DeadLetterEndpoint, 1, false);

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
            // arrange
            IMessageTransaction tr = null;

            var receiveTask = this.receiver.Receive().ContinueWith(t => tr = t.Result);

            // act
            await this.sender.Send(CreateMessageFor(TestInboundEndpoint));

            // assert
            await Task.WhenAny(receiveTask, Task.Delay(2000));

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
