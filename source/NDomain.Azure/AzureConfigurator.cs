using Microsoft.WindowsAzure.Storage;
using NDomain.Configuration;
using NDomain.EventSourcing.Azure;
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
        public static EventSourcingConfigurator WithAzureTableStorage(this EventSourcingConfigurator configurator,
                                                                     CloudStorageAccount account,
                                                                     string tableName)
        {
            configurator.EventStoreDb = new AzureEventStore(account, tableName);

            return configurator;
        }

        public static BusConfigurator WithAzureQueues(this BusConfigurator configurator,
                                                      CloudStorageAccount account,
                                                      string queueName)
        {

            configurator.MessagingFactory = new QueueTransportFactory(account, queueName);

            return configurator;
        }
    }
}
