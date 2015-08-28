using NDomain.Configuration;
using NDomain.Bus.Transport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.Logging;
using NDomain.IoC;
using NDomain.Bus;
using NDomain.CQRS;
using System.Threading;
using NDomain.Bus.Subscriptions;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace NDomain.Tests.CQRS
{
    /// <summary>
    /// Covers a lot of message bus functionallity and message serialization
    /// </summary>
    public class CommandBusTests
    {
        [Test]
        public async Task CanSendCommandsToHandler()
        {
            // arrange
            var sync = new CountdownEvent(6);

            var ctx = DomainContext.Configure()
                                         .Bus(b =>
                                             b.WithProcessor(p =>
                                                 p.Endpoint("p1")
                                                  .RegisterHandler(new TestCommandHandler(cmd => sync.Signal())))
                                          )
                                          .Start();
            // act
            using (ctx)
            {
                await ctx.CommandBus.Send(new Command<DoSimple>("cmd1", new DoSimple { }));
                await ctx.CommandBus.Send(new Command<DoComplex>("cmd2", new DoComplex { }));
                await ctx.CommandBus.Send(new Command<DoSimpleStuff>("cmd3", DoSimpleStuff.Create()));
                await ctx.CommandBus.Send(new Command<DoComplexStuff>("cmd4", DoComplexStuff.Create()));
                await ctx.CommandBus.Send(new Command<DoGenericStuff<DoComplexStuff>>("cmd5", new DoGenericStuff<DoComplexStuff> { Stuff = DoComplexStuff.Create() }));
                await ctx.CommandBus.Send(new Command<DoNonGenericStuff>("cmd6", new DoNonGenericStuff { Stuff = DoSimpleStuff.Create() }));

                sync.Wait(TimeSpan.FromSeconds(2));
            }

            // assert
            Assert.AreEqual(0, sync.CurrentCount); // asserts all messages were received
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public async Task CanSendCommandsToMultipleHandlersOnDifferentContexts(int nContexts)
        {
            // arrange
            var subscriptionStore = new LocalSubscriptionStore(); // shared store
            var subscriptionBroker = new LocalSubscriptionBroker(); // shared broker
            var transportFactory = new LocalTransportFactory(); // shared transport

            var syncs = new CountdownEvent[nContexts];
            var contexts = new IDomainContext[nContexts];

            for (var i = 0; i < nContexts; ++i)
            {
                var idx = i;
                syncs[i] = new CountdownEvent(2);
                contexts[i] = DomainContext.Configure()
                                         .Bus(b =>
                                             b.WithCustomSubscriptionStore(subscriptionStore)
                                              .WithCustomSubscriptionBroker(subscriptionBroker)
                                              .WithCustomTransportFactory(transportFactory)
                                              .WithProcessor(p =>
                                                 p.Endpoint("p" + idx)
                                                  .RegisterHandler(new TestCommandHandler(cmd => syncs[idx].Signal())
                                              ))
                                          )
                                          .Start();
            }

            try
            {
                // doesnt really matter which ctx sends the messages, they know about eachother subscriptions
                var ctx1 = contexts.First();
                var ctx2 = contexts.Last();

                await ctx1.CommandBus.Send(new Command<DoSimple>("cmd1", new DoSimple { }));
                await ctx2.CommandBus.Send(new Command<DoComplex>("cmd2", new DoComplex { }));

                foreach (var sync in syncs)
                {
                    sync.Wait(TimeSpan.FromSeconds(2));
                }
            }
            finally
            {
                foreach (var context in contexts)
                {
                    context.Dispose();
                }
            }

            // assert
            foreach (var sync in syncs)
            {
                // asserts all messages were received by each processor
                Assert.AreEqual(0, sync.CurrentCount);
            }
        }

        [Test]
        public async Task CanSendCommandsToMultipleHandlersOnDifferentProcessorsInSameContext()
        {
            // arrange
            var sync1 = new CountdownEvent(2);
            var sync2 = new CountdownEvent(2);

            var ctx = DomainContext.Configure()
                                         .Bus(b =>
                                              b.WithProcessor(p =>
                                                 p.Endpoint("p1")
                                                  .RegisterHandler(new TestCommandHandler(cmd => sync1.Signal())))
                                               .WithProcessor(p =>
                                                p.Endpoint("p2")
                                                 .RegisterHandler(new TestCommandHandler(cmd => sync2.Signal())))
                                          )
                                          .Start();

            // act
            using (ctx)
            {
                // doesnt really matter which ctx sends the messages, they know about eachother subscriptions
                await ctx.CommandBus.Send(new Command<DoSimple>("cmd1", new DoSimple { }));
                await ctx.CommandBus.Send(new Command<DoComplex>("cmd2", new DoComplex { }));

                sync1.Wait(TimeSpan.FromSeconds(2));
                sync2.Wait(TimeSpan.FromSeconds(2));
            }

            // assert
            Assert.AreEqual(0, sync1.CurrentCount); // asserts all messages were received
            Assert.AreEqual(0, sync2.CurrentCount); // asserts all messages were received
        }

        class SentReceivedMessage
        {
            public volatile object Sent;
            public volatile object Received;
        }

        [Test]
        public async Task CommandsAreProperlySerialized()
        {
            // arrange
            var cmds = new ConcurrentDictionary<Type, SentReceivedMessage>();

            cmds[typeof(Command<DoSimple>)] = new SentReceivedMessage
            {
                Sent = new Command<DoSimple>("cmd1", new DoSimple { Value = "0" })
            };
            cmds[typeof(Command<DoComplex>)] = new SentReceivedMessage
            {
                Sent = new Command<DoComplex>("cmd2", new DoComplex { IntValue = 10, StringValue = "abc" })
            };
            cmds[typeof(Command<DoSimpleStuff>)] = new SentReceivedMessage
            {
                Sent = new Command<DoSimpleStuff>("cmd3", DoSimpleStuff.Create())
            };
            cmds[typeof(Command<DoComplexStuff>)] = new SentReceivedMessage
            {
                Sent = new Command<DoComplexStuff>("cmd4", DoComplexStuff.Create())
            };
            cmds[typeof(Command<DoGenericStuff<DoComplexStuff>>)] = new SentReceivedMessage
            {
                Sent = new Command<DoGenericStuff<DoComplexStuff>>("cmd5", new DoGenericStuff<DoComplexStuff> { Stuff = DoComplexStuff.Create() })
            };
            cmds[typeof(Command<DoNonGenericStuff>)] = new SentReceivedMessage
            {
                Sent = new Command<DoNonGenericStuff>("cmd6", new DoNonGenericStuff { Stuff = DoSimpleStuff.Create() })
            };

            var sync = new CountdownEvent(cmds.Count);

            var ctx = DomainContext.Configure()
                                         .Bus(b =>
                                             b.WithProcessor(p =>
                                                 p.Endpoint("p1")
                                                  .RegisterHandler(new TestCommandHandler(cmd =>
                                                  {
                                                      // stores the received message and signals completion
                                                      cmds[cmd.GetType()].Received = cmd;
                                                      sync.Signal();
                                                  })))
                                          )
                                          .Start();
            // act
            using (ctx)
            {
                foreach (var cmd in cmds)
                {
                    await ctx.CommandBus.Send(cmd.Value.Sent as ICommand);
                }

                sync.Wait(TimeSpan.FromSeconds(2));
            }

            // assert
            // check that the sent message is the same as the received message
            // by serializing each and comparing the string
            Assert.AreEqual(0, sync.CurrentCount); // asserts all messages were received

            foreach (var cmd in cmds)
            {
                Assert.AreEqual(
                    JsonConvert.SerializeObject(cmd.Value.Sent),
                    JsonConvert.SerializeObject(cmd.Value.Received));
            }
        }
    }
}
