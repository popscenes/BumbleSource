using Website.Infrastructure.Command;

namespace Website.Infrastructure.Messaging
{
    public interface MessageBusInterface
    {
        object Send<CommandType>(CommandType command) where CommandType : class, CommandInterface;
    }
}