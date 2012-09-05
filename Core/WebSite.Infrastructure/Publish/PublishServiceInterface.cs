namespace Website.Infrastructure.Publish
{
    public interface PublishServiceInterface
    {
        bool IsEnabled { get; }
        string Description { get; }
        string Name { get; }
    }

    public interface PublishServiceInterface<in PublishType> : PublishServiceInterface
    {
        bool Publish(PublishType publish);
    }
}