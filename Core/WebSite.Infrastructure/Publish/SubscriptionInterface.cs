namespace Website.Infrastructure.Publish
{
    public interface SubscriptionInterface
    {
        bool IsEnabled { get; }
        string Description { get; }
        string Name { get; }
    }

    public interface SubscriptionInterface<in PublishType> : SubscriptionInterface
    {
        bool Publish(PublishType publish);
    }
}