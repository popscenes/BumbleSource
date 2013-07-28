namespace Website.Infrastructure.Command
{
    public interface MessageHandlerInterface<in MessageType>
        where MessageType : MessageInterface
    {
        object Handle(MessageType command);
    }
}