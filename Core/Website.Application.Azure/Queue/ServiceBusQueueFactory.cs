using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Configuration;

namespace Website.Application.Azure.Queue
{
    public class ServiceBusQueueFactory : QueueFactoryInterface
    {
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly string _namespaceConnectionString;

        public ServiceBusQueueFactory(ConfigurationServiceInterface configurationService,
                                      string namespaceConnectionString)
        {
            _configurationService = configurationService;
            _namespaceConnectionString = namespaceConnectionString;
        }

        public QueueInterface GetQueue(string queueName)
        {


            var connectionString =
                _configurationService.GetSetting(_namespaceConnectionString);



            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists(queueName))
            {
                var desc = namespaceManager.CreateQueue(queueName);
                desc.EnableDeadLetteringOnMessageExpiration = true;
                desc.EnableBatchedOperations = true;
                desc.LockDuration = TimeSpan.FromMinutes(2);
            }

            // Initialize the connection to Service Bus Queue
            //var client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            var msgFact = MessagingFactory.CreateFromConnectionString(connectionString);            
            var client = msgFact.CreateQueueClient(queueName);
            return new ServiceBusQueue(client);
        }

        public void DeleteQueue(string queueName)
        {
            var connectionString =
                _configurationService.GetSetting(_namespaceConnectionString);

            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (namespaceManager.QueueExists(queueName))
            {
                namespaceManager.DeleteQueue(queueName);
            }
        }
    }
}