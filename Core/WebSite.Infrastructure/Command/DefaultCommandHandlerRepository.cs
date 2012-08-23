using Ninject;
using Ninject.Syntax;

namespace WebSite.Infrastructure.Command
{
    public class DefaultCommandHandlerRepository : CommandHandlerRespositoryInterface
    {
        private readonly IResolutionRoot _resolver;
        public DefaultCommandHandlerRepository(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }
        public CommandHandlerInterface<TCommand> FindHandler<TCommand>(TCommand command) where TCommand : CommandInterface
        {
            var res = _resolver.Get<CommandHandlerInterface<TCommand>>();
            return res;
        }
    }
}