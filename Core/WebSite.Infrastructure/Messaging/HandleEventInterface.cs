namespace Website.Infrastructure.Messaging
{
    public interface HandleEventInterface
    {
    }

    public interface HandleEventInterface<in EventType> : HandleEventInterface
    {
        bool Handle(EventType @event);
    }
}