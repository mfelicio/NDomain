using Microsoft.WindowsAzure.Storage;
using NDomain.Model.EventSourcing;
using NDomain.Model.EventSourcing.Azure;
using NUnit.Framework;
using NDomain.Tests.Common.Specs;

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
