using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Website.Application.Messaging;

namespace Website.Application.Azure.ServiceBus
{
    public class AzureEventSubscriptionRecieverFactory : EventSubscriptionRecieverFactory
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;

        public AzureEventSubscriptionRecieverFactory(NamespaceManager namespaceManager, MessagingFactory messagingFactory)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
        }


        public SubscriptionReciever GetSubscriptionReciever(SubscriptionDetails subscriptionDetails)
        {
            if (!_namespaceManager.SubscriptionExists(subscriptionDetails.Topic, subscriptionDetails.Subscription))
                _namespaceManager.CreateSubscription(subscriptionDetails.Topic, subscriptionDetails.Subscription, new TrueFilter());

            var subscriptionClient = _messagingFactory.CreateSubscriptionClient(subscriptionDetails.Topic, subscriptionDetails.Subscription, ReceiveMode.PeekLock);
            var subscriptionRecieer = new AzureEventSubscriptionReciever(subscriptionClient);

            return subscriptionRecieer;
        }
    }
}