using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NDomain.EventSourcing;
using NDomain.EventSourcing.Azure;
using NDomain.Tests.Specs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Azure.Tests.EventSourcing
{
    [TestFixture]
    public class AzureEventStoreTests : EventStoreSpecs
    {
        protected override IEventStoreDb CreateEventStorage()
        {
            return new AzureEventStore(CloudStorageAccount.DevelopmentStorageAccount, "ndomaintestsevents");
        }

        protected override void OnSetUp()
        {
            // reset events table between each test
            CloudStorageAccount.DevelopmentStorageAccount
                               .CreateCloudTableClient()
                               .GetTableReference("ndomaintestsevents")
                               .DeleteIfExists();
        }
    }
}
