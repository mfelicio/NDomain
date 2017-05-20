using NDomain.Bus.Subscriptions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
