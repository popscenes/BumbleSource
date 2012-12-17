using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Website.Application.Queue;

namespace Website.Application.Azure.Queue
{
    public class AzureCloudQueueFactory : QueueFactoryInterface
    {
        private readonly CloudQueueClient _cloudQueueClient;

        public AzureCloudQueueFactory(CloudQueueClient cloudQueueClient)
        {
            _cloudQueueClient = cloudQueueClient;
        }

        public QueueInterface GetQueue(string queueName)
        {
            var azureCloudQueue = new AzureCloudQueue(_cloudQueueClient.GetQueueReference(queueName));
            azureCloudQueue.CreateIfNotExist();
            return azureCloudQueue;
        }

        public void DeleteQueue(string queueName)
        {
            var azureCloudQueue = new AzureCloudQueue(_cloudQueueClient.GetQueueReference(queueName));
            if (azureCloudQueue.Exists())
                azureCloudQueue.Delete();
        }
    }
}
