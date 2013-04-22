namespace Website.Infrastructure.Publish
{
    public interface HandleEventInterface
    {
    }

    public interface HandleEventInterface<in EventType> : HandleEventInterface
    {
        bool Handle(EventType @event);
    }
}