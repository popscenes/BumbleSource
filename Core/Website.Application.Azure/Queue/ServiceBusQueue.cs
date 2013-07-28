using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Website.Application.Queue;

namespace Website.Application.Azure.Queue
{
    public class ServiceBusQueue : QueueInterface
    {
        private readonly QueueClient queueClient;

        public ServiceBusQueue(QueueClient queueClient)
        {
            this.queueClient = queueClient;
        }

        public void AddMessage(QueueMessageInterface message)
        {
            var azureMsg = message as ServiceBusQueueMessage ?? new ServiceBusQueueMessage(message.Bytes);
            queueClient.Send(azureMsg.Message);
        }

        public QueueMessageInterface GetMessage()
        {
            var message = queueClient.Receive(new TimeSpan(0, 0, 10));
            return message == null ? null : new ServiceBusQueueMessage(message);
        }

        public QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut)
        {
            return GetMessage();
        }

        public void DeleteMessage(QueueMessageInterface message)
        {
            var azureMsg = message as ServiceBusQueueMessage;
            if (azureMsg == null) return;
            azureMsg.Message.Complete();
        }

        public int? ApproximateMessageCount { get { return null; } }
    }
}
