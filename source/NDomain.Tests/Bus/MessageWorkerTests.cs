using NDomain.Logging;
using NDomain.Bus.Transport;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDomain.Bus;

namespace NDomain.Tests.Messaging
{
    class MockTransaction : IMessageTransaction
    {
        public MockTransaction()
        {
            this.Message = new TransportMessage()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Name"
            };
            this.RetryCount = 0;
        }

        public TransportMessage Message { get; set; }

        public int RetryCount { get; set; }

        public Task Commit()
        {
            return Task.FromResult(true);
        }

        public Task Fail()
        {
            return Task.FromResult(true);
        }
    }

    public class MessageWorkerTests
    {
        private IInboundTransport SetupReceiver(int nMessages)
        {
            var transport = new Mock<IInboundTransport>();

            int dequeueCount = 0;

            transport.Setup(m => m.Receive(It.IsAny<TimeSpan?>()))
                        .Returns(() =>
                        {
                            IMessageTransaction tr;
                            if (Interlocked.Increment(ref dequeueCount) <= nMessages)
                            {
                                tr = new MockTransaction();
                            }
                            else
                            {
                                tr = null;
                            }

                            return Task.FromResult(tr);
                        });

            return transport.Object;
        }

        private IMessageDispatcher SetupDispatcher(int nMessages, int processingTime, ManualResetEvent callback)
        {
            var dispatcherMock = new Mock<IMessageDispatcher>();

            int dequeueCount = 0;

            dispatcherMock.Setup(m => m.ProcessMessage(It.IsAny<TransportMessage>()))
                          .Returns(() => Task.Delay(processingTime).ContinueWith(t =>
                          {
                              if (Interlocked.Increment(ref dequeueCount) == nMessages)
                              {
                                  callback.Set();
                              }
                          }));

            return dispatcherMock.Object;
        }

        [TestCase(1000, 10000, 100)]
        [TestCase(1000, 100000, 10)]
        public void TestConcurrencyLevelIsRespected(int concurrencyLevel, int nMessages, int processingTime)
        {
            var evt = new ManualResetEvent(false);
            
            // TODO: this test isn't testing any behaviour so far.. 
            // just ensuring the worker can cope with high number of messages

            var receiver = SetupReceiver(nMessages);
            var dispatcher = SetupDispatcher(nMessages, processingTime, evt);
            using (var processor = new MessageWorker(receiver, dispatcher, NullLoggerFactory.Instance, concurrencyLevel))
            {
                processor.Start();

                Task.Delay(TimeSpan.FromMinutes(15)).ContinueWith(t => evt.Set());

                evt.WaitOne();
            }
        }
    }
}
