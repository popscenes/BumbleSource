namespace WebSite.Infrastructure.Command
{
    public interface CommandHandlerInterface<in CommandType> where CommandType : CommandInterface
    {
        object Handle(CommandType command);
    }
}