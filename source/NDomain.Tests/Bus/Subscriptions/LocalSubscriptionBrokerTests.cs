using NDomain.Bus.Subscriptions;
using NUnit.Framework;
using NDomain.Tests.Common.Specs;

namespace NDomain.Tests.Bus.Subscriptions
{
    [TestFixture]
    public class LocalSubscriptionBrokerTests : SubscriptionBrokerSpecs
    {
        protected override ISubscriptionBroker CreateSubscriptionBroker()
        {
            return new LocalSubscriptionBroker();
        }
    }
}
