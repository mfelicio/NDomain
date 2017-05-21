using NDomain.Bus.Subscriptions;
using NUnit.Framework;
using NDomain.Tests.Common.Specs;

namespace NDomain.Tests.Bus.Subscriptions
{
    [TestFixture]
    public class LocalSubscriptionStoreTests : SubscriptionStoreSpecs
    {
        protected override ISubscriptionStore CreateSubscriptionStore()
        {
            return new LocalSubscriptionStore();
        }
    }
}
