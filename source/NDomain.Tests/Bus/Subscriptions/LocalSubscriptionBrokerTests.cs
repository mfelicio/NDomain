using NDomain.Bus.Subscriptions;
using NDomain.Tests.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.Bus.Subscriptions
{
    public class LocalSubscriptionBrokerTests : SubscriptionBrokerSpecs
    {
        protected override ISubscriptionBroker CreateSubscriptionBroker()
        {
            return new LocalSubscriptionBroker();
        }
    }
}
