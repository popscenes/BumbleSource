namespace Website.Infrastructure.Messaging
{
    public interface MessageHandlerRespositoryInterface
    {
        MessageHandlerInterface<MessageType> FindHandler<MessageType>(MessageType command) where MessageType : MessageInterface;
    }
}