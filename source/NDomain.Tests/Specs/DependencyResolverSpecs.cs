using NDomain.Configuration;
using NDomain.CQRS;
using NDomain.Model;
using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;
using NDomain.Tests.Sample;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.Specs
{
    [TestFixture]
    public class DependencyResolverSpecs
    {
        [TestCase(typeof(IEventBus))]
        [TestCase(typeof(ICommandBus))]
        [TestCase(typeof(IEventStore))]
        [TestCase(typeof(ISnapshotStore))]
        [TestCase(typeof(IAggregateRepository<Counter>))]
        [TestCase(typeof(IAggregateRepository<StateOnlyAggregate>))]
        public void ResolveType(Type type)
        {
            // arrange
            var context = DomainContext.Configure()
                                       .IoC(ioc => ConfigureIoC(ioc))
                                       .Start() as DomainContext;

            // act
            var service = context.Resolver.Resolve(type);

            // assert
            Assert.That(service, Is.Not.Null);
        }

        protected virtual void ConfigureIoC(IoCConfigurator ioc)
        {
            ioc.UseDefault(r => { });
        }
    }
}
