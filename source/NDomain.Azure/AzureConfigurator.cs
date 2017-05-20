using Microsoft.WindowsAzure.Storage;
using NDomain.Configuration;
using NDomain.Model.EventSourcing.Azure;
using NDomain.Bus.Transport.Azure.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
