using NDomain.Tests.Sample;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.Aggregates
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
