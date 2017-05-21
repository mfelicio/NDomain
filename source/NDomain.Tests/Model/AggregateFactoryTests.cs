using NDomain.Model;
using NDomain.Tests.Common.Sample;
using NUnit.Framework;

namespace NDomain.Tests.Model
{
    /// <summary>
    /// AggregateFactory tests based on the sample Race aggregate
    /// </summary>
    public class AggregateFactoryTests
    {
        [Test]
        public void CanCreateFactory()
        {
            Assert.DoesNotThrow(() => AggregateFactory.For<Race>());
        }

        [Test]
        public void FactoriesAreCached()
        {
            // arrange & act
            var f1 = AggregateFactory.For<Race>();
            var f2 = AggregateFactory.For<Race>();

            // assert
            Assert.AreSame(f1, f2);
        }
    }
}
