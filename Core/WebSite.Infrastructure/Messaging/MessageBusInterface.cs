using Website.Infrastructure.Command;

namespace Website.Infrastructure.Messaging
{
    public interface MessageBusInterface
    {
        void Send<CommandType>(CommandType command) where CommandType : class, CommandInterface;
    }
}