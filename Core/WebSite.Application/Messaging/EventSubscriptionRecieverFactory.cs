namespace Website.Application.Messaging
{
    public interface EventSubscriptionRecieverFactory
    {
        SubscriptionReciever GetSubscriptionReciever(SubscriptionDetails subscriptionDetails);
    }
}