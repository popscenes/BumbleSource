namespace Website.Infrastructure.Messaging
{
    public interface MessageHandlerInterface<in MessageType>
        where MessageType : MessageInterface
    {
        object Handle(MessageType command);
    }
}