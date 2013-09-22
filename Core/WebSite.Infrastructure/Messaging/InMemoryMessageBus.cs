using Website.Infrastructure.Command;

namespace Website.Infrastructure.Messaging
{
    public class InMemoryMessageBus : MessageBusInterface
    {
        private readonly MessageHandlerRespositoryInterface _handlerRespository;
        public InMemoryMessageBus(MessageHandlerRespositoryInterface handlerRespository)
        {
            _handlerRespository = handlerRespository;
        }

        public void Send<CommandType>(CommandType command) where CommandType : class, CommandInterface
        {
            var handler = _handlerRespository.FindHandler(command);

            handler.Handle(command);
        }
    }
}