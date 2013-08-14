using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public interface MessageSerializerInterface
    {
        MessageType FromByteArray<MessageType>(byte[] array) where MessageType : class, MessageInterface;
        byte[] ToByteArray<MessageType>(MessageType command) where MessageType : class, MessageInterface;
        void ReleaseCommand<MessageType>(MessageType command) where MessageType : class, MessageInterface;
    }
}