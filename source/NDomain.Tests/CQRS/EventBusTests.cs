using NDomain.Configuration;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using NDomain.Model;
using NDomain.Tests.Common.Sample;

namespace NDomain.Tests.CQRS
{
    /// <summary>
    /// Covers aggregate event handlers, since its the most relevant difference between EventBus and CommandBus
    /// and there's already a good set of tests for CommandBus
    /// </summary>
    public class EventBusTests
    {
        [Test]
        public async Task CanReceiveCounterEvents()
        {
            // arrange
            var aggregateId = "mycounter";
            var sync = new CountdownEvent(3);
            var ctx = DomainContext.Configure()
                                   .EventSourcing(e => e.BindAggregate<Counter>())
                                   .Bus(b => b.WithProcessor(
                                                p => p.Endpoint("p1")
                                                      .RegisterHandler(new CounterEventsHandler(
                                                          e =>
                                                          {
                                                              sync.Signal();
                                                          }))
                                                )
                                    ).Start();

            var repository = ctx.GetRepository<Counter>();
            
            // act
            using (ctx)
            {
                // when one event is published as part of saving changes
                await repository.CreateOrUpdate<Counter>(aggregateId, c => c.Increment());

                // when multiple events are published as part of saving changes
                await repository.CreateOrUpdate<Counter>(aggregateId,
                    c =>
                    {
                        c.Increment(2);
                        c.Multiply(5);
                    });

                sync.Wait(TimeSpan.FromSeconds(2));
            }

            // assert
            Assert.AreEqual(0, sync.CurrentCount);
        }
    }
}
