using System.Diagnostics;


namespace Website.Infrastructure.Command
{
    public class DefaultCommandBus : CommandBusInterface
    {
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        public DefaultCommandBus(CommandHandlerRespositoryInterface handlerRespository)
        {
            _handlerRespository = handlerRespository;
        }

        public object Send<CommandType>(CommandType command) where CommandType : class, CommandInterface
        {
            var handler = _handlerRespository.FindHandler(command);

             
            return handler.Handle(command);
        }
    }
}