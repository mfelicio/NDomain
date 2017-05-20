using System;
using NDomain.Configuration;
using NDomain.CQRS;
using NDomain.Model.EventSourcing;
using NDomain.Model.Snapshot;
using NDomain.Tests.Common.Sample;
using NUnit.Framework;

namespace NDomain.Tests.Common.Specs
{
    public abstract class DependencyResolverSpecs
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

        protected abstract void ConfigureIoC(IoCConfigurator ioc);
    }
}
