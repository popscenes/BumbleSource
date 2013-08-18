using System;
using System.Collections.Generic;
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
        private readonly string _queueSuffix;
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _msgFact;
        private readonly Dictionary<string, QueueClient> _queueClientCache = new Dictionary<string, QueueClient>();


        public ServiceBusQueueFactory(ConfigurationServiceInterface configurationService,
                                      string namespaceConnectionString, string queueSuffix = "")
        {
            _configurationService = configurationService;
            _namespaceConnectionString = namespaceConnectionString;
            _queueSuffix = queueSuffix;
            var connectionString = _configurationService.GetSetting(_namespaceConnectionString);
            _namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            _msgFact = MessagingFactory.CreateFromConnectionString(connectionString);

        }

        private string GetQueueName(string queueName)
        {
            return string.IsNullOrWhiteSpace(_queueSuffix) ? queueName : queueName + "-" + _queueSuffix;
        }

        public QueueInterface GetQueue(string queueName)
        {
            queueName = GetQueueName(queueName);

            QueueClient client;
            if (!_queueClientCache.TryGetValue(queueName, out client)
                && !_namespaceManager.QueueExists(queueName))
            {
                _namespaceManager.CreateQueue(new QueueDescription(queueName)
                {
                    EnableDeadLetteringOnMessageExpiration = true,
                    EnableBatchedOperations = true,
                    LockDuration = TimeSpan.FromMinutes(2)
                });
            }

            if (client == null)
            {
                _queueClientCache[queueName] = client = _msgFact.CreateQueueClient(queueName);
            }

            // Initialize the connection to Service Bus Queue
            //var client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            //_msgFact.CreateSubscriptionClient("Test", "Test", ReceiveMode.PeekLock);
            //msgFact.CreateTopicClient("Test");
              
            return new ServiceBusQueue(client);
        }

        public void DeleteQueue(string queueName)
        {
            queueName = GetQueueName(queueName);

            if (_namespaceManager.QueueExists(queueName))
            {
                _namespaceManager.DeleteQueue(queueName);
                _queueClientCache.Remove(queueName);
            }
        }

        public bool QueueExists(string queueName)
        {
            queueName = GetQueueName(queueName);

            return (_namespaceManager.QueueExists(queueName));
        }
    }
}