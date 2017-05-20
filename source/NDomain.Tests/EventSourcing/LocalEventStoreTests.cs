using NDomain.Model.EventSourcing;
using NDomain.Tests.Specs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.EventSourcing
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
