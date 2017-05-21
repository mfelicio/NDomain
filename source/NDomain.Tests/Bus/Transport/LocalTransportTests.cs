using NDomain.Bus.Transport;
using NUnit.Framework;
using NDomain.Tests.Common.Specs;

namespace NDomain.Tests.Bus.Transport
{
    [TestFixture]
    public class LocalTransportTests : TransportSpecs
    {
        public override ITransportFactory CreateFactory()
        {
            return new LocalTransportFactory();
        }
    }
}
