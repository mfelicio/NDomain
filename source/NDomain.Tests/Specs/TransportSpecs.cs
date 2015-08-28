using NDomain.Bus;
using NDomain.Bus.Transport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.Specs
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
            var endpoint = "queue";
            var sender = this.factory.CreateOutboundTransport();
            var receiver = this.factory.CreateInboundTransport(endpoint);

            await sender.Send(
                new TransportMessage
                {
                    Id = "id1",
                    Name = "msg",
                    Body = new Newtonsoft.Json.Linq.JObject()
                }.ForEndpoint(endpoint));

            var transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5));

            Assert.That(transaction.Message.Id, Is.EqualTo("id1"));
            Assert.That(transaction.RetryCount, Is.EqualTo(0));

            await transaction.Commit();

            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5)); //should be null because there are no more commands
            Assert.IsNull(transaction);

            await sender.Send(
                new TransportMessage
                {
                    Id = "id2",
                    Name = "msg",
                    Body = new Newtonsoft.Json.Linq.JObject()
                }.ForEndpoint(endpoint));

            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5));

            await transaction.Fail(); //should return to queue, to be retried
            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5)); //TODO: increase this timeout when defer times are supported on Fail

            Assert.NotNull(transaction);
            Assert.That(transaction.Message.Id, Is.EqualTo("id2"));
            Assert.That(transaction.RetryCount, Is.EqualTo(1));
            await transaction.Commit();

            transaction = await receiver.Receive(TimeSpan.FromMilliseconds(5)); //should return null because there are no more commands
            Assert.IsNull(transaction);
        }

        [Test]
        public async Task CanPopBeforePushing()
        {
            var endpoint = "queue";
            var receiver = this.factory.CreateInboundTransport(endpoint);
            var sender = this.factory.CreateOutboundTransport();

            IMessageTransaction tr = null;

            var receiveTask = receiver.Receive().ContinueWith(t => tr = t.Result);

            await sender.Send(
                new TransportMessage
                {
                    Id = "id1",
                    Name = "msg",
                    Body = new Newtonsoft.Json.Linq.JObject()
                }.ForEndpoint(endpoint));

            await Task.WhenAny(receiveTask, Task.Delay(2000));
            //receiveTask.Wait(TimeSpan.FromSeconds(2)); // can cause deadlocks on sync contexts

            Assert.NotNull(tr);
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
