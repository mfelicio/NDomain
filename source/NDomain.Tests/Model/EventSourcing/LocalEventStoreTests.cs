using NDomain.Model.EventSourcing;
using NDomain.Tests.Common.Specs;
using NUnit.Framework;

namespace NDomain.Tests.Model.EventSourcing
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
