namespace WebSite.Infrastructure.Command
{
    public interface CommandBusInterface
    {
        object Send<CommandType>(CommandType command) where CommandType : class, CommandInterface;
    }
}