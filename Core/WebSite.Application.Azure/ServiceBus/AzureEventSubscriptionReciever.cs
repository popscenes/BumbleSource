using System;
using Microsoft.ServiceBus.Messaging;
using Website.Application.Azure.Queue;
using Website.Application.Messaging;
using Website.Application.Queue;

namespace Website.Application.Azure.ServiceBus
{
    public class AzureEventSubscriptionReciever : SubscriptionReciever
    {
        private readonly SubscriptionClient _subscriptionClient;

        public AzureEventSubscriptionReciever(SubscriptionClient subscriptionClient)
        {
            _subscriptionClient = subscriptionClient;
        }

        public QueueMessageInterface GetMessage()
        {
            var receivedMessage = _subscriptionClient.Receive(new TimeSpan(0, 0, 10));
            return receivedMessage == null ? null : new ServiceBusQueueMessage(receivedMessage);
        }

        public QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut)
        {
            var receivedMessage = _subscriptionClient.Receive(invisibilityTimeOut);
            return receivedMessage == null ? null : new ServiceBusQueueMessage(receivedMessage);
        }

        public void DeleteMessage(QueueMessageInterface message)
        {
            var azureMsg = message as ServiceBusQueueMessage;
            if (azureMsg == null) return;
            azureMsg.Message.Complete();
        }

        public void ReturnMessage(QueueMessageInterface message)
        {
            var azureMsg = message as ServiceBusQueueMessage;
            if (azureMsg == null) return;
            azureMsg.Message.Abandon();
        }
    }
}