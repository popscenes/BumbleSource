using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Website.Application.Messaging;

namespace Website.Application.Azure.ServiceBus
{
    public class AzureEventTopicSenderFactory : EventTopicSenderFactoryInterface
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;

        private const string DefaultSubscription = "nofilter";

        public AzureEventTopicSenderFactory(NamespaceManager namespaceManager, MessagingFactory messagingFactory)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
        }

        public EventTopicSenderInterface GetTopicSender(Type type)
        {
            return GetTopicSender(type.Name.Replace("Event", ""));
        }

        public EventTopicSenderInterface GetTopicSender(string name)
        {
            if (_namespaceManager.TopicExists(name))
            {
                return new AzureEventTopcSender(_messagingFactory.CreateTopicClient(name));
            }

            _namespaceManager.CreateTopic(name);

            return new AzureEventTopcSender(_messagingFactory.CreateTopicClient(name));
        }
    }
}