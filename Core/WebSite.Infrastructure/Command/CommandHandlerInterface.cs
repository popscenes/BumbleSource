namespace Website.Infrastructure.Command
{
    public interface CommandHandlerInterface<in CommandType> : MessageHandlerInterface<CommandType>
        where CommandType : CommandInterface
    {

    }
}