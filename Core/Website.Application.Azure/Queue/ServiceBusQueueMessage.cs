using System;
using Microsoft.ServiceBus.Messaging;
using Website.Application.Queue;

namespace Website.Application.Azure.Queue
{
    public class ServiceBusQueueMessage : QueueMessageInterface
    {
        private BrokeredMessage _message;

        public ServiceBusQueueMessage(BrokeredMessage message)
        {
            _message = message;
        }

        public ServiceBusQueueMessage(byte[] bytes)
        {
            _message = new BrokeredMessage(bytes);
            _message.MessageId = Guid.NewGuid().ToString();

        }

        public byte[] Bytes {
            get { return _message.GetBody<byte[]>(); }
            set { _message = new BrokeredMessage(value); } }

        public BrokeredMessage Message{get { return _message; }}
    }
}