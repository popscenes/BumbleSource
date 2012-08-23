using System;

namespace WebSite.Infrastructure.Command
{
    public interface CommandHandlerRespositoryInterface
    {
        CommandHandlerInterface<CommandType> FindHandler<CommandType>(CommandType command) where CommandType : CommandInterface;
    }
}