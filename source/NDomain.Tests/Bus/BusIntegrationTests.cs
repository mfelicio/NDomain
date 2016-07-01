using NDomain.Bus;
using NDomain.Bus.Subscriptions;
using NDomain.Bus.Transport;
using NDomain.IoC;
using NDomain.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NDomain.Tests.Bus
{
    public class BusIntegrationTests
    {
        protected IDependencyResolver resolver;
        protected ISubscriptionBroker subscriptionBroker;
        protected ISubscriptionStore subscriptionStore;
        protected ITransportFactory transportFactory;
        protected ISubscriptionManager subscriptionManager;
        protected IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            this.resolver = new DefaultDependencyResolver();
            this.subscriptionStore = new LocalSubscriptionStore();
            this.subscriptionBroker = new LocalSubscriptionBroker();
            this.subscriptionManager = new SubscriptionManager(this.subscriptionStore, this.subscriptionBroker);
            this.transportFactory = new LocalTransportFactory();
            this.bus = new MessageBus(this.subscriptionManager, this.transportFactory.CreateOutboundTransport(), NullLoggerFactory.Instance);
        }

        [TestCase(2, 1, 3)]
        [TestCase(5, 3, 2)]
        public void MultipleProcessorsCanReceiveMessages(int nProcessors, int myMsgHandlers, int otherMsgHandlers)
        {
            // arrange
            var myMessageSync = new CountdownEvent(nProcessors * myMsgHandlers);
            var otherMessageSync = new CountdownEvent(nProcessors * otherMsgHandlers);

            // create the processors and register handlers, each with its own subscription manager
            // simulates real usage
            var processors = new List<IProcessor>();
            for (var i = 0; i < nProcessors; ++i)
            {
                var processor = new Processor(
                    new InboundTransportOptions("processor:" + i),
                    10, // concurrency level.. not really important for this test
                    new SubscriptionManager(this.subscriptionStore, this.subscriptionBroker),
                    this.transportFactory,
                    NullLoggerFactory.Instance,
                    this.resolver);

                // register myMsgHandlers
                for (var j = 0; j < myMsgHandlers; ++j)
                {
                    RegisterHandler<MyMessage>(processor, "myMsgHandler:" + j, myMessageSync);
                }
                // register otherMsgHandlers
                for (var j = 0; j < otherMsgHandlers; ++j)
                {
                    RegisterHandler<MyOtherMessage>(processor, "otherMsgHandler:" + j, otherMessageSync);
                }

                processors.Add(processor);
                processor.Start();
            }

            // act
            bus.Send(new MyMessage { MyValue = 321 });
            bus.Send(new MyOtherMessage { OtherValue = "threetwoone" });

            // assert
            myMessageSync.Wait(TimeSpan.FromSeconds(3)); // 3 second timeout for ev
            otherMessageSync.Wait(TimeSpan.FromSeconds(3)); // 3 second timeout for ev
            Assert.AreEqual(0, myMessageSync.CurrentCount); // means all handlers got its own message
            Assert.AreEqual(0, otherMessageSync.CurrentCount); // means all handlers got its own message

            foreach (var processor in processors)
            {
                processor.Dispose();
            }
        }

        private void RegisterHandler<TMessage>(Processor processor, string handlerName, CountdownEvent sync)
        {
            processor.RegisterMessageHandler<TMessage>(handlerName, new MessageHandler<TMessage>(
                        msg =>
                        {
                            sync.Signal();
                            return Task.FromResult(true);
                        }));
        }
        
        class MyMessage
        {
            public int MyValue { get; set; }
        }

        class MyOtherMessage
        {
            public string OtherValue { get; set; }
        }
    }
}
