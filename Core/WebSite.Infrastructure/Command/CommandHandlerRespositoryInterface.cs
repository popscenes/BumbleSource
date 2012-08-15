using System;

namespace WebSite.Infrastructure.Command
{
    public interface CommandHandlerRespositoryInterface
    {
        CommandHandlerInterface<TCommand> findHandler<TCommand>(TCommand command) where TCommand : CommandInterface;
    }
}