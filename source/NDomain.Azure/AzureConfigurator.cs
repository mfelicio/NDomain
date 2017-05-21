using Microsoft.WindowsAzure.Storage;
using NDomain.Model.EventSourcing.Azure;
using NDomain.Bus.Transport.Azure.Queues;

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
