using Microsoft.WindowsAzure.Storage;
using NDomain.Azure.Bus.Transport.Queues;
using NDomain.Azure.Model.EventSourcing;

// ReSharper disable once CheckNamespace
namespace NDomain.Configuration
{
    public static class AzureConfigurator
    {
        public static ModelConfigurator WithAzureTableStorage(this ModelConfigurator configurator,
                                                                     CloudStorageAccount account,
                                                                     string tableName)
        {
            configurator.EventStoreDb = new AzureEventStore(account, tableName);

            return configurator;
        }

        public static BusConfigurator WithAzureQueues(this BusConfigurator configurator,
                                                      CloudStorageAccount account,
                                                      string prefix)
        {

            configurator.TransportFactory = new QueueTransportFactory(account, prefix);

            return configurator;
        }
    }
}
