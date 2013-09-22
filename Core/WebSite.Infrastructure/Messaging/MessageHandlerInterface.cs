namespace Website.Infrastructure.Messaging
{
    public interface MessageHandlerInterface<in MessageType>
        where MessageType : MessageInterface
    {
        void Handle(MessageType command);
    }
}