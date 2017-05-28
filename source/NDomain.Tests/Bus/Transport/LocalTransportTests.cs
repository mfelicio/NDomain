using NDomain.Bus.Transport;
using NUnit.Framework;
using NDomain.Tests.Common.Specs;

namespace NDomain.Tests.Bus.Transport
{
    [TestFixture]
    public class LocalTransportTests : TransportSpecs
    {
        protected override ITransportFactory CreateFactory()
        {
            return new LocalTransportFactory();
        }
    }
}
