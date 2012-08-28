using Website.Infrastructure.Command;

namespace Website.Application.Command
{
    public interface CommandSerializerInterface
    {
        CommandType FromByteArray<CommandType>(byte[] array) where CommandType : class, CommandInterface;
        byte[] ToByteArray<CommandType>(CommandType command) where CommandType : class, CommandInterface;
        void ReleaseCommand<CommandType>(CommandType command ) where CommandType : class, CommandInterface;
    }
}