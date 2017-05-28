using NDomain.Persistence.EventSourcing;
using NDomain.Tests.Common.Specs;
using NUnit.Framework;

namespace NDomain.Tests.Persistence.EventSourcing
{
    [TestFixture]
    public class LocalEventStoreTests : EventStoreSpecs
    {
        protected override IEventStoreDb CreateEventStorage()
        {
            return new LocalEventStore();
        }
    }
}
