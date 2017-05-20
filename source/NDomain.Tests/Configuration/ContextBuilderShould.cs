using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.Tests.Common.Sample;

namespace NDomain.Tests.Configuration
{
    [TestFixture]
    public class ContextBuilderShould
    {
        [Test]
        public void ConfigureWithDefaults()
        {
            // arrange

            // act
            var context = DomainContext.Configure().Start();

            // assert
            AssertContextIsValid(context);
        }

        [Test]
        public void ConfigureEventSourcing()
        {
            // act
            var context = DomainContext.Configure()
                                       .EventSourcing(
                                            e => e.BindAggregate<Counter>())
                                       .Start();

            // assert
            AssertContextIsValid(context);
        }

        private void AssertContextIsValid(IDomainContext context)
        {
            Assert.That(context, Is.Not.Null);

            Assert.That(context.CommandBus, Is.Not.Null);
            Assert.That(context.EventBus, Is.Not.Null);

            var repository = context.GetRepository<Counter>();
            Assert.That(repository, Is.Not.Null);

        }
    }
}
